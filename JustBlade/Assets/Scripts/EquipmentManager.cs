using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    //public float DEBUG_movSpeedMulti;
    //public float DEBUG_newSpeedLimit;

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
    void Initialize()
    {
        ownerAgent = GetComponent<Agent>();
        animMgr = GetComponent<AnimationManager>();

        agentHeadSMR = ownerAgent.transform.Find(StaticVariables.HumanHeadName).GetComponent<SkinnedMeshRenderer>();
        agentTorsoSMR = ownerAgent.transform.Find(StaticVariables.HumanTorsoName).GetComponent<SkinnedMeshRenderer>();
        agentHandsSMR = ownerAgent.transform.Find(StaticVariables.HumanHandsName).GetComponent<SkinnedMeshRenderer>();
        agentLegsSMR = ownerAgent.transform.Find(StaticVariables.HumanLegsName).GetComponent<SkinnedMeshRenderer>();

        agentBones = agentHeadSMR.bones; // TODO: TEMP?
    }

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
    }

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

    void UpdateMovementSpeedMultiplier()
    {
        float movementSpeedMultiplier = CalculateMovementSpeedMultiplier(HeadArmorLevel, TorsoArmorLevel, HandArmorLevel, LegArmorLevel);
        float newSpeedLimit = Agent.DefaultMovementSpeedLimit * movementSpeedMultiplier;

        //DEBUG_movSpeedMulti = movementSpeedMultiplier;
        //DEBUG_newSpeedLimit = newSpeedLimit;

        ownerAgent.InitializeMovementSpeedLimit(newSpeedLimit);
    }

    void Awake()
    {
        Initialize();

        SpawnEquipment();
    }
}
