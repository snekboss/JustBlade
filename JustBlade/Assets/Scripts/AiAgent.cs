using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// A class which designates the attached game object as an AiAgent.
/// AiAgents are <see cref="Agent"/> object which are controlled by the state machine written in this class.
/// These agents are not controlled by the player.
/// An AiAgent also requires:
/// - <see cref="AnimationManager"/>.
/// - <see cref="EquipmentManager"/>.
/// - <see cref="LimbManager"/>.
/// </summary>
public class AiAgent : Agent
{
    /// <summary>
    /// An enum for the combat state of the AiAgent.
    /// </summary>
    public enum AiCombatState
    {
        Idling,
        Attacking,
        Defending,
    }

    const float HeadLookPercentPosY = 0.55f;
    const float TorsoLookPercentPosY = 0.75f;
    const float LegsLookPercentPosY = 0.5f;
    const float SlerpRateLookDirection = 0.2f;
    const float LerpRateYawAngle = 0.1f;

    const float AttackTimeMax = 0.5f;
    const float DefendTimeMax = 2.5f;
    const float SearchForEnemyTimeMax = 0.5f;

    const float TooCloseMultiplier = 0.75f;
    const float TooFarMultiplier = 1.0f;
    const float CloseEnoughPercent = 0.5f; // Percentage between TooClose and TooFar.
    //const float TooCloseBorderLowerBound = 0.9f;
    const float TooFarBorderLowerBound = 1.2f;
    const float AttackDistanceMultiplier = 2f;
    const float ChanceToChooseVerticalCombatDirection = 0.75f; // chance to choose up or down as combat dir.
    const float ChanceToChooseLegAsTargetLimbType = 0.1f;
    const float NavMeshAgentBaseAcceleration = 4.0f;
    const float ChanceToDefendWhenDamaged = 0.5f;
    const float ReducedChanceToDefendPerPoise = 0.1f;
    const float ChanceToTargetNearbyEnemy = 0.25f;

    #region Friendliness indicator related fields
    public GameObject friendlinessIndicator;
    public Material friendlyColorMat;
    public Material enemyColorMat;
    #endregion

    #region Distance to enemy related fields
    // These are meant to be initialized once when the weapon is equipped.
    float TooCloseBorder;
    float TooFarBorder;
    float AttackDistanceBorder;
    #endregion

    bool isAtk;
    bool isDef;

    CombatDirection combatDir;

    NavMeshAgent nma;

    Transform agentEyes;

    float yawAngle;
    float targetYawAngle;

    Agent enemyAgent;
    float distanceFromEnemy;
    Limb.LimbType targetLimbType;

    #region Fields regarding the attempt of smoothing out the stopping of the agent.
    // Unity's NavMeshAgent stops very abruptly. Below fields are to smooth out the Agent's animation.
    Vector3 lastNonZeroVelocity;
    float lastNonZeroSpeed;
    float lastNonZeroSpeedDecreaseLerpRate;
    // Unity's NavMeshAgent stops very abruptly. Above fields are to smooth out the Agent's animation. 
    #endregion

    Vector3 desiredMoveDestination;

    bool isPreferringVerticalCombatDirs;

    float attackTimer;
    float defendTimer;
    float searchForEnemyTimer;

    public bool IsOrderedToHoldPosition { get; protected set; }

    AiCombatState combatState;

    /// <summary>
    /// A delegate for when the AiAgent wants to search for an enemy.
    /// </summary>
    /// <param name="caller">The AiAgent who wants to search for an enemy.</param>
    /// <returns></returns>
    public delegate Agent AiAgentSearchForEnemyEvent(AiAgent caller);
    public event AiAgentSearchForEnemyEvent OnSearchForEnemyAgent;

    public override void InitializeAgent(Weapon weaponPrefab
        , Armor headArmorPrefab
        , Armor torsoArmorPrefab
        , Armor handArmorPrefab
        , Armor legArmorPrefab
        , CharacteristicSet characteristicPrefab)
    {
        base.InitializeAgent(weaponPrefab
            , headArmorPrefab
            , torsoArmorPrefab
            , handArmorPrefab
            , legArmorPrefab
            , characteristicPrefab);

        InitializeAiAgent();
    }

    void InitializeAiAgent()
    {
        // --- Combat distance related parameters ---
        // Must check if weapon is null in case EquipmentManager hasn't received its equipment yet.
        float weaponLength = (EqMgr.equippedWeapon == null) ? 0f : EqMgr.equippedWeapon.weaponLength;
        weaponLength *= CharMgr.AgentSizeMultiplier;

        TooFarBorder = CharMgr.AgentWorldRadius + weaponLength * TooFarMultiplier;
        TooCloseBorder = CharMgr.AgentWorldRadius + weaponLength * TooCloseMultiplier;

        if (TooFarBorder < TooFarBorderLowerBound)
        {
            TooFarBorder = TooFarBorderLowerBound;
        }

        //if (TooCloseBorder < TooCloseBorderLowerBound)
        //{
        //    TooCloseBorder = TooCloseBorderLowerBound;
        //}

        AttackDistanceBorder = CharMgr.AgentWorldRadius + weaponLength * AttackDistanceMultiplier;

        // --- NavMesh movement related parameters ---
        lastNonZeroSpeedDecreaseLerpRate = CharacteristicManager.DefaultMovementSpeedLimit / CharMgr.MovementSpeedLimit;
        lastNonZeroSpeedDecreaseLerpRate *= lastNonZeroSpeedDecreaseLerpRate;
        lastNonZeroSpeedDecreaseLerpRate = Mathf.Clamp01(lastNonZeroSpeedDecreaseLerpRate);

        InitializeNavMeshAgent();
    }

    /// <summary>
    /// Initializes the values of Unity's <see cref="NavMeshAgent"/>.
    /// It's also used to initialize the rigidbody attached to this agent.
    /// The rigidbody was attached so that the AiAgents don't walk through one another.
    /// </summary>
    void InitializeNavMeshAgent()
    {
        lastNonZeroSpeedDecreaseLerpRate = CharacteristicManager.DefaultMovementSpeedLimit / CharMgr.MovementSpeedLimit;
        lastNonZeroSpeedDecreaseLerpRate *= lastNonZeroSpeedDecreaseLerpRate;
        lastNonZeroSpeedDecreaseLerpRate = Mathf.Clamp01(lastNonZeroSpeedDecreaseLerpRate);

        nma = GetComponent<NavMeshAgent>();

        // Use default agent values here, as NavMeshAgent makes up for the change in scale automatically.
        nma.height = CharacteristicManager.DefaultAgentHeight;
        nma.radius = CharacteristicManager.DefaultAgentRadius;

        nma.speed = CharMgr.MovementSpeedLimit;
        nma.acceleration = 
            NavMeshAgentBaseAcceleration 
            * (CharMgr.MovementSpeedLimit / CharacteristicManager.DefaultMovementSpeedLimit);
        nma.stoppingDistance = 0;

        // While moving to position, don't let the AI code rotate the agent transform.
        // Just, go where you're told, and don't do any rotations...
        nma.angularSpeed = 0;
    }

    /// <summary>
    /// An override of <see cref="Agent.OnThisAgentDamaged(Agent, int)"/>.
    /// When damaged, AiAgents focus on the agent who damaged them.
    /// They also attack or defend based on the flip of a coin.
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="amount"></param>
    public override void OnThisAgentDamaged(Agent attacker, int amount)
    {
        enemyAgent = attacker;

        // Decide to defend based on chance.
        // For each poise remaining, reduce the chance to block, as we can just poise through.
        float rand = Random.Range(0f, 1f);
        float lostChanceAmount = CharMgr.CurrentPoise * ReducedChanceToDefendPerPoise;
        float totalDefendChance = ChanceToDefendWhenDamaged - lostChanceAmount;
        if (rand < totalDefendChance)
        {
            defendTimer = 0;
            combatState = AiCombatState.Defending;
            combatDir = GetBiasedRandomCombatDirection();
        }
    }

    public void ToggleHoldPosition(bool isPlayerOrderingToHoldPosition, Vector3 positionToHold)
    {
        IsOrderedToHoldPosition = isPlayerOrderingToHoldPosition;

        if (IsOrderedToHoldPosition)
        {
            nma.SetDestination(positionToHold);
        }
    }

    public override void ToggleCombatDirectionPreference(float distanceToClosestFriend)
    {
        float weaponLength = (EqMgr.equippedWeapon == null) ? 0f : EqMgr.equippedWeapon.weaponLength;
        float border = weaponLength + (CharMgr.AgentWorldRadius);

        // If distance to closest friend is less than the border,
        // then we prefer up/down attacks more (in order to avoid teamhitting).
        // If not, then we prefer all attacks equally likely, as there are no friendlies around.
        isPreferringVerticalCombatDirs = distanceToClosestFriend < border;
    }

    public override void ConsiderNearbyEnemy(Agent nearbyEnemy)
    {
        if (IsDead)
        {
            return;
        }

        if (AnimMgr.IsAttacking || AnimMgr.IsDefending)
        {
            // Don't switch targets mid combat.
            return;
        }

        if ((enemyAgent != null) && (enemyAgent == nearbyEnemy))
        {
            // Already fighting the nearby enemy.
            return;
        }

        Vector3 nearbyEnemyLocalPos = transform.InverseTransformPoint(nearbyEnemy.transform.position);
        if (nearbyEnemyLocalPos.z < 0f)
        {
            // Nearby enemy is behind me, and so I can't notice him.
            return;
        }

        // Target the nearby enemy, based on a coin flip.
        float randy = Random.Range(0f, 1f);
        if (randy < ChanceToTargetNearbyEnemy)
        {
            enemyAgent = nearbyEnemy;
        }
    }

    /// <summary>
    /// Returns the position in world coordinates based on the <see cref="targetLimbType"/>.
    /// Meaning, if the agent is currently meant to be looking at the <see cref="Limb.LimbType.Torso"/>,
    /// then this method returns the correct position in world coordinates of where he is meant to look at.
    /// </summary>
    /// <returns></returns>
    Vector3 GetLookPosition()
    {
        if (enemyAgent == null)
        {
            return agentEyes.position + agentEyes.forward * 1000; // straight ahead
        }

        if (targetLimbType == Limb.LimbType.Head)
        {
            float headStartPosY = enemyAgent.LimbMgr.limbHead.transform.position.y;
            float headEndPosY = CharMgr.AgentWorldHeight;

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

    /// <summary>
    /// Sets the <see cref="Agent.LookAngleX"/> angle value of the AiAgent.
    /// If this AiAgent has no <see cref="enemyAgent"/>, then he AiAgent simply looks forward.
    /// If he does have an enemy, then based on <see cref="GetLookPosition"/>, it sets the <see cref="Agent.LookAngleX"/> value.
    /// </summary>
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

    /// <summary>
    /// Sets the yaw angle (left/right) of this AiAgent.
    /// If this AiAgent has no enemy, then the existing yaw angle is kept.
    /// If he does have an enemy, then the yaw angle is recalculated based on the position of the enemy.
    /// </summary>
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

    /// <summary>
    /// Determines the desired move destination which is used by <see cref="NavMeshAgent.SetDestination(Vector3)"/>..
    /// The desired move destination is calculated via values like <see cref="TooCloseBorder"/> and <see cref="TooFarBorder"/>.
    /// </summary>
    void DetermineDesiredDestination()
    {
        Vector3 enemyToSelfDir = (transform.position - enemyAgent.transform.position).normalized;

        Vector3 tooFarPos = enemyAgent.transform.position + (enemyToSelfDir * TooFarBorder);
        Vector3 tooClosePos = enemyAgent.transform.position + (enemyToSelfDir * TooCloseBorder);

        desiredMoveDestination = Vector3.Lerp(tooClosePos, tooFarPos, CloseEnoughPercent);
    }

    /// <summary>
    /// Returns a combat direction which can be used to attack or defend.
    /// The method returns a biased combat direction. The meaning of the "bias" is the following:
    /// If the AiAgent has no friends left, then any combat direction can be given equally likely.
    /// If the AiAgent still has friends left, then up/down directions are more preferred.
    /// This is because we'd like to avoid swinging wildly left/right while we still have friends, lest we hit them.
    /// The bias depends on <see cref="ChanceToChooseVerticalCombatDirection"/>.
    /// </summary>
    /// <returns></returns>
    CombatDirection GetBiasedRandomCombatDirection()
    {
        CombatDirection ret;
        if (isPreferringVerticalCombatDirs == false)
        {
            // There are no allies around, so feel free to choose any direction.
            ret = (CombatDirection)Random.Range(0, 4);
        }
        else
        {
            // There are allies around, so we'll prefer stabs or overhead swings (to avoid hitting friends).

            float flip = Random.Range(0.0f, 1.0f);
            int flipInt = System.Convert.ToInt32(flip * 100.0f);

            if (flip < ChanceToChooseVerticalCombatDirection)
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

    /// <summary>
    /// Returns a target <see cref="Limb.LimbType"/>>, which this AiAgent can look at.
    /// The method returns a biased target limb type.
    /// The "bias" means that, we prefer not to choose <see cref="Limb.LimbType.Legs"/> as often.
    /// Meaning, we prefer to attack <see cref="Limb.LimbType.Head"/> or <see cref="Limb.LimbType.Torso"/>.
    /// The bias depends on <see cref="ChanceToChooseLegAsTargetLimbType"/>.
    /// </summary>
    /// <returns></returns>
    Limb.LimbType GetBiasedRandomTargetLimbType()
    {
        // Be less likely to choose legs as the target limb type.

        float flip = Random.Range(0.0f, 1.0f);
        if (flip < ChanceToChooseLegAsTargetLimbType)
        {
            return Limb.LimbType.Legs;
        }
        else
        {
            // We didn't choose legs, so choose Head or Torso based on the flip of a coin.

            int flipInt = System.Convert.ToInt32(flip * 100.0f);
            return flipInt % 2 == 0 ? Limb.LimbType.Head : Limb.LimbType.Torso;
        }
    }

    /// <summary>
    /// Returns the localMoveDir which is used by <see cref="AnimationManager.UpdateAnimations(Vector2, float, bool, bool, bool)"/>.
    /// It also returns the current movement speed as an out parameter.
    /// The reason for this method is to fight the abrupt stopping of Unity's <see cref="NavMeshAgent"/>.
    /// It tries to smooth out the foot movement of the AiAgent, despite the abrupt stop.
    /// </summary>
    /// <param name="outSpeed">An out parameter of the current speed of the AiAgent.</param>
    /// <returns>The move direction vector in local coordinates with respect to the AiAgent.</returns>
    Vector2 GetLocalMoveDir(out float outSpeed)
    {
        Vector3 chosenVelocity = Vector3.zero;

        if (CharMgr.CurrentMovementSpeed > 0)
        {
            chosenVelocity = nma.velocity;
            outSpeed = CharMgr.CurrentMovementSpeed;
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

    /// <summary>
    /// Moves to the desired destination.
    /// Currently, the desired destination is directly towards where the enemy is.
    /// If there is no enemy, then the agent simply stops.
    /// </summary>
    void ThinkMovement()
    {
        if (enemyAgent == null)
        {
            return;
        }


        if (IsOrderedToHoldPosition == false)
        {
            DetermineDesiredDestination();

            if ((distanceFromEnemy > TooFarBorder) || (distanceFromEnemy < TooCloseBorder))
            {
                nma.SetDestination(desiredMoveDestination); // TODO: Handle return value.
            }
        }
    }

    /// <summary>
    /// If the agent has no enemy, then he doesn't do anything.
    /// If the agent has an enemy, then this method governs the combat state.
    /// If the agent is close enough to his target, then he can attack (unless he's currently defending).
    /// If the agent is too far from his target, then he doesn't do anything.
    /// </summary>
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
                    if (AnimMgr.IsIdling)
                    {
                        combatDir = GetBiasedRandomCombatDirection();
                        targetLimbType = GetBiasedRandomTargetLimbType();
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

    /// <summary>
    /// It is used to set the <see cref="Agent.currentMovementSpeed"/>.
    /// It is also used to set <see cref="lastNonZeroSpeed"/> and <see cref="lastNonZeroVelocity"/>.
    /// The last two values are used to smooth out the foot movement animation
    /// to fight against the sudden stopping of Unity's <see cref="NavMeshAgent"/>.
    /// </summary>
    void SetMovementParameters()
    {
        CharMgr.CurrentMovementSpeed = nma.velocity.magnitude;

        if (CharMgr.CurrentMovementSpeed > 0)
        {
            lastNonZeroSpeed = CharMgr.CurrentMovementSpeed;
            lastNonZeroVelocity = nma.velocity;
        }
    }
    /// <summary>
    /// If this agent has an enemy, then this method doesn't do anything.
    /// If this agent has no enemy, then this method searches for a new enemy agent.
    /// The actual searching is done by calling <see cref="OnSearchForEnemyAgent"/>.
    /// Currently, the only subscriber to this event is <see cref="HordeGameLogic.OnAiAgentSearchForEnemy(AiAgent, out int)"/>.
    /// </summary>
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
                    enemyAgent = OnSearchForEnemyAgent(this);
                    nma.isStopped = (enemyAgent == null);
                    if (enemyAgent == null)
                    {
                        lastNonZeroSpeed = 0;
                        lastNonZeroVelocity = Vector3.zero;
                        isAtk = false;
                        isDef = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Initializes the friendliness indicator of this agent.
    /// If this agent is a friend of the player, then the friendly color is used.
    /// If this agent is an enemy of the player, then the enmity color is used.
    /// The colors are determined by <see cref="friendlyColorMat"/> and <see cref="enemyColorMat"/>.
    /// </summary>
    protected override void InitializeFriendlinessIndicator()
    {
        MeshRenderer mr = friendlinessIndicator.GetComponent<MeshRenderer>();
        if (IsFriendOfPlayer)
        {
            mr.material = friendlyColorMat;
        }
        else
        {
            mr.material = enemyColorMat;
        }
    }

    /// <summary>
    /// Unity's Awake method.
    /// An override of the <see cref="Agent.Awake"/> method.
    /// After calling the parent's Awake method, it initializes some fields, including the ones related to angle values.
    /// </summary>
    public override void Awake()
    {
        base.Awake();

        agentEyes = transform.Find("AgentEyes");

        yawAngle = transform.eulerAngles.y;
        targetYawAngle = yawAngle;
    }

    /// <summary>
    /// Unity's Update method.
    /// It governs the logic of this AiAgent.
    /// </summary>
    void Update()
    {
        if (StaticVariables.IsGamePaused)
        {
            return;
        }

        if (IsDead)
        {
            nma.enabled = false;
            friendlinessIndicator.SetActive(false);
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
        AudioMgr.UpdateAudioManager();
    }
}
