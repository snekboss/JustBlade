using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiAgent : Agent
{
    // These are temporary
    public float curMovSpeed; // TODO: Use navMeshAgent.speed or something, but this is temporary.
    public float moveX;
    public float moveY;
    public bool isGrounded;
    public bool isAtk;
    public bool isDef;
    public CombatDirection combatDir;

    public override float CurrentMovementSpeed { get; protected set; }

    NavMeshAgent nma;

    public static readonly float NavMeshAgentAcceleration = 8.0f;

    public override void Awake()
    {
        base.Awake();

        InitializeNavMeshAgent();
    }

    void InitializeNavMeshAgent()
    {
        nma = GetComponent<NavMeshAgent>();

        nma.height = AgentHeight;
        nma.radius = AgentRadius;
        nma.speed = MovementSpeedLimit;
        nma.acceleration = NavMeshAgentAcceleration;
    }

    void Update()
    {
        if (IsDead)
        {
            return;
        }

        CurrentMovementSpeed = curMovSpeed;

        AnimMgr.UpdateCombatDirection(combatDir);
        AnimMgr.UpdateAnimations(new Vector2(moveX, moveY), isGrounded, isAtk, isDef);
    }
}
