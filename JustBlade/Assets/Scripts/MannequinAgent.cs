using UnityEngine;

/// <summary>
/// A class which designates the attached game object as a <see cref="MannequinAgent"/>.
/// MannequinAgents are used only in Gear Selection Menu to show the chosen equipment of the player agent.
/// MannequinAgents are not playable, and their code does not contain any combat related logic.
/// A MannequinAgent also requires:
/// - <see cref="AnimationManager"/>.
/// - <see cref="EquipmentManager"/>.
/// - <see cref="LimbManager"/>.
/// - <see cref="AgentAudioManager"/>.
/// - <see cref="CharacteristicManager"/>.
/// See also: <seealso cref="GearSelectionUI"/>.
/// </summary>
public class MannequinAgent : Agent
{
    bool isFalling = true; // this is just to make the mannequin agent jump in the gear selection menu :D

    /// <summary>
    /// Unity's Start method.
    /// In this case, it is used to make the <see cref="MannequinAgent"/> look at the origin of the world.
    /// </summary>
    void Start()
    {
        Vector3 lookDir = Vector3.zero - transform.position;
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
    }

    /// <summary>
    /// Unity's Update method.
    /// In this case, it updates the animations on the mannequin agent.
    /// The "update" to the animations is rather simple, in the sense that the mannequin agent is just told to stand still.
    /// </summary>
    void Update()
    {
        AnimMgr.UpdateAnimations(Vector2.zero, 0, isFalling, false, false);

        // In the first frame, isFalling is true (because it is initialized as such).
        // After that, it's false. So, make sure this happens after the first invocation of AnimMgr.UpdateAnimations.
        isFalling = false;
    }
}
