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
    [System.Serializable]
    public class WaveSet
    {
        public List<Wave> waves;
        public bool isBossWaveSet;
    }

    [System.Serializable]
    public class Wave
    {
        public List<InvaderData> invaderDataList;
    }

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

    public static bool IsPlayerDied;
    public static bool IsPlayerBeatenTheGame;
    public static bool IsGameEnded { get { return IsPlayerDied || IsPlayerBeatenTheGame; } }
    public static bool IsGameHasJustBegun { get { return iCurWaveSet == 0; } }
    public static int NumberOfWavesBeaten { get; private set; }
    public static int TotalNumberOfWaves { get; private set; }
    public static bool IsBossBattleNext { get; private set; }
    static bool isTotalNumWavesCalculated;

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

    public Transform playerTeamSpawnPoint;
    public SpawnDirection playerTeamSpawnDirection;

    public Transform enemyTeamSpawnPoint;
    public SpawnDirection enemyTeamSpawnDirection;

    public PlayerAgent playerAgentPrefab;
    public AiAgent aiAgentPrefab;

    static int iCurWaveSet = 0;
    int iCurWave = -1; // must be -1

    public List<WaveSet> waveSets;

    readonly float SpawnAgentBaseDistance = 2.0f; // (3.0f was ok in the past. Got more agents now. Gotta fit them in.)

    readonly float HoldPositionPlayerBaseBackDistance = 0.25f; // hold position behind the player, by at least this much.
    readonly float HoldPositionBaseSideBySideDistance = 0.75f; // "side by side" distance when holding position

    readonly float sceneTransitionTime = 3.0f;

    List<Agent> playerTeamAgents;
    List<Agent> enemyTeamAgents;

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
    /// A method to which every <see cref="AiAgent"/> spawned by the <see cref="HordeGameLogic"/> is subscribed to.
    /// The subscribers of this method are reported the death of an Agent in the tournament round.
    /// This method also invokes <see cref="ConcludeWaveSet"/> if the victim agent is the player.
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
    /// A method to which every <see cref="AiAgent"/> spawned by the <see cref="HordeGameLogic"/> is subscribed.
    /// It provides an enemy agent to the calling <see cref="AiAgent"/>.
    /// If the calling agent has no enemies left, it retuns null.
    /// </summary>
    /// <param name="caller">The <see cref="AiAgent"/> who is searching for an enemy.</param>
    /// <returns></returns>
    Agent OnAiAgentSearchForEnemy(AiAgent caller)
    {
        Agent ret = null;

        // Assume that the caller is a friend of the player, thus needs an enemy from the enemy team.
        List<Agent> listOfEnemies = enemyTeamAgents;
        if (caller.isFriendOfPlayer == false)
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
    /// Spawns the <see cref="PlayerAgent"/> as well as his allied <see cref="AiAgent"/>s.
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

    void SpawnPlayer()
    {
        PlayerAgent player = Instantiate(playerAgentPrefab);

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

    void SpawnPlayerMercenaryFromData(MercenaryAgentData mercData, int count)
    {
        for (int i = 0; i < count; i++)
        {
            AiAgent merc = Instantiate(aiAgentPrefab);
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

    void SpawnInvadersFromData(InvaderData invaderData)
    {
        for (int i = 0; i < invaderData.invaderCount; i++)
        {
            AiAgent a = Instantiate(aiAgentPrefab);
            InitializeAgentFromHordeData(a
                , invaderData.invaderAgentDataPrefab.weaponSetPrefab
                , invaderData.invaderAgentDataPrefab.armorSetPrefab
                , invaderData.invaderAgentDataPrefab.charSetPrefab);

            a.OnSearchForEnemyAgent += OnAiAgentSearchForEnemy;
            a.IsFriendOfPlayer = false;
            a.OnDeath += OnAgentDeath;

            HordeRewardData hrd = a.gameObject.AddComponent<HordeRewardData>();
            hrd.CopyDataFromPrefab(invaderData.invaderAgentDataPrefab.invaderRewardDataPrefab);

            enemyTeamAgents.Add(a);

            AgentThinkQueue.Enqueue(a);
        }
    }

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
    /// Spawns the enemy team of <see cref="AiAgent"/>s.
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
    /// Concludes this waveset, and invokes the <see cref="ConcludeWaveSetCoroutine"/> coroutine.
    /// It also sets the necessary variables based on whatever happened in this waveset.
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

    void ConsiderNearbyEnemy(Agent agent)
    {
        // Assume that thisAgent is a friend of the player.
        List<Agent> listToChoose = enemyTeamAgents;
        if (agent.isFriendOfPlayer == false)
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

    void ToggleAiCombatDirectionPreference(Agent agent)
    {
        // Assume that thisAgent is a friend of the player.
        List<Agent> listToChoose = playerTeamAgents;
        if (agent.isFriendOfPlayer == false)
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
    /// A coroutine which is used to conclude the waveset based on <see cref="sceneTransitionTime"/>.
    /// </summary>
    /// <returns>Some kind of Unity coroutine magic thing.</returns>
    IEnumerator ConcludeWaveSetCoroutine()
    {
        yield return new WaitForSeconds(sceneTransitionTime);
        SceneManager.LoadScene("InformationMenuScene");
    }

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
    /// Unity's Start method.
    /// In this case, it mainly spawns the agents which are meant to compete in this tournament round.
    /// </summary>
    void Start()
    {
        CalculateTotalNumberOfWavesOnce();

        StartWaveSet();
    }

    void Update()
    {
        MakeOneAgentThink();
    }
}
