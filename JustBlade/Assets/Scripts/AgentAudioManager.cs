using UnityEngine;


/// <summary>
/// This script manages the audio that should be played for the attached <see cref="Agent"/>.
/// It manages and plays sound effects such as footsteps, grunting sounds, etc.
/// This script doesn't have an Update method of its own. Its <see cref="UpdateAudioManager"/> method
/// should be invoked from the Update method of the agent class to which it belongs.
/// </summary>
public class AgentAudioManager : MonoBehaviour
{
    Agent ownerAgent;

    /// <summary>
    /// Left foot bone, set in the Inspector.
    /// </summary>
    public Transform leftFoot;
    /// <summary>
    /// Right foot bone, set in the Inspector.
    /// </summary>
    public Transform rightFoot;
    /// <summary>
    /// Head bone, set in the Inspector.
    /// </summary>
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

    /// <summary>
    /// Initializes the audio manager for this <see cref="Agent"/>.
    /// It mostly just gets a reference to its owner agent.
    /// </summary>
    public void InitializeAgentAudioManager()
    {
        ownerAgent = GetComponent<Agent>();
    }

    /// <summary>
    /// This method should be invoked in the Update method of the <see cref="Agent"/> to which
    /// this <see cref="AgentAudioManager"/> belongs.
    /// It will manage and play footstep sounds, grunting, etc.
    /// </summary>
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

    /// <summary>
    /// Plays a hurt sound effect. It should be used when an <see cref="Agent"/> is hurt.
    /// The sound is not played if the agent is dead.
    /// </summary>
    public void PlayHurtSound()
    {
        if (ownerAgent.IsDead)
        {
            return;
        }

        SoundEffectManager.PlayHurtSound(head.position);
    }

    /// <summary>
    /// Plays the death sound effect. It should be used when an <see cref="Agent"/> is dead.
    /// </summary>
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
        SoundEffectManager.PlayWhiffSound(ownerAgent.EqMgr.EquippedWeapon.transform.position);
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

    /// <summary>
    /// Play a footstep sound if the foot touches the ground.
    /// This check is done via raycasting.
    /// Also, the agent should be moving (fast enough), and the footstep timer should
    /// be exceeded to prevent spammy footstep sounds.
    /// </summary>
    /// <param name="isLeftFoot">True to check for left foot; false for right foot.</param>
    /// <returns></returns>
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
