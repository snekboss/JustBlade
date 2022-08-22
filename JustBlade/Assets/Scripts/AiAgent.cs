using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiAgent : Agent
{
    static readonly float HeadLookPercentPosY = 0.55f;
    static readonly float TorsoLookPercentPosY = 0.75f;
    static readonly float LegsLookPercentPosY = 0.5f;
    static readonly float SlerpRateLookDirection = 0.2f;
    static readonly float LerpRateYawAngle = 0.1f;

    static readonly float DefendTimeMax = 2.0f;

    static readonly float TooCloseMultiplier = 0.75f;
    static readonly float TooFarMultiplier = 1.0f;
    static readonly float CloseEnoughPercent = 0.5f;

    float TooCloseBorder;
    float TooFarBorder;

    // Below are temporary
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

    public Vector3 lastNonZeroVelocity;
    public float lastNonZeroSpeed;

    public float lastNonZeroSpeedDecreaseRate = 0.1f;

    [Range(0.01f, 1.0f)]
    public float StopLerpRate;

    public bool downThere;

    Vector3 desiredDestination;

    float defendTimer;

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

        TooFarBorder = EqMgr.equippedWeapon.weaponLength * TooFarMultiplier;
        TooCloseBorder = EqMgr.equippedWeapon.weaponLength * TooCloseMultiplier;

        InitializeNavMeshAgent();
    }

    void InitializeNavMeshAgent()
    {
        nma = GetComponent<NavMeshAgent>();

        nma.height = AgentHeight;
        nma.radius = AgentRadius;
        nma.speed = MovementSpeedLimit;
        nma.acceleration = NavMeshAgentAcceleration;
        nma.stoppingDistance = 0;

        // While moving to position, don't let the AI code rotate the agent transform.
        // Just, go where you're told, and don't do any rotations...
        nma.angularSpeed = 0; 
    }

    protected override void OnDamaged(Agent attacker, int amount)
    {
        enemyAgent = attacker;

        defendTimer = 0;
        combatState = AiCombatState.Defending;
        combatDir = GetRandomCombatDirection();
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
            //Vector3 lookPos = DEBUG_TRANSFORM.position;

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

        if (distanceFromEnemy > TooFarBorder)
        {
            distanceState = DistanceToTargetState.TooFar;
            return;
        }

        if (distanceFromEnemy < TooCloseBorder)
        {
            distanceState = DistanceToTargetState.TooClose;
            return;
        }
    }

    void DetermineDesiredDestination()
    {
        Vector3 enemyToSelfDir = (transform.position - enemyAgent.transform.position).normalized;

        Vector3 tooFarPos = enemyAgent.transform.position + (enemyToSelfDir * TooFarBorder);
        Vector3 tooClosePos = enemyAgent.transform.position + (enemyToSelfDir * TooCloseBorder);

        desiredDestination = Vector3.Lerp(tooClosePos, tooFarPos, CloseEnoughPercent);
    }

    CombatDirection GetRandomCombatDirection()
    {
        int randInt = Random.Range(0, 4);
        return (CombatDirection)randInt;
    }

    Vector2 GetLocalMoveDir(out float outSpeed)
    {
        if (CurrentMovementSpeed > 0)
        {
            Vector3 curVelocity = nma.velocity;

            Vector3 curVelocityLocal = transform.InverseTransformDirection(curVelocity);

            Vector2 curVelocityLocalXZ = new Vector2(curVelocityLocal.x, curVelocityLocal.z).normalized;

            float speedRatio = CurrentMovementSpeed / MovementSpeedLimit;

            downThere = false;
            outSpeed = CurrentMovementSpeed;
            return curVelocityLocalXZ * speedRatio;

        }
        else
        {
            lastNonZeroVelocity = Vector3.Slerp(lastNonZeroVelocity, Vector3.zero, StopLerpRate);
            lastNonZeroSpeed = lastNonZeroVelocity.magnitude;

            Vector3 velocityLocal = transform.InverseTransformDirection(lastNonZeroVelocity);
            Vector2 velocityLocalXZ = new Vector2(velocityLocal.x, velocityLocal.z);

            downThere = true;
            outSpeed = lastNonZeroSpeed;
            return velocityLocalXZ;
        }
    }

    void ThinkMovement()
    {
        if (enemyAgent == null)
        {
            return;
        }

        SetDistanceToTargetState();

        DetermineDesiredDestination();

        switch (distanceState)
        {
            case DistanceToTargetState.TooFar:

                nma.SetDestination(desiredDestination); // TODO: Handle return value.

                if (combatState != AiCombatState.Defending)
                {
                    combatState = AiCombatState.Idling;
                }

                break;
            case DistanceToTargetState.CloseEnough:

                if (combatState != AiCombatState.Defending)
                {
                    combatState = AiCombatState.Attacking;
                }

                break;
            case DistanceToTargetState.TooClose:

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
                isAtk = false;
                isDef = true;

                defendTimer += Time.deltaTime;
                if (defendTimer >= DefendTimeMax)
                {
                    isDef = false;
                    combatState = AiCombatState.Attacking;
                }
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

        CurrentMovementSpeed = nma.velocity.magnitude;

        if (CurrentMovementSpeed > 0)
        {
            lastNonZeroSpeed = CurrentMovementSpeed;
            lastNonZeroVelocity = nma.velocity;
        }

        if (enemyAgent != null)
        {
            distanceFromEnemy = Vector3.Distance(transform.position, enemyAgent.transform.position);
        }

        ThinkMovement();
        ThinkCombat();

        SetYawAngle();
        SetLookAngleX();

        // Update animations.
        AnimMgr.UpdateCombatDirection(combatDir);

        float outSpeed;
        Vector2 localMoveDir = GetLocalMoveDir(out outSpeed);
        AnimMgr.UpdateAnimations(localMoveDir, outSpeed, isGrounded, isAtk, isDef);
    }
}
