using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public Transform spineBone;
    public Transform pelvisBone;

    Animator animator;

    static readonly int Hash_moveX = Animator.StringToHash("moveX");
    static readonly int Hash_moveY = Animator.StringToHash("moveY");
    static readonly int Hash_isAtk = Animator.StringToHash("isAtk");
    static readonly int Hash_isDef = Animator.StringToHash("isDef");
    static readonly int Hash_combatDir = Animator.StringToHash("combatDir");
    static readonly int Hash_jump = Animator.StringToHash("jump");
    static readonly int Hash_isGrounded = Animator.StringToHash("isGrounded");

    bool jump = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
        spineBone = this.transform.Find("spine");
    }

    public void UpdateCombatDirection(Agent.CombatDirection combatDir)
    {
        animator.SetInteger(Hash_combatDir, (int)combatDir);
    }

    public void SetJump(bool isJumping)
    {
        jump = isJumping;
    }

    public void UpdateAnimations(float moveX, float moveY, bool isGrounded, bool isAtk, bool isDef)
    {
        animator.SetFloat(Hash_moveX, moveX);
        animator.SetFloat(Hash_moveY, moveY);
        animator.SetBool(Hash_isAtk, isAtk);
        animator.SetBool(Hash_isDef, isDef);
        animator.SetBool(Hash_jump, jump);
        animator.SetBool(Hash_isGrounded, isGrounded);

        // TODO: Put in a function.
        // Reset triggers below.
        jump = false;
    }

    public void LateUpdateAnimations()
    {

    }
}
