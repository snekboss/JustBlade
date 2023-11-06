using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class which must be attached to the game objects which are also <see cref="Agent"/>s.
/// It governs the animations of the attached <see cref="Agent"/>.
/// </summary>
public class AnimationManager : MonoBehaviour
{
    /// <summary>
    /// No, this is not the same as <see cref="Agent.CombatDirection"/>.
    /// The fact that there are 4 "getting hurt" directions doesn't mean they should be combined.
    /// There are 4 animations because making all the specific animations I would want to have would be very time consuming.
    /// </summary>
    public enum GettingHurtDirection
    {
        Up = 0,
        Right,
        Down,
        Left,
    }

    const float SlowAgentSpeedRatioExponentForMoveXY = 1.85f;

    #region Animator Controller related fields
    Animator animat;
    Animator Animat
    {
        get
        {
            if (animat == null)
            {
                animat = GetComponent<Animator>();
            }

            return animat;
        }
    }
    RuntimeAnimatorController initialRuntimeAC;
    RuntimeAnimatorController InitialRuntimeAC
    {
        get
        {
            if (initialRuntimeAC == null)
            {
                initialRuntimeAC = Animat.runtimeAnimatorController;
            }

            return initialRuntimeAC;
        }
    }


    public AnimatorOverrideController poleAOC;
    #endregion

    #region Spine related fields
    Agent ownerAgent;
    public Transform spineBone;
    public Transform pelvisBone;
    Vector3 initialPelvisToSpineOffset;
    Quaternion initialPelvisRotation;
    Quaternion initialPelvisRotationInverse;
    Vector3 initialAgentScale;

    float targetSpineAngle;

    const float TargetSpineAngleMaxForOverheadSwings = 30f;

    float spineCurAngle;
    const float SpineRotationLerpRateWhileNotAttacking = 0.05f;
    const float SpineRotationLerpRateWhileAttacking = 0.2f;
    float chosenSpineRotationLerpRate; // choose a lerp rate from above.
    bool spineShouldBeRotated;
    #endregion

    #region Animator state and transition info related fields
    AnimatorStateInfo attackAndBlockLayerStateInfo;
    AnimatorTransitionInfo attackAndBlockLayerTransitionInfo;
    #endregion

    #region Layer and layer weight related fields
    // Layer IDs (WARNING: Their index order is in sync with how they're laid out in the Animator Controller).
    const int LayerIdBase = 0;
    const int LayerIdAttackAndBlock = 1;
    const int LayerIdIdle = 2;

    float idleTimer;
    const float IdleTimerMax = 0.1f; // wait for a while before shifting layer weights
    float attackAndBlockLayerWeight;
    float idleLayerWeight;

    float IdlingLerpRate_AttackAndBlockLayer = 0.1f; // 0.1f seems to be working well so far. Less is smoother, but slower.
    float NotIdlingLerpRate_AttackAndBlockLayer = 0.4f; // It's better if this is higher than NotIdlingLerpRate_IdleLayer.
    float IdlingLerpRate_IdleLayer = 1.0f; // Must lerp instantly, apparently...
    float NotIdlingLerpRate_IdleLayer = 0.35f; // Don't make this less than 0.35f, and don't make it higher than NotIdlingLerpRate_AttackAndBlockLayer. 
    #endregion

    #region Animator states and transitions
    // Animator states and transitions
    #region Animator states
    // AttackAndBlockLayer State tags
    // Idle
    bool isState_Idle;

    // Attack
    // atk_hold
    bool isState_AtkHoldUp;
    bool isState_AtkHoldRight;
    bool isState_AtkHoldDown;
    bool isState_AtkHoldLeft;

    // atk_release
    bool isState_AtkReleaseUp;
    bool isState_AtkReleaseRight;
    bool isState_AtkReleaseDown;
    bool isState_AtkReleaseLeft;

    // atk_bounce
    bool isState_AtkBounceUp;
    bool isState_AtkBounceRight;
    bool isState_AtkBounceDown;
    bool isState_AtkBounceLeft;

    // Defend
    // def_hold
    bool isState_DefHoldUp;
    bool isState_DefHoldRight;
    bool isState_DefHoldDown;
    bool isState_DefHoldLeft;

    // def_blocked
    bool isState_DefBlockedUp;
    bool isState_DefBlockedRight;
    bool isState_DefBlockedDown;
    bool isState_DefBlockedLeft;
    #endregion

    #region Animator transitions
    // AttackAndBlockLayer Transition custom names
    bool isTrans_AnyState;

    // Attack
    // idle_to_atk_hold
    bool isTrans_IdleToAtkUpHold;
    bool isTrans_IdleToAtkRightHold;
    bool isTrans_IdleToAtkDownHold;
    bool isTrans_IdleToAtkLeftHold;


    // atk_hold_to_release
    bool isTrans_AtkUpHoldToRelease;
    bool isTrans_AtkRightHoldToRelease;
    bool isTrans_AtkDownHoldToRelease;
    bool isTrans_AtkLeftHoldToRelease;

    // atk_release_to_bounce
    bool isTrans_AtkUpReleaseToBounce;
    bool isTrans_AtkRightReleaseToBounce;
    bool isTrans_AtkDownReleaseToBounce;
    bool isTrans_AtkLeftReleaseToBounce;

    // atk_release_to_idle
    bool isTrans_AtkUpReleaseToIdle;
    bool isTrans_AtkRightReleaseToIdle;
    bool isTrans_AtkDownReleaseToIdle;
    bool isTrans_AtkLeftReleaseToIdle;

    // atk_bounce_to_idle
    bool isTrans_AtkUpBounceToIdle;
    bool isTrans_AtkRightBounceToIdle;
    bool isTrans_AtkDownBounceToIdle;
    bool isTrans_AtkLeftBounceToIdle;

    // atk_up_hold_to_def_hold
    bool isTrans_AtkUpHoldToDefUpHold;
    bool isTrans_AtkUpHoldToDefRightHold;
    bool isTrans_AtkUpHoldToDefDownHold;
    bool isTrans_AtkUpHoldToDefLeftHold;

    // atk_right_hold_to_def_hold
    bool isTrans_AtkRightHoldToDefUpHold;
    bool isTrans_AtkRightHoldToDefRightHold;
    bool isTrans_AtkRightHoldToDefDownHold;
    bool isTrans_AtkRightHoldToDefLeftHold;

    // atk_down_hold_to_def_hold
    bool isTrans_AtkDownHoldToDefUpHold;
    bool isTrans_AtkDownHoldToDefRightHold;
    bool isTrans_AtkDownHoldToDefDownHold;
    bool isTrans_AtkDownHoldToDefLeftHold;

    // atk_left_hold_to_def_hold
    bool isTrans_AtkLeftHoldToDefUpHold;
    bool isTrans_AtkLeftHoldToDefRightHold;
    bool isTrans_AtkLeftHoldToDefDownHold;
    bool isTrans_AtkLeftHoldToDefLeftHold;

    // atk_up_release_to_def_hold
    bool isTrans_AtkUpReleaseToDefUpHold;
    bool isTrans_AtkUpReleaseToDefRightHold;
    bool isTrans_AtkUpReleaseToDefDownHold;
    bool isTrans_AtkUpReleaseToDefLeftHold;
    // atk_right_release_to_def_hold
    bool isTrans_AtkRightReleaseToDefUpHold;
    bool isTrans_AtkRightReleaseToDefRightHold;
    bool isTrans_AtkRightReleaseToDefDownHold;
    bool isTrans_AtkRightReleaseToDefLeftHold;
    // atk_down_release_to_def_hold
    bool isTrans_AtkDownReleaseToDefUpHold;
    bool isTrans_AtkDownReleaseToDefRightHold;
    bool isTrans_AtkDownReleaseToDefDownHold;
    bool isTrans_AtkDownReleaseToDefLeftHold;
    // atk_left_release_to_def_hold
    bool isTrans_AtkLeftReleaseToDefUpHold;
    bool isTrans_AtkLeftReleaseToDefRightHold;
    bool isTrans_AtkLeftReleaseToDefDownHold;
    bool isTrans_AtkLeftReleaseToDefLeftHold;


    // Defend
    // idle_to_def_hold
    bool isTrans_IdleToDefUpHold;
    bool isTrans_IdleToDefRightHold;
    bool isTrans_IdleToDefDownHold;
    bool isTrans_IdleToDefLeftHold;

    // def_hold_to_blocked
    bool isTrans_DefUpHoldToBlocked;
    bool isTrans_DefRightHoldToBlocked;
    bool isTrans_DefDownHoldToBlocked;
    bool isTrans_DefLeftHoldToBlocked;

    // def_blocked_to_hold
    bool isTrans_DefUpBlockedToHold;
    bool isTrans_DefRightBlockedToHold;
    bool isTrans_DefDownBlockedToHold;
    bool isTrans_DefLeftBlockedToHold;

    // def_up_hold_to_atk_hold
    bool isTrans_DefUpHoldToAtkUpHold;
    bool isTrans_DefUpHoldToAtkRightHold;
    bool isTrans_DefUpHoldToAtkDownHold;
    bool isTrans_DefUpHoldToAtkLeftHold;

    // def_right_hold_to_atk_hold
    bool isTrans_DefRightHoldToAtkUpHold;
    bool isTrans_DefRightHoldToAtkRightHold;
    bool isTrans_DefRightHoldToAtkDownHold;
    bool isTrans_DefRightHoldToAtkLeftHold;

    // def_down_hold_to_atk_hold
    bool isTrans_DefDownHoldToAtkUpHold;
    bool isTrans_DefDownHoldToAtkRightHold;
    bool isTrans_DefDownHoldToAtkDownHold;
    bool isTrans_DefDownHoldToAtkLeftHold;

    // def_left_hold_to_atk_hold
    bool isTrans_DefLeftHoldToAtkUpHold;
    bool isTrans_DefLeftHoldToAtkRightHold;
    bool isTrans_DefLeftHoldToAtkDownHold;
    bool isTrans_DefLeftHoldToAtkLeftHold;

    // def_up_hold_to_other_def_hold
    bool isTrans_DefUpHoldToDefRightHold;
    bool isTrans_DefUpHoldToDefDownHold;
    bool isTrans_DefUpHoldToDefLeftHold;
    // def_right_hold_to_other_def_hold
    bool isTrans_DefRightHoldToDefUpHold;
    bool isTrans_DefRightHoldToDefDownHold;
    bool isTrans_DefRightHoldToDefLeftHold;
    // def_down_hold_to_other_def_hold
    bool isTrans_DefDownHoldToDefUpHold;
    bool isTrans_DefDownHoldToDefRightHold;
    bool isTrans_DefDownHoldToDefLeftHold;
    // def_left_hold_to_other_def_hold
    bool isTrans_DefLeftHoldToDefUpHold;
    bool isTrans_DefLeftHoldToDefRightHold;
    bool isTrans_DefLeftHoldToDefDownHold;

    // def_up_blocked_to_other_def_hold
    bool isTrans_DefUpBlockedToDefRightHold;
    bool isTrans_DefUpBlockedToDefDownHold;
    bool isTrans_DefUpBlockedToDefLeftHold;
    // def_right_blocked_to_other_def_hold
    bool isTrans_DefRightBlockedToDefUpHold;
    bool isTrans_DefRightBlockedToDefDownHold;
    bool isTrans_DefRightBlockedToDefLeftHold;
    // def_down_blocked_to_other_def_hold
    bool isTrans_DefDownBlockedToDefUpHold;
    bool isTrans_DefDownBlockedToDefRightHold;
    bool isTrans_DefDownBlockedToDefLeftHold;
    // def_left_blocked_to_other_def_hold
    bool isTrans_DefLeftBlockedToDefUpHold;
    bool isTrans_DefLeftBlockedToDefRightHold;
    bool isTrans_DefLeftBlockedToDefDownHold;

    // def_hold_to_idle
    bool isTrans_DefUpHoldToIdle;
    bool isTrans_DefRightHoldToIdle;
    bool isTrans_DefDownHoldToIdle;
    bool isTrans_DefLeftHoldToIdle;
    // def_blocked_to_idle
    bool isTrans_DefUpBlockedToIdle;
    bool isTrans_DefRightBlockedToIdle;
    bool isTrans_DefDownBlockedToIdle;
    bool isTrans_DefLeftBlockedToIdle;
    #endregion
    #endregion

    #region Animator trigger parameters
    // Trigger parameters which must be set to false every frame, after "Set" methods.
    bool trigger_isAtkBounced;
    bool trigger_isDefBlocked;
    bool trigger_jump;
    bool trigger_isHurt;
    #endregion

    #region Combat related fields
    public bool IsAttackingFromUp { get; private set; }
    public bool IsAttackingFromRight { get; private set; }
    public bool IsAttackingFromDown { get; private set; }
    public bool IsAttackingFromLeft { get; private set; }
    public bool IsAttacking { get { return IsAttackingFromUp || IsAttackingFromRight || IsAttackingFromDown || IsAttackingFromLeft; } }

    public bool IsDefendingFromUp { get; private set; }
    public bool IsDefendingFromRight { get; private set; }
    public bool IsDefendingFromDown { get; private set; }
    public bool IsDefendingFromLeft { get; private set; }
    public bool IsDefending { get { return IsDefendingFromUp || IsDefendingFromRight || IsDefendingFromDown || IsDefendingFromLeft; } }
    public bool IsIdling { get; private set; }
    #endregion

    /// <summary>
    /// Unity's Awake method.
    /// It is used to initialize some of the fields of this script.
    /// </summary>
    void Awake()
    {
        ownerAgent = GetComponent<Agent>();

        initialPelvisToSpineOffset = spineBone.position - pelvisBone.position;
        initialPelvisRotation = pelvisBone.rotation;
        initialPelvisRotationInverse = Quaternion.Inverse(initialPelvisRotation);
        initialAgentScale = ownerAgent.transform.lossyScale;

        if (ownerAgent.IsPlayerAgent)
        {
            Animat.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }
    }

    /// <summary>
    /// Reports <see cref="Weapon.WeaponType"/> of the equipped weapon of this agent.
    /// It is mainly used by <see cref="EquipmentManager"/>.
    /// </summary>
    /// <param name="equippedWeaponType"></param>
    public void ReportEquippedWeaponType(Weapon.WeaponType equippedWeaponType)
    {
        if (equippedWeaponType == Weapon.WeaponType.TwoHanded)
        {
            Animat.runtimeAnimatorController = InitialRuntimeAC;
        }
        else if (equippedWeaponType == Weapon.WeaponType.Polearm)
        {
            Animat.runtimeAnimatorController = poleAOC;
        }
    }

    /// <summary>
    /// Updates the <see cref="Agent.CombatDirection"/> of the agent.
    /// Meaning, when ever the agent changes the chosen combat direction, they should update the animations accordingly via this method.
    /// </summary>
    /// <param name="combatDir">The chosen combat direction.</param>
    public void UpdateCombatDirection(Agent.CombatDirection combatDir)
    {
        Animat.SetInteger(AHD.Hash_combatDir, (int)combatDir);
    }

    /// <summary>
    /// Plays the jumping animation.
    /// </summary>
    /// <param name="isJumping">True if the agent is jumping; false otherwise</param>
    public void SetJump(bool isJumping)
    {
        trigger_jump = isJumping;
    }

    /// <summary>
    /// Plays the "attack is bounced" animation.
    /// </summary>
    /// <param name="isAtkBounced">True if the agent's attack is bounced; false otherwise.</param>
    public void SetIsAttackBounced(bool isAtkBounced)
    {
        trigger_isAtkBounced = isAtkBounced;
    }

    /// <summary>
    /// Plays the "defend is blocked" animation.
    /// </summary>
    /// <param name="isDefBlocked">True if the agent's defend was blocked; false otherwise.</param>
    public void SetIsDefBlocked(bool isDefBlocked)
    {
        trigger_isDefBlocked = isDefBlocked;
    }

    /// <summary>
    /// Plays the corresponding "getting hurt" animation.
    /// </summary>
    /// <param name="gettingHurtDirection">The getting hurt animation to play.</param>
    public void SetGettingHurt(GettingHurtDirection gettingHurtDirection)
    {
        trigger_isHurt = true;
        Animat.SetInteger(AHD.Hash_isHurtDir, (int)gettingHurtDirection);
    }

    /// <summary>
    /// Plays the death animation.
    /// Also disables weapon collisions.
    /// </summary>
    public void PlayDeathAnimation()
    {
        Animat.SetBool(AHD.Hash_isDead, true);

        attackAndBlockLayerWeight = 0;
        idleLayerWeight = 0;

        Animat.SetLayerWeight(LayerIdAttackAndBlock, attackAndBlockLayerWeight);
        Animat.SetLayerWeight(LayerIdIdle, idleLayerWeight);

        if (ownerAgent.EqMgr.equippedWeapon != null)
        {
            ownerAgent.EqMgr.equippedWeapon.SetCollisionAbility(false);
        }
    }

    /// <summary>
    /// Works out the out parameters moveX and moveY based on the local move direction and the current movement speed of the agent.
    /// Then, it sets the movement animation speed and updates the animator.
    /// The movement animation of faster agents play out faster.
    /// The movement animation of slower agents play at default speed, but they "walk" rather than "run".
    /// </summary>
    /// <param name="localMoveDir">Move direction local to the agent.</param>
    /// <param name="curMoveSpeed">Current movement speed of the agent.</param>
    /// <param name="moveX">Out parameter moveX, which is to be updated for the animator.</param>
    /// <param name="moveY">Out parameter moveY, which is to be updated for the animator.</param>
    void HandleMovementAnimationParameters(Vector2 localMoveDir, float curMoveSpeed, out float moveX, out float moveY)
    {
        float speedRatio = curMoveSpeed / CharacteristicManager.DefaultMovementSpeedLimit;

        // Initialize moveX and moveY based on localMoveDir.
        // The scale depends on speedRatio.
        // However, the absolute value of both moveX and moveY must never be above 1.0f.

        // Not normalizing the localMoveDir vector anymore, because otherwise it causes slowdown in moveX moveY values
        // during diagonal movement for agents whose movement speed multiplier is 1.0f.
        Vector2 moveXY = localMoveDir;
        float moveXYmulti = speedRatio;

        moveX = Mathf.Clamp(moveXY.x * moveXYmulti, -1.0f, 1.0f);
        moveY = Mathf.Clamp(moveXY.y * moveXYmulti, -1.0f, 1.0f);

        // This multiplier is allowed to be greater than 1.0f, but it can never be less than 1.0f.
        float moveAnimSpeedMulti = 1.0f;

        // We compare the agent's curMoveSpeed to the Agent.DefaultMovementSpeedLimit. 
        if (curMoveSpeed >= CharacteristicManager.DefaultMovementSpeedLimit)
        {
            // Here, we know that curMovSpeed is faster than the default.
            // The only kind of agent who can do this is a fast agent.

            // So let the movement animation play faster.
            moveAnimSpeedMulti = speedRatio;
        }
        else
        {
            // Here, we know that curMovSpeed is slower than the default.
            // However, we don't know if this is because the agent is a slow agent, or is just speeding up.

            if (ownerAgent.CharMgr.IsOverEncumbered)
            {
                // Here, we know that agent is a slow guy, and can never reach the default speed.

                // So, we reduce moveX and moveY values accordingly.
                speedRatio = Mathf.Pow(speedRatio, SlowAgentSpeedRatioExponentForMoveXY);
                moveX = Mathf.Clamp(moveX, -speedRatio, speedRatio);
                moveY = Mathf.Clamp(moveY, -speedRatio, speedRatio);
            }
        }

        // Finally, set the movement animation moveAnimSpeedMulti.
        Animat.SetFloat(AHD.Hash_moveAnimSpeedMulti, moveAnimSpeedMulti);
    }

    /// <summary>
    /// Reads which state the animator is currently in.
    /// </summary>
    void ReadStateInfo()
    {
        attackAndBlockLayerStateInfo = Animat.GetCurrentAnimatorStateInfo(LayerIdAttackAndBlock);

        isState_Idle = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_idle;

        isState_AtkHoldUp = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_atk_up_hold;
        isState_AtkHoldRight = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_atk_right_hold;
        isState_AtkHoldDown = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_atk_down_hold;
        isState_AtkHoldLeft = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_atk_left_hold;

        isState_AtkReleaseUp = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_atk_up_release;
        isState_AtkReleaseRight = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_atk_right_release;
        isState_AtkReleaseDown = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_atk_down_release;
        isState_AtkReleaseLeft = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_atk_left_release;

        isState_AtkBounceUp = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_atk_up_bounce;
        isState_AtkBounceRight = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_atk_right_bounce;
        isState_AtkBounceDown = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_atk_down_bounce;
        isState_AtkBounceLeft = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_atk_left_bounce;

        isState_DefHoldUp = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_def_up_hold;
        isState_DefHoldRight = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_def_right_hold;
        isState_DefHoldDown = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_def_down_hold;
        isState_DefHoldLeft = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_def_left_hold;

        isState_DefBlockedUp = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_def_up_blocked;
        isState_DefBlockedRight = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_def_right_blocked;
        isState_DefBlockedDown = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_def_down_blocked;
        isState_DefBlockedLeft = attackAndBlockLayerStateInfo.tagHash == AHD.Hash_StateTag_def_left_blocked;
    }

    /// <summary>
    /// Reads which transition the animator is currently in.
    /// </summary>
    void ReadTransitionInfo()
    {
        attackAndBlockLayerTransitionInfo = Animat.GetAnimatorTransitionInfo(LayerIdAttackAndBlock);

        isTrans_AnyState = attackAndBlockLayerTransitionInfo.anyState;

        // Attack
        // idle_to_atk_hold
        isTrans_IdleToAtkUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_idle_to_atk_up_hold;
        isTrans_IdleToAtkRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_idle_to_atk_right_hold;
        isTrans_IdleToAtkDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_idle_to_atk_down_hold;
        isTrans_IdleToAtkLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_idle_to_atk_left_hold;

        // atk_hold_to_release
        isTrans_AtkUpHoldToRelease = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_up_hold_to_release;
        isTrans_AtkRightHoldToRelease = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_right_hold_to_release;
        isTrans_AtkDownHoldToRelease = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_down_hold_to_release;
        isTrans_AtkLeftHoldToRelease = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_left_hold_to_release;

        // atk_release_to_bounce
        isTrans_AtkUpReleaseToBounce = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_up_release_to_bounce;
        isTrans_AtkRightReleaseToBounce = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_right_release_to_bounce;
        isTrans_AtkDownReleaseToBounce = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_down_release_to_bounce;
        isTrans_AtkLeftReleaseToBounce = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_left_release_to_bounce;

        // atk_release_to_idle
        isTrans_AtkUpReleaseToIdle = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_up_release_to_idle;
        isTrans_AtkRightReleaseToIdle = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_right_release_to_idle;
        isTrans_AtkDownReleaseToIdle = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_down_release_to_idle;
        isTrans_AtkLeftReleaseToIdle = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_left_release_to_idle;

        // atk_bounce_to_idle
        isTrans_AtkUpBounceToIdle = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_up_bounce_to_idle;
        isTrans_AtkRightBounceToIdle = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_right_bounce_to_idle;
        isTrans_AtkDownBounceToIdle = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_down_bounce_to_idle;
        isTrans_AtkLeftBounceToIdle = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_left_bounce_to_idle;

        // Ok... I wasn't expecting this nonsense.
        // When leaving a state via a transition, Mecanim still considers you to be in the source state.
        // Hence, we have to account for each transition below...
        // atk_up_hold_to_def_hold
        isTrans_AtkUpHoldToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_up_hold_to_def_up_hold;
        isTrans_AtkUpHoldToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_up_hold_to_def_right_hold;
        isTrans_AtkUpHoldToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_up_hold_to_def_down_hold;
        isTrans_AtkUpHoldToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_up_hold_to_def_left_hold;

        // atk_right_hold_to_def_hold
        isTrans_AtkRightHoldToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_right_hold_to_def_up_hold;
        isTrans_AtkRightHoldToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_right_hold_to_def_right_hold;
        isTrans_AtkRightHoldToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_right_hold_to_def_down_hold;
        isTrans_AtkRightHoldToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_right_hold_to_def_left_hold;

        // atk_down_hold_to_def_hold
        isTrans_AtkDownHoldToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_down_hold_to_def_up_hold;
        isTrans_AtkDownHoldToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_down_hold_to_def_right_hold;
        isTrans_AtkDownHoldToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_down_hold_to_def_down_hold;
        isTrans_AtkDownHoldToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_down_hold_to_def_left_hold;

        // atk_left_hold_to_def_hold
        isTrans_AtkLeftHoldToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_left_hold_to_def_up_hold;
        isTrans_AtkLeftHoldToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_left_hold_to_def_right_hold;
        isTrans_AtkLeftHoldToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_left_hold_to_def_down_hold;
        isTrans_AtkLeftHoldToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_left_hold_to_def_left_hold;

        // atk_up_release_to_def_hold
        isTrans_AtkUpReleaseToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_up_release_to_def_up_hold;
        isTrans_AtkUpReleaseToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_up_release_to_def_right_hold;
        isTrans_AtkUpReleaseToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_up_release_to_def_down_hold;
        isTrans_AtkUpReleaseToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_up_release_to_def_left_hold;
        // atk_right_release_to_def_hold
        isTrans_AtkRightReleaseToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_right_release_to_def_up_hold;
        isTrans_AtkRightReleaseToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_right_release_to_def_right_hold;
        isTrans_AtkRightReleaseToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_right_release_to_def_down_hold;
        isTrans_AtkRightReleaseToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_right_release_to_def_left_hold;
        // atk_down_release_to_def_hold
        isTrans_AtkDownReleaseToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_down_release_to_def_up_hold;
        isTrans_AtkDownReleaseToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_down_release_to_def_right_hold;
        isTrans_AtkDownReleaseToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_down_release_to_def_down_hold;
        isTrans_AtkDownReleaseToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_down_release_to_def_left_hold;
        // atk_left_release_to_def_hold
        isTrans_AtkLeftReleaseToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_left_release_to_def_up_hold;
        isTrans_AtkLeftReleaseToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_left_release_to_def_right_hold;
        isTrans_AtkLeftReleaseToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_left_release_to_def_down_hold;
        isTrans_AtkLeftReleaseToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_atk_left_release_to_def_left_hold;


        // Defend
        // idle_to_def_hold
        isTrans_IdleToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_idle_to_def_up_hold;
        isTrans_IdleToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_idle_to_def_right_hold;
        isTrans_IdleToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_idle_to_def_down_hold;
        isTrans_IdleToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_idle_to_def_left_hold;

        // def_hold_to_blocked
        isTrans_DefUpHoldToBlocked = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_up_hold_to_blocked;
        isTrans_DefRightHoldToBlocked = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_right_hold_to_blocked;
        isTrans_DefDownHoldToBlocked = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_down_hold_to_blocked;
        isTrans_DefLeftHoldToBlocked = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_left_hold_to_blocked;

        // def_blocked_to_hold
        isTrans_DefUpBlockedToHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_up_blocked_to_hold;
        isTrans_DefRightBlockedToHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_right_blocked_to_hold;
        isTrans_DefDownBlockedToHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_down_blocked_to_hold;
        isTrans_DefLeftBlockedToHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_left_blocked_to_hold;

        // def_up_hold_to_atk_hold
        isTrans_DefUpHoldToAtkUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_up_hold_to_atk_up_hold;
        isTrans_DefUpHoldToAtkRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_up_hold_to_atk_right_hold;
        isTrans_DefUpHoldToAtkDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_up_hold_to_atk_down_hold;
        isTrans_DefUpHoldToAtkLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_up_hold_to_atk_left_hold;

        // def_right_hold_to_atk_hold
        isTrans_DefRightHoldToAtkUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_right_hold_to_atk_up_hold;
        isTrans_DefRightHoldToAtkRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_right_hold_to_atk_right_hold;
        isTrans_DefRightHoldToAtkDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_right_hold_to_atk_down_hold;
        isTrans_DefRightHoldToAtkLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_right_hold_to_atk_left_hold;

        // def_down_hold_to_atk_hold
        isTrans_DefDownHoldToAtkUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_down_hold_to_atk_up_hold;
        isTrans_DefDownHoldToAtkRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_down_hold_to_atk_right_hold;
        isTrans_DefDownHoldToAtkDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_down_hold_to_atk_down_hold;
        isTrans_DefDownHoldToAtkLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_down_hold_to_atk_left_hold;

        // def_left_hold_to_atk_hold
        isTrans_DefLeftHoldToAtkUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_left_hold_to_atk_up_hold;
        isTrans_DefLeftHoldToAtkRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_left_hold_to_atk_right_hold;
        isTrans_DefLeftHoldToAtkDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_left_hold_to_atk_down_hold;
        isTrans_DefLeftHoldToAtkLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_left_hold_to_atk_left_hold;

        // def_up_hold_to_other_def_hold
        isTrans_DefUpHoldToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_up_hold_to_def_right_hold;
        isTrans_DefUpHoldToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_up_hold_to_def_down_hold;
        isTrans_DefUpHoldToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_up_hold_to_def_left_hold;
        // def_right_hold_to_other_def_hold
        isTrans_DefRightHoldToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_right_hold_to_def_up_hold;
        isTrans_DefRightHoldToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_right_hold_to_def_down_hold;
        isTrans_DefRightHoldToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_right_hold_to_def_left_hold;
        // def_down_hold_to_other_def_hold
        isTrans_DefDownHoldToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_down_hold_to_def_up_hold;
        isTrans_DefDownHoldToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_down_hold_to_def_right_hold;
        isTrans_DefDownHoldToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_down_hold_to_def_left_hold;
        // def_left_hold_to_other_def_hold
        isTrans_DefLeftHoldToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_left_hold_to_def_up_hold;
        isTrans_DefLeftHoldToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_left_hold_to_def_right_hold;
        isTrans_DefLeftHoldToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_left_hold_to_def_down_hold;

        // def_up_blocked_to_other_def_hold
        isTrans_DefUpBlockedToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_up_blocked_to_def_right_hold;
        isTrans_DefUpBlockedToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_up_blocked_to_def_down_hold;
        isTrans_DefUpBlockedToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_up_blocked_to_def_left_hold;
        // def_right_blocked_to_other_def_hold
        isTrans_DefRightBlockedToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_right_blocked_to_def_up_hold;
        isTrans_DefRightBlockedToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_right_blocked_to_def_down_hold;
        isTrans_DefRightBlockedToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_right_blocked_to_def_left_hold;
        // def_down_blocked_to_other_def_hold
        isTrans_DefDownBlockedToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_down_blocked_to_def_up_hold;
        isTrans_DefDownBlockedToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_down_blocked_to_def_right_hold;
        isTrans_DefDownBlockedToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_down_blocked_to_def_left_hold;
        // def_left_blocked_to_other_def_hold
        isTrans_DefLeftBlockedToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_left_blocked_to_def_up_hold;
        isTrans_DefLeftBlockedToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_left_blocked_to_def_right_hold;
        isTrans_DefLeftBlockedToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_left_blocked_to_def_down_hold;

        // def_hold_to_idle
        isTrans_DefUpHoldToIdle = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_up_hold_to_idle;
        isTrans_DefRightHoldToIdle = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_right_hold_to_idle;
        isTrans_DefDownHoldToIdle = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_down_hold_to_idle;
        isTrans_DefLeftHoldToIdle = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_left_hold_to_idle;
        // def_blocked_to_idle
        isTrans_DefUpBlockedToIdle = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_up_blocked_to_idle;
        isTrans_DefRightBlockedToIdle = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_right_blocked_to_idle;
        isTrans_DefDownBlockedToIdle = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_down_blocked_to_idle;
        isTrans_DefLeftBlockedToIdle = attackAndBlockLayerTransitionInfo.userNameHash == AHD.Hash_TransName_def_left_blocked_to_idle;
    }

    /// <summary>
    /// Determines whether the agent is currently attacking or defending in a particular combat direction.
    /// </summary>
    void SetCombatParameters()
    {
        // Setting values based on state and transition info
        // Attack
        bool isTransAtkUpReleaseToDefHold = 
            isTrans_AtkUpReleaseToDefUpHold
            || isTrans_AtkUpReleaseToDefRightHold
            || isTrans_AtkUpReleaseToDefDownHold
            || isTrans_AtkUpReleaseToDefLeftHold;

        bool isTransAtkRightReleaseToDefHold =
            isTrans_AtkRightReleaseToDefUpHold
            || isTrans_AtkRightReleaseToDefRightHold
            || isTrans_AtkRightReleaseToDefDownHold
            || isTrans_AtkRightReleaseToDefLeftHold;

        bool isTransAtkDownReleaseToDefHold =
            isTrans_AtkDownReleaseToDefUpHold
            || isTrans_AtkDownReleaseToDefRightHold
            || isTrans_AtkDownReleaseToDefDownHold
            || isTrans_AtkDownReleaseToDefLeftHold;

        bool isTransAtkLeftReleaseToDefHold =
            isTrans_AtkLeftReleaseToDefUpHold
            || isTrans_AtkLeftReleaseToDefRightHold
            || isTrans_AtkLeftReleaseToDefDownHold
            || isTrans_AtkLeftReleaseToDefLeftHold;

        bool isTransOutOfAtkUpRelease =  
            isTrans_AtkUpReleaseToIdle 
            || isTrans_AtkUpReleaseToBounce
            || isTransAtkUpReleaseToDefHold
            || isTrans_AnyState;
        bool isTransOutOfAtkRightRelease =  
            isTrans_AtkRightReleaseToIdle 
            || isTrans_AtkRightReleaseToBounce
            || isTransAtkRightReleaseToDefHold
            || isTrans_AnyState;
        bool isTransOutOfAtkDownRelease = 
            isTrans_AtkDownReleaseToIdle 
            || isTrans_AtkDownReleaseToBounce 
            || isTransAtkDownReleaseToDefHold
            || isTrans_AnyState;
        bool isTransOutOfAtkLeftRelease = 
            isTrans_AtkLeftReleaseToIdle 
            || isTrans_AtkLeftReleaseToBounce 
            || isTransAtkLeftReleaseToDefHold
            || isTrans_AnyState;

        IsAttackingFromUp = isState_AtkReleaseUp && (isTransOutOfAtkUpRelease == false);
        IsAttackingFromRight = isState_AtkReleaseRight && (isTransOutOfAtkRightRelease == false);
        IsAttackingFromDown = isState_AtkReleaseDown && (isTransOutOfAtkDownRelease == false);
        IsAttackingFromLeft = isState_AtkReleaseLeft && (isTransOutOfAtkLeftRelease == false);

        // Defend

        // First, determine the "transition out" transitions.
        // Meaning, we're check if we're transitioning out of a "def_hold" or "def_blocked" state.
        
        // Up
        // def_up_hold transition outs
        bool isTransDefUpHoldToAtkHold =
            isTrans_DefUpHoldToAtkUpHold
            || isTrans_DefUpHoldToAtkRightHold
            || isTrans_DefUpHoldToAtkDownHold
            || isTrans_DefUpHoldToAtkLeftHold;

        bool isTransDefUpHoldToOtherDefHold = 
            isTrans_DefUpHoldToDefRightHold 
            || isTrans_DefUpHoldToDefDownHold 
            || isTrans_DefUpHoldToDefLeftHold;

        bool isTransOutOfDefendUp = 
            isTransDefUpHoldToAtkHold 
            || isTransDefUpHoldToOtherDefHold 
            || isTrans_DefUpHoldToIdle;

        // def_up_blocked transition outs
        bool isTransOutOfDefBlockedUp = 
            isTrans_DefUpBlockedToDefRightHold 
            || isTrans_DefUpBlockedToDefDownHold 
            || isTrans_DefUpBlockedToDefLeftHold
            || isTrans_DefUpBlockedToIdle;

        // Right
        // def_right_hold transition outs
        bool isTransDefRightHoldToAtkHold =
            isTrans_DefRightHoldToAtkUpHold
            || isTrans_DefRightHoldToAtkRightHold
            || isTrans_DefRightHoldToAtkDownHold
            || isTrans_DefRightHoldToAtkLeftHold;

        bool isTransDefRightHoldToOtherDefHold =
            isTrans_DefRightHoldToDefUpHold
            || isTrans_DefRightHoldToDefDownHold
            || isTrans_DefRightHoldToDefLeftHold;

        bool isTransOutOfDefendRight =
            isTransDefRightHoldToAtkHold
            || isTransDefRightHoldToOtherDefHold
            || isTrans_DefRightHoldToIdle;

        // def_right_blocked transition outs
        bool isTransOutOfDefBlockedRight = 
            isTrans_DefRightBlockedToDefUpHold
            || isTrans_DefRightBlockedToDefDownHold
            || isTrans_DefRightBlockedToDefLeftHold
            || isTrans_DefRightBlockedToIdle;

        // Down
        // def_down_hold transition outs
        bool isTransDefDownHoldToAtkHold = 
            isTrans_DefDownHoldToAtkUpHold
            || isTrans_DefDownHoldToAtkRightHold
            || isTrans_DefDownHoldToAtkDownHold
            || isTrans_DefDownHoldToAtkLeftHold;

        bool isTransDefDownHoldToOtherDefHold =
            isTrans_DefDownHoldToDefUpHold
            || isTrans_DefDownHoldToDefRightHold
            || isTrans_DefDownHoldToDefLeftHold;

        bool isTransOutOfDefendDown =
            isTransDefDownHoldToAtkHold
            || isTransDefDownHoldToOtherDefHold
            || isTrans_DefDownHoldToIdle;

        // def_down_blocked transition outs
        bool isTransOutOfDefBlockedDown =
            isTrans_DefDownBlockedToDefUpHold
            || isTrans_DefDownBlockedToDefRightHold
            || isTrans_DefDownBlockedToDefLeftHold
            || isTrans_DefDownBlockedToIdle;

        // Left
        // def_left_hold transition outs
        bool isTransDefLeftHoldToAtkHold = 
            isTrans_DefLeftHoldToAtkUpHold
            || isTrans_DefLeftHoldToAtkRightHold
            || isTrans_DefLeftHoldToAtkDownHold
            || isTrans_DefLeftHoldToAtkLeftHold;

        bool isTransDefLeftHoldToOtherDefHold =
            isTrans_DefLeftHoldToDefUpHold
            || isTrans_DefLeftHoldToDefRightHold
            || isTrans_DefLeftHoldToDefDownHold;

        bool isTransOutOfDefendLeft = 
            isTransDefLeftHoldToAtkHold 
            || isTransDefLeftHoldToOtherDefHold 
            || isTrans_DefLeftHoldToIdle;

        // def_left_blocked transition outs
        bool isTransOutOfDefBlockedLeft = 
            isTrans_DefLeftBlockedToDefUpHold
            || isTrans_DefLeftBlockedToDefRightHold
            || isTrans_DefLeftBlockedToDefDownHold
            || isTrans_DefLeftBlockedToIdle;


        // Finally, combine all information.
        // If we're in a state which is considered "defending", then we set defending to true.
        // Else, if we're transitioning out of what's considered "defending", then set defending to false.
        IsDefendingFromUp = (isTransOutOfDefendUp == false && isTransOutOfDefBlockedUp == false && isTrans_AnyState == false) 
            && (isState_DefHoldUp
            || isState_DefBlockedUp
            || isTrans_DefUpHoldToBlocked
            || isTrans_DefUpBlockedToHold);
        IsDefendingFromRight = (isTransOutOfDefendRight == false && isTransOutOfDefBlockedRight == false && isTrans_AnyState == false)
            && (isState_DefHoldRight 
            || isState_DefBlockedRight 
            || isTrans_DefRightHoldToBlocked 
            || isTrans_DefRightBlockedToHold);
        IsDefendingFromDown = (isTransOutOfDefendDown == false && isTransOutOfDefBlockedDown == false && isTrans_AnyState == false)
            && (isState_DefHoldDown 
            || isState_DefBlockedDown 
            || isTrans_DefDownHoldToBlocked 
            || isTrans_DefDownBlockedToHold);
        IsDefendingFromLeft = (isTransOutOfDefendLeft == false && isTransOutOfDefBlockedLeft == false && isTrans_AnyState == false)
            && (isState_DefHoldLeft 
            || isState_DefBlockedLeft 
            || isTrans_DefLeftHoldToBlocked 
            || isTrans_DefLeftBlockedToHold);
    }

    /// <summary>
    /// Activates the collision hitbox of the equipped weapon based on whether the agent is currently attacking.
    /// </summary>
    void DecideIfWeaponHitboxShouldBeActive()
    {
        if (ownerAgent.EqMgr.equippedWeapon != null)
        {
            ownerAgent.EqMgr.equippedWeapon.SetCollisionAbility(IsAttacking);
        }
    }

    /// <summary>
    /// Decides if the spine bone should be rotated.
    /// The spine bone is rotated mainly while attacking, and never while blocking.
    /// It also sets the rotation limit for overhead swings, (since the rotation of overhead swings should be limited).
    /// The overhead swing rotation limit is determined via <see cref="TargetSpineAngleMaxForOverheadSwings"/>.
    /// </summary>
    void DecideIfSpineShouldBeRotated()
    {
        bool upStates = isState_AtkHoldUp || isState_AtkReleaseUp || isState_AtkBounceUp;
        bool rightStates = isState_AtkHoldRight || isState_AtkReleaseRight || isState_AtkBounceRight;
        bool downStates = isState_AtkHoldDown || isState_AtkReleaseDown || isState_AtkBounceDown;
        bool leftStates = isState_AtkHoldLeft || isState_AtkReleaseLeft || isState_AtkBounceLeft;
        bool allStates = upStates || rightStates || downStates || leftStates;

        bool upTransitions = isTrans_IdleToAtkUpHold || isTrans_AtkUpHoldToRelease
            || isTrans_AtkUpReleaseToIdle || isTrans_AtkUpReleaseToBounce || isTrans_AtkUpBounceToIdle;
        bool rightTransitions = isTrans_IdleToAtkRightHold || isTrans_AtkRightHoldToRelease
            || isTrans_AtkRightReleaseToIdle || isTrans_AtkRightReleaseToBounce || isTrans_AtkRightBounceToIdle;
        bool downTransitions = isTrans_IdleToAtkDownHold || isTrans_AtkDownHoldToRelease
            || isTrans_AtkDownReleaseToIdle || isTrans_AtkDownReleaseToBounce || isTrans_AtkDownBounceToIdle;
        bool leftTransitions = isTrans_IdleToAtkLeftHold || isTrans_AtkLeftHoldToRelease
            || isTrans_AtkLeftReleaseToIdle || isTrans_AtkLeftReleaseToBounce || isTrans_AtkLeftBounceToIdle;
        bool allTransitions = upTransitions || rightTransitions || downTransitions || leftTransitions;

        bool atkHoldToDefHoldTransitions =
            isTrans_AtkUpHoldToDefUpHold
        || isTrans_AtkUpHoldToDefRightHold
        || isTrans_AtkUpHoldToDefDownHold
        || isTrans_AtkUpHoldToDefLeftHold
        || isTrans_AtkRightHoldToDefUpHold
        || isTrans_AtkRightHoldToDefRightHold
        || isTrans_AtkRightHoldToDefDownHold
        || isTrans_AtkRightHoldToDefLeftHold
        || isTrans_AtkDownHoldToDefUpHold
        || isTrans_AtkDownHoldToDefRightHold
        || isTrans_AtkDownHoldToDefDownHold
        || isTrans_AtkDownHoldToDefLeftHold
        || isTrans_AtkLeftHoldToDefUpHold
        || isTrans_AtkLeftHoldToDefRightHold
        || isTrans_AtkLeftHoldToDefDownHold
        || isTrans_AtkLeftHoldToDefLeftHold;

        bool defHoldToAtkUpHoldTransitions =
            isTrans_DefUpHoldToAtkUpHold
        || isTrans_DefRightHoldToAtkUpHold
        || isTrans_DefDownHoldToAtkUpHold
        || isTrans_DefLeftHoldToAtkUpHold;

        bool defHoldToAtkRightHoldTransitions =
            isTrans_DefUpHoldToAtkRightHold
        || isTrans_DefRightHoldToAtkRightHold
        || isTrans_DefDownHoldToAtkRightHold
        || isTrans_DefLeftHoldToAtkRightHold;

        bool defHoldToAtkDownHoldTransitions =
            isTrans_DefUpHoldToAtkDownHold
        || isTrans_DefRightHoldToAtkDownHold
        || isTrans_DefDownHoldToAtkDownHold
        || isTrans_DefLeftHoldToAtkDownHold;

        bool defHoldToAtkLeftHoldTransitions =
            isTrans_DefUpHoldToAtkLeftHold
        || isTrans_DefRightHoldToAtkLeftHold
        || isTrans_DefDownHoldToAtkLeftHold
        || isTrans_DefLeftHoldToAtkLeftHold;

        bool defHoldToAtkHoldTransitions =
            defHoldToAtkUpHoldTransitions
        || defHoldToAtkRightHoldTransitions
        || defHoldToAtkDownHoldTransitions
        || defHoldToAtkLeftHoldTransitions;

        spineShouldBeRotated = 
            (allStates || allTransitions || defHoldToAtkHoldTransitions) && (atkHoldToDefHoldTransitions == false);


        chosenSpineRotationLerpRate = SpineRotationLerpRateWhileNotAttacking;

        if (spineShouldBeRotated)
        {
            targetSpineAngle = ownerAgent.LookAngleX;
            chosenSpineRotationLerpRate = SpineRotationLerpRateWhileAttacking;

            if (upStates || upTransitions || defHoldToAtkUpHoldTransitions)
            {
                // Being able to look all the way up/down while attacking from up looks weird, so put a limit to it.
                targetSpineAngle = Mathf.Clamp(targetSpineAngle, -TargetSpineAngleMaxForOverheadSwings, TargetSpineAngleMaxForOverheadSwings);
            }
        }
    }

    /// <summary>
    /// Changes the weights of layers based on whether the agent is currently idling or not.
    /// </summary>
    void SetLayerWeights()
    {
        // When you are transitioning from the a source state to a target state, Unity still considers you to be in that source state.
        // In this case, even while we're transitioning from the idle state (source), Unity still considers you to be in the idle state.
        // For this reason, we have to explicitly state that the transitions are not considered "idle".
        bool isNotIdling = isState_Idle == false
            || isTrans_IdleToAtkUpHold
            || isTrans_IdleToAtkRightHold
            || isTrans_IdleToAtkDownHold
            || isTrans_IdleToAtkLeftHold
            || isTrans_IdleToDefUpHold
            || isTrans_IdleToDefRightHold
            || isTrans_IdleToDefDownHold
            || isTrans_IdleToDefLeftHold;

        IsIdling = !isNotIdling; // yeah sorry for the negations but ugh.

        // If we're in a transitioning from AnyState, and it just so happens to be the idle state, then these transitions are also NOT considered "idle".
        // Therefore, we exclude them too.
        isNotIdling |= isTrans_AnyState;

        if (isNotIdling)
        {
            // not idling, therefore lerp (quicker) AttackAndBlock layer weight to 1, Idle Layer weight to 0

            idleTimer = 0f;

            attackAndBlockLayerWeight = Mathf.Lerp(Animat.GetLayerWeight(LayerIdAttackAndBlock), 1f, NotIdlingLerpRate_AttackAndBlockLayer);
            idleLayerWeight = Mathf.Lerp(Animat.GetLayerWeight(LayerIdIdle), 0f, NotIdlingLerpRate_IdleLayer); // lerp rate was 0.5f

            //attackAndBlockLayerWeight = 1f;
            //idleLayerWeight = 0f;
        }
        else
        {
            // is idling

            if (idleTimer <= IdleTimerMax)
            {
                idleTimer += Time.deltaTime;
            }

            if (idleTimer > IdleTimerMax)
            {
                attackAndBlockLayerWeight = Mathf.Lerp(Animat.GetLayerWeight(LayerIdAttackAndBlock), 0f, IdlingLerpRate_AttackAndBlockLayer);
                idleLayerWeight = Mathf.Lerp(Animat.GetLayerWeight(LayerIdIdle), 1f, IdlingLerpRate_IdleLayer);
            }
        }

        // Now, set the layer weights, dependong on whatever values were chosen above.
        Animat.SetLayerWeight(LayerIdAttackAndBlock, attackAndBlockLayerWeight);
        Animat.SetLayerWeight(LayerIdIdle, idleLayerWeight);
    }

    /// <summary>
    /// Updates the manually managed trigger parameters of the animator.
    /// The actual triggers of the Unity's Mecanim are not used, since they're very unreliable.
    /// Instead, we're manually managing our own triggers, which are just booleans set to false in the next frame.
    /// </summary>
    void SetTriggerParameters()
    {
        Animat.SetBool(AHD.Hash_isAtkBounced, trigger_isAtkBounced);
        Animat.SetBool(AHD.Hash_isDefBlocked, trigger_isDefBlocked);
        Animat.SetBool(AHD.Hash_jump, trigger_jump);
        Animat.SetBool(AHD.Hash_isHurt, trigger_isHurt);
    }

    /// <summary>
    /// Resets the values of the manually managed trigger parameters of the animator.
    /// The actual triggers of the Unity's Mecanim are not used, since they're very unreliable.
    /// Instead, we're manually managing our own triggers, which are just booleans set to false in the next frame.
    /// </summary>
    void ResetTriggerParameters()
    {
        trigger_isAtkBounced = false;
        trigger_isDefBlocked = false;
        trigger_jump = false;
        trigger_isHurt = false;
    }

    /// <summary>
    /// Updates the animations of the agent every frame.
    /// This is mainly an extension of the agent's Update method, since this is called at the end of their Update method.
    /// </summary>
    /// <param name="localMoveDir">Local move direction relative to the agent.</param>
    /// <param name="curMoveSpeed">The current movement speed of the agent.</param>
    /// <param name="isFalling">Whether or not the agent is falling.</param>
    /// <param name="isAtk">Whether or not the agent wants to attack.</param>
    /// <param name="isDef">Whether or not the agent wants to defend.</param>
    public void UpdateAnimations(Vector2 localMoveDir, float curMoveSpeed, bool isFalling, bool isAtk, bool isDef)
    {
        // Every update frame, assume that the target is zero degrees.
        // If the spine needs to be rotated, then the targetAngle will be read from the Agent later on.
        targetSpineAngle = 0;

        float moveX;
        float moveY;
        HandleMovementAnimationParameters(localMoveDir, curMoveSpeed, out moveX, out moveY);

        ReadStateInfo();
        ReadTransitionInfo();
        SetCombatParameters();
        DecideIfWeaponHitboxShouldBeActive();
        DecideIfSpineShouldBeRotated();
        SetLayerWeights();

        Animat.SetFloat(AHD.Hash_moveX, moveX);
        Animat.SetFloat(AHD.Hash_moveY, moveY);
        Animat.SetBool(AHD.Hash_isAtk, isAtk);
        Animat.SetBool(AHD.Hash_isDef, isDef);
        Animat.SetBool(AHD.Hash_isFalling, isFalling);

        // Set triggers.
        SetTriggerParameters();

        // Reset triggers below, after every "Set" call.
        ResetTriggerParameters();
    }

    /// <summary>
    /// Post processing of the animations after they have played for this frame.
    /// Mainly used to rotate and position the spine bone of the agent.
    /// </summary>
    public void LateUpdateAnimations()
    {
        ConnectSpineToPelvis();
        RotateSpineByLookDirectionAngleX();
    }

    /// <summary>
    /// Manually connects the spine bone to the pelvis bone (with the necessary offsets).
    /// Normally, the pelvis bone should be the parent of the spine bone.
    /// However, this is not the case in our character model.
    /// This is because we want to be able to attack while moving.
    /// If the pelvis bone was the parent of the spine bone, then we wouldn't be able to look
    /// at the direction we're attacking. Rather, the movement would have full control over the upper body, hence it would also affect
    /// where the agent is looking at while attacking.
    /// My solution was to manage the spine and pelvis bones separately, and connect them via code manually.
    /// </summary>
    void ConnectSpineToPelvis()
    {
        Quaternion finalPelvisRotation = pelvisBone.rotation;
        Quaternion offsetRotatorQuaternion = finalPelvisRotation * initialPelvisRotationInverse;

        Vector3 finalPelvisToSpineOffset = offsetRotatorQuaternion * initialPelvisToSpineOffset;

        float scaleRatioX = ownerAgent.transform.lossyScale.x / initialAgentScale.x;
        float scaleRatioY = ownerAgent.transform.lossyScale.y / initialAgentScale.y;
        float scaleRatioZ = ownerAgent.transform.lossyScale.z / initialAgentScale.z;

        Vector3 offset = finalPelvisToSpineOffset;
        offset.x *= scaleRatioX;
        offset.y *= scaleRatioY;
        offset.z *= scaleRatioZ;

        spineBone.position = pelvisBone.position + offset;
#if UNITY_EDITOR
        //Debug.DrawRay(pelvis.position, finalPelvisToSpineOffset, Color.red);
#endif
    }

    /// <summary>
    /// Rotates the spine bone based on where the agent is looking at.
    /// This is done after all animations have played for this frame.
    /// See the explanation of <see cref="ConnectSpineToPelvis"/> method for more info.
    /// </summary>
    void RotateSpineByLookDirectionAngleX()
    {
        if (ownerAgent.IsDead)
        {
            targetSpineAngle = 0;
        }

        if (StaticVariables.IsGamePaused)
        {
            // Do not rotate the spine while the game is paused.
            // Let it stay the way it was.
            chosenSpineRotationLerpRate = 0f;
        }

        spineCurAngle = Mathf.LerpAngle(spineCurAngle, targetSpineAngle, chosenSpineRotationLerpRate);

        Transform spineAfterAnim = spineBone.transform;
        spineAfterAnim.RotateAround(spineBone.position, ownerAgent.transform.right, spineCurAngle);
    }
}
