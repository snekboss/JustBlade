using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbManager : MonoBehaviour
{
    public Transform rootBone;
    public Transform spineBone;
    public Transform neckBone;
    
    // height means "up-down"; width means "left-right"; depth means "forward-backward".
    static readonly float HeadHeight = 0.25f; 
    static readonly float HeadWidth = 0.2f;
    static readonly float HeadDepth = 0.2f;
    static readonly float TorsoWidth = 0.45f;
    static readonly float TorsoDepth = 0.35f;
    static readonly float LegsWidth = 0.45f;
    static readonly float LegsDepth = 0.35f;

    public Agent ownerAgent { get; private set; }

    public Limb limbHead { get; private set; }
    public Limb limbTorso { get; private set; }
    public Limb limbLegs { get; private set; }

    void InitializeLimbs()
    {
        InitializeLegsLimb();
        InitializeTorsoLimb();
        InitializeHeadLimb();
    }

    void InitializeLegsLimb()
    {
        float rootToSpineHeight = spineBone.transform.position.y - rootBone.transform.position.y;

        GameObject legsLimbGO = new GameObject("LegsLimb");
        legsLimbGO.layer = StaticVariables.Instance.LimbLayer;

        legsLimbGO.transform.parent = rootBone.transform;
        legsLimbGO.transform.localPosition = Vector3.zero;
        legsLimbGO.transform.localRotation  = Quaternion.identity;

        limbLegs = legsLimbGO.AddComponent<Limb>();
        limbLegs.limbType = Limb.LimbType.Legs;

        limbLegs.col = limbLegs.gameObject.AddComponent<BoxCollider>();

        // rootBone.forward coincides with world's up.
        Vector3 legsDimensions = new Vector3(LegsWidth, LegsDepth, rootToSpineHeight);
        limbLegs.col.size = legsDimensions;
        limbLegs.col.center = Vector3.forward * rootToSpineHeight / 2;

        limbLegs.InitializeOwnerAgent(ownerAgent);
    }

    void InitializeTorsoLimb()
    {
        float spineToNeckHeight = neckBone.transform.position.y - spineBone.transform.position.y;

        GameObject torsoLimbGo = new GameObject("TorsoLimb");
        torsoLimbGo.layer = StaticVariables.Instance.LimbLayer;

        torsoLimbGo.transform.parent = spineBone.transform;
        torsoLimbGo.transform.localPosition = Vector3.zero;
        torsoLimbGo.transform.localRotation = Quaternion.identity;

        limbTorso = torsoLimbGo.AddComponent<Limb>();
        limbTorso.limbType = Limb.LimbType.Torso;
        
        limbTorso.col = limbTorso.gameObject.AddComponent<BoxCollider>();

        // spineBone.left coincides with world's up.
        Vector3 torsoDimensions = new Vector3(spineToNeckHeight, TorsoWidth, TorsoDepth);
        limbTorso.col.size = torsoDimensions;
        limbTorso.col.center = Vector3.left * spineToNeckHeight / 2;

        limbTorso.InitializeOwnerAgent(ownerAgent);
    }

    void InitializeHeadLimb()
    {
        // We call it "head limb", but it's a child of the neckBone.

        GameObject headLimbGo = new GameObject("HeadLimb");
        headLimbGo.layer = StaticVariables.Instance.LimbLayer;

        headLimbGo.transform.parent = neckBone.transform;
        headLimbGo.transform.localPosition = Vector3.zero;
        headLimbGo.transform.localRotation = Quaternion.identity;

        limbHead = headLimbGo.AddComponent<Limb>();
        limbHead.limbType = Limb.LimbType.Head;

        limbHead.col = limbHead.gameObject.AddComponent<BoxCollider>();

        // neckBone.left coincides with world's up.
        Vector3 headDimensions = new Vector3(HeadHeight, HeadWidth, HeadDepth);
        limbHead.col.size = headDimensions;
        limbHead.col.center = Vector3.left * HeadHeight / 2;

        limbHead.InitializeOwnerAgent(ownerAgent);
    }

    void Awake()
    {
        ownerAgent = GetComponent<Agent>();

        InitializeLimbs();
    }
}
