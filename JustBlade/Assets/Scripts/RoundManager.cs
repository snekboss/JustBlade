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

    float distanceBetweenAgents = 0.25f;

    float roundEndTime = 3.0f;

    bool isRoundEnded;

    List<Agent> livingPlayerTeamAgents;
    List<Agent> livingEnemyTeamAgents;

    int numEnemiesBeatenByPlayer;

    void SpawnPlayerTeamAgents()
    {
        livingPlayerTeamAgents = new List<Agent>();

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

            a.transform.position = spawnPos;
            livingPlayerTeamAgents.Add(a);

            spawnPos = spawnPos + nextAgentSpawnOffset;
        }

    }

    void SpawnEnemyTeamAgents()
    {
        livingEnemyTeamAgents = new List<Agent>();

        Vector3 spawnPos = enemyTeamSpawnPoint.position;
        Vector3 dir = enemyTeamSpawnDirection == SpawnDirection.Right ? Vector3.right : Vector3.left;
        Vector3 nextAgentSpawnOffset = dir * (2 * Agent.AgentRadius + distanceBetweenAgents);

        for (int i = 0; i < TournamentVariables.MaxNumAgentsInEachTeam; i++)
        {
            AiAgent a = Instantiate(aiAgentPrefab);
            a.OnSearchForEnemyAgent += OnAiAgentSearchForEnemy;
            a.isFriendOfPlayer = false;
            a.OnDeath += OnAgentDeath;

            a.transform.position = spawnPos;
            livingEnemyTeamAgents.Add(a);

            spawnPos = spawnPos + nextAgentSpawnOffset;
        }
    }

    void OnAgentDeath(Agent victim, Agent killer)
    {
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
            livingPlayerTeamAgents.Remove(victim);
        }
        else
        {
            livingEnemyTeamAgents.Remove(victim);
        }

        if (livingPlayerTeamAgents.Count == 0 || livingEnemyTeamAgents.Count == 0 || victim.IsPlayerAgent)
        {
            if (isRoundEnded == false)
            {
                EndRound();
            }
        }
    }

    Agent OnAiAgentSearchForEnemy(AiAgent caller)
    {
        Agent ret = null;
        if (caller.isFriendOfPlayer)
        {
            ret = livingEnemyTeamAgents.Find(agent => !agent.IsDead);
        }
        else
        {
            ret = livingPlayerTeamAgents.Find(agent => !agent.IsDead);
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