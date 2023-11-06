using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentAudioManager : MonoBehaviour
{
    Agent ownerAgent;

    public Transform leftFoot;
    public Transform rightFoot;
    public Transform head;

    #region Footstep related fields
    // Footstep related fields

    const float RaycastLengthGrounded = 0.1f;
    const float RaycastLengthJump = 0.375f;
    const float FootstepMovementSpeedLimit = 0.375f;

    const float FootstepCooldown = 0.375f; // do not play footstep if it's more frequent than this timer

    float leftFootstepTimer;
    float rightFootstepTimer;
    #endregion

    // Grunting related fields
    bool isAttackingPrevFrame;
    const float GruntChance = 0.5f;

    public void InitializeAgentAudioManager()
    {
        ownerAgent = GetComponent<Agent>();
    }

    public void UpdateAudioManager()
    {
        ManageFootsteps();

        ManageGrunting();
    }

    void ManageGrunting()
    {
        if (ownerAgent.AnimMgr.IsAttacking && isAttackingPrevFrame == false)
        {
            float randy = Random.Range(0.0f, 1.0f);
            if (randy < GruntChance)
            {
                PlayGruntSound();
            }

            PlayWeaponWhiffSound();
        }

        isAttackingPrevFrame = ownerAgent.AnimMgr.IsAttacking;
    }

    public void PlayHurtSound()
    {
        if (ownerAgent.IsDead)
        {
            return;
        }

        SoundEffectManager.PlayHurtSound(head.position);
    }

    public void PlayDeathSound()
    {
        SoundEffectManager.PlayDeathSound(head.position);
    }

    void PlayGruntSound()
    {
        SoundEffectManager.PlayGruntSound(head.position);
    }

    void PlayWeaponWhiffSound()
    {
        SoundEffectManager.PlayWhiffSound(ownerAgent.EqMgr.equippedWeapon.transform.position);
    }

    void ManageFootsteps()
    {
        leftFootstepTimer += Time.deltaTime;
        rightFootstepTimer += Time.deltaTime;

        if (IsShouldPlayFootstepSound(true))
        {
            SoundEffectManager.PlayFootstepSound(leftFoot.position);
            leftFootstepTimer = 0f;
        }

        if (IsShouldPlayFootstepSound(false))
        {
            SoundEffectManager.PlayFootstepSound(rightFoot.position);
            rightFootstepTimer = 0f;
        }
    }

    bool IsShouldPlayFootstepSound(bool isLeftFoot)
    {
        LayerMask walkableLayerMask = 1 << StaticVariables.DefaultLayer.value;
        Transform foot = isLeftFoot ? leftFoot : rightFoot;
        float cooldownTimer = isLeftFoot ? leftFootstepTimer : rightFootstepTimer;
        float rayDist = ownerAgent.IsGrounded() ? RaycastLengthGrounded : RaycastLengthJump;

        rayDist *= ownerAgent.CharMgr.AgentSizeMultiplier;

        Ray footRay = new Ray(foot.position, Vector3.down);
        bool rayCastHit = Physics.Raycast(footRay, rayDist, walkableLayerMask);

        bool isMovingFastEnough = ownerAgent.CharMgr.CurrentMovementSpeed > FootstepMovementSpeedLimit;
        if (ownerAgent.IsGrounded() == false)
        {
            // The agent is jumping, therefore it is "moving fast enough".
            isMovingFastEnough = true;
        }

        bool isCooldownOk = cooldownTimer > FootstepCooldown;

        return rayCastHit && isMovingFastEnough && isCooldownOk;
    }
}
