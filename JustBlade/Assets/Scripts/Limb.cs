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

    public Agent ownerAgent;

    public BoxCollider col;


    Rigidbody rbody;

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
