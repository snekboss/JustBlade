using UnityEngine;

/// <summary>
/// AHD means Animator Hash Data.
/// Unity doesn't like to check for the states and transitions via a string due to performance issues.
/// Therefore, we have to use the <see cref="Animator.StringToHash(string)"/> method to get the
/// corresponding hash values, and check for the states and transitions that way.
/// In order to reduce clutter in <see cref="AnimationManager"/>, these hash values were put here,
/// since they're the same across all Animators.
/// For any given state in Unity's Mecanim, it considers you to be on that state until you have
/// fully transitioned out of it. This causes issues. For example, if you're in the "atk_release" state,
/// you are considered to be "attacking". However, while you're transitioning out of it that state,
/// if you haven't FULLY transitioned out of it, then you are still considered to be "attacking",
/// due to how Mecanim works as described above. Therefore, in such cases, it is required to explicitly
/// state that we do not want these transitions to be considered "attacking". Since there are many cases
/// similar to this (eg: many "atk" states, many "def" states), there are a lot of transitions to consider,
/// which increases the number of hash integers we have to store. This is the reason why there is a rather
/// large amount of hash values stored in this class.
/// This class can be easily turned into Singleton if it's necessary (eg: if static memory is getting too large).
/// However, at the time of writing this, it wasn't necessary.
/// </summary>
public static class AHD
{
    #region Animator parameters
    // Animator parameters
    public static readonly int Hash_moveX = Animator.StringToHash("moveX");
    public static readonly int Hash_moveY = Animator.StringToHash("moveY");
    public static readonly int Hash_combatDir = Animator.StringToHash("combatDir");
    public static readonly int Hash_isAtk = Animator.StringToHash("isAtk");
    public static readonly int Hash_isDef = Animator.StringToHash("isDef");
    public static readonly int Hash_isAtkBounced = Animator.StringToHash("isAtkBounced");
    public static readonly int Hash_isDefBlocked = Animator.StringToHash("isDefBlocked");
    public static readonly int Hash_jump = Animator.StringToHash("jump");
    public static readonly int Hash_isFalling = Animator.StringToHash("isFalling");
    public static readonly int Hash_isHurt = Animator.StringToHash("isHurt");
    public static readonly int Hash_isHurtDir = Animator.StringToHash("isHurtDir");
    public static readonly int Hash_isDead = Animator.StringToHash("isDead");
    public static readonly int Hash_moveAnimSpeedMulti = Animator.StringToHash("moveAnimSpeedMulti"); 
    #endregion
    // Animator states and transitions
    #region Animator states
    // AttackAndBlockLayer State tags
    // Idle
    public static readonly int Hash_StateTag_idle = Animator.StringToHash("idle");
    // Attack
    // atk_hold
    public static readonly int Hash_StateTag_atk_up_hold = Animator.StringToHash("atk_up_hold");
    public static readonly int Hash_StateTag_atk_right_hold = Animator.StringToHash("atk_right_hold");
    public static readonly int Hash_StateTag_atk_down_hold = Animator.StringToHash("atk_down_hold");
    public static readonly int Hash_StateTag_atk_left_hold = Animator.StringToHash("atk_left_hold");
    // atk_release
    public static readonly int Hash_StateTag_atk_up_release = Animator.StringToHash("atk_up_release");
    public static readonly int Hash_StateTag_atk_right_release = Animator.StringToHash("atk_right_release");
    public static readonly int Hash_StateTag_atk_down_release = Animator.StringToHash("atk_down_release");
    public static readonly int Hash_StateTag_atk_left_release = Animator.StringToHash("atk_left_release");
    // atk_bounce
    public static readonly int Hash_StateTag_atk_up_bounce = Animator.StringToHash("atk_up_bounce");
    public static readonly int Hash_StateTag_atk_right_bounce = Animator.StringToHash("atk_right_bounce");
    public static readonly int Hash_StateTag_atk_down_bounce = Animator.StringToHash("atk_down_bounce");
    public static readonly int Hash_StateTag_atk_left_bounce = Animator.StringToHash("atk_left_bounce");
    // Defend
    // def_hold
    public static readonly int Hash_StateTag_def_up_hold = Animator.StringToHash("def_up_hold");
    public static readonly int Hash_StateTag_def_right_hold = Animator.StringToHash("def_right_hold");
    public static readonly int Hash_StateTag_def_down_hold = Animator.StringToHash("def_down_hold");
    public static readonly int Hash_StateTag_def_left_hold = Animator.StringToHash("def_left_hold");
    // def_blocked
    public static readonly int Hash_StateTag_def_up_blocked = Animator.StringToHash("def_up_blocked");
    public static readonly int Hash_StateTag_def_right_blocked = Animator.StringToHash("def_right_blocked");
    public static readonly int Hash_StateTag_def_down_blocked = Animator.StringToHash("def_down_blocked");
    public static readonly int Hash_StateTag_def_left_blocked = Animator.StringToHash("def_left_blocked");
    #endregion

    #region Animator transitions
    // AttackAndBlockLayer Transition custom names
    // Attack
    // idle_to_atk_hold
    public static readonly int Hash_TransName_idle_to_atk_up_hold = Animator.StringToHash("idle_to_atk_up_hold");
    public static readonly int Hash_TransName_idle_to_atk_right_hold = Animator.StringToHash("idle_to_atk_right_hold");
    public static readonly int Hash_TransName_idle_to_atk_down_hold = Animator.StringToHash("idle_to_atk_down_hold");
    public static readonly int Hash_TransName_idle_to_atk_left_hold = Animator.StringToHash("idle_to_atk_left_hold");
    // atk_hold_to_release
    public static readonly int Hash_TransName_atk_up_hold_to_release = Animator.StringToHash("atk_up_hold_to_release");
    public static readonly int Hash_TransName_atk_right_hold_to_release = Animator.StringToHash("atk_right_hold_to_release");
    public static readonly int Hash_TransName_atk_down_hold_to_release = Animator.StringToHash("atk_down_hold_to_release");
    public static readonly int Hash_TransName_atk_left_hold_to_release = Animator.StringToHash("atk_left_hold_to_release");
    // atk_release_to_bounce
    public static readonly int Hash_TransName_atk_up_release_to_bounce = Animator.StringToHash("atk_up_release_to_bounce");
    public static readonly int Hash_TransName_atk_right_release_to_bounce = Animator.StringToHash("atk_right_release_to_bounce");
    public static readonly int Hash_TransName_atk_down_release_to_bounce = Animator.StringToHash("atk_down_release_to_bounce");
    public static readonly int Hash_TransName_atk_left_release_to_bounce = Animator.StringToHash("atk_left_release_to_bounce");
    // atk_release_to_idle
    public static readonly int Hash_TransName_atk_up_release_to_idle = Animator.StringToHash("atk_up_release_to_idle");
    public static readonly int Hash_TransName_atk_right_release_to_idle = Animator.StringToHash("atk_right_release_to_idle");
    public static readonly int Hash_TransName_atk_down_release_to_idle = Animator.StringToHash("atk_down_release_to_idle");
    public static readonly int Hash_TransName_atk_left_release_to_idle = Animator.StringToHash("atk_left_release_to_idle");
    // atk_bounce_to_idle
    public static readonly int Hash_TransName_atk_up_bounce_to_idle = Animator.StringToHash("atk_up_bounce_to_idle");
    public static readonly int Hash_TransName_atk_right_bounce_to_idle = Animator.StringToHash("atk_right_bounce_to_idle");
    public static readonly int Hash_TransName_atk_down_bounce_to_idle = Animator.StringToHash("atk_down_bounce_to_idle");
    public static readonly int Hash_TransName_atk_left_bounce_to_idle = Animator.StringToHash("atk_left_bounce_to_idle");
    // Ok... I wasn't expecting this nonsense.
    // When leaving a state via a transition, Mecanim still considers you to be in the source state.
    // Hence, we have to account for each transition below...
    // atk_up_hold_to_def_hold
    public static readonly int Hash_TransName_atk_up_hold_to_def_up_hold = Animator.StringToHash("atk_up_hold_to_def_up_hold");
    public static readonly int Hash_TransName_atk_up_hold_to_def_right_hold = Animator.StringToHash("atk_up_hold_to_def_right_hold");
    public static readonly int Hash_TransName_atk_up_hold_to_def_down_hold = Animator.StringToHash("atk_up_hold_to_def_down_hold");
    public static readonly int Hash_TransName_atk_up_hold_to_def_left_hold = Animator.StringToHash("atk_up_hold_to_def_left_hold");
    // atk_right_hold_to_def_hold
    public static readonly int Hash_TransName_atk_right_hold_to_def_up_hold = Animator.StringToHash("atk_right_hold_to_def_up_hold");
    public static readonly int Hash_TransName_atk_right_hold_to_def_right_hold = Animator.StringToHash("atk_right_hold_to_def_right_hold");
    public static readonly int Hash_TransName_atk_right_hold_to_def_down_hold = Animator.StringToHash("atk_right_hold_to_def_down_hold");
    public static readonly int Hash_TransName_atk_right_hold_to_def_left_hold = Animator.StringToHash("atk_right_hold_to_def_left_hold");
    // atk_down_hold_to_def_hold
    public static readonly int Hash_TransName_atk_down_hold_to_def_up_hold = Animator.StringToHash("atk_down_hold_to_def_up_hold");
    public static readonly int Hash_TransName_atk_down_hold_to_def_right_hold = Animator.StringToHash("atk_down_hold_to_def_right_hold");
    public static readonly int Hash_TransName_atk_down_hold_to_def_down_hold = Animator.StringToHash("atk_down_hold_to_def_down_hold");
    public static readonly int Hash_TransName_atk_down_hold_to_def_left_hold = Animator.StringToHash("atk_down_hold_to_def_left_hold");
    // atk_left_hold_to_def_hold
    public static readonly int Hash_TransName_atk_left_hold_to_def_up_hold = Animator.StringToHash("atk_left_hold_to_def_up_hold");
    public static readonly int Hash_TransName_atk_left_hold_to_def_right_hold = Animator.StringToHash("atk_left_hold_to_def_right_hold");
    public static readonly int Hash_TransName_atk_left_hold_to_def_down_hold = Animator.StringToHash("atk_left_hold_to_def_down_hold");
    public static readonly int Hash_TransName_atk_left_hold_to_def_left_hold = Animator.StringToHash("atk_left_hold_to_def_left_hold");
    // atk_up_release_to_def_hold
    public static readonly int Hash_TransName_atk_up_release_to_def_up_hold = Animator.StringToHash("atk_up_release_to_def_up_hold");
    public static readonly int Hash_TransName_atk_up_release_to_def_right_hold = Animator.StringToHash("atk_up_release_to_def_right_hold");
    public static readonly int Hash_TransName_atk_up_release_to_def_down_hold = Animator.StringToHash("atk_up_release_to_def_down_hold");
    public static readonly int Hash_TransName_atk_up_release_to_def_left_hold = Animator.StringToHash("atk_up_release_to_def_left_hold");
    // atk_right_release_to_def_hold
    public static readonly int Hash_TransName_atk_right_release_to_def_up_hold = Animator.StringToHash("atk_right_release_to_def_up_hold");
    public static readonly int Hash_TransName_atk_right_release_to_def_right_hold = Animator.StringToHash("atk_right_release_to_def_right_hold");
    public static readonly int Hash_TransName_atk_right_release_to_def_down_hold = Animator.StringToHash("atk_right_release_to_def_down_hold");
    public static readonly int Hash_TransName_atk_right_release_to_def_left_hold = Animator.StringToHash("atk_right_release_to_def_left_hold");
    // atk_down_release_to_def_hold
    public static readonly int Hash_TransName_atk_down_release_to_def_up_hold = Animator.StringToHash("atk_down_release_to_def_up_hold");
    public static readonly int Hash_TransName_atk_down_release_to_def_right_hold = Animator.StringToHash("atk_down_release_to_def_right_hold");
    public static readonly int Hash_TransName_atk_down_release_to_def_down_hold = Animator.StringToHash("atk_down_release_to_def_down_hold");
    public static readonly int Hash_TransName_atk_down_release_to_def_left_hold = Animator.StringToHash("atk_down_release_to_def_left_hold");
    // atk_left_release_to_def_hold
    public static readonly int Hash_TransName_atk_left_release_to_def_up_hold = Animator.StringToHash("atk_left_release_to_def_up_hold");
    public static readonly int Hash_TransName_atk_left_release_to_def_right_hold = Animator.StringToHash("atk_left_release_to_def_right_hold");
    public static readonly int Hash_TransName_atk_left_release_to_def_down_hold = Animator.StringToHash("atk_left_release_to_def_down_hold");
    public static readonly int Hash_TransName_atk_left_release_to_def_left_hold = Animator.StringToHash("atk_left_release_to_def_left_hold");
    // Defend
    // idle_to_def_hold
    public static readonly int Hash_TransName_idle_to_def_up_hold = Animator.StringToHash("idle_to_def_up_hold");
    public static readonly int Hash_TransName_idle_to_def_right_hold = Animator.StringToHash("idle_to_def_right_hold");
    public static readonly int Hash_TransName_idle_to_def_down_hold = Animator.StringToHash("idle_to_def_down_hold");
    public static readonly int Hash_TransName_idle_to_def_left_hold = Animator.StringToHash("idle_to_def_left_hold");
    // def_hold_to_blocked
    public static readonly int Hash_TransName_def_up_hold_to_blocked = Animator.StringToHash("def_up_hold_to_blocked");
    public static readonly int Hash_TransName_def_right_hold_to_blocked = Animator.StringToHash("def_right_hold_to_blocked");
    public static readonly int Hash_TransName_def_down_hold_to_blocked = Animator.StringToHash("def_down_hold_to_blocked");
    public static readonly int Hash_TransName_def_left_hold_to_blocked = Animator.StringToHash("def_left_hold_to_blocked");
    // def_blocked_to_hold
    public static readonly int Hash_TransName_def_up_blocked_to_hold = Animator.StringToHash("def_up_blocked_to_hold");
    public static readonly int Hash_TransName_def_right_blocked_to_hold = Animator.StringToHash("def_right_blocked_to_hold");
    public static readonly int Hash_TransName_def_down_blocked_to_hold = Animator.StringToHash("def_down_blocked_to_hold");
    public static readonly int Hash_TransName_def_left_blocked_to_hold = Animator.StringToHash("def_left_blocked_to_hold");
    // def_up_hold_to_atk_hold
    public static readonly int Hash_TransName_def_up_hold_to_atk_up_hold = Animator.StringToHash("def_up_hold_to_atk_up_hold");
    public static readonly int Hash_TransName_def_up_hold_to_atk_right_hold = Animator.StringToHash("def_up_hold_to_atk_right_hold");
    public static readonly int Hash_TransName_def_up_hold_to_atk_down_hold = Animator.StringToHash("def_up_hold_to_atk_down_hold");
    public static readonly int Hash_TransName_def_up_hold_to_atk_left_hold = Animator.StringToHash("def_up_hold_to_atk_left_hold");
    // def_right_hold_to_atk_hold
    public static readonly int Hash_TransName_def_right_hold_to_atk_up_hold = Animator.StringToHash("def_right_hold_to_atk_up_hold");
    public static readonly int Hash_TransName_def_right_hold_to_atk_right_hold = Animator.StringToHash("def_right_hold_to_atk_right_hold");
    public static readonly int Hash_TransName_def_right_hold_to_atk_down_hold = Animator.StringToHash("def_right_hold_to_atk_down_hold");
    public static readonly int Hash_TransName_def_right_hold_to_atk_left_hold = Animator.StringToHash("def_right_hold_to_atk_left_hold");
    // def_down_hold_to_atk_hold
    public static readonly int Hash_TransName_def_down_hold_to_atk_up_hold = Animator.StringToHash("def_down_hold_to_atk_up_hold");
    public static readonly int Hash_TransName_def_down_hold_to_atk_right_hold = Animator.StringToHash("def_down_hold_to_atk_right_hold");
    public static readonly int Hash_TransName_def_down_hold_to_atk_down_hold = Animator.StringToHash("def_down_hold_to_atk_down_hold");
    public static readonly int Hash_TransName_def_down_hold_to_atk_left_hold = Animator.StringToHash("def_down_hold_to_atk_left_hold");
    // def_left_hold_to_atk_hold
    public static readonly int Hash_TransName_def_left_hold_to_atk_up_hold = Animator.StringToHash("def_left_hold_to_atk_up_hold");
    public static readonly int Hash_TransName_def_left_hold_to_atk_right_hold = Animator.StringToHash("def_left_hold_to_atk_right_hold");
    public static readonly int Hash_TransName_def_left_hold_to_atk_down_hold = Animator.StringToHash("def_left_hold_to_atk_down_hold");
    public static readonly int Hash_TransName_def_left_hold_to_atk_left_hold = Animator.StringToHash("def_left_hold_to_atk_left_hold");
    // def_up_hold_to_other_def_hold --- new stuff
    public static readonly int Hash_TransName_def_up_hold_to_def_right_hold = Animator.StringToHash("def_up_hold_to_def_right_hold");
    public static readonly int Hash_TransName_def_up_hold_to_def_down_hold = Animator.StringToHash("def_up_hold_to_def_down_hold");
    public static readonly int Hash_TransName_def_up_hold_to_def_left_hold = Animator.StringToHash("def_up_hold_to_def_left_hold");
    // def_right_hold_to_other_def_hold
    public static readonly int Hash_TransName_def_right_hold_to_def_up_hold = Animator.StringToHash("def_right_hold_to_def_up_hold");
    public static readonly int Hash_TransName_def_right_hold_to_def_down_hold = Animator.StringToHash("def_right_hold_to_def_down_hold");
    public static readonly int Hash_TransName_def_right_hold_to_def_left_hold = Animator.StringToHash("def_right_hold_to_def_left_hold");
    // def_down_hold_to_other_def_hold
    public static readonly int Hash_TransName_def_down_hold_to_def_up_hold = Animator.StringToHash("def_down_hold_to_def_up_hold");
    public static readonly int Hash_TransName_def_down_hold_to_def_right_hold = Animator.StringToHash("def_down_hold_to_def_right_hold");
    public static readonly int Hash_TransName_def_down_hold_to_def_left_hold = Animator.StringToHash("def_down_hold_to_def_left_hold");
    // def_left_hold_to_other_def_hold
    public static readonly int Hash_TransName_def_left_hold_to_def_up_hold = Animator.StringToHash("def_left_hold_to_def_up_hold");
    public static readonly int Hash_TransName_def_left_hold_to_def_right_hold = Animator.StringToHash("def_left_hold_to_def_right_hold");
    public static readonly int Hash_TransName_def_left_hold_to_def_down_hold = Animator.StringToHash("def_left_hold_to_def_down_hold");
    // def_up_blocked_to_other_def_hold
    public static readonly int Hash_TransName_def_up_blocked_to_def_right_hold = Animator.StringToHash("def_up_blocked_to_def_right_hold");
    public static readonly int Hash_TransName_def_up_blocked_to_def_down_hold = Animator.StringToHash("def_up_blocked_to_def_down_hold");
    public static readonly int Hash_TransName_def_up_blocked_to_def_left_hold = Animator.StringToHash("def_up_blocked_to_def_left_hold");
    // def_right_blocked_to_other_def_hold
    public static readonly int Hash_TransName_def_right_blocked_to_def_up_hold = Animator.StringToHash("def_right_blocked_to_def_up_hold");
    public static readonly int Hash_TransName_def_right_blocked_to_def_down_hold = Animator.StringToHash("def_right_blocked_to_def_down_hold");
    public static readonly int Hash_TransName_def_right_blocked_to_def_left_hold = Animator.StringToHash("def_right_blocked_to_def_left_hold");
    // def_down_blocked_to_other_def_hold
    public static readonly int Hash_TransName_def_down_blocked_to_def_up_hold = Animator.StringToHash("def_down_blocked_to_def_up_hold");
    public static readonly int Hash_TransName_def_down_blocked_to_def_right_hold = Animator.StringToHash("def_down_blocked_to_def_right_hold");
    public static readonly int Hash_TransName_def_down_blocked_to_def_left_hold = Animator.StringToHash("def_down_blocked_to_def_left_hold");
    // def_left_blocked_to_other_def_hold
    public static readonly int Hash_TransName_def_left_blocked_to_def_up_hold = Animator.StringToHash("def_left_blocked_to_def_up_hold");
    public static readonly int Hash_TransName_def_left_blocked_to_def_right_hold = Animator.StringToHash("def_left_blocked_to_def_right_hold");
    public static readonly int Hash_TransName_def_left_blocked_to_def_down_hold = Animator.StringToHash("def_left_blocked_to_def_down_hold");
    // def_hold_to_idle
    public static readonly int Hash_TransName_def_up_hold_to_idle = Animator.StringToHash("def_up_hold_to_idle");
    public static readonly int Hash_TransName_def_right_hold_to_idle = Animator.StringToHash("def_right_hold_to_idle");
    public static readonly int Hash_TransName_def_down_hold_to_idle = Animator.StringToHash("def_down_hold_to_idle");
    public static readonly int Hash_TransName_def_left_hold_to_idle = Animator.StringToHash("def_left_hold_to_idle");
    // def_blocked_to_idle
    public static readonly int Hash_TransName_def_up_blocked_to_idle = Animator.StringToHash("def_up_blocked_to_idle");
    public static readonly int Hash_TransName_def_right_blocked_to_idle = Animator.StringToHash("def_right_blocked_to_idle");
    public static readonly int Hash_TransName_def_down_blocked_to_idle = Animator.StringToHash("def_down_blocked_to_idle");
    public static readonly int Hash_TransName_def_left_blocked_to_idle = Animator.StringToHash("def_left_blocked_to_idle");
    #endregion
}
