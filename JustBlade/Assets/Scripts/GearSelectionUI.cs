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
        ItemShop.PlayerChosenWeaponIndex++;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Increments the selected head armor index.
    /// </summary>
    public void OnButtonClick_NextHeadArmor()
    {
        ItemShop.PlayerChosenHeadArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Increments the selected torso armor index.
    /// </summary>
    public void OnButtonClick_NextTorsoArmor()
    {
        ItemShop.PlayerChosenTorsoArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Increments the selected hand armor index.
    /// </summary>
    public void OnButtonClick_NextHandArmor()
    {
        ItemShop.PlayerChosenHandArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Increments the selected leg armor index.
    /// </summary>
    public void OnButtonClick_NextLegArmor()
    {
        ItemShop.PlayerChosenLegArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Decrements the selected weapon index.
    /// </summary>
    public void OnButtonClick_PrevWeapon()
    {
        ItemShop.PlayerChosenWeaponIndex--;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Decrements the selected head armor index.
    /// </summary>
    public void OnButtonClick_PrevHeadArmor()
    {
        ItemShop.PlayerChosenHeadArmorIndex--;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Decrements the selected torso armor index.
    /// </summary>
    public void OnButtonClick_PrevTorsoArmor()
    {
        ItemShop.PlayerChosenTorsoArmorIndex--;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Decrements the selected hand armor index.
    /// </summary>
    public void OnButtonClick_PrevHandArmor()
    {
        ItemShop.PlayerChosenHandArmorIndex--;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Decrements the selected leg armor index.
    /// </summary>
    public void OnButtonClick_PrevLegArmor()
    {
        ItemShop.PlayerChosenLegArmorIndex--;
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

    void ConfirmAndPurchase(TextMeshProUGUI textToConfirm, Action purchaseAction)
    {
        if (isInPurchaseConfirmationPhase == false)
        {
            isInPurchaseConfirmationPhase = true;

            textToConfirm.text = purchaseConfirmText;
        }
        else
        {
            purchaseAction();

            UpdateTexts();
            ManageButtons();

            isInPurchaseConfirmationPhase = false;
        }
    }

    public void OnButtonClick_BuyWeapon()
    {
        ConfirmAndPurchase(txtBtnBuyWeapon, ItemShop.BuyChosenWeapon);
    }

    public void OnButtonClick_BuyHeadArmor()
    {
        ConfirmAndPurchase(txtBtnBuyWeapon, ItemShop.BuyChosenHeadArmor);
    }

    public void OnButtonClick_BuyTorsoArmor()
    {
        ConfirmAndPurchase(txtBtnBuyTorsoArmor, ItemShop.BuyChosenTorsoArmor);
    }

    public void OnButtonClick_BuyHandArmor()
    {
        ConfirmAndPurchase(txtBtnBuyHandArmor, ItemShop.BuyChosenHandArmor);
    }

    public void OnButtonClick_BuyLegArmor()
    {
        ConfirmAndPurchase(txtBtnBuyLegArmor, ItemShop.BuyChosenLegArmor);
    }

    public void OnButtonClick_HireBasicMercenary()
    {
        ConfirmAndPurchase(txtBtnHireBasicMerc, ItemShop.HireBasicMercenary);
    }

    public void OnButtonClick_HireLightMercenary()
    {
        ConfirmAndPurchase(txtBtnHireLightMerc, ItemShop.HireLightMercenary);
    }

    public void OnButtonClick_HireMediumMercenary()
    {
        ConfirmAndPurchase(txtBtnHireMediumMerc, ItemShop.HireMediumMercenary);
    }

    public void OnButtonClick_HireHeavyMercenary()
    {
        ConfirmAndPurchase(txtBtnHireHeavyMerc, ItemShop.HireHeavyMercenary);
    }

    public void OnButtonClick_UpgradeBasicMercenary()
    {
        ConfirmAndPurchase(txtBtnUpgradeBasicMerc, ItemShop.UpgradeBasicMercenary);
    }

    public void OnButtonClick_UpgradeLightMercenary()
    {
        ConfirmAndPurchase(txtBtnUpgradeLightMerc, ItemShop.UpgradeLightMercenary);
    }

    public void OnButtonClick_UpgradeMediumMercenary()
    {
        ConfirmAndPurchase(txtBtnUpgradeMediumMerc, ItemShop.UpgradeMediumMercenary);
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
        Weapon chosenWeapon = PrefabManager.Weapons[ItemShop.PlayerChosenWeaponIndex];
        Armor chosenHeadArmor = PrefabManager.HeadArmors[ItemShop.PlayerChosenHeadArmorIndex];
        Armor chosenTorsoArmor = PrefabManager.TorsoArmors[ItemShop.PlayerChosenTorsoArmorIndex];
        Armor chosenHandArmor = PrefabManager.HandArmors[ItemShop.PlayerChosenHandArmorIndex];
        Armor chosenLegArmor = PrefabManager.LegArmors[ItemShop.PlayerChosenLegArmorIndex];

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
            "Basic mercenaries: " + ItemShop.GetMercenaryCount(Armor.ArmorLevel.None).ToString() + NL
            + "Light mercenaries: " + ItemShop.GetMercenaryCount(Armor.ArmorLevel.Light).ToString() + NL
            + "Medium mercenaries: " + ItemShop.GetMercenaryCount(Armor.ArmorLevel.Medium).ToString() + NL
            + "Heavy mercenaries: " + ItemShop.GetMercenaryCount(Armor.ArmorLevel.Heavy).ToString() + NL
            + "Total party size: " + ItemShop.NumTotalMercenaries.ToString() 
            + "/" + ItemShop.MaxNumberOfMercenaries.ToString();

        txtPlayerGold.text = "Gold: " + ItemShop.PlayerGold.ToString();

        // Button texts
        string buyString = "Buy for {0} gold";
        string hireString = "Hire for {0} gold";
        string upgradeString = "Upgrade for {0} gold";

        txtBtnBuyWeapon.text = string.Format(buyString, chosenWeapon.purchaseCost);

        txtBtnBuyHeadArmor.text = string.Format(buyString, chosenHeadArmor.purchaseCost);
        txtBtnBuyTorsoArmor.text = string.Format(buyString, chosenTorsoArmor.purchaseCost);
        txtBtnBuyHandArmor.text = string.Format(buyString, chosenHandArmor.purchaseCost);
        txtBtnBuyLegArmor.text = string.Format(buyString, chosenLegArmor.purchaseCost);

        txtBtnHireBasicMerc.text = string.Format(hireString, ItemShop.GetMercenaryHireCost(Armor.ArmorLevel.None));
        txtBtnHireLightMerc.text = string.Format(hireString, ItemShop.GetMercenaryHireCost(Armor.ArmorLevel.Light));
        txtBtnHireMediumMerc.text = string.Format(hireString, ItemShop.GetMercenaryHireCost(Armor.ArmorLevel.Medium));
        txtBtnHireHeavyMerc.text = string.Format(hireString, ItemShop.GetMercenaryHireCost(Armor.ArmorLevel.Heavy));

        txtBtnUpgradeBasicMerc.text = string.Format(upgradeString, ItemShop.GetMercenaryUpgradeCost(Armor.ArmorLevel.None));
        txtBtnUpgradeLightMerc.text = string.Format(upgradeString, ItemShop.GetMercenaryUpgradeCost(Armor.ArmorLevel.Light));
        txtBtnUpgradeMediumMerc.text = string.Format(upgradeString, ItemShop.GetMercenaryUpgradeCost(Armor.ArmorLevel.Medium));
    }

    /// <summary>
    /// Updates the state of buy/hire/upgrade buttons.
    /// </summary>
    void ManageButtons()
    {
        Weapon chosenWeapon = PrefabManager.Weapons[ItemShop.PlayerChosenWeaponIndex];
        Armor chosenHeadArmor = PrefabManager.HeadArmors[ItemShop.PlayerChosenHeadArmorIndex];
        Armor chosenTorsoArmor = PrefabManager.TorsoArmors[ItemShop.PlayerChosenTorsoArmorIndex];
        Armor chosenHandArmor = PrefabManager.HandArmors[ItemShop.PlayerChosenHandArmorIndex];
        Armor chosenLegArmor = PrefabManager.LegArmors[ItemShop.PlayerChosenLegArmorIndex];

        bool allItemsArePurchased = chosenWeapon.IsPurchasedByPlayer()
            && chosenHeadArmor.IsPurchasedByPlayer()
            && chosenTorsoArmor.IsPurchasedByPlayer()
            && chosenHandArmor.IsPurchasedByPlayer()
            && chosenLegArmor.IsPurchasedByPlayer();

        // --- Buttons to gray out ---
        btnFight.interactable = allItemsArePurchased;

        btnBuyWeapon.interactable = ItemShop.CanBuyItem(chosenWeapon);
        btnBuyHeadArmor.interactable = ItemShop.CanBuyItem(chosenHeadArmor);
        btnBuyTorsoArmor.interactable = ItemShop.CanBuyItem(chosenTorsoArmor);
        btnBuyHandArmor.interactable = ItemShop.CanBuyItem(chosenHandArmor);
        btnBuyLegArmor.interactable = ItemShop.CanBuyItem(chosenLegArmor);

        btnHireBasicMerc.interactable = ItemShop.CanHireMercenary(Armor.ArmorLevel.None);
        btnHireLightMerc.interactable = ItemShop.CanHireMercenary(Armor.ArmorLevel.Light);
        btnHireMediumMerc.interactable = ItemShop.CanHireMercenary(Armor.ArmorLevel.Medium);
        btnHireHeavyMerc.interactable = ItemShop.CanHireMercenary(Armor.ArmorLevel.Heavy);

        btnUpgradeBasicMerc.interactable = ItemShop.CanUpgradeMercenary(Armor.ArmorLevel.None);
        btnUpgradeLightMerc.interactable = ItemShop.CanUpgradeMercenary(Armor.ArmorLevel.Light);
        btnUpgradeMediumMerc.interactable = ItemShop.CanUpgradeMercenary(Armor.ArmorLevel.Medium);

        // --- Buttons to hide (ie, disable) ---
        btnBuyWeapon.gameObject.SetActive(!chosenWeapon.IsPurchasedByPlayer());
        btnBuyHeadArmor.gameObject.SetActive(!chosenHeadArmor.IsPurchasedByPlayer());
        btnBuyTorsoArmor.gameObject.SetActive(!chosenTorsoArmor.IsPurchasedByPlayer());
        btnBuyHandArmor.gameObject.SetActive(!chosenHandArmor.IsPurchasedByPlayer());
        btnBuyLegArmor.gameObject.SetActive(!chosenLegArmor.IsPurchasedByPlayer());
    }

    /// <summary>
    /// Callback for when the mannequin agent's equipment is changed.
    /// </summary>
    void OnMannequinEquipmentChanged()
    {
        UpdateTexts();
        ManageButtons();

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
