using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A script which designates the attached game object as a Wave Manager.
/// It contains the logic the horde game mode.
/// </summary>
public class WaveManager : MonoBehaviour
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
        public HordeCharacteristicSet invaderCharacteristicSetPrefab;
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

    public Transform playerTeamSpawnPoint;
    public SpawnDirection playerTeamSpawnDirection;

    public Transform enemyTeamSpawnPoint;
    public SpawnDirection enemyTeamSpawnDirection;

    public PlayerAgent playerAgentPrefab;
    public AiAgent aiAgentPrefab;

    static int iCurWaveSet = 0;
    int iCurWave = -1;

    public List<WaveSet> waveSets;

    /// <summary>
    /// An event which is called when any agent dies in the tournament round.
    /// All agents are subscribed to this method when they are spawned by this Round Manager.
    /// </summary>
    public event Agent.AgentDeathEvent OnAnyAgentDeath;

    float distanceBetweenAgents = 3.0f; // 3.0f seems ok

    readonly float sceneTransitionTime = 3.0f;

    List<Agent> playerTeamAgents;
    List<Agent> enemyTeamAgents;

    /// <summary>
    /// The number of enemies beaten by player in this round.
    /// At the end of every round, these are added to <see cref="ItemShop.TotalOpponentsBeatenByPlayer"/>.
    /// </summary>
    int numEnemiesBeatenByPlayer;

    void SpawnInvadersFromData(InvaderData invaderData, ref Vector3 spawnPos, Vector3 dir)
    {
        for (int i = 0; i < invaderData.invaderCount; i++)
        {
            AiAgent a = Instantiate(aiAgentPrefab);
            Vector3 nextAgentSpawnOffset = dir * (2 * a.AgentWorldRadius + distanceBetweenAgents);

            a.OnSearchForEnemyAgent += OnAiAgentSearchForEnemy;
            a.isFriendOfPlayer = false;
            a.OnDeath += OnAgentDeath;
            OnAnyAgentDeath += a.OnOtherAgentDeath;

            a.ArmorSetRequest += invaderData.invaderArmorSetPrefab.ProvideRequestedArmorSet;
            a.WeaponRequest += invaderData.invaderWeaponSetPrefab.ProvideRequestedWeapon;

            SetAgentCharacteristicFromData(a, invaderData.invaderCharacteristicSetPrefab);

            HordeRewardData hrd = a.gameObject.AddComponent<HordeRewardData>();
            hrd.CopyDataFromPrefab(invaderData.invaderRewardDataPrefab);

            a.transform.position = spawnPos;
            enemyTeamAgents.Add(a);

            spawnPos = spawnPos + nextAgentSpawnOffset;
        }
    }

    void SetAgentCharacteristicFromData(Agent agent, HordeCharacteristicSet charSetPrefab)
    {
        agent.Health = charSetPrefab.MaximumHealth;
        agent.AgentScale = charSetPrefab.ModelSizeMultiplier;
        agent.ExtraMovementSpeedLimitMultiplier = charSetPrefab.ExtraMovementSpeedLimitMultiplier;
        agent.ExtraDamageMultiplier = charSetPrefab.ExtraDamageMultiplier;
        agent.ExtraDamageResistanceMultiplier = charSetPrefab.ExtraDamageResistanceMultiplier;
    }

    /// <summary>
    /// A method to which every <see cref="AiAgent"/> spawned by the <see cref="WaveManager"/> is subscribed to.
    /// The subscribers of this method are reported the death of an Agent in the tournament round.
    /// This method also invokes <see cref="ConcludeWaveSet"/> if the victim agent is the player.
    /// </summary>
    /// <param name="victim">The agent who died.</param>
    /// <param name="killer">The agent who killed the victim.</param>
    void OnAgentDeath(Agent victim, Agent killer)
    {
        if (OnAnyAgentDeath != null)
        {
            OnAnyAgentDeath(victim, killer);
        }

        if (killer.IsPlayerAgent)
        {
            numEnemiesBeatenByPlayer++;
        }

        if (victim.IsPlayerAgent)
        {
            ItemShop.PlayerWasBestedInThisMelee = true;
        }

        if (victim.isFriendOfPlayer)
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
    /// A method to which every <see cref="AiAgent"/> spawned by the <see cref="WaveManager"/> is subscribed.
    /// It provides an enemy agent to the calling <see cref="AiAgent"/>.
    /// If the calling agent has no enemies left, it retuns null.
    /// It also returns the number of friends the calling <see cref="AiAgent"/> has left by an out parameter.
    /// </summary>
    /// <param name="caller">The <see cref="AiAgent"/> who is searching for an enemy.</param>
    /// <param name="numRemainingFriends">Out parameter, which denotes the number friends the calling agent has left.</param>
    /// <returns></returns>
    Agent OnAiAgentSearchForEnemy(AiAgent caller, out int numRemainingFriends)
    {
        Agent ret = null;
        if (caller.isFriendOfPlayer)
        {
            List<Agent> agents = enemyTeamAgents.FindAll(a => !a.IsDead);

            if (agents.Count >= 1)
            {
                ret = agents[Random.Range(0, agents.Count)];
            }

            numRemainingFriends = playerTeamAgents.Count - 1;
        }
        else
        {
            List<Agent> agents = playerTeamAgents.FindAll(a => !a.IsDead);

            if (agents.Count >= 1)
            {
                ret = agents[Random.Range(0, agents.Count)];
            }

            numRemainingFriends = enemyTeamAgents.Count - 1;
        }

        return ret;
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
        SpawnPlayerMercenaries(ref spawnPos, dir);
    }

    void SpawnPlayer(ref Vector3 spawnPos, Vector3 dir)
    {
        Agent player = Instantiate(playerAgentPrefab);

        player.isFriendOfPlayer = true;
        player.OnDeath += OnAgentDeath;
        OnAnyAgentDeath += player.OnOtherAgentDeath;

        player.transform.position = spawnPos;
        playerTeamAgents.Add(player);

        Vector3 nextAgentSpawnOffset = dir * (2 * player.AgentWorldRadius + distanceBetweenAgents);
        spawnPos = spawnPos + nextAgentSpawnOffset;
    }

    void SpawnPlayerMercenaries(ref Vector3 spawnPos, Vector3 dir)
    {
        SpawnPlayerMercenaryFromData(
            PrefabManager.MercenaryDataByArmorLevel[Armor.ArmorLevel.None]
            , ItemShop.GetMercenaryCount(Armor.ArmorLevel.None), ref spawnPos, dir);

        SpawnPlayerMercenaryFromData(
            PrefabManager.MercenaryDataByArmorLevel[Armor.ArmorLevel.Light]
            , ItemShop.GetMercenaryCount(Armor.ArmorLevel.Light), ref spawnPos, dir);

        SpawnPlayerMercenaryFromData(
            PrefabManager.MercenaryDataByArmorLevel[Armor.ArmorLevel.Medium]
            , ItemShop.GetMercenaryCount(Armor.ArmorLevel.Medium), ref spawnPos, dir);

        SpawnPlayerMercenaryFromData(
            PrefabManager.MercenaryDataByArmorLevel[Armor.ArmorLevel.Heavy]
            , ItemShop.GetMercenaryCount(Armor.ArmorLevel.Heavy), ref spawnPos, dir);
    }

    void SpawnPlayerMercenaryFromData(MercenaryData mercData, int count, ref Vector3 spawnPos, Vector3 dir)
    {
        for (int i = 0; i < count; i++)
        {
            AiAgent merc = Instantiate(aiAgentPrefab);
            merc.OnSearchForEnemyAgent += OnAiAgentSearchForEnemy;

            merc.isFriendOfPlayer = true;
            merc.OnDeath += OnAgentDeath;
            OnAnyAgentDeath += merc.OnOtherAgentDeath;

            merc.ArmorSetRequest += mercData.mercArmorSetPrefab.ProvideRequestedArmorSet;
            merc.WeaponRequest += mercData.mercWeaponSetPrefab.ProvideRequestedWeapon;

            SetAgentCharacteristicFromData(merc, mercData.mercCharSetPrefab);

            MercenaryDescriptionData mdd = merc.gameObject.AddComponent<MercenaryDescriptionData>();
            mdd.InitializeFromMercenaryData(mercData);

            merc.transform.position = spawnPos;
            playerTeamAgents.Add(merc);

            Vector3 nextAgentSpawnOffset = dir * (2 * merc.AgentWorldRadius + distanceBetweenAgents);

            spawnPos = spawnPos + nextAgentSpawnOffset;
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

        ItemShop.TotalOpponentsBeatenByPlayer += numEnemiesBeatenByPlayer;

        // TODO: This is not how the game works anymore. Remove this.
        if (ItemShop.PlayerWasBestedInThisMelee && numEnemiesBeatenByPlayer < ItemShop.CurrentRoundNumber)
        {
            // The player was bested in melee, and was not able to beat enough opponents to proceed to the next round.
            ItemShop.IsPlayerEliminated = true;
        }

        ItemShop.CurrentRoundNumber++;

        StartCoroutine("ConcludeWaveSetCoroutine");
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
        ItemShop.PlayerWasBestedInThisMelee = false;

        StartWaveSet();
    }
}
