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
    }

    [System.Serializable]
    public class Wave
    {
        public List<InvaderData> invaderDataList;
    }

    [System.Serializable]
    public class InvaderData
    {
        public CharacteristicSet invaderCharacteristicSetPrefab;
        public HordeArmorSet invaderArmorSetPrefab;
        public HordeWeaponSet invaderWeaponSetPrefab;
        public HordeRewardData invaderRewardDataPrefab;
        public int invaderCount;
    }

    /// <summary>
    /// Determines direction in which the agents are spawned.
    /// <seealso cref="playerTeamSpawnPoint"/>.
    /// <seealso cref="enemyTeamSpawnPoint"/>.
    /// </summary>
    public enum SpawnDirection { Left, Right }

    // TODO: Below, remove unnecessary variables and rename necessary variables.
    public static int CurrentRoundNumber = 1;
    public static int MaximumRoundNumber = 6;

    public static int MaxNumAgentsInEachTeamMultiplier = 2;
    public static bool IsPlayerEliminated = false;
    public static bool IsTournamentEnded { get { return IsPlayerEliminated || CurrentRoundNumber > MaximumRoundNumber; } }
    public static bool IsFinalRound { get { return CurrentRoundNumber == MaximumRoundNumber; } }

    public static int TotalOpponentsBeatenByPlayer;
    public static bool PlayerWasBestedInThisMelee;
    public static int MaxNumAgentsInEachTeam
    {
        get
        {
            if (CurrentRoundNumber == MaximumRoundNumber)
            {
                return 1;
            }

            return (MaximumRoundNumber - CurrentRoundNumber) * MaxNumAgentsInEachTeamMultiplier;
        }
    }

    public static void StartNewTournament()
    {
        IsPlayerEliminated = false;
        PlayerWasBestedInThisMelee = false;
        TotalOpponentsBeatenByPlayer = 0;
        CurrentRoundNumber = 1;
    }
    // TODO: Above, remove unnecessary variables and rename necessary variables.

    // Item shop stuff below

    public Transform playerTeamSpawnPoint;
    public SpawnDirection playerTeamSpawnDirection;

    public Transform enemyTeamSpawnPoint;
    public SpawnDirection enemyTeamSpawnDirection;

    public PlayerAgent playerAgentPrefab;
    public AiAgent aiAgentPrefab;

    static int iCurWaveSet = 0;
    int iCurWave = -1;

    public List<WaveSet> waveSets;

    float distanceBetweenAgents = 3.0f; // 3.0f seems ok

    readonly float sceneTransitionTime = 3.0f;

    List<Agent> playerTeamAgents;
    List<Agent> enemyTeamAgents;

    Queue<Agent> AgentCombatPrefQueue
    {
        get
        {
            if (agentCombatPrefQueue == null)
            {
                agentCombatPrefQueue = new Queue<Agent>();
            }

            return agentCombatPrefQueue;
        }
    }
    Queue<Agent> agentCombatPrefQueue;

    /// <summary>
    /// TODO: Remove this?
    /// The number of enemies beaten by player in this round.
    /// At the end of every round, these are added to <see cref="PlayerInventoryManager.TotalOpponentsBeatenByPlayer"/>.
    /// </summary>
    int numEnemiesBeatenByPlayer;

    void SpawnInvadersFromData(InvaderData invaderData, ref Vector3 spawnPos, Vector3 dir)
    {
        for (int i = 0; i < invaderData.invaderCount; i++)
        {
            AiAgent a = Instantiate(aiAgentPrefab);
            InitializeAgentFromHordeData(a
                , invaderData.invaderWeaponSetPrefab
                , invaderData.invaderArmorSetPrefab
                , invaderData.invaderCharacteristicSetPrefab);

            Vector3 nextAgentSpawnOffset = dir * (2 * a.CharMgr.AgentWorldRadius + distanceBetweenAgents);

            a.OnSearchForEnemyAgent += OnAiAgentSearchForEnemy;
            a.IsFriendOfPlayer = false;
            a.OnDeath += OnAgentDeath;

            HordeRewardData hrd = a.gameObject.AddComponent<HordeRewardData>();
            hrd.CopyDataFromPrefab(invaderData.invaderRewardDataPrefab);

            a.transform.position = spawnPos;
            enemyTeamAgents.Add(a);

            spawnPos = spawnPos + nextAgentSpawnOffset;

            AgentCombatPrefQueue.Enqueue(a);
        }
    }

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
        if (killer.IsPlayerAgent)
        {
            numEnemiesBeatenByPlayer++;
        }

        if (victim.IsPlayerAgent)
        {
            HordeGameLogic.PlayerWasBestedInThisMelee = true;
        }

        if (victim.IsFriendOfPlayer)
        {
            playerTeamAgents.Remove(victim);
        }
        else
        {
            enemyTeamAgents.Remove(victim);
        }

        if (playerTeamAgents.Count == 0 || victim.IsPlayerAgent)
        {
            ConcludeWaveSet();
        }

        if (enemyTeamAgents.Count == 0)
        {
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
        float playerRadiusMulti = (1f + playerAgent.CharMgr.AgentWorldRadius);
        Vector3 playerBack = (-playerAgent.transform.forward) * playerRadiusMulti;
        Vector3 spawnPos = playerPos + playerBack;

        List<Vector3> posList = new List<Vector3>();

        for (int i = 0; i < friendlyAiAgents.Count; i++)
        {
            Vector3 offset = (playerAgent.transform.right) * (1f + (friendlyAiAgents[i].CharMgr.AgentWorldRadius * 2f));

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

        SpawnPlayer(ref spawnPos, dir);
        SpawnPlayerFriendlyAgents(ref spawnPos, dir);
    }

    void SpawnPlayer(ref Vector3 spawnPos, Vector3 dir)
    {
        PlayerAgent player = Instantiate(playerAgentPrefab);

        // Player gets default Characteristics.
        player.InitializeAgent(
            PrefabManager.Weapons[PlayerInventoryManager.PlayerChosenWeaponIndex]
          , PrefabManager.HeadArmors[PlayerInventoryManager.PlayerChosenHeadArmorIndex]
          , PrefabManager.TorsoArmors[PlayerInventoryManager.PlayerChosenTorsoArmorIndex]
          , PrefabManager.HandArmors[PlayerInventoryManager.PlayerChosenHandArmorIndex]
          , PrefabManager.LegArmors[PlayerInventoryManager.PlayerChosenLegArmorIndex]);

        player.IsFriendOfPlayer = true;
        player.OnDeath += OnAgentDeath;

        player.PlayerOrderToggle += OnPlayerOrderToggleEvent;

        player.transform.position = spawnPos;
        playerTeamAgents.Add(player);

        Vector3 nextAgentSpawnOffset = dir * (2 * player.CharMgr.AgentWorldRadius + distanceBetweenAgents);
        spawnPos = spawnPos + nextAgentSpawnOffset;
    }

    void SpawnPlayerFriendlyAgents(ref Vector3 spawnPos, Vector3 dir)
    {
        SpawnPlayerMercenaryFromData(
            PrefabManager.MercenaryDataByArmorLevel[Armor.ArmorLevel.None]
            , PlayerPartyManager.GetMercenaryCount(Armor.ArmorLevel.None), ref spawnPos, dir);

        SpawnPlayerMercenaryFromData(
            PrefabManager.MercenaryDataByArmorLevel[Armor.ArmorLevel.Light]
            , PlayerPartyManager.GetMercenaryCount(Armor.ArmorLevel.Light), ref spawnPos, dir);

        SpawnPlayerMercenaryFromData(
            PrefabManager.MercenaryDataByArmorLevel[Armor.ArmorLevel.Medium]
            , PlayerPartyManager.GetMercenaryCount(Armor.ArmorLevel.Medium), ref spawnPos, dir);

        SpawnPlayerMercenaryFromData(
            PrefabManager.MercenaryDataByArmorLevel[Armor.ArmorLevel.Heavy]
            , PlayerPartyManager.GetMercenaryCount(Armor.ArmorLevel.Heavy), ref spawnPos, dir);
    }

    void SpawnPlayerMercenaryFromData(MercenaryData mercData, int count, ref Vector3 spawnPos, Vector3 dir)
    {
        for (int i = 0; i < count; i++)
        {
            AiAgent merc = Instantiate(aiAgentPrefab);
            merc.OnSearchForEnemyAgent += OnAiAgentSearchForEnemy;
            InitializeAgentFromHordeData(merc
                , mercData.mercWeaponSetPrefab
                , mercData.mercArmorSetPrefab
                , mercData.mercCharSetPrefab);

            merc.IsFriendOfPlayer = true;
            merc.OnDeath += OnAgentDeath;

            MercenaryDescriptionData mdd = merc.gameObject.AddComponent<MercenaryDescriptionData>();
            mdd.InitializeFromMercenaryData(mercData);

            merc.transform.position = spawnPos;
            playerTeamAgents.Add(merc);

            Vector3 nextAgentSpawnOffset = dir * (2 * merc.CharMgr.AgentWorldRadius + distanceBetweenAgents);

            spawnPos = spawnPos + nextAgentSpawnOffset;

            AgentCombatPrefQueue.Enqueue(merc);
        }

    }

    /// <summary>
    /// Spawns the enemy team of <see cref="AiAgent"/>s.
    /// </summary>
    void SpawnNextWave()
    {
        iCurWave++;

        enemyTeamAgents = new List<Agent>();
        if (waveSets == null || waveSets.Count == 0)
        {
            ConcludeWaveSet();
            return;
        }

        WaveSet waveSet = waveSets[iCurWaveSet];

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
            SpawnInvadersFromData(invaderData, ref spawnPos, dir);
        }
    }

    /// <summary>
    /// Concludes this waveset, and invokes the <see cref="ConcludeWaveSetCoroutine"/> coroutine.
    /// It also sets the necessary variables based on whatever happened in this waveset.
    /// </summary>
    void ConcludeWaveSet()
    {
        iCurWaveSet++;

        HordeGameLogic.TotalOpponentsBeatenByPlayer += numEnemiesBeatenByPlayer;

        // TODO: This is not how the game works anymore. Remove this.
        if (HordeGameLogic.PlayerWasBestedInThisMelee && numEnemiesBeatenByPlayer < HordeGameLogic.CurrentRoundNumber)
        {
            // The player was bested in melee, and was not able to beat enough opponents to proceed to the next round.
            HordeGameLogic.IsPlayerEliminated = true;
        }

        HordeGameLogic.CurrentRoundNumber++;

        StartCoroutine("ConcludeWaveSetCoroutine");
    }

    void ToggleAiCombatDirectionPreference()
    {
        if (agentCombatPrefQueue.Count < 1)
        {
            return;
        }

        Agent thisAgent = AgentCombatPrefQueue.Dequeue();
        if (thisAgent == null || thisAgent.IsDead || thisAgent.IsPlayerAgent)
        {
            return;
        }
        else
        {
            AgentCombatPrefQueue.Enqueue(thisAgent);
        }

        // Assume that thisAgent is a friend of the player.
        List<Agent> listToChoose = playerTeamAgents;
        if (thisAgent.isFriendOfPlayer == false)
        {
            // Turns out, thisAgent is an enemy of the player.
            listToChoose = enemyTeamAgents;
        }

        List<Agent> agentFriends = listToChoose.FindAll(otherAgent =>
        (otherAgent != null) && (otherAgent.IsDead == false) && (otherAgent != thisAgent));

        if (agentFriends.Count < 1)
        {
            thisAgent.ToggleCombatDirectionPreference(float.MaxValue);
            return;
        }

        // Calculate squared distances.
        List<float> squaredDists = new List<float>();
        for (int i = 0; i < agentFriends.Count; i++)
        {
            Vector3 diff = thisAgent.transform.position - agentFriends[i].transform.position;
            squaredDists.Add(diff.sqrMagnitude);
        }

        float minSquaredDist = squaredDists.Min();

        thisAgent.ToggleCombatDirectionPreference(Mathf.Sqrt(minSquaredDist));
    }

    /// <summary>
    /// A coroutine which is used to conclude the waveset based on <see cref="sceneTransitionTime"/>.
    /// </summary>
    /// <returns>Some kind of Unity coroutine magic thing.</returns>
    IEnumerator ConcludeWaveSetCoroutine()
    {
        yield return new WaitForSeconds(sceneTransitionTime);
        SceneManager.LoadScene("TournamentInfoMenuScene");
    }

    /// <summary>
    /// Unity's Start method.
    /// In this case, it mainly spawns the agents which are meant to compete in this tournament round.
    /// </summary>
    void Start()
    {
        HordeGameLogic.PlayerWasBestedInThisMelee = false;

        StartWaveSet();
    }

    void Update()
    {
        ToggleAiCombatDirectionPreference();
    }
}