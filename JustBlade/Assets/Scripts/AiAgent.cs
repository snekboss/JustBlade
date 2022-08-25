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

    static readonly float AttackTimeMax = 1.0f;
    static readonly float DefendTimeMax = 2.0f;

    static readonly float TooCloseMultiplier = 0.75f;
    static readonly float TooFarMultiplier = 1.0f;
    static readonly float CloseEnoughPercent = 0.5f; // Percentage between TooClose and TooFar.
    public static readonly float NavMeshAgentBaseAcceleration = 4.0f;

    float TooCloseBorder;
    float TooFarBorder;

    // Below are temporary
    public bool isGrounded;
    public bool isAtk;
    public bool isDef;
    // Above are temporary

    CombatDirection combatDir;

    NavMeshAgent nma;

    Transform agentEyes;

    float yawAngle;
    float targetYawAngle;

    Agent enemyAgent;
    float distanceFromEnemy;
    Limb.LimbType targetLimbType;

    // Unity's NavMeshAgent stops very abruptly. Below fields are to smooth out the Agent's animation.
    Vector3 lastNonZeroVelocity;
    float lastNonZeroSpeed;
    float lastNonZeroSpeedDecreaseLerpRate;
    // Unity's NavMeshAgent stops very abruptly. Above fields are to smooth out the Agent's animation.

    Vector3 desiredMoveDestination;

    float attackTimer;
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

    DistanceToTargetState distanceState;
    AiCombatState combatState;


    public override void Awake()
    {
        base.Awake();

        agentEyes = transform.Find("AgentEyes");

        yawAngle = transform.eulerAngles.y;
        targetYawAngle = yawAngle;

        TooFarBorder = EqMgr.equippedWeapon.weaponLength * TooFarMultiplier;
        TooCloseBorder = EqMgr.equippedWeapon.weaponLength * TooCloseMultiplier;
    }

    public override void InitializeMovementSpeedLimit(float movementSpeedLimit)
    {
        base.InitializeMovementSpeedLimit(movementSpeedLimit);

        lastNonZeroSpeedDecreaseLerpRate = DefaultMovementSpeedLimit / MovementSpeedLimit;
        lastNonZeroSpeedDecreaseLerpRate *= lastNonZeroSpeedDecreaseLerpRate;
        lastNonZeroSpeedDecreaseLerpRate = Mathf.Clamp01(lastNonZeroSpeedDecreaseLerpRate);

        InitializeNavMeshAgent();
    }

    public override void RequestEquipmentSet(out Weapon weaponPrefab
        , out Armor headArmorPrefab
        , out Armor torsoArmorPrefab
        , out Armor handArmorPrefab
        , out Armor legArmorPrefab)
    {
        weaponPrefab = PrefabManager.Weapons[Random.Range(0, PrefabManager.Weapons.Count)];

        headArmorPrefab = PrefabManager.HeadArmors[Random.Range(0, PrefabManager.HeadArmors.Count)];
        torsoArmorPrefab = PrefabManager.TorsoArmors[Random.Range(0, PrefabManager.TorsoArmors.Count)];
        handArmorPrefab = PrefabManager.HandArmors[Random.Range(0, PrefabManager.HandArmors.Count)];
        legArmorPrefab = PrefabManager.LegArmors[Random.Range(0, PrefabManager.LegArmors.Count)];
    }

    void InitializeNavMeshAgent()
    {
        nma = GetComponent<NavMeshAgent>();

        nma.height = AgentHeight;
        nma.radius = AgentRadius;
        nma.speed = MovementSpeedLimit;
        nma.acceleration = NavMeshAgentBaseAcceleration * (MovementSpeedLimit / DefaultMovementSpeedLimit);
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

        desiredMoveDestination = Vector3.Lerp(tooClosePos, tooFarPos, CloseEnoughPercent);
    }

    CombatDirection GetRandomCombatDirection()
    {
        int randInt = Random.Range(0, 4);
        return (CombatDirection)randInt;
    }

    Vector2 GetLocalMoveDir(out float outSpeed)
    {
        Vector3 chosenVelocity = Vector3.zero;

        if (currentMovementSpeed > 0)
        {
            chosenVelocity = nma.velocity;
            outSpeed = currentMovementSpeed;
        }
        else
        {
            chosenVelocity = lastNonZeroVelocity;
            lastNonZeroSpeed = Mathf.Lerp(lastNonZeroSpeed, 0, lastNonZeroSpeedDecreaseLerpRate);
            outSpeed = lastNonZeroSpeed;
        }

        Vector3 chosenVelocityLocal = transform.InverseTransformDirection(chosenVelocity);
        Vector2 chosenVelocityLocalXZ = new Vector2(chosenVelocityLocal.x, chosenVelocityLocal.z).normalized;

        return chosenVelocityLocalXZ;
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

                nma.SetDestination(desiredMoveDestination); // TODO: Handle return value.

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

                nma.SetDestination(desiredMoveDestination); // TODO: Handle return value.

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
                isAtk = false;

                attackTimer += Time.deltaTime;

                if (attackTimer >= AttackTimeMax)
                {
                    attackTimer = 0;
                    isAtk = true;
                    combatDir = GetRandomCombatDirection();
                }
                
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

    void SetMovementParameters()
    {
        currentMovementSpeed = nma.velocity.magnitude;

        if (currentMovementSpeed > 0)
        {
            lastNonZeroSpeed = currentMovementSpeed;
            lastNonZeroVelocity = nma.velocity;
        }
    }

    void Update()
    {
        if (IsDead)
        {
            return;
        }

        SetMovementParameters();

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

        float speed;
        Vector2 localMoveDir = GetLocalMoveDir(out speed);
        AnimMgr.UpdateAnimations(localMoveDir, speed, isGrounded, isAtk, isDef);
    }
}
