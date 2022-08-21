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

    [Range(0.0f, 5.0f)]
    public float TooFarPercent;

    [Range(0.0f, 1.0f)]
    public float CloseEnoughPercent;

    [Range(0.0f, 5.0f)]
    public float TooClosePercent;

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
    float distanceFromEnemy;
    public Limb.LimbType targetLimbType;

    public enum DistanceToTargetState
    {
        TooFar,
        CloseEnough,
        TooClose,
    }

    public enum AiCombatState
    {
        Idling,
        Attacking,
        Defending,
    }

    public DistanceToTargetState distanceState;
    public AiCombatState combatState;


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

    void SetDistanceToTargetState()
    {
        // Assume we're close enough.
        distanceState = DistanceToTargetState.CloseEnough;

        float tooFarDistLimit = EqMgr.equippedWeapon.weaponLength * TooFarPercent;
        if (distanceFromEnemy > tooFarDistLimit)
        {
            distanceState = DistanceToTargetState.TooFar;
            return;
        }

        float tooCloseDistLimit = EqMgr.equippedWeapon.weaponLength * TooClosePercent;
        if (distanceFromEnemy < tooCloseDistLimit)
        {
            distanceState = DistanceToTargetState.TooClose;
            return;
        }
    }

    Vector3 GetDesiredDestination()
    {
        Vector3 enemyToSelfDir = (transform.position - enemyAgent.transform.position).normalized;

        Vector3 tooFarPos = enemyAgent.transform.position + (enemyToSelfDir * TooFarPercent);
        Vector3 tooClosePos = enemyAgent.transform.position + (enemyToSelfDir * TooClosePercent);

        Vector3 desiredPos = Vector3.Lerp(tooClosePos, tooFarPos, CloseEnoughPercent);

        return desiredPos;
    }

    CombatDirection GetRandomCombatDirection()
    {
        int randInt = Random.Range(0, 4);
        return (CombatDirection)randInt;
    }


    void ThinkMovement()
    {
        if (enemyAgent == null)
        {
            return;
        }

        SetDistanceToTargetState();

        Vector3 desiredDestination = GetDesiredDestination();

        switch (distanceState)
        {
            case DistanceToTargetState.TooFar:

                if (nma.isStopped)
                {
                    nma.isStopped = false;
                }

                nma.SetDestination(desiredDestination); // TODO: Handle return value.

                combatState = AiCombatState.Idling;

                break;
            case DistanceToTargetState.CloseEnough:

                if (nma.isStopped == false)
                {
                    nma.isStopped = true;
                }

                if (combatState != AiCombatState.Defending)
                {
                    combatState = AiCombatState.Attacking;
                }

                break;
            case DistanceToTargetState.TooClose:

                if (nma.isStopped)
                {
                    nma.isStopped = false;
                }

                nma.SetDestination(desiredDestination); // TODO: Handle return value.

                if (combatState != AiCombatState.Defending)
                {
                    combatState = AiCombatState.Attacking;
                }

                break;
            default:
                break;
        }
    }

    void ThinkCombat()
    {
        if (enemyAgent == null)
        {
            return;
        }

        switch (combatState)
        {
            case AiCombatState.Idling:
                isAtk = false;
                isDef = false;
                break;
            case AiCombatState.Attacking:
                isDef = false;

                //isAtk = !isAtk; // spamming attack
                combatDir = GetRandomCombatDirection();
                break;
            case AiCombatState.Defending:
                break;
            default:
                break;
        }


    }

    void Update()
    {
        if (IsDead)
        {
            return;
        }

        CurrentMovementSpeed = curMovSpeed;

        if (enemyAgent != null)
        {
            distanceFromEnemy = Vector3.Distance(transform.position, enemyAgent.transform.position);
        }

        ThinkMovement();
        ThinkCombat();

        SetYawAngle();
        SetLookAngleX();

        AnimMgr.UpdateCombatDirection(combatDir);
        AnimMgr.UpdateAnimations(new Vector2(moveX, moveY), isGrounded, isAtk, isDef);
    }
}
