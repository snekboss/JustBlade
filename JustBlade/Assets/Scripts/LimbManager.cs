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
    /// <summary>
    /// Root bone of the agent, set in the Inspector menu.
    /// </summary>
    public Transform rootBone;
    /// <summary>
    /// Spine bone of the agent, set in the Inspector menu.
    /// </summary>
    public Transform spineBone;
    /// <summary>
    /// Neck bone of the agent, set in the Inspector menu.
    /// </summary>
    public Transform neckBone;

    // height means "up-down"; width means "left-right"; depth means "forward-backward".
    const float HeadHeight = 0.25f; 
    const float HeadWidth = 0.2f;
    const float HeadDepth = 0.2f;
    const float TorsoWidth = 0.45f;
    const float TorsoDepth = 0.35f;
    const float LegsWidth = 0.45f;
    const float LegsDepth = 0.35f;

    /// <summary>
    /// The <see cref="Agent"/> to which this <see cref="LimbManager"/> belongs.
    /// </summary>
    public Agent OwnerAgent { get; private set; }

    /// <summary>
    /// The head <see cref="Limb"/> game object to which this <see cref="LimbManager"/> manages.
    /// </summary>
    public Limb LimbHead { get; private set; }

    /// <summary>
    /// The torso <see cref="Limb"/> game object to which this <see cref="LimbManager"/> manages.
    /// </summary>
    public Limb LimbTorso { get; private set; }

    /// <summary>
    /// The leg <see cref="Limb"/> game object to which this <see cref="LimbManager"/> manages.
    /// </summary>
    public Limb LimbLegs { get; private set; }

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
        legsLimbGO.layer = StaticVariables.LimbLayer;

        legsLimbGO.transform.parent = rootBone.transform;
        legsLimbGO.transform.localPosition = Vector3.zero;
        legsLimbGO.transform.localRotation  = Quaternion.identity;

        LimbLegs = legsLimbGO.AddComponent<Limb>();
        LimbLegs.limbType = Limb.LimbType.Legs;

        LimbLegs.col = LimbLegs.gameObject.AddComponent<BoxCollider>();

        // rootBone.forward coincides with world's up.
        Vector3 legsDimensions = new Vector3(LegsWidth, LegsDepth, rootToSpineHeight);
        LimbLegs.col.size = legsDimensions;
        LimbLegs.col.center = Vector3.forward * rootToSpineHeight / 2;

        LimbLegs.InitializeOwnerAgent(OwnerAgent);
    }

    /// <summary>
    /// Initializes torso <see cref="Limb"/> by generating its hitbox.
    /// </summary>
    void InitializeTorsoLimb()
    {
        float spineToNeckHeight = neckBone.transform.position.y - spineBone.transform.position.y;

        GameObject torsoLimbGo = new GameObject("TorsoLimb");
        torsoLimbGo.layer = StaticVariables.LimbLayer;

        torsoLimbGo.transform.parent = spineBone.transform;
        torsoLimbGo.transform.localPosition = Vector3.zero;
        torsoLimbGo.transform.localRotation = Quaternion.identity;

        LimbTorso = torsoLimbGo.AddComponent<Limb>();
        LimbTorso.limbType = Limb.LimbType.Torso;
        
        LimbTorso.col = LimbTorso.gameObject.AddComponent<BoxCollider>();

        // spineBone.left coincides with world's up.
        Vector3 torsoDimensions = new Vector3(spineToNeckHeight, TorsoWidth, TorsoDepth);
        LimbTorso.col.size = torsoDimensions;
        LimbTorso.col.center = Vector3.left * spineToNeckHeight / 2;

        LimbTorso.InitializeOwnerAgent(OwnerAgent);
    }

    /// <summary>
    /// Initializes head <see cref="Limb"/> by generating its hitbox.
    /// </summary>
    void InitializeHeadLimb()
    {
        // We call it "head limb", but it's a child of the neckBone.

        GameObject headLimbGo = new GameObject("HeadLimb");
        headLimbGo.layer = StaticVariables.LimbLayer;

        headLimbGo.transform.parent = neckBone.transform;
        headLimbGo.transform.localPosition = Vector3.zero;
        headLimbGo.transform.localRotation = Quaternion.identity;

        LimbHead = headLimbGo.AddComponent<Limb>();
        LimbHead.limbType = Limb.LimbType.Head;

        LimbHead.col = LimbHead.gameObject.AddComponent<BoxCollider>();

        // neckBone.left coincides with world's up.
        Vector3 headDimensions = new Vector3(HeadHeight, HeadWidth, HeadDepth);
        LimbHead.col.size = headDimensions;
        LimbHead.col.center = Vector3.left * HeadHeight / 2;

        LimbHead.InitializeOwnerAgent(OwnerAgent);
    }

    /// <summary>
    /// Unity's Awake method.
    /// In this case, it gets the reference of its <see cref="OwnerAgent"/>, and invokes <see cref="InitializeLimbs"/>.
    /// </summary>
    void Awake()
    {
        OwnerAgent = GetComponent<Agent>();

        InitializeLimbs();
    }
}
