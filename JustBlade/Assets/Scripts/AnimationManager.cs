using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    // No, this is not the same as CombatDirection.
    // The fact that there are 4 "getting hurt" directions doesn't mean they should be combined.
    // There are 4 animations because making all the specific animations I would want to have would be very time consuming.
    public enum GettingHurtDirection
    {
        Up = 0,
        Right,
        Down,
        Left,
    }

    // TODO: Remove [SerializeField] attribute once you're done with the debugging.

    static readonly float SlowAgentSpeedRatioExponentForMoveXY = 1.85f;

    Agent ownerAgent;
    public Transform spineBone;
    public Transform pelvisBone;
    Vector3 initialPelvisToSpineOffset;
    Quaternion initialPelvisRotation;
    Quaternion initialPelvisRotationInverse;

    float targetSpineAngle;
    float spineCurAngle;
    float SpineRotationLerpRate = 0.2f;
    bool spineShouldBeRotated;

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
    AnimatorStateInfo attackAndBlockLayerStateInfo;
    AnimatorTransitionInfo attackAndBlockLayerTransitionInfo;
    [SerializeField] float idleTimer;
    [SerializeField] float IdleTimerMax = 0.1f; // wait for a while before shifting layer weights
    float attackAndBlockLayerWeight;
    float idleLayerWeight;

    float IdlingLerpRate_AttackAndBlockLayer = 0.1f; // 0.1f seems to be working well so far. Less is smoother, but slower.
    float NotIdlingLerpRate_AttackAndBlockLayer = 0.4f; // It's better if this is higher than NotIdlingLerpRate_IdleLayer.
    float IdlingLerpRate_IdleLayer = 1.0f; // Must lerp instantly, apparently...
    float NotIdlingLerpRate_IdleLayer = 0.35f; // Don't make this less than 0.35f, and don't make it higher than NotIdlingLerpRate_AttackAndBlockLayer.

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

    // Layer IDs (WARNING: Their index order is in sync with how they're laid out in the Animator Controller).
    const int LayerIdBase = 0;
    const int LayerIdAttackAndBlock = 1;
    const int LayerIdIdle = 2;

    // Animator parameters
    static readonly int Hash_moveX = Animator.StringToHash("moveX");
    static readonly int Hash_moveY = Animator.StringToHash("moveY");
    static readonly int Hash_combatDir = Animator.StringToHash("combatDir");
    static readonly int Hash_isAtk = Animator.StringToHash("isAtk");
    static readonly int Hash_isDef = Animator.StringToHash("isDef");
    static readonly int Hash_isAtkBounced = Animator.StringToHash("isAtkBounced");
    static readonly int Hash_isDefBlocked = Animator.StringToHash("isDefBlocked");
    static readonly int Hash_jump = Animator.StringToHash("jump");
    static readonly int Hash_isGrounded = Animator.StringToHash("isGrounded");
    static readonly int Hash_isHurt = Animator.StringToHash("isHurt");
    static readonly int Hash_isHurtDir = Animator.StringToHash("isHurtDir");
    static readonly int Hash_isDead = Animator.StringToHash("isDead");
    static readonly int Hash_moveAnimSpeedMulti = Animator.StringToHash("moveAnimSpeedMulti");

    // AttackAndBlockLayer State tags
    // Idle
    static readonly int Hash_StateTag_idle = Animator.StringToHash("idle");
    [SerializeField] bool isState_Idle;

    // Attack
    // atk_hold
    static readonly int Hash_StateTag_atk_up_hold = Animator.StringToHash("atk_up_hold");
    static readonly int Hash_StateTag_atk_right_hold = Animator.StringToHash("atk_right_hold");
    static readonly int Hash_StateTag_atk_down_hold = Animator.StringToHash("atk_down_hold");
    static readonly int Hash_StateTag_atk_left_hold = Animator.StringToHash("atk_left_hold");
    [SerializeField] bool isState_AtkHoldUp;
    [SerializeField] bool isState_AtkHoldRight;
    [SerializeField] bool isState_AtkHoldDown;
    [SerializeField] bool isState_AtkHoldLeft;

    // atk_release
    static readonly int Hash_StateTag_atk_up_release = Animator.StringToHash("atk_up_release");
    static readonly int Hash_StateTag_atk_right_release = Animator.StringToHash("atk_right_release");
    static readonly int Hash_StateTag_atk_down_release = Animator.StringToHash("atk_down_release");
    static readonly int Hash_StateTag_atk_left_release = Animator.StringToHash("atk_left_release");
    [SerializeField] bool isState_AtkReleaseUp;
    [SerializeField] bool isState_AtkReleaseRight;
    [SerializeField] bool isState_AtkReleaseDown;
    [SerializeField] bool isState_AtkReleaseLeft;

    // atk_bounce
    static readonly int Hash_StateTag_atk_up_bounce = Animator.StringToHash("atk_up_bounce");
    static readonly int Hash_StateTag_atk_right_bounce = Animator.StringToHash("atk_right_bounce");
    static readonly int Hash_StateTag_atk_down_bounce = Animator.StringToHash("atk_down_bounce");
    static readonly int Hash_StateTag_atk_left_bounce = Animator.StringToHash("atk_left_bounce");
    [SerializeField] bool isState_AtkBounceUp;
    [SerializeField] bool isState_AtkBounceRight;
    [SerializeField] bool isState_AtkBounceDown;
    [SerializeField] bool isState_AtkBounceLeft;

    // Defend
    // def_hold
    static readonly int Hash_StateTag_def_up_hold = Animator.StringToHash("def_up_hold");
    static readonly int Hash_StateTag_def_right_hold = Animator.StringToHash("def_right_hold");
    static readonly int Hash_StateTag_def_down_hold = Animator.StringToHash("def_down_hold");
    static readonly int Hash_StateTag_def_left_hold = Animator.StringToHash("def_left_hold");
    [SerializeField] bool isState_DefHoldUp;
    [SerializeField] bool isState_DefHoldRight;
    [SerializeField] bool isState_DefHoldDown;
    [SerializeField] bool isState_DefHoldLeft;

    // def_blocked
    static readonly int Hash_StateTag_def_up_blocked = Animator.StringToHash("def_up_blocked");
    static readonly int Hash_StateTag_def_right_blocked = Animator.StringToHash("def_right_blocked");
    static readonly int Hash_StateTag_def_down_blocked = Animator.StringToHash("def_down_blocked");
    static readonly int Hash_StateTag_def_left_blocked = Animator.StringToHash("def_left_blocked");
    [SerializeField] bool isState_DefBlockedUp;
    [SerializeField] bool isState_DefBlockedRight;
    [SerializeField] bool isState_DefBlockedDown;
    [SerializeField] bool isState_DefBlockedLeft;

    // AttackAndBlockLayer Transition custom names
    // Attack
    // idle_to_atk_hold
    static readonly int Hash_TransName_idle_to_atk_up_hold = Animator.StringToHash("idle_to_atk_up_hold");
    static readonly int Hash_TransName_idle_to_atk_right_hold = Animator.StringToHash("idle_to_atk_right_hold");
    static readonly int Hash_TransName_idle_to_atk_down_hold = Animator.StringToHash("idle_to_atk_down_hold");
    static readonly int Hash_TransName_idle_to_atk_left_hold = Animator.StringToHash("idle_to_atk_left_hold");
    [SerializeField] bool isTrans_IdleToAtkUpHold;
    [SerializeField] bool isTrans_IdleToAtkRightHold;
    [SerializeField] bool isTrans_IdleToAtkDownHold;
    [SerializeField] bool isTrans_IdleToAtkLeftHold;


    // atk_hold_to_release
    static readonly int Hash_TransName_atk_up_hold_to_release = Animator.StringToHash("atk_up_hold_to_release");
    static readonly int Hash_TransName_atk_right_hold_to_release = Animator.StringToHash("atk_right_hold_to_release");
    static readonly int Hash_TransName_atk_down_hold_to_release = Animator.StringToHash("atk_down_hold_to_release");
    static readonly int Hash_TransName_atk_left_hold_to_release = Animator.StringToHash("atk_left_hold_to_release");
    [SerializeField] bool isTrans_AtkUpHoldToRelease;
    [SerializeField] bool isTrans_AtkRightHoldToRelease;
    [SerializeField] bool isTrans_AtkDownHoldToRelease;
    [SerializeField] bool isTrans_AtkLeftHoldToRelease;

    // atk_release_to_bounce
    static readonly int Hash_TransName_atk_up_release_to_bounce = Animator.StringToHash("atk_up_release_to_bounce");
    static readonly int Hash_TransName_atk_right_release_to_bounce = Animator.StringToHash("atk_right_release_to_bounce");
    static readonly int Hash_TransName_atk_down_release_to_bounce = Animator.StringToHash("atk_down_release_to_bounce");
    static readonly int Hash_TransName_atk_left_release_to_bounce = Animator.StringToHash("atk_left_release_to_bounce");
    [SerializeField] bool isTrans_AtkUpReleaseToBounce;
    [SerializeField] bool isTrans_AtkRightReleaseToBounce;
    [SerializeField] bool isTrans_AtkDownReleaseToBounce;
    [SerializeField] bool isTrans_AtkLeftReleaseToBounce;

    // atk_release_to_idle
    static readonly int Hash_TransName_atk_up_release_to_idle = Animator.StringToHash("atk_up_release_to_idle");
    static readonly int Hash_TransName_atk_right_release_to_idle = Animator.StringToHash("atk_right_release_to_idle");
    static readonly int Hash_TransName_atk_down_release_to_idle = Animator.StringToHash("atk_down_release_to_idle");
    static readonly int Hash_TransName_atk_left_release_to_idle = Animator.StringToHash("atk_left_release_to_idle");
    [SerializeField] bool isTrans_AtkUpReleaseToIdle;
    [SerializeField] bool isTrans_AtkRightReleaseToIdle;
    [SerializeField] bool isTrans_AtkDownReleaseToIdle;
    [SerializeField] bool isTrans_AtkLeftReleaseToIdle;

    // atk_bounce_to_idle
    static readonly int Hash_TransName_atk_up_bounce_to_idle = Animator.StringToHash("atk_up_bounce_to_idle");
    static readonly int Hash_TransName_atk_right_bounce_to_idle = Animator.StringToHash("atk_right_bounce_to_idle");
    static readonly int Hash_TransName_atk_down_bounce_to_idle = Animator.StringToHash("atk_down_bounce_to_idle");
    static readonly int Hash_TransName_atk_left_bounce_to_idle = Animator.StringToHash("atk_left_bounce_to_idle");
    [SerializeField] bool isTrans_AtkUpBounceToIdle;
    [SerializeField] bool isTrans_AtkRightBounceToIdle;
    [SerializeField] bool isTrans_AtkDownBounceToIdle;
    [SerializeField] bool isTrans_AtkLeftBounceToIdle;

    // Ok... I wasn't expecting this nonsense.
    // When leaving a state via a transition, Mecanim still considers you to be in the source state.
    // Hence the nonsense below...
    // atk_up_hold_to_def_hold
    static readonly int Hash_TransName_atk_up_hold_to_def_up_hold = Animator.StringToHash("atk_up_hold_to_def_up_hold");
    static readonly int Hash_TransName_atk_up_hold_to_def_right_hold = Animator.StringToHash("atk_up_hold_to_def_right_hold");
    static readonly int Hash_TransName_atk_up_hold_to_def_down_hold = Animator.StringToHash("atk_up_hold_to_def_down_hold");
    static readonly int Hash_TransName_atk_up_hold_to_def_left_hold = Animator.StringToHash("atk_up_hold_to_def_left_hold");
    [SerializeField] bool isTrans_AtkUpHoldToDefUpHold;
    [SerializeField] bool isTrans_AtkUpHoldToDefRightHold;
    [SerializeField] bool isTrans_AtkUpHoldToDefDownHold;
    [SerializeField] bool isTrans_AtkUpHoldToDefLeftHold;

    // atk_right_hold_to_def_hold
    static readonly int Hash_TransName_atk_right_hold_to_def_up_hold = Animator.StringToHash("atk_right_hold_to_def_up_hold");
    static readonly int Hash_TransName_atk_right_hold_to_def_right_hold = Animator.StringToHash("atk_right_hold_to_def_right_hold");
    static readonly int Hash_TransName_atk_right_hold_to_def_down_hold = Animator.StringToHash("atk_right_hold_to_def_down_hold");
    static readonly int Hash_TransName_atk_right_hold_to_def_left_hold = Animator.StringToHash("atk_right_hold_to_def_left_hold");
    [SerializeField] bool isTrans_AtkRightHoldToDefUpHold;
    [SerializeField] bool isTrans_AtkRightHoldToDefRightHold;
    [SerializeField] bool isTrans_AtkRightHoldToDefDownHold;
    [SerializeField] bool isTrans_AtkRightHoldToDefLeftHold;

    // atk_down_hold_to_def_hold
    static readonly int Hash_TransName_atk_down_hold_to_def_up_hold = Animator.StringToHash("atk_down_hold_to_def_up_hold");
    static readonly int Hash_TransName_atk_down_hold_to_def_right_hold = Animator.StringToHash("atk_down_hold_to_def_right_hold");
    static readonly int Hash_TransName_atk_down_hold_to_def_down_hold = Animator.StringToHash("atk_down_hold_to_def_down_hold");
    static readonly int Hash_TransName_atk_down_hold_to_def_left_hold = Animator.StringToHash("atk_down_hold_to_def_left_hold");
    [SerializeField] bool isTrans_AtkDownHoldToDefUpHold;
    [SerializeField] bool isTrans_AtkDownHoldToDefRightHold;
    [SerializeField] bool isTrans_AtkDownHoldToDefDownHold;
    [SerializeField] bool isTrans_AtkDownHoldToDefLeftHold;

    // atk_left_hold_to_def_hold
    static readonly int Hash_TransName_atk_left_hold_to_def_up_hold = Animator.StringToHash("atk_left_hold_to_def_up_hold");
    static readonly int Hash_TransName_atk_left_hold_to_def_right_hold = Animator.StringToHash("atk_left_hold_to_def_right_hold");
    static readonly int Hash_TransName_atk_left_hold_to_def_down_hold = Animator.StringToHash("atk_left_hold_to_def_down_hold");
    static readonly int Hash_TransName_atk_left_hold_to_def_left_hold = Animator.StringToHash("atk_left_hold_to_def_left_hold");
    [SerializeField] bool isTrans_AtkLeftHoldToDefUpHold;
    [SerializeField] bool isTrans_AtkLeftHoldToDefRightHold;
    [SerializeField] bool isTrans_AtkLeftHoldToDefDownHold;
    [SerializeField] bool isTrans_AtkLeftHoldToDefLeftHold;


    // Defend
    // idle_to_def_hold
    static readonly int Hash_TransName_idle_to_def_up_hold = Animator.StringToHash("idle_to_def_up_hold");
    static readonly int Hash_TransName_idle_to_def_right_hold = Animator.StringToHash("idle_to_def_right_hold");
    static readonly int Hash_TransName_idle_to_def_down_hold = Animator.StringToHash("idle_to_def_down_hold");
    static readonly int Hash_TransName_idle_to_def_left_hold = Animator.StringToHash("idle_to_def_left_hold");
    [SerializeField] bool isTrans_IdleToDefUpHold;
    [SerializeField] bool isTrans_IdleToDefRightHold;
    [SerializeField] bool isTrans_IdleToDefDownHold;
    [SerializeField] bool isTrans_IdleToDefLeftHold;

    // def_hold_to_blocked
    static readonly int Hash_TransName_def_up_hold_to_blocked = Animator.StringToHash("def_up_hold_to_blocked");
    static readonly int Hash_TransName_def_right_hold_to_blocked = Animator.StringToHash("def_right_hold_to_blocked");
    static readonly int Hash_TransName_def_down_hold_to_blocked = Animator.StringToHash("def_down_hold_to_blocked");
    static readonly int Hash_TransName_def_left_hold_to_blocked = Animator.StringToHash("def_left_hold_to_blocked");
    [SerializeField] bool isTrans_DefUpHoldToBlocked;
    [SerializeField] bool isTrans_DefRightHoldToBlocked;
    [SerializeField] bool isTrans_DefDownHoldToBlocked;
    [SerializeField] bool isTrans_DefLeftHoldToBlocked;

    // def_blocked_to_hold
    static readonly int Hash_TransName_def_up_blocked_to_hold = Animator.StringToHash("def_up_blocked_to_hold");
    static readonly int Hash_TransName_def_right_blocked_to_hold = Animator.StringToHash("def_right_blocked_to_hold");
    static readonly int Hash_TransName_def_down_blocked_to_hold = Animator.StringToHash("def_down_blocked_to_hold");
    static readonly int Hash_TransName_def_left_blocked_to_hold = Animator.StringToHash("def_left_blocked_to_hold");
    [SerializeField] bool isTrans_DefUpBlockedToHold;
    [SerializeField] bool isTrans_DefRightBlockedToHold;
    [SerializeField] bool isTrans_DefDownBlockedToHold;
    [SerializeField] bool isTrans_DefLeftBlockedToHold;

    // Ok... I wasn't expecting this nonsense.
    // When leaving a state via a transition, Mecanim still considers you to be in the source state.
    // Hence the nonsense below...
    // def_up_hold_to_atk_hold
    static readonly int Hash_TransName_def_up_hold_to_atk_up_hold = Animator.StringToHash("def_up_hold_to_atk_up_hold");
    static readonly int Hash_TransName_def_up_hold_to_atk_right_hold = Animator.StringToHash("def_up_hold_to_atk_right_hold");
    static readonly int Hash_TransName_def_up_hold_to_atk_down_hold = Animator.StringToHash("def_up_hold_to_atk_down_hold");
    static readonly int Hash_TransName_def_up_hold_to_atk_left_hold = Animator.StringToHash("def_up_hold_to_atk_left_hold");
    [SerializeField] bool isTrans_DefUpHoldToAtkUpHold;
    [SerializeField] bool isTrans_DefUpHoldToAtkRightHold;
    [SerializeField] bool isTrans_DefUpHoldToAtkDownHold;
    [SerializeField] bool isTrans_DefUpHoldToAtkLeftHold;

    // def_right_hold_to_atk_hold
    static readonly int Hash_TransName_def_right_hold_to_atk_up_hold = Animator.StringToHash("def_right_hold_to_atk_up_hold");
    static readonly int Hash_TransName_def_right_hold_to_atk_right_hold = Animator.StringToHash("def_right_hold_to_atk_right_hold");
    static readonly int Hash_TransName_def_right_hold_to_atk_down_hold = Animator.StringToHash("def_right_hold_to_atk_down_hold");
    static readonly int Hash_TransName_def_right_hold_to_atk_left_hold = Animator.StringToHash("def_right_hold_to_atk_left_hold");
    [SerializeField] bool isTrans_DefRightHoldToAtkUpHold;
    [SerializeField] bool isTrans_DefRightHoldToAtkRightHold;
    [SerializeField] bool isTrans_DefRightHoldToAtkDownHold;
    [SerializeField] bool isTrans_DefRightHoldToAtkLeftHold;

    // def_down_hold_to_atk_hold
    static readonly int Hash_TransName_def_down_hold_to_atk_up_hold = Animator.StringToHash("def_down_hold_to_atk_up_hold");
    static readonly int Hash_TransName_def_down_hold_to_atk_right_hold = Animator.StringToHash("def_down_hold_to_atk_right_hold");
    static readonly int Hash_TransName_def_down_hold_to_atk_down_hold = Animator.StringToHash("def_down_hold_to_atk_down_hold");
    static readonly int Hash_TransName_def_down_hold_to_atk_left_hold = Animator.StringToHash("def_down_hold_to_atk_left_hold");
    [SerializeField] bool isTrans_DefDownHoldToAtkUpHold;
    [SerializeField] bool isTrans_DefDownHoldToAtkRightHold;
    [SerializeField] bool isTrans_DefDownHoldToAtkDownHold;
    [SerializeField] bool isTrans_DefDownHoldToAtkLeftHold;

    // def_left_hold_to_atk_hold
    static readonly int Hash_TransName_def_left_hold_to_atk_up_hold = Animator.StringToHash("def_left_hold_to_atk_up_hold");
    static readonly int Hash_TransName_def_left_hold_to_atk_right_hold = Animator.StringToHash("def_left_hold_to_atk_right_hold");
    static readonly int Hash_TransName_def_left_hold_to_atk_down_hold = Animator.StringToHash("def_left_hold_to_atk_down_hold");
    static readonly int Hash_TransName_def_left_hold_to_atk_left_hold = Animator.StringToHash("def_left_hold_to_atk_left_hold");
    [SerializeField] bool isTrans_DefLeftHoldToAtkUpHold;
    [SerializeField] bool isTrans_DefLeftHoldToAtkRightHold;
    [SerializeField] bool isTrans_DefLeftHoldToAtkDownHold;
    [SerializeField] bool isTrans_DefLeftHoldToAtkLeftHold;

    // Trigger parameters which must be set to false every frame, after "Set" methods.
    bool trigger_isAtkBounced;
    bool trigger_isDefBlocked;
    bool trigger_jump;
    bool trigger_isHurt;

    void Awake()
    {
        ownerAgent = GetComponent<Agent>();

        initialPelvisToSpineOffset = spineBone.position - pelvisBone.position;
        initialPelvisRotation = pelvisBone.rotation;
        initialPelvisRotationInverse = Quaternion.Inverse(initialPelvisRotation);
    }

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

    public void UpdateCombatDirection(Agent.CombatDirection combatDir)
    {
        Animat.SetInteger(Hash_combatDir, (int)combatDir);
    }

    public void SetJump(bool isJumping)
    {
        trigger_jump = isJumping;
    }

    public void SetIsAttackBounced(bool isAtkBounced)
    {
        trigger_isAtkBounced = isAtkBounced;
    }

    public void SetIsDefBlocked(bool isDefBlocked)
    {
        trigger_isDefBlocked = isDefBlocked;
    }

    public void SetGettingHurt(GettingHurtDirection gettingHurtDirection)
    {
        trigger_isHurt = true;
        Animat.SetInteger(Hash_isHurtDir, (int)gettingHurtDirection);
    }

    public void PlayDeathAnimation()
    {
        Animat.SetBool(Hash_isDead, true);

        attackAndBlockLayerWeight = 0;
        idleLayerWeight = 0;

        Animat.SetLayerWeight(LayerIdAttackAndBlock, attackAndBlockLayerWeight);
        Animat.SetLayerWeight(LayerIdIdle, idleLayerWeight);

        ownerAgent.EqMgr.equippedWeapon.SetCollisionAbility(false);
    }

    void HandleMovementAnimationParameters(Vector2 localMoveDir, float curMoveSpeed, out float moveX, out float moveY)
    {
        float speedRatio = curMoveSpeed / Agent.DefaultMovementSpeedLimit;

        // Initialize moveX and moveY based on localMoveDir.
        // The scale depends on speedRatio.
        // However, the absolute value of both moveX and moveY must never be above 1.0f.
        Vector2 moveXY = localMoveDir.normalized;
        float moveXYmulti = Mathf.Clamp01(speedRatio);

        moveX = moveXY.x * moveXYmulti;
        moveY = moveXY.y * moveXYmulti;

        // This multiplier is allowed to be greater than 1.0f, but it can never be less than 1.0f.
        float moveAnimSpeedMulti = 1.0f; 

        // We compare the agent's curMoveSpeed to the Agent.DefaultMovementSpeedLimit. 
        if (curMoveSpeed >= Agent.DefaultMovementSpeedLimit)
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

            if (ownerAgent.MovementSpeedLimit < Agent.DefaultMovementSpeedLimit)
            {
                // Here, we know that agent is a slow guy, and can never reach the default speed.

                // So, we reduce moveX and moveY values accordingly.
                speedRatio = Mathf.Pow(speedRatio, SlowAgentSpeedRatioExponentForMoveXY);
                moveX = Mathf.Clamp(moveX, -speedRatio, speedRatio);
                moveY = Mathf.Clamp(moveY, -speedRatio, speedRatio);
            }
        }

        // Finally, set the movement animation moveAnimSpeedMulti.
        Animat.SetFloat(Hash_moveAnimSpeedMulti, moveAnimSpeedMulti);
    }

    void ReadStateInfo()
    {
        attackAndBlockLayerStateInfo = Animat.GetCurrentAnimatorStateInfo(LayerIdAttackAndBlock);

        isState_Idle = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_idle;

        isState_AtkHoldUp = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_atk_up_hold;
        isState_AtkHoldRight = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_atk_right_hold;
        isState_AtkHoldDown = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_atk_down_hold;
        isState_AtkHoldLeft = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_atk_left_hold;

        isState_AtkReleaseUp = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_atk_up_release;
        isState_AtkReleaseRight = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_atk_right_release;
        isState_AtkReleaseDown = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_atk_down_release;
        isState_AtkReleaseLeft = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_atk_left_release;

        isState_AtkBounceUp = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_atk_up_bounce;
        isState_AtkBounceRight = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_atk_right_bounce;
        isState_AtkBounceDown = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_atk_down_bounce;
        isState_AtkBounceLeft = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_atk_left_bounce;

        isState_DefHoldUp = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_def_up_hold;
        isState_DefHoldRight = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_def_right_hold;
        isState_DefHoldDown = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_def_down_hold;
        isState_DefHoldLeft = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_def_left_hold;

        isState_DefBlockedUp = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_def_up_blocked;
        isState_DefBlockedRight = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_def_right_blocked;
        isState_DefBlockedDown = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_def_down_blocked;
        isState_DefBlockedLeft = attackAndBlockLayerStateInfo.tagHash == Hash_StateTag_def_left_blocked;
    }

    void ReadTransitionInfo()
    {
        attackAndBlockLayerTransitionInfo = Animat.GetAnimatorTransitionInfo(LayerIdAttackAndBlock);
        // Attack
        // idle_to_atk_hold
        isTrans_IdleToAtkUpHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_idle_to_atk_up_hold;
        isTrans_IdleToAtkRightHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_idle_to_atk_right_hold;
        isTrans_IdleToAtkDownHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_idle_to_atk_down_hold;
        isTrans_IdleToAtkLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_idle_to_atk_left_hold;

        // atk_hold_to_release
        isTrans_AtkUpHoldToRelease = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_up_hold_to_release;
        isTrans_AtkRightHoldToRelease = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_right_hold_to_release;
        isTrans_AtkDownHoldToRelease = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_down_hold_to_release;
        isTrans_AtkLeftHoldToRelease = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_left_hold_to_release;

        // atk_release_to_bounce
        isTrans_AtkUpReleaseToBounce = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_up_release_to_bounce;
        isTrans_AtkRightReleaseToBounce = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_right_release_to_bounce;
        isTrans_AtkDownReleaseToBounce = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_down_release_to_bounce;
        isTrans_AtkLeftReleaseToBounce = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_left_release_to_bounce;

        // atk_release_to_idle
        isTrans_AtkUpReleaseToIdle = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_up_release_to_idle;
        isTrans_AtkRightReleaseToIdle = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_right_release_to_idle;
        isTrans_AtkDownReleaseToIdle = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_down_release_to_idle;
        isTrans_AtkLeftReleaseToIdle = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_left_release_to_idle;

        // atk_bounce_to_idle
        isTrans_AtkUpBounceToIdle = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_up_bounce_to_idle;
        isTrans_AtkRightBounceToIdle = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_right_bounce_to_idle;
        isTrans_AtkDownBounceToIdle = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_down_bounce_to_idle;
        isTrans_AtkLeftBounceToIdle = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_left_bounce_to_idle;

        // Ok... I wasn't expecting this nonsense.
        // When leaving a state via a transition, Mecanim still considers you to be in the source state.
        // Hence the nonsense below...
        // atk_up_hold_to_def_hold
        isTrans_AtkUpHoldToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_up_hold_to_def_up_hold;
        isTrans_AtkUpHoldToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_up_hold_to_def_right_hold;
        isTrans_AtkUpHoldToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_up_hold_to_def_down_hold;
        isTrans_AtkUpHoldToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_up_hold_to_def_left_hold;

        // atk_right_hold_to_def_hold
        isTrans_AtkRightHoldToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_right_hold_to_def_up_hold;
        isTrans_AtkRightHoldToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_right_hold_to_def_right_hold;
        isTrans_AtkRightHoldToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_right_hold_to_def_down_hold;
        isTrans_AtkRightHoldToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_right_hold_to_def_left_hold;

        // atk_down_hold_to_def_hold
        isTrans_AtkDownHoldToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_down_hold_to_def_up_hold;
        isTrans_AtkDownHoldToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_down_hold_to_def_right_hold;
        isTrans_AtkDownHoldToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_down_hold_to_def_down_hold;
        isTrans_AtkDownHoldToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_down_hold_to_def_left_hold;

        // atk_left_hold_to_def_hold
        isTrans_AtkLeftHoldToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_left_hold_to_def_up_hold;
        isTrans_AtkLeftHoldToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_left_hold_to_def_right_hold;
        isTrans_AtkLeftHoldToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_left_hold_to_def_down_hold;
        isTrans_AtkLeftHoldToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_atk_left_hold_to_def_left_hold;


        // Defend
        // idle_to_def_hold
        isTrans_IdleToDefUpHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_idle_to_def_up_hold;
        isTrans_IdleToDefRightHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_idle_to_def_right_hold;
        isTrans_IdleToDefDownHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_idle_to_def_down_hold;
        isTrans_IdleToDefLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_idle_to_def_left_hold;

        // def_hold_to_blocked
        isTrans_DefUpHoldToBlocked = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_up_hold_to_blocked;
        isTrans_DefRightHoldToBlocked = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_right_hold_to_blocked;
        isTrans_DefDownHoldToBlocked = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_down_hold_to_blocked;
        isTrans_DefLeftHoldToBlocked = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_left_hold_to_blocked;

        // def_blocked_to_hold
        isTrans_DefUpBlockedToHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_up_blocked_to_hold;
        isTrans_DefRightBlockedToHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_right_blocked_to_hold;
        isTrans_DefDownBlockedToHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_down_blocked_to_hold;
        isTrans_DefLeftBlockedToHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_left_blocked_to_hold;

        // Ok... I wasn't expecting this nonsense.
        // When leaving a state via a transition, Mecanim still considers you to be in the source state.
        // Hence the nonsense below...
        // def_up_hold_to_atk_hold
        isTrans_DefUpHoldToAtkUpHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_up_hold_to_atk_up_hold;
        isTrans_DefUpHoldToAtkRightHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_up_hold_to_atk_right_hold;
        isTrans_DefUpHoldToAtkDownHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_up_hold_to_atk_down_hold;
        isTrans_DefUpHoldToAtkLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_up_hold_to_atk_left_hold;

        // def_right_hold_to_atk_hold
        isTrans_DefRightHoldToAtkUpHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_right_hold_to_atk_up_hold;
        isTrans_DefRightHoldToAtkRightHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_right_hold_to_atk_right_hold;
        isTrans_DefRightHoldToAtkDownHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_right_hold_to_atk_down_hold;
        isTrans_DefRightHoldToAtkLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_right_hold_to_atk_left_hold;

        // def_down_hold_to_atk_hold
        isTrans_DefDownHoldToAtkUpHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_down_hold_to_atk_up_hold;
        isTrans_DefDownHoldToAtkRightHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_down_hold_to_atk_right_hold;
        isTrans_DefDownHoldToAtkDownHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_down_hold_to_atk_down_hold;
        isTrans_DefDownHoldToAtkLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_down_hold_to_atk_left_hold;

        // def_left_hold_to_atk_hold
        isTrans_DefLeftHoldToAtkUpHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_left_hold_to_atk_up_hold;
        isTrans_DefLeftHoldToAtkRightHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_left_hold_to_atk_right_hold;
        isTrans_DefLeftHoldToAtkDownHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_left_hold_to_atk_down_hold;
        isTrans_DefLeftHoldToAtkLeftHold = attackAndBlockLayerTransitionInfo.userNameHash == Hash_TransName_def_left_hold_to_atk_left_hold;
    }

    void SetCombatParameters()
    {
        // Setting values based on state and transition info
        IsAttackingFromUp = isState_AtkReleaseUp;
        IsAttackingFromRight = isState_AtkReleaseRight;
        IsAttackingFromDown = isState_AtkReleaseDown;
        IsAttackingFromLeft = isState_AtkReleaseLeft;

        IsDefendingFromUp = isState_DefHoldUp || isTrans_DefUpHoldToBlocked || isTrans_DefUpBlockedToHold;
        IsDefendingFromRight = isState_DefHoldRight || isTrans_DefRightHoldToBlocked || isTrans_DefRightBlockedToHold;
        IsDefendingFromDown = isState_DefHoldDown || isTrans_DefDownHoldToBlocked || isTrans_DefDownBlockedToHold;
        IsDefendingFromLeft = isState_DefHoldLeft || isTrans_DefLeftHoldToBlocked || isTrans_DefLeftBlockedToHold;
    }

    void DecideIfWeaponHitboxShouldBeActive()
    {
        ownerAgent.EqMgr.equippedWeapon.SetCollisionAbility(IsAttacking);
    }

    void DecideIfSpineShouldBeRotated()
    {
        bool upStates = isState_AtkHoldUp || isState_AtkReleaseUp || isState_AtkBounceUp;
        bool rightStates = isState_AtkHoldRight || isState_AtkReleaseRight || isState_AtkBounceRight;
        bool downStates = isState_AtkHoldDown || isState_AtkReleaseDown || isState_AtkBounceDown;
        bool leftStates = isState_AtkHoldLeft || isState_AtkReleaseLeft || isState_AtkBounceLeft;
        bool allStates = upStates || rightStates || downStates || leftStates;

        bool upTransitions = isTrans_IdleToAtkUpHold || isTrans_AtkUpHoldToRelease
            || isTrans_AtkUpReleaseToBounce || isTrans_AtkUpReleaseToIdle || isTrans_AtkUpBounceToIdle;
        bool rightTransitions = isTrans_IdleToAtkRightHold || isTrans_AtkRightHoldToRelease
            || isTrans_AtkRightReleaseToBounce || isTrans_AtkRightReleaseToIdle || isTrans_AtkRightBounceToIdle;
        bool downTransitions = isTrans_IdleToAtkDownHold || isTrans_AtkDownHoldToRelease
            || isTrans_AtkDownReleaseToBounce || isTrans_AtkDownReleaseToIdle || isTrans_AtkDownBounceToIdle;
        bool leftTransitions = isTrans_IdleToAtkLeftHold || isTrans_AtkLeftHoldToRelease
            || isTrans_AtkLeftReleaseToBounce || isTrans_AtkLeftReleaseToIdle || isTrans_AtkLeftBounceToIdle;
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

        bool defHoldToAtkHoldTransitions =
            isTrans_DefUpHoldToAtkUpHold
        || isTrans_DefUpHoldToAtkRightHold
        || isTrans_DefUpHoldToAtkDownHold
        || isTrans_DefUpHoldToAtkLeftHold
        || isTrans_DefRightHoldToAtkUpHold
        || isTrans_DefRightHoldToAtkRightHold
        || isTrans_DefRightHoldToAtkDownHold
        || isTrans_DefRightHoldToAtkLeftHold
        || isTrans_DefDownHoldToAtkUpHold
        || isTrans_DefDownHoldToAtkRightHold
        || isTrans_DefDownHoldToAtkDownHold
        || isTrans_DefDownHoldToAtkLeftHold
        || isTrans_DefLeftHoldToAtkUpHold
        || isTrans_DefLeftHoldToAtkRightHold
        || isTrans_DefLeftHoldToAtkDownHold
        || isTrans_DefLeftHoldToAtkLeftHold;

        spineShouldBeRotated = (allStates || allTransitions || defHoldToAtkHoldTransitions) && (atkHoldToDefHoldTransitions == false);

        if (spineShouldBeRotated)
        {
            targetSpineAngle = ownerAgent.LookAngleX;
        }
    }

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

        // If we're in a transitioning from AnyState, and it just so happens to be the idle state, then these transitions are also NOT considered "idle".
        // Therefore, we exclude them too.
        isNotIdling |= attackAndBlockLayerTransitionInfo.anyState;

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

    void SetTriggerParameters()
    {
        Animat.SetBool(Hash_isAtkBounced, trigger_isAtkBounced);
        Animat.SetBool(Hash_isDefBlocked, trigger_isDefBlocked);
        Animat.SetBool(Hash_jump, trigger_jump);
        Animat.SetBool(Hash_isHurt, trigger_isHurt);
    }

    void ResetTriggerParameters()
    {
        trigger_isAtkBounced = false;
        trigger_isDefBlocked = false;
        trigger_jump = false;
        trigger_isHurt = false;
    }

    public void UpdateAnimations(Vector2 localMoveDir, float curMoveSpeed, bool isGrounded, bool isAtk, bool isDef)
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

        Animat.SetFloat(Hash_moveX, moveX);
        Animat.SetFloat(Hash_moveY, moveY);
        Animat.SetBool(Hash_isAtk, isAtk);
        Animat.SetBool(Hash_isDef, isDef);
        Animat.SetBool(Hash_isGrounded, isGrounded);

        // Set triggers.
        SetTriggerParameters();

        // Reset triggers below, after every "Set" call.
        ResetTriggerParameters();
    }

    public void LateUpdateAnimations()
    {
        ConnectSpineToPelvis();
        RotateSpineByLookDirectionAngleX();
    }

    void ConnectSpineToPelvis()
    {
        Quaternion finalPelvisRotation = pelvisBone.rotation;
        Quaternion offsetRotatorQuaternion = finalPelvisRotation * initialPelvisRotationInverse;

        Vector3 finalPelvisToSpineOffset = offsetRotatorQuaternion * initialPelvisToSpineOffset;

        spineBone.position = pelvisBone.position + finalPelvisToSpineOffset;
        //Debug.DrawRay(pelvis.position, finalPelvisToSpineOffset, Color.red);
    }
    void RotateSpineByLookDirectionAngleX()
    {
        spineCurAngle = Mathf.LerpAngle(spineCurAngle, targetSpineAngle, SpineRotationLerpRate);

        Transform spineAfterAnim = spineBone.transform;
        spineAfterAnim.RotateAround(spineBone.position, ownerAgent.transform.right, spineCurAngle);
    }
}
