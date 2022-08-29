using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// A script which designates the attached game object as Gear Selection UI.
/// It contains the logic of the controls of the Gear Selection UI.
/// </summary>
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

    /// <summary>
    /// Increments the selected weapon index.
    /// </summary>
    public void OnButtonClick_NextWeapon()
    {
        TournamentVariables.PlayerChosenWeaponIndex++;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Increments the selected head armor index.
    /// </summary>
    public void OnButtonClick_NextHeadArmor()
    {
        TournamentVariables.PlayerChosenHeadArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Increments the selected torso armor index.
    /// </summary>
    public void OnButtonClick_NextTorsoArmor()
    {
        TournamentVariables.PlayerChosenTorsoArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Increments the selected hand armor index.
    /// </summary>
    public void OnButtonClick_NextHandArmor()
    {
        TournamentVariables.PlayerChosenHandArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Increments the selected leg armor index.
    /// </summary>
    public void OnButtonClick_NextLegArmor()
    {
        TournamentVariables.PlayerChosenLegArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Decrements the selected weapon index.
    /// </summary>
    public void OnButtonClick_PrevWeapon()
    {
        TournamentVariables.PlayerChosenWeaponIndex--;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Decrements the selected head armor index.
    /// </summary>
    public void OnButtonClick_PrevHeadArmor()
    {
        TournamentVariables.PlayerChosenHeadArmorIndex--;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Decrements the selected torso armor index.
    /// </summary>
    public void OnButtonClick_PrevTorsoArmor()
    {
        TournamentVariables.PlayerChosenTorsoArmorIndex--;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Decrements the selected hand armor index.
    /// </summary>
    public void OnButtonClick_PrevHandArmor()
    {
        TournamentVariables.PlayerChosenHandArmorIndex--;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Decrements the selected leg armor index.
    /// </summary>
    public void OnButtonClick_PrevLegArmor()
    {
        TournamentVariables.PlayerChosenLegArmorIndex--;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Loads the TournamentInfoMenuScene.
    /// </summary>
    public void OnButtonClick_Back()
    {
        SceneManager.LoadScene("TournamentInfoMenuScene");
    }

    /// <summary>
    /// Loads the arena scene.
    /// </summary>
    public void OnButtonClick_Fight()
    {
        SceneManager.LoadScene("ArenaScene");
    }

    /// <summary>
    /// Unity's Start method.
    /// In this case, it sets the cursor visible and initializes the mannequin agent.
    /// </summary>
    void Start()
    {
        Cursor.visible = true;

        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Updates the text widgets.
    /// </summary>
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
            "Name: " + chosenWeapon.shownName + NL
            + "Type: " + typeStr + NL
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

    /// <summary>
    /// Callback for when the mannequin agent's equipment is changed.
    /// </summary>
    void OnMannequinEquipmentChanged()
    {
        UpdateTexts();

        RespawnMannequinAgent();
    }

    /// <summary>
    /// Destroys the existing mannequin agent (if exists), and respawns it.
    /// </summary>
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
