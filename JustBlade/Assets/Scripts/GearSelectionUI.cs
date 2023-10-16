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
    public TextMeshProUGUI txtMercInfoBody;

    public TextMeshProUGUI txtBtnBuyWeapon;
    public TextMeshProUGUI txtBtnBuyHeadArmor;
    public TextMeshProUGUI txtBtnBuyTorsoArmor;
    public TextMeshProUGUI txtBtnBuyHandArmor;
    public TextMeshProUGUI txtBtnBuyLegArmor;

    public TextMeshProUGUI txtBtnHireBasicMerc;
    public TextMeshProUGUI txtBtnHireLightMerc;
    public TextMeshProUGUI txtBtnHireMediumMerc;
    public TextMeshProUGUI txtBtnHireHeavyMerc;

    public TextMeshProUGUI txtBtnUpgradeBasicMerc;
    public TextMeshProUGUI txtBtnUpgradeLightMerc;
    public TextMeshProUGUI txtBtnUpgradeMediumMerc;

    public TextMeshProUGUI txtPlayerGold;

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

    public Button btnBuyWeapon;
    public Button btnBuyHeadArmor;
    public Button btnBuyTorsoArmor;
    public Button btnBuyHandArmor;
    public Button btnBuyLegArmor;

    public Button btnHireBasicMerc;
    public Button btnHireLightMerc;
    public Button btnHireMediumMerc;
    public Button btnHireHeavyMerc;

    public Button btnUpgradeBasicMerc;
    public Button btnUpgradeLightMerc;
    public Button btnUpgradeMediumMerc;

    public Button btnFight;

    bool isInPurchaseConfirmationPhase = false;

    MannequinAgent mannequinAgent;

    Vector3 prevFrameMousePos;

    readonly float MouseMoveEpsilon = 0.0001f;

    readonly string purchaseConfirmText = "Confirm";

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

    void MakeItemPurchase(EquippableItem item)
    {
        TournamentVariables.PlayerGold -= item.purchaseCost;
        item.isPurchasedByPlayer = true;

        isInPurchaseConfirmationPhase = false;

        UpdateTexts();
        ManageButtons();
    }

    public void OnButtonClick_BuyWeapon()
    {
        if (isInPurchaseConfirmationPhase == false)
        {
            isInPurchaseConfirmationPhase = true;

            txtBtnBuyWeapon.text = purchaseConfirmText;
        }
        else
        {
            MakeItemPurchase(PrefabManager.Weapons[TournamentVariables.PlayerChosenWeaponIndex]);
        }
    }

    public void OnButtonClick_BuyHeadArmor()
    {
        if (isInPurchaseConfirmationPhase == false)
        {
            isInPurchaseConfirmationPhase = true;

            txtBtnBuyHeadArmor.text = purchaseConfirmText;
        }
        else
        {
            MakeItemPurchase(PrefabManager.HeadArmors[TournamentVariables.PlayerChosenHeadArmorIndex]);
        }
    }

    public void OnButtonClick_BuyTorsoArmor()
    {
        if (isInPurchaseConfirmationPhase == false)
        {
            isInPurchaseConfirmationPhase = true;

            txtBtnBuyTorsoArmor.text = purchaseConfirmText;
        }
        else
        {
            MakeItemPurchase(PrefabManager.TorsoArmors[TournamentVariables.PlayerChosenTorsoArmorIndex]);
        }
    }

    public void OnButtonClick_BuyHandArmor()
    {
        if (isInPurchaseConfirmationPhase == false)
        {
            isInPurchaseConfirmationPhase = true;

            txtBtnBuyHandArmor.text = purchaseConfirmText;
        }
        else
        {
            MakeItemPurchase(PrefabManager.HandArmors[TournamentVariables.PlayerChosenHandArmorIndex]);
        }
    }

    public void OnButtonClick_BuyLegArmor()
    {
        if (isInPurchaseConfirmationPhase == false)
        {
            isInPurchaseConfirmationPhase = true;

            txtBtnBuyLegArmor.text = purchaseConfirmText;
        }
        else
        {
            MakeItemPurchase(PrefabManager.LegArmors[TournamentVariables.PlayerChosenLegArmorIndex]);
        }
    }

    public void OnButtonClick_HireBasicMercenary()
    {
        if (isInPurchaseConfirmationPhase == false)
        {
            isInPurchaseConfirmationPhase = true;

            txtBtnHireBasicMerc.text = purchaseConfirmText;
        }
        else
        {
            TournamentVariables.PlayerGold -= TournamentVariables.BasicMercenaryHireCost;
            TournamentVariables.NumBasicMercenaries++;
            UpdateTexts();
            ManageButtons();

            isInPurchaseConfirmationPhase = false;
        }
    }

    public void OnButtonClick_HireLightMercenary()
    {
        if (isInPurchaseConfirmationPhase == false)
        {
            isInPurchaseConfirmationPhase = true;

            txtBtnHireLightMerc.text = purchaseConfirmText;
        }
        else
        {
            TournamentVariables.PlayerGold -= TournamentVariables.LightMercenaryHireCost;
            TournamentVariables.NumLightMercenaries++;
            UpdateTexts();
            ManageButtons();

            isInPurchaseConfirmationPhase = false;
        }
    }

    public void OnButtonClick_HireMediumMercenary()
    {
        if (isInPurchaseConfirmationPhase == false)
        {
            isInPurchaseConfirmationPhase = true;

            txtBtnHireMediumMerc.text = purchaseConfirmText;
        }
        else
        {
            TournamentVariables.PlayerGold -= TournamentVariables.MediumMercenaryHireCost;
            TournamentVariables.NumMediumMercenaries++;
            UpdateTexts();
            ManageButtons();

            isInPurchaseConfirmationPhase = false;
        }
    }

    public void OnButtonClick_HireHeavyMercenary()
    {
        if (isInPurchaseConfirmationPhase == false)
        {
            isInPurchaseConfirmationPhase = true;

            txtBtnHireHeavyMerc.text = purchaseConfirmText;
        }
        else
        {
            TournamentVariables.PlayerGold -= TournamentVariables.HeavyMercenaryHireCost;
            TournamentVariables.NumHeavyMercenaries++;
            UpdateTexts();
            ManageButtons();

            isInPurchaseConfirmationPhase = false;
        }
    }

    public void OnButtonClick_UpgradeBasicMercenary()
    {
        if (isInPurchaseConfirmationPhase == false)
        {
            isInPurchaseConfirmationPhase = true;

            txtBtnUpgradeBasicMerc.text = purchaseConfirmText;
        }
        else
        {
            TournamentVariables.PlayerGold -= TournamentVariables.BasicMercenaryUpgradeCost;
            TournamentVariables.NumBasicMercenaries--;
            TournamentVariables.NumLightMercenaries++;
            UpdateTexts();
            ManageButtons();

            isInPurchaseConfirmationPhase = false;
        }
    }

    public void OnButtonClick_UpgradeLightMercenary()
    {
        if (isInPurchaseConfirmationPhase == false)
        {
            isInPurchaseConfirmationPhase = true;

            txtBtnUpgradeLightMerc.text = purchaseConfirmText;
        }
        else
        {
            TournamentVariables.PlayerGold -= TournamentVariables.LightMercenaryUpgradeCost;
            TournamentVariables.NumLightMercenaries--;
            TournamentVariables.NumMediumMercenaries++;
            UpdateTexts();
            ManageButtons();

            isInPurchaseConfirmationPhase = false;
        }
    }

    public void OnButtonClick_UpgradeMediumMercenary()
    {
        if (isInPurchaseConfirmationPhase == false)
        {
            isInPurchaseConfirmationPhase = true;

            txtBtnUpgradeMediumMerc.text = purchaseConfirmText;
        }
        else
        {
            TournamentVariables.PlayerGold -= TournamentVariables.MediumMercenaryUpgradeCost;
            TournamentVariables.NumMediumMercenaries--;
            TournamentVariables.NumHeavyMercenaries++;
            UpdateTexts();
            ManageButtons();

            isInPurchaseConfirmationPhase = false;
        }
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

    void Update()
    {
        Vector3 curFrameMousePos = Input.mousePosition;

        if (isInPurchaseConfirmationPhase == true)
        {
            float dist = Vector3.Distance(prevFrameMousePos, curFrameMousePos);

            if (dist > MouseMoveEpsilon)
            {
                isInPurchaseConfirmationPhase = false;

                UpdateTexts();
            }
        }

        prevFrameMousePos = curFrameMousePos;
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
        //int lengthInt = Convert.ToInt32(chosenWeapon.weaponLength * 100);
        txtWeaponInfoBody.text =
            "Name: " + chosenWeapon.shownName + NL
            + "Type: " + typeStr + NL
            //+ "Length: " + lengthInt.ToString() + NL
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

        /*
        Basic mercenaries: 2
        Light mercenaries: 1
        Medium mercenaries: 2
        Heavy mercenaries: 3
        Total party size: 8/10
         */
        txtMercInfoBody.text =
            "Basic mercenaries: " + TournamentVariables.NumBasicMercenaries.ToString() + NL
            + "Light mercenaries: " + TournamentVariables.NumLightMercenaries.ToString() + NL
            + "Medium mercenaries: " + TournamentVariables.NumMediumMercenaries.ToString() + NL
            + "Medium mercenaries: " + TournamentVariables.NumHeavyMercenaries.ToString() + NL
            + "Total party size: " + TournamentVariables.NumTotalMercenaries.ToString() 
            + "/" + TournamentVariables.MaxNumberOfMercenaries.ToString();

        txtPlayerGold.text = "Gold: " + TournamentVariables.PlayerGold.ToString();

        // Button texts
        string buyString = "Buy for {0} gold";
        string hireString = "Hire for {0} gold";
        string upgradeString = "Upgrade for {0} gold";

        txtBtnBuyWeapon.text = string.Format(buyString, chosenWeapon.purchaseCost);

        txtBtnBuyHeadArmor.text = string.Format(buyString, chosenHeadArmor.purchaseCost);
        txtBtnBuyTorsoArmor.text = string.Format(buyString, chosenTorsoArmor.purchaseCost);
        txtBtnBuyHandArmor.text = string.Format(buyString, chosenHandArmor.purchaseCost);
        txtBtnBuyLegArmor.text = string.Format(buyString, chosenLegArmor.purchaseCost);

        txtBtnHireBasicMerc.text = string.Format(hireString, TournamentVariables.BasicMercenaryHireCost);
        txtBtnHireLightMerc.text = string.Format(hireString, TournamentVariables.LightMercenaryHireCost);
        txtBtnHireMediumMerc.text = string.Format(hireString, TournamentVariables.MediumMercenaryHireCost);
        txtBtnHireHeavyMerc.text = string.Format(hireString, TournamentVariables.HeavyMercenaryHireCost);

        txtBtnUpgradeBasicMerc.text = string.Format(upgradeString, TournamentVariables.BasicMercenaryUpgradeCost);
        txtBtnUpgradeLightMerc.text = string.Format(upgradeString, TournamentVariables.LightMercenaryUpgradeCost);
        txtBtnUpgradeMediumMerc.text = string.Format(upgradeString, TournamentVariables.MediumMercenaryUpgradeCost);
    }

    /// <summary>
    /// Updates the state of buy/hire/upgrade buttons.
    /// </summary>
    void ManageButtons()
    {
        // TODO: 
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
