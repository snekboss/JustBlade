using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A script which designates the attached game object as a Round Manager.
/// It contains the logic of a tournament round.
/// </summary>
public class RoundManager : MonoBehaviour
{
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

    /// <summary>
    /// An event which is called when any agent dies in the tournament round.
    /// All agents are subscribed to this method when they are spawned by this Round Manager.
    /// </summary>
    public event Agent.AgentDeathEvent OnAnyAgentDeath;

    float distanceBetweenAgents = 3.0f; // 3.0f seems ok

    readonly float roundEndTime = 3.0f;

    bool isRoundEnded;

    List<Agent> playerTeamAgents;
    List<Agent> enemyTeamAgents;

    /// <summary>
    /// The number of enemies beaten by player in this round.
    /// At the end of every round, these are added to <see cref="TournamentVariables.TotalOpponentsBeatenByPlayer"/>.
    /// </summary>
    int numEnemiesBeatenByPlayer;

    /// <summary>
    /// Spawns the <see cref="PlayerAgent"/> as well as his allied <see cref="AiAgent"/>s.
    /// </summary>
    void SpawnPlayerTeamAgents()
    {
        playerTeamAgents = new List<Agent>();

        Vector3 spawnPos = playerTeamSpawnPoint.position;
        Vector3 dir = playerTeamSpawnDirection == SpawnDirection.Right ? Vector3.right : Vector3.left;
        Vector3 nextAgentSpawnOffset = dir * (2 * Agent.AgentRadius + distanceBetweenAgents);

        int numMaxAgents = TournamentVariables.MaxNumAgentsInEachTeam;
        int playerSpawnIndex = Random.Range(0, numMaxAgents);
        for (int i = 0; i < numMaxAgents; i++)
        {
            Agent a = null;

            if (i == playerSpawnIndex)
            {
                a = Instantiate(playerAgentPrefab);
            }
            else
            {
                AiAgent ai = Instantiate(aiAgentPrefab);
                ai.OnSearchForEnemyAgent += OnAiAgentSearchForEnemy;
                a = ai;
            }

            a.isFriendOfPlayer = true;
            a.OnDeath += OnAgentDeath;
            OnAnyAgentDeath += a.OnOtherAgentDeath;

            a.transform.position = spawnPos;
            playerTeamAgents.Add(a);

            spawnPos = spawnPos + nextAgentSpawnOffset;
        }

    }

    /// <summary>
    /// Spawns the enemy team of <see cref="AiAgent"/>s.
    /// </summary>
    void SpawnEnemyTeamAgents()
    {
        enemyTeamAgents = new List<Agent>();

        Vector3 spawnPos = enemyTeamSpawnPoint.position;
        Vector3 dir = enemyTeamSpawnDirection == SpawnDirection.Right ? Vector3.right : Vector3.left;
        Vector3 nextAgentSpawnOffset = dir * (2 * Agent.AgentRadius + distanceBetweenAgents);

        for (int i = 0; i < TournamentVariables.MaxNumAgentsInEachTeam; i++)
        {
            AiAgent a = Instantiate(aiAgentPrefab);
            a.OnSearchForEnemyAgent += OnAiAgentSearchForEnemy;
            a.isFriendOfPlayer = false;
            a.OnDeath += OnAgentDeath;
            OnAnyAgentDeath += a.OnOtherAgentDeath;

            a.transform.position = spawnPos;
            enemyTeamAgents.Add(a);

            spawnPos = spawnPos + nextAgentSpawnOffset;
        }
    }

    /// <summary>
    /// A method to which every <see cref="AiAgent"/> spawned by the <see cref="RoundManager"/> is subscribed to.
    /// The subscribers of this method are reported the death of an Agent in the tournament round.
    /// This method also invokes <see cref="EndRound"/> if the victim agent is the player.
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
            TournamentVariables.PlayerWasBestedInThisMelee = true;
        }

        if (victim.isFriendOfPlayer)
        {
            playerTeamAgents.Remove(victim);
        }
        else
        {
            enemyTeamAgents.Remove(victim);
        }

        if (playerTeamAgents.Count == 0 || enemyTeamAgents.Count == 0 || victim.IsPlayerAgent)
        {
            if (isRoundEnded == false)
            {
                EndRound();
            }
        }
    }

    /// <summary>
    /// A method to which every <see cref="AiAgent"/> spawned by the <see cref="RoundManager"/> is subscribed.
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
    /// Ends this tournament round, and invokes the <see cref="RoundEndTimer"/> coroutine.
    /// It also sets the necessary tournament variables based on whatever happened this round.
    /// </summary>
    void EndRound()
    {
        isRoundEnded = true;

        TournamentVariables.TotalOpponentsBeatenByPlayer += numEnemiesBeatenByPlayer;

        if (TournamentVariables.PlayerWasBestedInThisMelee && numEnemiesBeatenByPlayer < TournamentVariables.CurrentRoundNumber)
        {
            // The player was bested in melee, and was not able to beat enough opponents to proceed to the next round.
            TournamentVariables.IsPlayerEliminated = true;
        }

        TournamentVariables.CurrentRoundNumber++;

        StartCoroutine("RoundEndTimer");
    }

    /// <summary>
    /// A coroutine which is used to end the round based on <see cref="roundEndTime"/>.
    /// </summary>
    /// <returns>Some kind of Unity coroutine magic thing.</returns>
    IEnumerator RoundEndTimer()
    {
        yield return new WaitForSeconds(roundEndTime);
        SceneManager.LoadScene("TournamentInfoMenuScene");
    }

    /// <summary>
    /// Spawns agents in all teams by calling <see cref="SpawnPlayerTeamAgents"/> and <see cref="SpawnEnemyTeamAgents"/>.
    /// </summary>
    void SpawnAgents()
    {
        SpawnPlayerTeamAgents();
        SpawnEnemyTeamAgents();
    }

    /// <summary>
    /// Unity's Start method.
    /// In this case, it mainly spawns the agents which are meant to compete in this tournament round.
    /// </summary>
    void Start()
    {
        TournamentVariables.PlayerWasBestedInThisMelee = false;

        SpawnAgents();
    }
}
