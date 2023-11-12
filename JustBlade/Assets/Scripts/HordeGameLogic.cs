using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A script which designates the attached game object as a Horde Game Logic.
/// It contains the logic the horde game mode.
/// </summary>
public class HordeGameLogic : MonoBehaviour
{
    /// <summary>
    /// A simple class which describes a wave set.
    /// A wave set contains data about a list of waves.
    /// The <see cref="isBossWaveSet"/> is used by <see cref="InformationMenuUI"/> to
    /// add different flavor text when there's going to be a boss battle.
    /// </summary>
    [System.Serializable]
    public class WaveSet
    {
        public List<Wave> waves;
        public bool isBossWaveSet;
    }

    /// <summary>
    /// A simple class which describes a wave.
    /// A wave contains a list of <see cref="InvaderData"/>.
    /// There can be many types of invaders within a single wave.
    /// </summary>
    [System.Serializable]
    public class Wave
    {
        public List<InvaderData> invaderDataList;
    }

    /// <summary>
    /// A simple class which describes a single invader data.
    /// It contains a prefab reference <see cref="invaderAgentDataPrefab"/>, which is meant to be
    /// set in the Inspector menu. It describes the properties of an invader.
    /// The <see cref="invaderCount"/> field describes how many of such invaders should be spawned.
    /// </summary>
    [System.Serializable]
    public class InvaderData
    {
        public InvaderAgentData invaderAgentDataPrefab;
        public int invaderCount;
    }

    /// <summary>
    /// Determines direction in which the agents are spawned.
    /// <seealso cref="playerTeamSpawnPoint"/>.
    /// <seealso cref="enemyTeamSpawnPoint"/>.
    /// </summary>
    public enum SpawnDirection { Left, Right }

    /// <summary>
    /// True if the player died; false otherwise.
    /// </summary>
    public static bool IsPlayerDied;
    /// <summary>
    /// True if the player has beaten the horde game; false otherwise.
    /// </summary>
    public static bool IsPlayerBeatenTheGame;
    /// <summary>
    /// True if the game has ended; false otherwise.
    /// </summary>
    public static bool IsGameEnded { get { return IsPlayerDied || IsPlayerBeatenTheGame; } }
    /// <summary>
    /// True if the game has just begun; false otherwise.
    /// It is used by <see cref="InformationMenuUI"/> to show the introductory flavor text.
    /// </summary>
    public static bool IsGameHasJustBegun { get { return iCurWaveSet == 0; } }
    /// <summary>
    /// Describes the number of waves the player has beaten.
    /// Note that this is incremented after each wave. If, for whatever reason, the game does not start
    /// from the beginning, then the waves that have not been played will not have been tracked by this field.
    /// </summary>
    public static int NumberOfWavesBeaten { get; private set; }
    /// <summary>
    /// Describes the total number of waves in the horde game.
    /// The number of wave sets and waves are different things.
    /// In the end, this field tells you how many waves there are, regardless of the number of wave sets.
    /// It is calculated once when the game starts, done by <see cref="CalculateTotalNumberOfWavesOnce"/>.
    /// </summary>
    public static int TotalNumberOfWaves { get; private set; }
    /// <summary>
    /// True if the next wave set is a <see cref="WaveSet.isBossWaveSet"/>; false otherwise.
    /// Used by <see cref="InformationMenuUI"/> to show different flavor text for boss battles.
    /// </summary>
    public static bool IsBossBattleNext { get; private set; }
    static bool isTotalNumWavesCalculated;

    /// <summary>
    /// Starts a new horde game.
    /// This is the method you want to invoke when your "Start Game" button is pressed.
    /// Currently, it is invoked by <see cref="MainMenuUI.btnStartGame"/>.
    /// </summary>
    public static void StartNewHordeGame()
    {
        iCurWaveSet = 0;

        NumberOfWavesBeaten = 0;

        IsPlayerDied = false;
        IsPlayerBeatenTheGame = false;
        IsBossBattleNext = false;

        isTotalNumWavesCalculated = false;

        PlayerInventoryManager.InitializePlayerInventory();
        PlayerPartyManager.InitializePlayerParty();
        PlayerStatisticsTracker.Initialize();
    }

    /// <summary>
    /// Reference to a game object which describes the player team's spawn point, set in the Inspector menu.
    /// </summary>
    public Transform playerTeamSpawnPoint;
    /// <summary>
    /// The direction in which the player team agents will spawn, set in the Inspector menu.
    /// </summary>
    public SpawnDirection playerTeamSpawnDirection;

    /// <summary>
    /// Reference to a game object which describes the enemy team's spawn point, set in the Inspector menu.
    /// </summary>
    public Transform enemyTeamSpawnPoint;
    /// <summary>
    /// The direction in which the enemy team agents will spawn, set in the Inspector menu.
    /// </summary>
    public SpawnDirection enemyTeamSpawnDirection;

    /// <summary>
    /// Reference to the <see cref="PlayerAgent"/> prefab, set in the Inspector menu.
    /// </summary>
    public PlayerAgent playerAgentPrefab;
    /// <summary>
    /// Reference to the <see cref="AiAgent"/> prefab, set in the Inspector menu.
    /// </summary>
    public AiAgent aiAgentPrefab;

    static int iCurWaveSet = 0;
    int iCurWave = -1; // must be -1

    /// <summary>
    /// A list of wave sets, set in the Inspector menu.
    /// The user (ie, designer) can easily fill in the wave sets, waves, types of enemies, etc.
    /// by filling in this list in the Inspector menu.
    /// </summary>
    public List<WaveSet> waveSets;

    readonly float SpawnAgentBaseDistance = 2.0f; // (3.0f was ok in the past. Got more agents now. Gotta fit them in.)

    readonly float HoldPositionPlayerBaseBackDistance = 0.25f; // hold position behind the player, by at least this much.
    readonly float HoldPositionBaseSideBySideDistance = 0.75f; // "side by side" distance when holding position

    readonly float sceneTransitionTime = 3.0f; // time spent before transitioning out from this scene

    List<Agent> playerTeamAgents;
    List<Agent> enemyTeamAgents;

    /// <summary>
    /// A queue of <see cref="Agent"/>s to "think".
    /// In each Update frame, an Agent will be selected from the queue to "think".
    /// Thinking could mean:
    /// - Thinking about choosing to target a nearby enemy
    /// - Setting combat direction preference (ie, preferring vertical strikes more).
    /// </summary>
    Queue<Agent> AgentThinkQueue
    {
        get
        {
            if (agentThinkQueue == null)
            {
                agentThinkQueue = new Queue<Agent>();
            }

            return agentThinkQueue;
        }
    }
    Queue<Agent> agentThinkQueue;

    /// <summary>
    /// Initializes an <see cref="Agent"/> from <see cref="HordeAgentData"/> data.
    /// The method argument takes individual elements of <see cref="HordeAgentData"/>, so that we can re-use
    /// this method to initialize the common aspects of both
    /// <see cref="InvaderAgentData"/> and <see cref="MercenaryAgentData"/>.
    /// </summary>
    /// <param name="agent">Agent to be initialized.</param>
    /// <param name="hordeWeaponSet">A horde weapon set prefab.</param>
    /// <param name="hordeArmorSet">A horde armor set prefab.</param>
    /// <param name="characteristicSet">A characteristic set prefab.</param>
    void InitializeAgentFromHordeData(Agent agent
        , HordeWeaponSet hordeWeaponSet
        , HordeArmorSet hordeArmorSet
        , CharacteristicSet characteristicSet)
    {
        Weapon weaponPrefab = hordeWeaponSet.GetRandomWeapon();
        Armor headArmorPrefab = hordeArmorSet.GetRandomArmor(Armor.ArmorType.Head);
        Armor torsoArmorPrefab = hordeArmorSet.GetRandomArmor(Armor.ArmorType.Torso);
        Armor handArmorPrefab = hordeArmorSet.GetRandomArmor(Armor.ArmorType.Hand);
        Armor legArmorPrefab = hordeArmorSet.GetRandomArmor(Armor.ArmorType.Leg);

        agent.InitializeAgent(weaponPrefab
            , headArmorPrefab
            , torsoArmorPrefab
            , handArmorPrefab
            , legArmorPrefab
            , characteristicSet);
    }

    /// <summary>
    /// A callback method for when any <see cref="Agent"/> invokes the <see cref="Agent.AgentDeathEvent"/> event.
    /// The <see cref="HordeGameLogic"/> will decide what to do, depending on what kind of <see cref="Agent"/> died.
    /// </summary>
    /// <param name="victim">The agent who died.</param>
    /// <param name="killer">The agent who killed the victim.</param>
    void OnAgentDeath(Agent victim, Agent killer)
    {
        if (victim.IsPlayerAgent)
        {
            IsPlayerDied = true;
            ConcludeWaveSet();
        }

        if (IsPlayerDied)
        {
            // No point in doing anything else if the player is dead, since it's just game over.
            return;
        }

        if (killer.IsPlayerAgent)
        {
            PlayerStatisticsTracker.PlayerTotalKillCount++;
        }
        else if (killer.IsFriendOfPlayer)
        {
            PlayerStatisticsTracker.MercenariesTotalKillCount++;
        }

        if (victim.IsFriendOfPlayer)
        {
            playerTeamAgents.Remove(victim);

            MercenaryDescriptionData mdd = victim.GetComponent<MercenaryDescriptionData>();
            if (mdd != null)
            {
                PlayerPartyManager.KillMercenary(mdd.mercArmorLevel);
            }
        }
        else
        {
            enemyTeamAgents.Remove(victim);

            HordeRewardData hrd = victim.GetComponent<HordeRewardData>();
            bool isFinalWaveSet = iCurWaveSet == (waveSets.Count - 1);
            if (hrd != null && isFinalWaveSet == false)
            {
                PlayerInventoryManager.AddPlayerGold(hrd.GetRandomGoldAmountWithinRange());
            }
        }

        if (enemyTeamAgents.Count == 0)
        {
            NumberOfWavesBeaten++;
            SpawnNextWave();
        }
    }

    /// <summary>
    /// A callback method for when any <see cref="AiAgent"/> invokes the <see cref="AiAgent.AiAgentSearchForEnemyEvent"/> event.
    /// When an <see cref="AiAgent"/> searches for an enemy to fight, this method provides it.
    /// If there are no enemies to be provided, then the method returns null.
    /// </summary>
    /// <param name="caller">The <see cref="AiAgent"/> who is searching for an enemy.</param>
    /// <returns>An enemy for the caller agent (or null).</returns>
    Agent OnAiAgentSearchForEnemy(AiAgent caller)
    {
        Agent ret = null;

        // Assume that the caller is a friend of the player, thus needs an enemy from the enemy team.
        List<Agent> listOfEnemies = enemyTeamAgents;
        if (caller.IsFriendOfPlayer == false)
        {
            // Turns out, the caller is an enemy of the player, thus needs an enemy from the player team.
            listOfEnemies = playerTeamAgents;
        }

        List<Agent> agents = listOfEnemies.FindAll(a => (a != null) && (a.IsDead == false));

        if (agents.Count >= 1)
        {
            ret = agents[Random.Range(0, agents.Count)];
        }

        return ret;
    }

    /// <summary>
    /// A callback method for when the <see cref="PlayerAgent"/> invokes the <see cref="PlayerAgent.PlayerOrderToggleEvent"/> event.
    /// When the player toggles the orders for his mercenaries, this callback method will receive this information.
    /// If the order is to attack, then the player's mercenaries are told to attack at will.
    /// If the order is to hold position, then this method will calculate which position must be held for each mercenary,
    /// and it will inform each mercenary <see cref="AiAgent"/> using <see cref="AiAgent.ToggleHoldPosition(bool, Vector3)"/>.
    /// </summary>
    /// <param name="playerAgent">A reference to the player agent game object, to find out his transform.position.</param>
    /// <param name="isPlayerOrderingToHoldPosition">True if the order is "hold position"; false if the order is "attack".</param>
    void OnPlayerOrderToggleEvent(PlayerAgent playerAgent, bool isPlayerOrderingToHoldPosition)
    {
        if (isPlayerOrderingToHoldPosition == false)
        {
            for (int i = 0; i < playerTeamAgents.Count; i++)
            {
                if (playerTeamAgents[i].IsPlayerAgent)
                {
                    continue;
                }

                AiAgent ai = playerTeamAgents[i] as AiAgent;

                if (ai != null)
                {
                    ai.ToggleHoldPosition(false, Vector3.zero);
                }
            }

            return;
        }

        List<Agent> friendlyAiAgents =
            playerTeamAgents.FindAll(a => (a != null) && (a.IsPlayerAgent == false) && (a.IsDead == false));

        // From now on, we assume that the order given is to "hold position".
        Vector3 playerPos = playerAgent.transform.position;
        float playerRadiusMulti = (HoldPositionPlayerBaseBackDistance + (playerAgent.CharMgr.AgentWorldRadius * 2f));
        Vector3 playerBack = (-playerAgent.transform.forward) * playerRadiusMulti;
        Vector3 spawnPos = playerPos + playerBack;

        List<Vector3> posList = new List<Vector3>();

        for (int i = 0; i < friendlyAiAgents.Count; i++)
        {
            //Vector3 offset = (playerAgent.transform.right) * (1f + (friendlyAiAgents[i].CharMgr.AgentWorldRadius * 2f));
            float dist = HoldPositionBaseSideBySideDistance + (friendlyAiAgents[i].CharMgr.AgentWorldRadius * 2f);
            Vector3 offset = Vector3.right * dist;
            offset = playerAgent.transform.TransformDirection(offset);

            posList.Add(spawnPos);

            spawnPos += offset;
        }

        // If there are more than 1 friendly agents, then calculate the "spawn line length".
        // Meaning, how far do these spawn positions go as a line? Then, what is the length of this line?
        // Finally, move the spawn positions towards the direction of player's left by half of the spawn line length.
        if (posList.Count > 1)
        {
            int lastIndex = posList.Count - 1;

            Vector3 firstPos = posList[0];
            Vector3 lastPos = posList[lastIndex];

            float dist = Vector3.Distance(firstPos, lastPos);

            // Move all positions towards player's left by half of distance.
            for (int i = 0; i < friendlyAiAgents.Count; i++)
            {
                posList[i] += (-playerAgent.transform.right) * (dist / 2f);
            }
        }

        // Now, give the order.
        for (int i = 0; i < friendlyAiAgents.Count; i++)
        {
            AiAgent ai = friendlyAiAgents[i] as AiAgent;
            ai.ToggleHoldPosition(true, posList[i]);
        }
    }

    /// <summary>
    /// Starts current wave set by calling <see cref="SpawnPlayerTeamAgents"/> and <see cref="SpawnNextWave"/>.
    /// </summary>
    void StartWaveSet()
    {
        SpawnPlayerTeamAgents();

        SpawnNextWave();
    }

    /// <summary>
    /// Spawns the <see cref="PlayerAgent"/> as well as his mercenary <see cref="AiAgent"/>s.
    /// </summary>
    void SpawnPlayerTeamAgents()
    {
        playerTeamAgents = new List<Agent>();

        Vector3 spawnPos = playerTeamSpawnPoint.position;
        Vector3 dir = playerTeamSpawnDirection == SpawnDirection.Right ? Vector3.right : Vector3.left;

        SpawnPlayer();
        SpawnPlayerFriendlyAgents();
        InitializeTeamPositions(playerTeamAgents, spawnPos, dir, true);
    }

    /// <summary>
    /// Spawns the player.
    /// </summary>
    void SpawnPlayer()
    {
        // Spawn the player at the spawn point so that NavMeshAgent doesn't get glued to the NavMesh
        // around the default position of instantiation.
        PlayerAgent player = Instantiate(playerAgentPrefab, playerTeamSpawnPoint.position, Quaternion.identity);

        // Player gets default Characteristics.
        player.InitializeAgent(
            PrefabManager.Weapons[PlayerInventoryManager.PlayerChosenWeaponIndex]
          , PrefabManager.HeadArmors[PlayerInventoryManager.PlayerChosenHeadArmorIndex]
          , PrefabManager.TorsoArmors[PlayerInventoryManager.PlayerChosenTorsoArmorIndex]
          , PrefabManager.HandArmors[PlayerInventoryManager.PlayerChosenHandArmorIndex]
          , PrefabManager.LegArmors[PlayerInventoryManager.PlayerChosenLegArmorIndex]
          , PlayerCharacteristicProgressionTracker.PlayerCharSet);

        player.IsFriendOfPlayer = true;
        player.OnDeath += OnAgentDeath;

        player.PlayerOrderToggle += OnPlayerOrderToggleEvent;

        playerTeamAgents.Add(player);
    }

    /// <summary>
    /// Spawns the player's mercenary <see cref="AiAgent"/>s.
    /// The kind of agents which will be spawned are based on the player's party in the horde game mode.
    /// See also: <see cref="PlayerPartyManager"/>.
    /// </summary>
    void SpawnPlayerFriendlyAgents()
    {
        SpawnPlayerMercenaryFromData(
            PrefabManager.MercenaryDataByArmorLevel[Armor.ArmorLevel.None]
            , PlayerPartyManager.GetMercenaryCount(Armor.ArmorLevel.None));

        SpawnPlayerMercenaryFromData(
            PrefabManager.MercenaryDataByArmorLevel[Armor.ArmorLevel.Light]
            , PlayerPartyManager.GetMercenaryCount(Armor.ArmorLevel.Light));

        SpawnPlayerMercenaryFromData(
            PrefabManager.MercenaryDataByArmorLevel[Armor.ArmorLevel.Medium]
            , PlayerPartyManager.GetMercenaryCount(Armor.ArmorLevel.Medium));

        SpawnPlayerMercenaryFromData(
            PrefabManager.MercenaryDataByArmorLevel[Armor.ArmorLevel.Heavy]
            , PlayerPartyManager.GetMercenaryCount(Armor.ArmorLevel.Heavy));
    }

    /// <summary>
    /// Spawns a friendly mercenary for the player, using <see cref="MercenaryAgentData"/>.
    /// </summary>
    /// <param name="mercData">A reference o the mercenary data to be spawned.</param>
    /// <param name="count">The number of mercenaries of this type to be spawned.</param>
    void SpawnPlayerMercenaryFromData(MercenaryAgentData mercData, int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Spawn the agent at the spawn point so that NavMeshAgent doesn't get glued to the NavMesh
            // around the default position of instantiation.
            AiAgent merc = Instantiate(aiAgentPrefab, playerTeamSpawnPoint.position, Quaternion.identity);
            merc.OnSearchForEnemyAgent += OnAiAgentSearchForEnemy;
            InitializeAgentFromHordeData(merc
                , mercData.weaponSetPrefab
                , mercData.armorSetPrefab
                , mercData.charSetPrefab);

            merc.IsFriendOfPlayer = true;
            merc.OnDeath += OnAgentDeath;

            MercenaryDescriptionData mdd = merc.gameObject.AddComponent<MercenaryDescriptionData>();
            mdd.InitializeFromMercenaryData(mercData);

            playerTeamAgents.Add(merc);

            AgentThinkQueue.Enqueue(merc);
        }
    }

    /// <summary>
    /// Spawns enemy invaders to fight the player and his mercenaries.
    /// The number of how many such invaders should be spawned depends on <see cref="InvaderData.invaderCount"/>,
    /// which is set in the Inspector menu.
    /// </summary>
    /// <param name="invaderData"></param>
    void SpawnInvadersFromData(InvaderData invaderData)
    {
        for (int i = 0; i < invaderData.invaderCount; i++)
        {
            // Spawn the agent at the spawn point so that NavMeshAgent doesn't get glued to the NavMesh
            // around the default position of instantiation.
            AiAgent a = Instantiate(aiAgentPrefab, enemyTeamSpawnPoint.position, Quaternion.identity);
            InitializeAgentFromHordeData(a
                , invaderData.invaderAgentDataPrefab.weaponSetPrefab
                , invaderData.invaderAgentDataPrefab.armorSetPrefab
                , invaderData.invaderAgentDataPrefab.charSetPrefab);

            a.OnSearchForEnemyAgent += OnAiAgentSearchForEnemy;
            a.IsFriendOfPlayer = false;
            a.OnDeath += OnAgentDeath;

            a.IsAggressive = invaderData.invaderAgentDataPrefab.isAggressive;

            HordeRewardData hrd = a.gameObject.AddComponent<HordeRewardData>();
            hrd.CopyDataFromPrefab(invaderData.invaderAgentDataPrefab.invaderRewardDataPrefab);

            enemyTeamAgents.Add(a);

            AgentThinkQueue.Enqueue(a);
        }
    }

    /// <summary>
    /// Initializes the positions of each <see cref="Agent"/> on the scene.
    /// Note that the agents must have already been spawned for this method to work properly.
    /// This method can also optionally randomize the positions of the spawned agents, as well as
    /// center their positions based on the spawn position argument.
    /// </summary>
    /// <param name="agentTeam">The team of the agents whose positions will be initialized.</param>
    /// <param name="spawnPos">The initial spawn position.</param>
    /// <param name="dir">The direction in which spawn position will be offset as the agents are be spawned.</param>
    /// <param name="randomizePositions">True if the positions of the agents should be randomized; false otherwise.</param>
    /// <param name="centerThePositions">True if the method should center the positions of the agents; false otherwise.</param>
    void InitializeTeamPositions(List<Agent> agentTeam
        , Vector3 spawnPos
        , Vector3 dir
        , bool randomizePositions = false
        , bool centerThePositions = true)
    {
        if (randomizePositions)
        {
            Shuffle(agentTeam);
        }

        Agent player = null;

        for (int i = 0; i < agentTeam.Count; i++)
        {
            if (agentTeam[i].IsPlayerAgent)
            {
                player = agentTeam[i];
                break;
            }
        }

        if (player != null)
        {
            // It's the player team. Put the player in the middle.
            agentTeam.Remove(player);
            int iMid = agentTeam.Count / 2;
            agentTeam.Insert(iMid, player);
        }

        List<Vector3> spawnPositions = new List<Vector3>();

        for (int i = 0; i < agentTeam.Count; i++)
        {
            Agent agent = agentTeam[i];
            Vector3 nextAgentSpawnOffset = dir * (2 * agent.CharMgr.AgentWorldRadius + SpawnAgentBaseDistance);

            spawnPositions.Add(spawnPos);

            spawnPos = spawnPos + nextAgentSpawnOffset;
        }

        if (centerThePositions && agentTeam.Count > 1)
        {
            // Find the distance between minPos and maxPos.
            // Then, move all positions in the opposite direction of "dir", by half of the distance.
            // This way, we centralize the positions.

            Vector3 minPos = spawnPositions[0];
            Vector3 maxPos = spawnPositions[spawnPositions.Count - 1];
            float dist = Vector3.Distance(minPos, maxPos);

            for (int i = 0; i < spawnPositions.Count; i++)
            {
                spawnPositions[i] += (-dir) * (dist / 2f);
            }
        }

        // Finally, set positions.
        for (int i = 0; i < agentTeam.Count; i++)
        {
            agentTeam[i].InitializePosition(spawnPositions[i]);
            //agentTeam[i].transform.position = spawnPositions[i];
        }
    }

    /// <summary>
    /// Shuffles an <see cref="List{T}"/> based on Fisher-Yates shuffle.
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <param name="list">List to be shuffled</param>
    void Shuffle<T>(IList<T> list)
    {
        int count = list.Count;
        for (int i = 0; i < count; i++)
        {
            int iRand = Random.Range(i, count);
            T temp = list[i];
            list[i] = list[iRand];
            list[iRand] = temp;
        }
    }

    /// <summary>
    /// Spawns the next wave of invaders.
    /// If there are no more waves left, then the wave set is concluded via <see cref="ConcludeWaveSet"/>.
    /// </summary>
    void SpawnNextWave()
    {
        iCurWave++;

        enemyTeamAgents = new List<Agent>();

        // Check if there's any wave set at all.
        if (waveSets == null || waveSets.Count == 0)
        {
            ConcludeWaveSet();
            return;
        }

        // We have a waveset.
        WaveSet waveSet = waveSets[iCurWaveSet];

        // Check if we have any waves remaining.
        if (iCurWave == waveSet.waves.Count)
        {
            ConcludeWaveSet();
            return;
        }

        Wave curWave = waveSet.waves[iCurWave];
        List<InvaderData> invaderDataList = curWave.invaderDataList;

        Vector3 spawnPos = enemyTeamSpawnPoint.position;
        Vector3 dir = (enemyTeamSpawnDirection == SpawnDirection.Right) ? Vector3.right : Vector3.left;

        for (int i = 0; i < invaderDataList.Count; i++)
        {
            InvaderData invaderData = invaderDataList[i];
            SpawnInvadersFromData(invaderData);
        }

        InitializeTeamPositions(enemyTeamAgents, spawnPos, dir, true);
    }

    /// <summary>
    /// Concludes this wave set, and invokes the <see cref="ConcludeWaveSetCoroutine"/> coroutine.
    /// It also sets the necessary variables based on whatever happened in this wave set.
    /// </summary>
    void ConcludeWaveSet()
    {
        iCurWaveSet++;

        IsBossBattleNext = false;

        if (iCurWaveSet == waveSets.Count)
        {
            IsPlayerBeatenTheGame = true;
        }
        else if (waveSets[iCurWaveSet].isBossWaveSet)
        {
            IsBossBattleNext = true;
        }

        StartCoroutine("ConcludeWaveSetCoroutine");
    }

    /// <summary>
    /// Make one agent "think", based on the <see cref="AgentThinkQueue"/>.
    /// </summary>
    void MakeOneAgentThink()
    {
        if (AgentThinkQueue.Count < 1)
        {
            return;
        }

        Agent agent = AgentThinkQueue.Dequeue();
        if (agent == null || agent.IsDead || agent.IsPlayerAgent)
        {
            return;
        }
        else
        {
            AgentThinkQueue.Enqueue(agent);
        }

        ToggleAiCombatDirectionPreference(agent);
        ConsiderNearbyEnemy(agent);
    }

    /// <summary>
    /// Make an agent consider a nearby enemy.
    /// Perhaps the argument agent will consider to target the nearby enemy.
    /// This is done by <see cref="Agent.ConsiderNearbyEnemy(Agent)"/>.
    /// </summary>
    /// <param name="agent">Agent who should do the consideration.</param>
    void ConsiderNearbyEnemy(Agent agent)
    {
        // Assume that thisAgent is a friend of the player.
        List<Agent> listToChoose = enemyTeamAgents;
        if (agent.IsFriendOfPlayer == false)
        {
            // Turns out, thisAgent is an enemy of the player.
            listToChoose = playerTeamAgents;
        }

        List<Agent> agentEnemies = listToChoose.FindAll(otherAgent =>
        (otherAgent != null) && (otherAgent.IsDead == false) && (otherAgent != agent));

        if (agentEnemies.Count < 1)
        {
            // No enemies, nothing to consider.
            return;
        }

        (Agent, float) minDistAgentData = GetMinDistanceAgent(agent, agentEnemies);
        agent.ConsiderNearbyEnemy(minDistAgentData.Item1);
    }

    /// <summary>
    /// Toggle the combat direction preference for the argument <see cref="Agent"/>.
    /// Primarily used by <see cref="AiAgent"/>s.
    /// This method calculates the nearest friend of the argument agent.
    /// Then, it invokes <see cref="Agent.ToggleCombatDirectionPreference(float)"/>.
    /// If the argument agent has many friendly agents nearby, then perhaps he will consider using vertical attacks,
    /// rather than using every attack direction equally likely.
    /// </summary>
    /// <param name="agent"></param>
    void ToggleAiCombatDirectionPreference(Agent agent)
    {
        // Assume that thisAgent is a friend of the player.
        List<Agent> listToChoose = playerTeamAgents;
        if (agent.IsFriendOfPlayer == false)
        {
            // Turns out, thisAgent is an enemy of the player.
            listToChoose = enemyTeamAgents;
        }

        List<Agent> agentFriends = listToChoose.FindAll(otherAgent =>
        (otherAgent != null) && (otherAgent.IsDead == false) && (otherAgent != agent));

        if (agentFriends.Count < 1)
        {
            agent.ToggleCombatDirectionPreference(float.MaxValue);
            return;
        }


        (Agent, float) minDistAgentData = GetMinDistanceAgent(agent, agentFriends);
        agent.ToggleCombatDirectionPreference(minDistAgentData.Item2);
    }

    /// <summary>
    /// Takes an <paramref name="agent"/> and a list of <paramref name="otherAgents"/>,
    /// and returns an other agent to which the first argument agent is the closest.
    /// Note that this method will throw an exception is <paramref name="otherAgents"/> is null or empty.
    /// </summary>
    /// <param name="agent">Agent against which the distance calculations are done.</param>
    /// <param name="otherAgents">List of agents which are different from the first argument.</param>
    /// <returns>The closest agent in terms of distance, and the distance value with it</returns>
    (Agent, float) GetMinDistanceAgent(Agent agent, List<Agent> otherAgents)
    {
        List<(Agent, float)> agentData = new List<(Agent, float)>();
        for (int i = 0; i < otherAgents.Count; i++)
        {
            Vector3 diff = agent.transform.position - otherAgents[i].transform.position;
            agentData.Add((otherAgents[i], diff.sqrMagnitude));
        }

        (Agent, float) minAgentData =
            agentData.OrderBy(agentData => agentData.Item2).First();

        // The distances were squared above.
        // Now that we know who is the closest, take the square root of the distance, and return it.
        minAgentData.Item2 = Mathf.Sqrt(minAgentData.Item2);

        return minAgentData;
    }

    /// <summary>
    /// A coroutine which is used to conclude the wave set based on <see cref="sceneTransitionTime"/>.
    /// </summary>
    /// <returns>Some kind of Unity coroutine magic thing.</returns>
    IEnumerator ConcludeWaveSetCoroutine()
    {
        yield return new WaitForSeconds(sceneTransitionTime);
        SceneManager.LoadScene("InformationMenuScene");
    }

    /// <summary>
    /// Calculates how many waves exist in the horde game.
    /// Wave sets and waves are different concepts.
    /// Regardless of how many wave sets exist, we sometimes only care about the number of waves.
    /// The wave sets and waves are set in Unity's Inspector menu, so we have to calculate how many of them
    /// exist at runtime. We only have to do this calculation once. However, since <see cref="TotalNumberOfWaves"/>
    /// is a static method, and since we want to do this once, we have to use an instance method to achieve this.
    /// Perhaps a bit hacky, but the alternative is to use non-static fields and track them over multiple scenes,
    /// as Unity destroys everything in a scene when transitioning to another scene.
    /// </summary>
    void CalculateTotalNumberOfWavesOnce()
    {
        if (isTotalNumWavesCalculated == false)
        {
            int sum = 0;
            if (waveSets == null)
            {
                return;
            }

            for (int i = 0; i < waveSets.Count; i++)
            {
                if (waveSets[i] != null && waveSets[i].waves != null)
                {
                    sum += waveSets[i].waves.Count;
                }
            }

            TotalNumberOfWaves = sum;
            isTotalNumWavesCalculated = true;
        }
    }

    /// <summary>
    /// Unity's Start method, used to start a new wave set.
    /// </summary>
    void Start()
    {
        CalculateTotalNumberOfWavesOnce();

        StartWaveSet();
    }

    /// <summary>
    /// Unity's Update method.
    /// Every frame, one agent from the <see cref="AgentThinkQueue"/> will be chosen, and
    /// that agent will be allowed to "think" about what to do.
    /// We avoid making all agents "think" every frame to save performance.
    /// Perhaps it's a premature optimization, but I think it makes sense.
    /// </summary>
    void Update()
    {
        MakeOneAgentThink();
    }
}
