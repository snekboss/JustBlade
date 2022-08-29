using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A script which designates the attached game object as a Limb.
/// It is mainly used by <see cref="LimbManager"/>.
/// Limbs are detected by <see cref="Weapon"/>s via collision, and are used to inflict damage on <see cref="Agent"/>s.
/// </summary>
public class Limb : MonoBehaviour
{
    public enum LimbType
    {
        Head = 0,
        Torso = 1,
        Legs = 2,
    }

    public LimbType limbType;

    public Agent OwnerAgent { get; private set; }

    public BoxCollider col;

    Rigidbody rbody;

    /// <summary>
    /// Lets this limb object know which agent owns it, by setting the <see cref="OwnerAgent"/> property.
    /// </summary>
    /// <param name="ownerAgent">The agent which owns this limb.</param>
    public void InitializeOwnerAgent(Agent ownerAgent)
    {
        OwnerAgent = ownerAgent;
    }

    /// <summary>
    /// Unity's Awake method.
    /// In this case, it is used to initialize the rigidbody of this game object.
    /// If the rigidbody doesn't exist, a default one will be generated in this method.
    /// </summary>
    void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        if (rbody == null)
        {
            rbody = gameObject.AddComponent<Rigidbody>();
        }

        rbody.isKinematic = true;
        rbody.useGravity = false;
    }
}
