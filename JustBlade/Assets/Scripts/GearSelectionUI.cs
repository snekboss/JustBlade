using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GearSelectionUI : MonoBehaviour
{
    public float mannequinAgentSize;

    public Transform mannequinContainer;
    public GameObject mannequinAgentPrefab;

    public TextMeshProUGUI txtSelectedWeapon;
    public TextMeshProUGUI txtSelectedHeadArmor;
    public TextMeshProUGUI txtSelectedTorsoArmor;
    public TextMeshProUGUI txtSelectedHandArmor;
    public TextMeshProUGUI txtSelectedLegArmor;

    public TextMeshProUGUI txtWeaponInfoBody;
    public TextMeshProUGUI txtArmorInfoBody;

    public Button btnNextWeapon;
    public Button btnNextHeadArmor;
    public Button btnNextTorsoArmor;
    public Button btnNextHandArmor;
    public Button btnNextLegArmor;

    public Button btnPrevWeapon;
    public Button btnPrevHeadArmor;
    public Button btnPrevTorsoArmor;
    public Button btnPrevHandArmor;
    public Button btnPrevLegArmor;

    public Button btnFight;

    MannequinAgent mannequinAgent;

    public void OnClick_ButtonNextWeapon()
    {
        TournamentVariables.PlayerChosenWeaponIndex++;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonNextHeadArmor()
    {
        TournamentVariables.PlayerChosenHeadArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonNextTorsoArmor()
    {
        TournamentVariables.PlayerChosenTorsoArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonNextHandArmor()
    {
        TournamentVariables.PlayerChosenHandArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonNextLegArmor()
    {
        TournamentVariables.PlayerChosenLegArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonPrevWeapon()
    {
        TournamentVariables.PlayerChosenWeaponIndex--;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonPrevHeadArmor()
    {
        TournamentVariables.PlayerChosenHeadArmorIndex--;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonPrevTorsoArmor()
    {
        TournamentVariables.PlayerChosenTorsoArmorIndex--;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonPrevHandArmor()
    {
        TournamentVariables.PlayerChosenHandArmorIndex--;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonPrevLegArmor()
    {
        TournamentVariables.PlayerChosenLegArmorIndex--;
        OnMannequinEquipmentChanged();
    }

    public void OnClick_ButtonBack()
    {
        SceneManager.LoadScene("TournamentInfoMenuScene");
    }

    public void OnClick_ButtonFight()
    {
        SceneManager.LoadScene("ArenaScene");
    }

    void Start()
    {
        OnMannequinEquipmentChanged();
    }

    void UpdateTexts()
    {
        Weapon chosenWeapon = PrefabManager.Weapons[TournamentVariables.PlayerChosenWeaponIndex];
        Armor chosenHeadArmor = PrefabManager.HeadArmors[TournamentVariables.PlayerChosenHeadArmorIndex];
        Armor chosenTorsoArmor = PrefabManager.TorsoArmors[TournamentVariables.PlayerChosenTorsoArmorIndex];
        Armor chosenHandArmor = PrefabManager.HandArmors[TournamentVariables.PlayerChosenHandArmorIndex];
        Armor chosenLegArmor = PrefabManager.LegArmors[TournamentVariables.PlayerChosenLegArmorIndex];

        txtSelectedWeapon.text = chosenWeapon.shownName;
        txtSelectedHeadArmor.text = chosenHeadArmor.shownName;
        txtSelectedTorsoArmor.text = chosenTorsoArmor.shownName;
        txtSelectedHandArmor.text = chosenHandArmor.shownName;
        txtSelectedLegArmor.text = chosenLegArmor.shownName;

        /*
        Type: Two Handed
        Length: 132
        Average swing damage: 58
        Average stab damage: 45
         */
        string NL = Environment.NewLine;
        string typeStr = chosenWeapon.weaponType == Weapon.WeaponType.TwoHanded ? "Two handed" : "Polearm";
        int lengthInt = Convert.ToInt32(chosenWeapon.weaponLength * 100);
        txtWeaponInfoBody.text =
            "Type: " + typeStr + NL
            + "Length: " + lengthInt.ToString() + NL
            + "Average swing damage: " + chosenWeapon.AverageSwingDamage.ToString() + NL
            + "Average stab damage: " + chosenWeapon.AverageStabDamage.ToString();

        /*
        Head armor level: Medium
        Torso armor level: Medium
        Hand armor level: Medium
        Leg armor level: Medium
        Movement speed: 160%
         */
        float movSpeedMulti = EquipmentManager.CalculateMovementSpeedMultiplier(chosenHeadArmor.armorLevel
            , chosenTorsoArmor.armorLevel, chosenHandArmor.armorLevel, chosenLegArmor.armorLevel);
        int movSpeedMultiInt = Convert.ToInt32(movSpeedMulti * 100);
        txtArmorInfoBody.text =
            "Head armor level: " + chosenHeadArmor.armorLevel.ToString() + NL
            + "Torso armor level: " + chosenTorsoArmor.armorLevel.ToString() + NL
            + "Hand armor level: " + chosenHandArmor.armorLevel.ToString() + NL
            + "Leg armor level: " + chosenLegArmor.armorLevel.ToString() + NL
            + "Movement speed: " + movSpeedMultiInt + "%";
    }

    void OnMannequinEquipmentChanged()
    {
        UpdateTexts();

        RespawnMannequinAgent();
    }

    void RespawnMannequinAgent()
    {
        if (mannequinAgent != null)
        {
            Destroy(mannequinAgent.gameObject);
            mannequinAgent = null;
        }

        GameObject mannequinGO = Instantiate(mannequinAgentPrefab);
        mannequinAgent = mannequinGO.GetComponent<MannequinAgent>();
        mannequinAgent.transform.parent = mannequinContainer;
        mannequinAgent.transform.localPosition = Vector3.zero;
        mannequinAgent.transform.localScale = Vector3.one * mannequinAgentSize;
    }
}
