using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class which must be attached to the game objects which are also <see cref="Agent"/>s.
/// It governs the equipment of the attached <see cref="Agent"/>.
/// </summary>
public class EquipmentManager : MonoBehaviour
{
    static readonly float DefaultHandArmorMovementSpeedPenalty = 0.01f;
    static readonly float DefaultHeadArmorMovementSpeedPenalty = 0.02f;
    static readonly float DefaultLegArmorMovementSpeedPenalty = 0.03f;
    static readonly float DefaultTorsoArmorMovementSpeedPenalty = 0.04f;
    static readonly float FinalMovementSpeedPenaltyMultiplier = 3.0f;

    static readonly float DefaultMovementSpeedMultiplier = 1.6f;

    public Agent ownerAgent { get; private set; }
    public AnimationManager animMgr { get; set; }

    public Transform weaponBone;

    Transform[] agentBones;
    SkinnedMeshRenderer agentHeadSMR;
    SkinnedMeshRenderer agentTorsoSMR;
    SkinnedMeshRenderer agentHandsSMR;
    SkinnedMeshRenderer agentLegsSMR;

    // Actual equipment game object references which have been instantiated on the scene are below.
    public Weapon equippedWeapon;

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
    /// Initializes the fields of the EquipmentManager.
    /// This is done by getting the references to the fields and components.
    /// </summary>
    void Initialize()
    {
        ownerAgent = GetComponent<Agent>();
        animMgr = GetComponent<AnimationManager>();

        agentHeadSMR = ownerAgent.transform.Find(StaticVariables.HumanHeadName).GetComponent<SkinnedMeshRenderer>();
        agentTorsoSMR = ownerAgent.transform.Find(StaticVariables.HumanTorsoName).GetComponent<SkinnedMeshRenderer>();
        agentHandsSMR = ownerAgent.transform.Find(StaticVariables.HumanHandsName).GetComponent<SkinnedMeshRenderer>();
        agentLegsSMR = ownerAgent.transform.Find(StaticVariables.HumanLegsName).GetComponent<SkinnedMeshRenderer>();

        agentBones = agentHeadSMR.bones;
    }

    /// <summary>
    /// Spawns the equipment for this agent, and updates values like movement speed multiplier.
    /// This is done by requesting equipment set from <see cref="Agent.RequestEquipmentSet(out Weapon, out Armor, out Armor, out Armor, out Armor)"/>.
    /// The details are found in the corresponding overridden methods.
    /// </summary>
    void SpawnEquipment()
    {
        Weapon weaponPrefab;
        Armor headArmorPrefab;
        Armor torsoArmorPrefab;
        Armor handArmorPrefab;
        Armor legArmorPrefab;

        ownerAgent.RequestEquipmentSet(out weaponPrefab
            , out headArmorPrefab
            , out torsoArmorPrefab
            , out handArmorPrefab
            , out legArmorPrefab);

        SpawnWeapon(weaponPrefab);
        SpawnHeadArmor(headArmorPrefab);
        SpawnTorsoArmor(torsoArmorPrefab);
        SpawnHandArmor(handArmorPrefab);
        SpawnLegArmor(legArmorPrefab);

        UpdateMovementSpeedMultiplier();
        ownerAgent.OnGearInitialized();

        SetSkinnedMeshVisibility();
    }

    /// <summary>
    /// Spawns the equipped weapon based on the given weapon prefab.
    /// </summary>
    /// <param name="weaponPrefab">The weapon prefab.</param>
    void SpawnWeapon(Weapon weaponPrefab)
    {
        // Let Unity complain if weaponPrefab is null. Meaning, disallow spawning without weapons.
        equippedWeapon = Instantiate(weaponPrefab);

        equippedWeapon.transform.parent = weaponBone.transform;
        equippedWeapon.transform.localPosition = Vector3.zero;
        equippedWeapon.transform.localRotation = Quaternion.identity;

        ownerAgent.AnimMgr.ReportEquippedWeaponType(equippedWeapon.weaponType);

        equippedWeapon.InitializeOwnerAgent(ownerAgent);
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

            equippedHeadArmor.transform.parent = ownerAgent.transform;
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

            equippedTorsoArmor.transform.parent = ownerAgent.transform;
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

            equippedHandArmor.transform.parent = ownerAgent.transform;
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

            equippedLegArmor.transform.parent = ownerAgent.transform;
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
    /// </summary>
    /// <param name="isVisible"></param>
    public void ToggleHelmetVisibility(bool isVisible)
    {
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
    /// Sets the <see cref="SkinnedMeshRenderer.updateWhenOffscreen"/> values of all armor, as well as the body parts of the agent.
    /// This is so that the player is still able to see the animations of his <see cref="PlayerAgent"/> character in first person view mode.
    /// </summary>
    void SetSkinnedMeshVisibility()
    {
        if (ownerAgent.IsPlayerAgent)
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

        float movementSpeedMultiplier = DefaultMovementSpeedMultiplier - finalPenalty;
        return movementSpeedMultiplier;
    }

    /// <summary>
    /// Updates the movement speed of the owner agent based on the equipped armor.
    /// This is done by first calculating the movement speed multiplier, and then calculating the new movement speed limit of the agent.
    /// </summary>
    void UpdateMovementSpeedMultiplier()
    {
        float movementSpeedMultiplier = CalculateMovementSpeedMultiplier(HeadArmorLevel, TorsoArmorLevel, HandArmorLevel, LegArmorLevel);
        float newSpeedLimit = Agent.DefaultMovementSpeedLimit * movementSpeedMultiplier;

        //DEBUG_movSpeedMulti = movementSpeedMultiplier;
        //DEBUG_newSpeedLimit = newSpeedLimit;

        ownerAgent.InitializeMovementSpeedLimit(newSpeedLimit);
    }

    /// <summary>
    /// Unity's Awake method.
    /// In this case, it is used to initialize some fields of the script, and then spawn the equipment of the agent.
    /// </summary>
    void Awake()
    {
        Initialize();

        SpawnEquipment();
    }
}
