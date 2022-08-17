using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void InitializeOwnerAgent(Agent ownerAgent)
    {
        OwnerAgent = ownerAgent;
    }

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
