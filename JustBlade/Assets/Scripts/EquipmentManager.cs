using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
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

    public Armor EquippedHeadArmor { get; private set; }
    public Armor EquippedTorsoArmor { get; private set; }
    public Armor EquippedHandArmor { get; private set; }
    public Armor EquippedLegArmor { get; private set; }

    // TODO: THIS IS TEMPORARY. REMOVE THE BELOW LINES WHEN THERE'S AN IN-GAME MENU TO CHOOSE EQUIPMENT.
    public Weapon TEMP_weaponPrefab;
    public Armor TEMP_headArmorPrefab;
    public Armor TEMP_torsoArmorPrefab;
    public Armor TEMP_handArmorPrefab;
    public Armor TEMP_legArmorPrefab;

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


    public void SpawnEquipment(Weapon weaponPrefab, Armor headArmorPrefab, Armor torsoArmorPrefab, Armor handArmorPrefab, Armor legArmorPrefab)
    {
        SpawnWeapon(weaponPrefab);
        SpawnHeadArmor(headArmorPrefab);
        SpawnTorsoArmor(torsoArmorPrefab);
        SpawnHandArmor(handArmorPrefab);
        SpawnLegArmor(legArmorPrefab);
    }

    void SpawnWeapon(Weapon weaponPrefab)
    {
        // Let Unity complain if weaponPrefab is null. Meaning, disallow spawning without weapons.
        equippedWeapon = Instantiate(weaponPrefab);

        equippedWeapon.transform.parent = weaponBone.transform;
        equippedWeapon.transform.localPosition = Vector3.zero;
        equippedWeapon.transform.localRotation = Quaternion.identity;

        ownerAgent.AnimMgr.ReportEquippedWeaponType(equippedWeapon.weaponType);
    }

    void SpawnHeadArmor(Armor headArmorPrefab)
    {
        if (headArmorPrefab != null && headArmorPrefab.armorType == Armor.ArmorType.Head)
        {
            EquippedHeadArmor = Instantiate(headArmorPrefab);

            EquippedHeadArmor.transform.parent = ownerAgent.transform;
            EquippedHeadArmor.transform.localPosition = Vector3.zero;
            EquippedHeadArmor.transform.localRotation = Quaternion.identity;
            EquippedHeadArmor.transform.localScale = Vector3.one;

            EquippedHeadArmor.skinnedMeshRenderer.bones = agentBones;

            if (EquippedHeadArmor.coversTheEntireBodyPart)
            {
                agentHeadSMR.gameObject.SetActive(false);
            }
        }
    }

    void SpawnTorsoArmor(Armor torsoArmorPrefab)
    {
        if (torsoArmorPrefab != null && torsoArmorPrefab.armorType == Armor.ArmorType.Torso)
        {
            EquippedTorsoArmor = Instantiate(torsoArmorPrefab);

            EquippedTorsoArmor.transform.parent = ownerAgent.transform;
            EquippedTorsoArmor.transform.localPosition = Vector3.zero;
            EquippedTorsoArmor.transform.localRotation = Quaternion.identity;
            EquippedTorsoArmor.transform.localScale = Vector3.one;

            EquippedTorsoArmor.skinnedMeshRenderer.bones = agentBones;

            if (EquippedTorsoArmor.coversTheEntireBodyPart)
            {
                agentTorsoSMR.gameObject.SetActive(false);
            }
        }
    }

    void SpawnHandArmor(Armor handArmorPrefab)
    {
        if (handArmorPrefab != null && handArmorPrefab.armorType == Armor.ArmorType.Hand)
        {
            EquippedHandArmor = Instantiate(handArmorPrefab);

            EquippedHandArmor.transform.parent = ownerAgent.transform;
            EquippedHandArmor.transform.localPosition = Vector3.zero;
            EquippedHandArmor.transform.localRotation = Quaternion.identity;
            EquippedHandArmor.transform.localScale = Vector3.one;

            EquippedHandArmor.skinnedMeshRenderer.bones = agentBones;

            if (EquippedHandArmor.coversTheEntireBodyPart)
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
            EquippedLegArmor = Instantiate(legArmorPrefab);

            EquippedLegArmor.transform.parent = ownerAgent.transform;
            EquippedLegArmor.transform.localPosition = Vector3.zero;
            EquippedLegArmor.transform.localRotation = Quaternion.identity;
            EquippedLegArmor.transform.localScale = Vector3.one;

            EquippedLegArmor.skinnedMeshRenderer.bones = agentBones;

            if (EquippedLegArmor.coversTheEntireBodyPart)
            {
                agentLegsSMR.gameObject.SetActive(false);
            }
        }
    }

    void Awake()
    {
        Initialize();

        // TODO: THIS IS TEMPORARY UNTIL I MAKE AN IN-GAME MENU.
        SpawnEquipment(TEMP_weaponPrefab, TEMP_headArmorPrefab, TEMP_torsoArmorPrefab, TEMP_handArmorPrefab, TEMP_legArmorPrefab);
    }
}
