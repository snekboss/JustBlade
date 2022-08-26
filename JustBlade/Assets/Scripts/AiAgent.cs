//using System;
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
    static readonly float SearchForEnemyTimeMax = 0.5f;

    static readonly float TooCloseMultiplier = 0.75f;
    static readonly float TooFarMultiplier = 1.0f;
    static readonly float CloseEnoughPercent = 0.5f; // Percentage between TooClose and TooFar.
    static readonly float AttackDistanceMultiplier = 1.5f;
    static readonly float ChanceToChooseNonSideCombatDirection = 0.75f; // chance to choose up or down as combat dir.
    public static readonly float NavMeshAgentBaseAcceleration = 4.0f;

    public GameObject friendlinessIndicator;

    float TooCloseBorder;
    float TooFarBorder;
    float AttackDistanceBorder;

    bool isAtk;
    bool isDef;

    CombatDirection combatDir;

    NavMeshAgent nma;
    Rigidbody rBody;

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

    int numRemainingFriends;
    float attackTimer;
    float defendTimer;
    float searchForEnemyTimer;

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

    public delegate Agent AiAgentSearchForEnemyEvent(AiAgent caller, out int numRemainingFriends);
    public virtual event AiAgentSearchForEnemyEvent OnSearchForEnemyAgent;

    

    public override void InitializeMovementSpeedLimit(float movementSpeedLimit)
    {
        base.InitializeMovementSpeedLimit(movementSpeedLimit);

        lastNonZeroSpeedDecreaseLerpRate = DefaultMovementSpeedLimit / MovementSpeedLimit;
        lastNonZeroSpeedDecreaseLerpRate *= lastNonZeroSpeedDecreaseLerpRate;
        lastNonZeroSpeedDecreaseLerpRate = Mathf.Clamp01(lastNonZeroSpeedDecreaseLerpRate);

        InitializeNavMeshAgent();
    }

    public override void OnGearInitialized()
    {
        TooFarBorder = EqMgr.equippedWeapon.weaponLength * TooFarMultiplier;
        TooCloseBorder = EqMgr.equippedWeapon.weaponLength * TooCloseMultiplier;
        AttackDistanceBorder = EqMgr.equippedWeapon.weaponLength * AttackDistanceMultiplier;
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

        rBody = GetComponent<Rigidbody>();
        if (rBody == null)
        {
            rBody = gameObject.AddComponent<Rigidbody>();
        }
        //rBody.isKinematic = true;
        rBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rBody.useGravity = true;
        rBody.mass = AgentMass;
    }

    protected override void OnDamaged(Agent attacker, int amount)
    {
        enemyAgent = attacker;

        defendTimer = 0;

        // Decide to defend based on the flip of a coin.
        int rand = Random.Range(0, 3);
        if (rand % 2 == 0)
        {
            combatState = AiCombatState.Defending;
            combatDir = GetBiasedRandomCombatDirection();
        }
        
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

    void DetermineDesiredDestination()
    {
        Vector3 enemyToSelfDir = (transform.position - enemyAgent.transform.position).normalized;

        Vector3 tooFarPos = enemyAgent.transform.position + (enemyToSelfDir * TooFarBorder);
        Vector3 tooClosePos = enemyAgent.transform.position + (enemyToSelfDir * TooCloseBorder);

        desiredMoveDestination = Vector3.Lerp(tooClosePos, tooFarPos, CloseEnoughPercent);
    }

    CombatDirection GetBiasedRandomCombatDirection()
    {
        CombatDirection ret;
        if (numRemainingFriends < 1)
        {
            // There are no allies around, so feel free to choose any direction.
            ret = (CombatDirection)Random.Range(0, 4);
        }
        else
        {
            // There are allies around, so we'll prefer stabs or overhead swings (to avoid hitting friends).

            float flip = Random.Range(0.0f, 1.0f);
            int flipInt = System.Convert.ToInt32(flip * 100.0f);

            if (flip < ChanceToChooseNonSideCombatDirection)
            {
                // We rolled to choose a non-side direction (ie, we rolled "up" or "down").
                // So choose one of them based on whether or not randInt is even or odd.
                ret = flipInt % 2 == 0 ? CombatDirection.Up : CombatDirection.Down;
            }
            else
            {
                // Same as above, except for "left" or "right".
                ret = flipInt % 2 == 0 ? CombatDirection.Left : CombatDirection.Right;
            }
        }

        return ret;
    }

    Limb.LimbType GetRandomLimbType()
    {
        return (Limb.LimbType)Random.Range(0, 3);
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

        DetermineDesiredDestination();

        if ((distanceFromEnemy > TooFarBorder) || (distanceFromEnemy < TooCloseBorder))
        {
            nma.SetDestination(desiredMoveDestination); // TODO: Handle return value.
        }
    }

    void ThinkCombat()
    {
        if (enemyAgent == null)
        {
            return;
        }

        if (distanceFromEnemy < AttackDistanceBorder)
        {
            if (combatState != AiCombatState.Defending)
            {
                combatState = AiCombatState.Attacking;
            }
        }
        else
        {
            if (combatState != AiCombatState.Defending)
            {
                combatState = AiCombatState.Idling;
            }
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
                    if (AnimMgr.IsAttacking == false)
                    {
                        combatDir = GetBiasedRandomCombatDirection();
                        targetLimbType = GetRandomLimbType();
                    }
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

    void HandleSearchForEnemyAgent()
    {
        if (enemyAgent != null)
        {
            if (enemyAgent.IsDead)
            {
                enemyAgent = null;
            }
            
            searchForEnemyTimer = 0;
        }

        if (enemyAgent == null)
        {
            searchForEnemyTimer += Time.deltaTime;
            if (searchForEnemyTimer > SearchForEnemyTimeMax)
            {
                searchForEnemyTimer = 0;
                if (OnSearchForEnemyAgent != null)
                {
                    enemyAgent = OnSearchForEnemyAgent(this, out numRemainingFriends);
                    nma.isStopped = (enemyAgent == null);
                }
            }
        }
    }

    public override void Awake()
    {
        base.Awake();

        agentEyes = transform.Find("AgentEyes");

        yawAngle = transform.eulerAngles.y;
        targetYawAngle = yawAngle;
    }

    void Start()
    {
        friendlinessIndicator.SetActive(isFriendOfPlayer);
    }
    void Update()
    {
        if (IsDead)
        {
            rBody.velocity = Vector3.zero;
            rBody.angularVelocity = Vector3.zero;
            return;
        }

        SetMovementParameters();

        HandleSearchForEnemyAgent();

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

        // AiAgents are always considered to be grounded, since NavMeshAgents can't jump anyway...
        AnimMgr.UpdateAnimations(localMoveDir, speed, true, isAtk, isDef);
    }
}
