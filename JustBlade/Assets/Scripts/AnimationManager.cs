using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public Agent ownerAgent;
    public Transform spineBone;
    public Transform pelvisBone;
    float pelvisToSpineDistance;

    float targetSpineAngle;
    float spineCurAngle;
    float SpineRotationLerpRate = 0.2f;
    public bool spineShouldBeRotated; // TODO: This needs to happen while attacking etc. Write the code for it.

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
        pelvisToSpineDistance = Vector3.Distance(spineBone.position, pelvisBone.position);
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
        // Every update frame, assume that the target is zero degrees.
        // If the spine needs to be rotated, then the targetAngle will be read from the Agent later on.
        targetSpineAngle = 0;

        animator.SetFloat(Hash_moveX, moveX);
        animator.SetFloat(Hash_moveY, moveY);
        animator.SetBool(Hash_isAtk, isAtk);
        animator.SetBool(Hash_isDef, isDef);
        animator.SetBool(Hash_jump, jump);
        animator.SetBool(Hash_isGrounded, isGrounded);

        if (spineShouldBeRotated)
        {
            // This should be called while attacking, etc.
            targetSpineAngle = ownerAgent.lookAngleX;
        }

        // TODO: Put in a function.
        // Reset triggers below, after every "Set" call.
        jump = false;
    }

    public void LateUpdateAnimations()
    {
        ConnectSpineToPelvis();
        RotateSpineByLookDirectionAngleX();
    }

    void ConnectSpineToPelvis()
    {
        Vector3 pelvisToSpineDir = (spineBone.position - pelvisBone.position).normalized;
        Vector3 pelvisToSpineOffset = pelvisToSpineDir * pelvisToSpineDistance;

        spineBone.position = pelvisBone.position + pelvisToSpineOffset;
        Debug.DrawRay(pelvisBone.position, pelvisToSpineOffset, Color.red);
    }
    void RotateSpineByLookDirectionAngleX()
    {
        spineCurAngle = Mathf.LerpAngle(spineCurAngle, targetSpineAngle, SpineRotationLerpRate);

        Transform spineAfterAnim = spineBone.transform;
        spineAfterAnim.RotateAround(spineBone.position, ownerAgent.transform.right, spineCurAngle);
    }
}
