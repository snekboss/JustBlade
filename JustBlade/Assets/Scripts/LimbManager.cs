using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class which must be attached to the game objects which are also <see cref="Agent"/>.
/// It governs the <see cref="Limb"/>s of the attached <see cref="Agent"/>.
/// See <see cref="Limb"/> on more info about what they are.
/// </summary>
public class LimbManager : MonoBehaviour
{
    public Transform rootBone;
    public Transform spineBone;
    public Transform neckBone;

    // height means "up-down"; width means "left-right"; depth means "forward-backward".
    const float HeadHeight = 0.25f; 
    const float HeadWidth = 0.2f;
    const float HeadDepth = 0.2f;
    const float TorsoWidth = 0.45f;
    const float TorsoDepth = 0.35f;
    const float LegsWidth = 0.45f;
    const float LegsDepth = 0.35f;

    public Agent ownerAgent { get; private set; }
    public Limb limbHead { get; private set; }
    public Limb limbTorso { get; private set; }
    public Limb limbLegs { get; private set; }

    /// <summary>
    /// Initializes all <see cref="Limb"/>s.
    /// </summary>
    void InitializeLimbs()
    {
        InitializeLegsLimb();
        InitializeTorsoLimb();
        InitializeHeadLimb();
    }

    /// <summary>
    /// Initializes leg <see cref="Limb"/> by generating its hitbox.
    /// </summary>
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

    /// <summary>
    /// Initializes torso <see cref="Limb"/> by generating its hitbox.
    /// </summary>
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

    /// <summary>
    /// Initializes head <see cref="Limb"/> by generating its hitbox.
    /// </summary>
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

    /// <summary>
    /// Unity's Awake method.
    /// In this case, it gets the reference of its <see cref="ownerAgent"/>, and invokes <see cref="InitializeLimbs"/>.
    /// </summary>
    void Awake()
    {
        ownerAgent = GetComponent<Agent>();

        InitializeLimbs();
    }
}
