using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiAgent : Agent
{
    public static readonly float HeadLookPercentPosY = 0.55f;
    public static readonly float TorsoLookPercentPosY = 0.75f;
    public static readonly float LegsLookPercentPosY = 0.5f;
    public static readonly float SlerpRateLookDirection = 0.2f;
    public static readonly float LerpRateYawAngle = 0.1f;

    // Below are temporary
    public float curMovSpeed; // TODO: Use navMeshAgent.speed or something, but this is temporary.
    public float moveX;
    public float moveY;
    public bool isGrounded;
    public bool isAtk;
    public bool isDef;
    public CombatDirection combatDir;
    // Above are temporary

    public override float CurrentMovementSpeed { get; protected set; }

    NavMeshAgent nma;

    public static readonly float NavMeshAgentAcceleration = 8.0f;
    Transform agentEyes;

    float yawAngle;
    public float targetYawAngle;

    Agent enemyAgent;
    public Limb.LimbType targetLimbType;


    public override void Awake()
    {
        base.Awake();

        agentEyes = transform.Find("AgentEyes");

        yawAngle = transform.eulerAngles.y;
        targetYawAngle = yawAngle;

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

    protected override void OnDamaged(Agent attacker, int amount)
    {
        enemyAgent = attacker;

        // TODO: Defend for a while.
    }

    Vector3 GetLookPosition()
    {
        if (enemyAgent == null)
        {
            return agentEyes.position + agentEyes.forward * 1000; // straight ahead
        }

        if (targetLimbType == Limb.LimbType.Head)
        {
            float headStartPosY = enemyAgent.LimbMgr.limbHead.transform.position.y;
            float headEndPosY = AgentHeight;

            float chosenPosY = Mathf.Lerp(headStartPosY, headEndPosY, HeadLookPercentPosY);

            Vector3 lookPos = enemyAgent.LimbMgr.limbHead.transform.position;
            lookPos.y = chosenPosY;

            return lookPos;
        }
        else if (targetLimbType == Limb.LimbType.Torso)
        {
            float torsoStartPosY = enemyAgent.LimbMgr.limbTorso.transform.position.y;
            float headStartPosY = enemyAgent.LimbMgr.limbHead.transform.position.y;

            float chosenPosY = Mathf.Lerp(torsoStartPosY, headStartPosY, TorsoLookPercentPosY);

            Vector3 lookPos = enemyAgent.LimbMgr.limbTorso.transform.position;
            lookPos.y = chosenPosY;

            return lookPos;
        }
        else /*if (targetLimbType == Limb.LimbType.Legs)*/
        {
            float legsStartPosY = enemyAgent.LimbMgr.limbLegs.transform.position.y;
            float torsoStartPosY = enemyAgent.LimbMgr.limbTorso.transform.position.y;

            float chosenPosY = Mathf.Lerp(legsStartPosY, torsoStartPosY, LegsLookPercentPosY);

            Vector3 lookPos = enemyAgent.LimbMgr.limbLegs.transform.position;
            lookPos.y = chosenPosY;

            return lookPos;
        }
    }

    void SetLookAngleX()
    {
        Quaternion targetlookRot = Quaternion.identity;

        if (enemyAgent != null)
        {
            Vector3 lookPos = GetLookPosition();
            Vector3 lookDir = lookPos - agentEyes.position;
            targetlookRot = Quaternion.LookRotation(lookDir);
        }

        agentEyes.rotation = Quaternion.Slerp(agentEyes.rotation, targetlookRot, SlerpRateLookDirection);
        LookAngleX = agentEyes.rotation.eulerAngles.x;

        //LookAngleX = Mathf.LerpAngle(LookAngleX, targetLookAngleX, LerpRatelookAngleX);
    }

    void SetYawAngle()
    {
        yawAngle = transform.eulerAngles.y;

        if (enemyAgent != null)
        {
            Vector3 curLookDir = transform.forward;

            Vector3 lookPos = enemyAgent.transform.position;

            Vector3 targetLookDir = lookPos - transform.position;
            targetLookDir.y = 0;

            float angleDiff = Vector3.SignedAngle(curLookDir, targetLookDir, Vector3.up);

            targetYawAngle = yawAngle + angleDiff;
        }

        yawAngle = Mathf.LerpAngle(yawAngle, targetYawAngle, LerpRateYawAngle);

        // First, reset rotation.
        transform.rotation = Quaternion.identity;

        // Then, rotate with the new angle.
        transform.Rotate(Vector3.up, yawAngle);
    }

    void ThinkMovement()
    {

    }

    void ThinkCombat()
    {

    }

    void Update()
    {
        if (IsDead)
        {
            return;
        }

        CurrentMovementSpeed = curMovSpeed;

        ThinkMovement();
        ThinkCombat();

        SetYawAngle();
        SetLookAngleX();

        AnimMgr.UpdateCombatDirection(combatDir);
        AnimMgr.UpdateAnimations(new Vector2(moveX, moveY), isGrounded, isAtk, isDef);
    }
}
