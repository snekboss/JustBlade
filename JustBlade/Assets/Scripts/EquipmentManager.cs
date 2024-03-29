using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class which must be attached to the game objects which are also <see cref="Agent"/>s.
/// It governs the equipment of the attached <see cref="Agent"/>.
/// </summary>
public class EquipmentManager : MonoBehaviour
{
    const float DefaultHandArmorMovementSpeedPenalty = 0.01f;
    const float DefaultHeadArmorMovementSpeedPenalty = 0.02f;
    const float DefaultLegArmorMovementSpeedPenalty = 0.03f;
    const float DefaultTorsoArmorMovementSpeedPenalty = 0.04f;
    const float FinalMovementSpeedPenaltyMultiplier = 3.0f;

    const float DefaultMovementSpeedMultiplierFromArmor = 1.6f;

    /// <summary>
    /// A multiplier for the movement speed, determined by worn armor.
    /// </summary>
    public float MovementSpeedMultiplierFromArmor { get; protected set; }

    /// <summary>
    /// The owner <see cref="Agent"/> to which this <see cref="EquipmentManager"/> belongs.
    /// </summary>
    public Agent OwnerAgent { get; private set; }

    /// <summary>
    /// Item bone of the agent which is used to parent the weapon game object, set in the Inspector menu.
    /// </summary>
    public Transform weaponBone;

    Transform[] agentBones; // bones of the skeleton rig of my character model
    SkinnedMeshRenderer agentHeadSMR;
    SkinnedMeshRenderer agentTorsoSMR;
    SkinnedMeshRenderer agentHandsSMR;
    SkinnedMeshRenderer agentLegsSMR;

    // Actual equipment game object references which have been instantiated on the scene are below.
    public Weapon EquippedWeapon { get; private set; }

    /// <summary>
    /// <see cref="Armor.ArmorLevel"/> of the head.
    /// </summary>
    public Armor.ArmorLevel HeadArmorLevel
    {
        get
        {
            if (equippedHeadArmor == null)
            {
                return Armor.ArmorLevel.None;
            }
            else
            {
                return equippedHeadArmor.armorLevel;
            }
        }
    }

    /// <summary>
    /// <see cref="Armor.ArmorLevel"/> of the torso.
    /// </summary>
    public Armor.ArmorLevel TorsoArmorLevel
    {
        get
        {
            if (equippedTorsoArmor == null)
            {
                return Armor.ArmorLevel.None;
            }
            else
            {
                return equippedTorsoArmor.armorLevel;
            }
        }
    }

    /// <summary>
    /// <see cref="Armor.ArmorLevel"/> of the hands.
    /// </summary>
    public Armor.ArmorLevel HandArmorLevel
    {
        get
        {
            if (equippedHandArmor == null)
            {
                return Armor.ArmorLevel.None;
            }
            else
            {
                return equippedHandArmor.armorLevel;
            }
        }
    }

    /// <summary>
    /// <see cref="Armor.ArmorLevel"/> of the legs.
    /// </summary>
    public Armor.ArmorLevel LegArmorLevel
    {
        get
        {
            if (equippedLegArmor == null)
            {
                return Armor.ArmorLevel.None;
            }
            else
            {
                return equippedLegArmor.armorLevel;
            }
        }
    }

    Armor equippedHeadArmor;
    Armor equippedTorsoArmor;
    Armor equippedHandArmor;
    Armor equippedLegArmor;

    /// <summary>
    /// Initializes the <see cref="EquipmentManager"/> for this <see cref="Agent"/>.
    /// Prefabs are used for initialization.
    /// A copy of these prefabs are used to instantiate the actual weapon/armor game objects
    /// which are seen in the game.
    /// </summary>
    /// <param name="weaponPrefab">A weapon prefab game object.</param>
    /// <param name="headArmorPrefab">An armor prefab game object with an <see cref="Armor.ArmorType"/>
    /// of <see cref="Armor.ArmorType.Head"/></param>
    /// <param name="torsoArmorPrefab">An armor prefab game object with an <see cref="Armor.ArmorType"/>
    /// of <see cref="Armor.ArmorType.Torso"/></param>
    /// <param name="handArmorPrefab">An armor prefab game object with an <see cref="Armor.ArmorType"/>
    /// of <see cref="Armor.ArmorType.Hand"/></param>
    /// <param name="legArmorPrefab">An armor prefab game object with an <see cref="Armor.ArmorType"/>
    /// of <see cref="Armor.ArmorType.Leg"/></param>
    public void InitializeEquipmentManager(Weapon weaponPrefab
        , Armor headArmorPrefab
        , Armor torsoArmorPrefab
        , Armor handArmorPrefab
        , Armor legArmorPrefab)
    {
        // Initialize fields
        OwnerAgent = GetComponent<Agent>();

        agentHeadSMR = OwnerAgent.transform.Find(StaticVariables.HumanHeadName).GetComponent<SkinnedMeshRenderer>();
        agentTorsoSMR = OwnerAgent.transform.Find(StaticVariables.HumanTorsoName).GetComponent<SkinnedMeshRenderer>();
        agentHandsSMR = OwnerAgent.transform.Find(StaticVariables.HumanHandsName).GetComponent<SkinnedMeshRenderer>();
        agentLegsSMR = OwnerAgent.transform.Find(StaticVariables.HumanLegsName).GetComponent<SkinnedMeshRenderer>();

        agentBones = agentHeadSMR.bones;

        // Spawn items
        SpawnWeapon(weaponPrefab);

        SpawnHeadArmor(headArmorPrefab);
        SpawnTorsoArmor(torsoArmorPrefab);
        SpawnHandArmor(handArmorPrefab);
        SpawnLegArmor(legArmorPrefab);

        MovementSpeedMultiplierFromArmor =
            CalculateMovementSpeedMultiplier(HeadArmorLevel
            , TorsoArmorLevel
            , HandArmorLevel
            , LegArmorLevel);

        SetSkinnedMeshVisibility();
    }

    /// <summary>
    /// Spawns the equipped weapon based on the given weapon prefab.
    /// Method's argument cannot be null, or it will cause an error.
    /// </summary>
    /// <param name="weaponPrefab">The weapon prefab.</param>
    void SpawnWeapon(Weapon weaponPrefab)
    {
        // Let Unity complain if weaponPrefab is null. Meaning, disallow spawning without weapons.
        EquippedWeapon = Instantiate(weaponPrefab);

        EquippedWeapon.transform.parent = weaponBone.transform;
        EquippedWeapon.transform.localPosition = Vector3.zero;
        EquippedWeapon.transform.localRotation = Quaternion.identity;
        EquippedWeapon.transform.localScale = Vector3.one;

        OwnerAgent.AnimMgr.ReportEquippedWeaponType(EquippedWeapon.weaponType);

        EquippedWeapon.InitializeOwnerAgent(OwnerAgent);
    }

    /// <summary>
    /// Spawns the equipped head armor based on the given head armor prefab.
    /// If the prefab reference is null, then nothing is equipped, and the corresponding body part of the agent remains naked.
    /// </summary>
    /// <param name="headArmorPrefab">The head armor prefab.</param>
    void SpawnHeadArmor(Armor headArmorPrefab)
    {
        if (headArmorPrefab != null && headArmorPrefab.armorType == Armor.ArmorType.Head)
        {
            equippedHeadArmor = Instantiate(headArmorPrefab);

            equippedHeadArmor.transform.parent = OwnerAgent.transform;
            equippedHeadArmor.transform.localPosition = Vector3.zero;
            equippedHeadArmor.transform.localRotation = Quaternion.identity;
            equippedHeadArmor.transform.localScale = Vector3.one;

            equippedHeadArmor.skinnedMeshRenderer.bones = agentBones;

            if (equippedHeadArmor.coversTheEntireBodyPart)
            {
                agentHeadSMR.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Spawns the equipped torso armor based on the given torso armor prefab.
    /// If the prefab reference is null, then nothing is equipped, and the corresponding body part of the agent remains naked.
    /// </summary>
    /// <param name="headArmorPrefab">The torso armor prefab.</param>
    void SpawnTorsoArmor(Armor torsoArmorPrefab)
    {
        if (torsoArmorPrefab != null && torsoArmorPrefab.armorType == Armor.ArmorType.Torso)
        {
            equippedTorsoArmor = Instantiate(torsoArmorPrefab);

            equippedTorsoArmor.transform.parent = OwnerAgent.transform;
            equippedTorsoArmor.transform.localPosition = Vector3.zero;
            equippedTorsoArmor.transform.localRotation = Quaternion.identity;
            equippedTorsoArmor.transform.localScale = Vector3.one;

            equippedTorsoArmor.skinnedMeshRenderer.bones = agentBones;

            if (equippedTorsoArmor.coversTheEntireBodyPart)
            {
                agentTorsoSMR.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Spawns the equipped hand armor based on the given hand armor prefab.
    /// If the prefab reference is null, then nothing is equipped, and the corresponding body part of the agent remains naked.
    /// </summary>
    /// <param name="headArmorPrefab">The hand armor prefab.</param>
    void SpawnHandArmor(Armor handArmorPrefab)
    {
        if (handArmorPrefab != null && handArmorPrefab.armorType == Armor.ArmorType.Hand)
        {
            equippedHandArmor = Instantiate(handArmorPrefab);

            equippedHandArmor.transform.parent = OwnerAgent.transform;
            equippedHandArmor.transform.localPosition = Vector3.zero;
            equippedHandArmor.transform.localRotation = Quaternion.identity;
            equippedHandArmor.transform.localScale = Vector3.one;

            equippedHandArmor.skinnedMeshRenderer.bones = agentBones;

            if (equippedHandArmor.coversTheEntireBodyPart)
            {
                // IMPORTANT: Always allows at least one original SMR to be active in the scene.
                // Because if all of them are turned off, then Unity decides not to play the animations, and the character remains in T-pose.
                // We'll allow hands to be always visible, since clipping issues won't be too conspicuous.

                // agentHandsSMR.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Spawns the equipped leg armor based on the given leg armor prefab.
    /// If the prefab reference is null, then nothing is equipped, and the corresponding body part of the agent remains naked.
    /// </summary>
    /// <param name="headArmorPrefab">The leg armor prefab.</param>
    void SpawnLegArmor(Armor legArmorPrefab)
    {
        if (legArmorPrefab != null && legArmorPrefab.armorType == Armor.ArmorType.Leg)
        {
            equippedLegArmor = Instantiate(legArmorPrefab);

            equippedLegArmor.transform.parent = OwnerAgent.transform;
            equippedLegArmor.transform.localPosition = Vector3.zero;
            equippedLegArmor.transform.localRotation = Quaternion.identity;
            equippedLegArmor.transform.localScale = Vector3.one;

            equippedLegArmor.skinnedMeshRenderer.bones = agentBones;

            if (equippedLegArmor.coversTheEntireBodyPart)
            {
                agentLegsSMR.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Sets the visibility of the helmet (and the head) meshes.
    /// Mainly used for the <see cref="PlayerAgent"/>'s character when the camera is in first person view mode.
    /// See also: <see cref="CameraManager"/>.
    /// See also: <see cref="StaticVariables.IsCameraModeFirstPerson"/>.
    /// </summary>
    /// <param name="isVisible"></param>
    public void ToggleHelmetVisibility(bool isVisible)
    {
        if (agentHeadSMR == null)
        {
            return;
        }

        if (isVisible)
        {
            agentHeadSMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            if (equippedHeadArmor != null)
            {
                equippedHeadArmor.skinnedMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }
        else
        {
            agentHeadSMR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            if (equippedHeadArmor != null)
            {
                equippedHeadArmor.skinnedMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
        }
    }

    /// <summary>
    /// Sets the <see cref="SkinnedMeshRenderer.updateWhenOffscreen"/> values of all armor,
    /// as well as the body parts of the agent.
    /// This is so that the player is still able to see the animations of his
    /// <see cref="PlayerAgent"/> character in first person view mode.
    /// </summary>
    void SetSkinnedMeshVisibility()
    {
        if (OwnerAgent.IsPlayerAgent)
        {
            agentHeadSMR.updateWhenOffscreen = true;
            agentTorsoSMR.updateWhenOffscreen = true;
            agentHandsSMR.updateWhenOffscreen = true;
            agentLegsSMR.updateWhenOffscreen = true;

            // The question mark syntax thing like equippedHeadArmor?.blah doesn't work... -.-

            if (equippedHeadArmor != null)
            {
                equippedHeadArmor.skinnedMeshRenderer.updateWhenOffscreen = true;
            }

            if (equippedTorsoArmor != null)
            {
                equippedTorsoArmor.skinnedMeshRenderer.updateWhenOffscreen = true;
            }

            if (equippedHandArmor != null)
            {
                equippedHandArmor.skinnedMeshRenderer.updateWhenOffscreen = true;
            }

            if (equippedLegArmor != null)
            {
                equippedLegArmor.skinnedMeshRenderer.updateWhenOffscreen = true;
            }
        }
    }

    /// <summary>
    /// Calculates the movement speed multiplier based on the given armor levels.
    /// The heavier the armor level, the slower the movement speed becomes.
    /// </summary>
    /// <param name="HeadArmorLevel">The head armor level.</param>
    /// <param name="TorsoArmorLevel">The torso armor level.</param>
    /// <param name="HandArmorLevel">The hand armor level.</param>
    /// <param name="LegArmorLevel">The leg armor level.</param>
    /// <returns></returns>
    public static float CalculateMovementSpeedMultiplier(Armor.ArmorLevel HeadArmorLevel
        , Armor.ArmorLevel TorsoArmorLevel
        , Armor.ArmorLevel HandArmorLevel
        , Armor.ArmorLevel LegArmorLevel)
    {
        float headPenalty = (int)(HeadArmorLevel) * DefaultHeadArmorMovementSpeedPenalty;
        float torsoPenalty = (int)(TorsoArmorLevel) * DefaultTorsoArmorMovementSpeedPenalty;
        float handPenalty = (int)(HandArmorLevel) * DefaultHandArmorMovementSpeedPenalty;
        float legPenalty = (int)(LegArmorLevel) * DefaultLegArmorMovementSpeedPenalty;
        float sumPenalty = headPenalty + torsoPenalty + handPenalty + legPenalty;

        float finalPenalty = sumPenalty * FinalMovementSpeedPenaltyMultiplier;

        float movementSpeedMultiplier = DefaultMovementSpeedMultiplierFromArmor - finalPenalty;
        return movementSpeedMultiplier;
    }
}
