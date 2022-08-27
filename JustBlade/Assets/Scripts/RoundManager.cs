using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoundManager : MonoBehaviour
{
    public enum SpawnDirection { Left, Right }

    public Transform playerTeamSpawnPoint;
    public SpawnDirection playerTeamSpawnDirection;

    public Transform enemyTeamSpawnPoint;
    public SpawnDirection enemyTeamSpawnDirection;

    public PlayerAgent playerAgentPrefab;
    public AiAgent aiAgentPrefab;

    public event Agent.AgentDeathEvent OnAnyAgentDeath;

    float distanceBetweenAgents = 3.0f; // 3.0f seems ok

    readonly float roundEndTime = 3.0f;

    bool isRoundEnded;

    List<Agent> playerTeamAgents;
    List<Agent> enemyTeamAgents;

    int numEnemiesBeatenByPlayer;

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

    IEnumerator RoundEndTimer()
    {
        yield return new WaitForSeconds(roundEndTime);
        SceneManager.LoadScene("TournamentInfoMenuScene");
    }

    void SpawnAgents()
    {
        SpawnPlayerTeamAgents();
        SpawnEnemyTeamAgents();
    }

    void Start()
    {
        TournamentVariables.PlayerWasBestedInThisMelee = false;

        SpawnAgents();
    }
}
