using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// A script which designates the attached game object as Gear Selection UI.
/// It contains the logic of the controls of the Gear Selection UI.
/// Since this is a UI script, many of the fields are meant to be set using Unity's Inspector menu.
/// For this reason, these fields were not given commented documentation, as there are many of them.
/// Some of public methods may also not have been given commented documentation, as they're just
/// callbacks for the UI widgets.
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
    public TextMeshProUGUI txtFightWarning;

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
        PlayButtonSound();
        PlayerInventoryManager.PlayerChosenWeaponIndex++;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Increments the selected head armor index.
    /// </summary>
    public void OnButtonClick_NextHeadArmor()
    {
        PlayButtonSound();
        PlayerInventoryManager.PlayerChosenHeadArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Increments the selected torso armor index.
    /// </summary>
    public void OnButtonClick_NextTorsoArmor()
    {
        PlayButtonSound();
        PlayerInventoryManager.PlayerChosenTorsoArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Increments the selected hand armor index.
    /// </summary>
    public void OnButtonClick_NextHandArmor()
    {
        PlayButtonSound();
        PlayerInventoryManager.PlayerChosenHandArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Increments the selected leg armor index.
    /// </summary>
    public void OnButtonClick_NextLegArmor()
    {
        PlayButtonSound();
        PlayerInventoryManager.PlayerChosenLegArmorIndex++;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Decrements the selected weapon index.
    /// </summary>
    public void OnButtonClick_PrevWeapon()
    {
        PlayButtonSound();
        PlayerInventoryManager.PlayerChosenWeaponIndex--;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Decrements the selected head armor index.
    /// </summary>
    public void OnButtonClick_PrevHeadArmor()
    {
        PlayButtonSound();
        PlayerInventoryManager.PlayerChosenHeadArmorIndex--;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Decrements the selected torso armor index.
    /// </summary>
    public void OnButtonClick_PrevTorsoArmor()
    {
        PlayButtonSound();
        PlayerInventoryManager.PlayerChosenTorsoArmorIndex--;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Decrements the selected hand armor index.
    /// </summary>
    public void OnButtonClick_PrevHandArmor()
    {
        PlayButtonSound();
        PlayerInventoryManager.PlayerChosenHandArmorIndex--;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Decrements the selected leg armor index.
    /// </summary>
    public void OnButtonClick_PrevLegArmor()
    {
        PlayButtonSound();
        PlayerInventoryManager.PlayerChosenLegArmorIndex--;
        OnMannequinEquipmentChanged();
    }

    /// <summary>
    /// Loads the InformationMenuScene.
    /// </summary>
    public void OnButtonClick_Back()
    {
        PlayButtonSound();
        SceneManager.LoadScene("InformationMenuScene");
    }

    /// <summary>
    /// Loads the arena scene.
    /// </summary>
    public void OnButtonClick_Fight()
    {
        PlayButtonSound();
        SceneManager.LoadScene("ArenaScene");
    }

    /// <summary>
    /// A method used by buttons which are related to shopping.
    /// For these buttons, the first click will not perform the purchase action.
    /// Instead, it'll show a confirmation text.
    /// If the mouse is moved, the button is reverted to its original state.
    /// If the mouse is clicked again without being moved, the purchase action is completed.
    /// This was done to avoid purchasing things by mistake.
    /// </summary>
    /// <param name="textToConfirm">The UI text widget where the confirmation text is shown.</param>
    /// <param name="purchaseAction">Purchase action. For example,
    /// <see cref="PlayerInventoryManager.BuyChosenWeapon"/>.</param>
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

    /// <summary>
    /// Buys selected weapon.
    /// </summary>
    public void OnButtonClick_BuyWeapon()
    {
        PlayButtonSound();
        ConfirmAndPurchase(txtBtnBuyWeapon, PlayerInventoryManager.BuyChosenWeapon);
    }

    /// <summary>
    /// Buys selected head armor.
    /// </summary>
    public void OnButtonClick_BuyHeadArmor()
    {
        PlayButtonSound();
        ConfirmAndPurchase(txtBtnBuyWeapon, PlayerInventoryManager.BuyChosenHeadArmor);
    }

    /// <summary>
    /// Buys selected torso armor.
    /// </summary>
    public void OnButtonClick_BuyTorsoArmor()
    {
        PlayButtonSound();
        ConfirmAndPurchase(txtBtnBuyTorsoArmor, PlayerInventoryManager.BuyChosenTorsoArmor);
    }

    /// <summary>
    /// Buys selected hand armor.
    /// </summary>
    public void OnButtonClick_BuyHandArmor()
    {
        PlayButtonSound();
        ConfirmAndPurchase(txtBtnBuyHandArmor, PlayerInventoryManager.BuyChosenHandArmor);
    }

    /// <summary>
    /// Buys selected leg armor.
    /// </summary>
    public void OnButtonClick_BuyLegArmor()
    {
        PlayButtonSound();
        ConfirmAndPurchase(txtBtnBuyLegArmor, PlayerInventoryManager.BuyChosenLegArmor);
    }

    /// <summary>
    /// Hires basic mercenary.
    /// </summary>
    public void OnButtonClick_HireBasicMercenary()
    {
        PlayButtonSound();
        ConfirmAndPurchase(txtBtnHireBasicMerc, PlayerPartyManager.HireBasicMercenary);
    }

    /// <summary>
    /// Hires light mercenary.
    /// </summary>
    public void OnButtonClick_HireLightMercenary()
    {
        PlayButtonSound();
        ConfirmAndPurchase(txtBtnHireLightMerc, PlayerPartyManager.HireLightMercenary);
    }

    /// <summary>
    /// Hires medium mercenary.
    /// </summary>
    public void OnButtonClick_HireMediumMercenary()
    {
        PlayButtonSound();
        ConfirmAndPurchase(txtBtnHireMediumMerc, PlayerPartyManager.HireMediumMercenary);
    }

    /// <summary>
    /// Hires heavy mercenary.
    /// </summary>
    public void OnButtonClick_HireHeavyMercenary()
    {
        PlayButtonSound();
        ConfirmAndPurchase(txtBtnHireHeavyMerc, PlayerPartyManager.HireHeavyMercenary);
    }

    /// <summary>
    /// Upgrades a basic mercenary.
    /// </summary>
    public void OnButtonClick_UpgradeBasicMercenary()
    {
        PlayButtonSound();
        ConfirmAndPurchase(txtBtnUpgradeBasicMerc, PlayerPartyManager.UpgradeBasicMercenary);
    }

    /// <summary>
    /// Upgrades a light mercenary.
    /// </summary>
    public void OnButtonClick_UpgradeLightMercenary()
    {
        PlayButtonSound();
        ConfirmAndPurchase(txtBtnUpgradeLightMerc, PlayerPartyManager.UpgradeLightMercenary);
    }

    /// <summary>
    /// Upgrades a medium mercenary.
    /// </summary>
    public void OnButtonClick_UpgradeMediumMercenary()
    {
        PlayButtonSound();
        ConfirmAndPurchase(txtBtnUpgradeMediumMerc, PlayerPartyManager.UpgradeMediumMercenary);
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
    /// Unity's Update method.
    /// It contains logic of the "confirm and purchase" feature.
    /// When purchasing something, we double click (without moving the mouse) to confirm the purchase.
    /// </summary>
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
        Weapon chosenWeapon = PrefabManager.Weapons[PlayerInventoryManager.PlayerChosenWeaponIndex];
        Armor chosenHeadArmor = PrefabManager.HeadArmors[PlayerInventoryManager.PlayerChosenHeadArmorIndex];
        Armor chosenTorsoArmor = PrefabManager.TorsoArmors[PlayerInventoryManager.PlayerChosenTorsoArmorIndex];
        Armor chosenHandArmor = PrefabManager.HandArmors[PlayerInventoryManager.PlayerChosenHandArmorIndex];
        Armor chosenLegArmor = PrefabManager.LegArmors[PlayerInventoryManager.PlayerChosenLegArmorIndex];

        txtSelectedWeapon.text = chosenWeapon.shownName;
        txtSelectedHeadArmor.text = chosenHeadArmor.shownName;
        txtSelectedTorsoArmor.text = chosenTorsoArmor.shownName;
        txtSelectedHandArmor.text = chosenHandArmor.shownName;
        txtSelectedLegArmor.text = chosenLegArmor.shownName;

        bool allItemsArePurchased = chosenWeapon.IsPurchasedByPlayer()
                && chosenHeadArmor.IsPurchasedByPlayer()
                && chosenTorsoArmor.IsPurchasedByPlayer()
                && chosenHandArmor.IsPurchasedByPlayer()
                && chosenLegArmor.IsPurchasedByPlayer();

        txtFightWarning.gameObject.SetActive(allItemsArePurchased == false);

        /*
        Type: Two Handed
        Length: 132
        Average swing damage: 58
        Average stab damage: 45
         */
        string NL = Environment.NewLine;
        //string typeStr = chosenWeapon.weaponType == Weapon.WeaponType.TwoHanded ? "Two handed" : "Polearm";
        int lengthInt = Convert.ToInt32(chosenWeapon.weaponLength * 100);
        txtWeaponInfoBody.text =
            "Name: " + chosenWeapon.shownName + NL
            //+ "Type: " + typeStr + NL
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
            + "Movement speed multiplier: " + movSpeedMultiInt + "%";

        /*
        Basic mercenaries: 2
        Light mercenaries: 1
        Medium mercenaries: 2
        Heavy mercenaries: 3
        Total party size: 8/10
         */
        txtMercInfoBody.text =
            "Basic mercenaries: " + PlayerPartyManager.GetMercenaryCount(Armor.ArmorLevel.None).ToString() + NL
            + "Light mercenaries: " + PlayerPartyManager.GetMercenaryCount(Armor.ArmorLevel.Light).ToString() + NL
            + "Medium mercenaries: " + PlayerPartyManager.GetMercenaryCount(Armor.ArmorLevel.Medium).ToString() + NL
            + "Heavy mercenaries: " + PlayerPartyManager.GetMercenaryCount(Armor.ArmorLevel.Heavy).ToString() + NL
            + "Party size: " + PlayerPartyManager.NumMercenariesInParty.ToString()
            + "/" + PlayerPartyManager.PartySize.ToString();

        txtPlayerGold.text = "Gold: " + PlayerInventoryManager.PlayerGold.ToString();

        // Button texts
        string buyString = "Buy for {0} gold";
        string hireString = "Hire for {0} gold";
        string upgradeString = "Upgrade for {0} gold";

        txtBtnBuyWeapon.text = string.Format(buyString, chosenWeapon.purchaseCost);

        txtBtnBuyHeadArmor.text = string.Format(buyString, chosenHeadArmor.purchaseCost);
        txtBtnBuyTorsoArmor.text = string.Format(buyString, chosenTorsoArmor.purchaseCost);
        txtBtnBuyHandArmor.text = string.Format(buyString, chosenHandArmor.purchaseCost);
        txtBtnBuyLegArmor.text = string.Format(buyString, chosenLegArmor.purchaseCost);

        txtBtnHireBasicMerc.text = string.Format(hireString, PlayerPartyManager.GetMercenaryHireCost(Armor.ArmorLevel.None));
        txtBtnHireLightMerc.text = string.Format(hireString, PlayerPartyManager.GetMercenaryHireCost(Armor.ArmorLevel.Light));
        txtBtnHireMediumMerc.text = string.Format(hireString, PlayerPartyManager.GetMercenaryHireCost(Armor.ArmorLevel.Medium));
        txtBtnHireHeavyMerc.text = string.Format(hireString, PlayerPartyManager.GetMercenaryHireCost(Armor.ArmorLevel.Heavy));

        txtBtnUpgradeBasicMerc.text = string.Format(upgradeString, PlayerPartyManager.GetMercenaryUpgradeCost(Armor.ArmorLevel.None));
        txtBtnUpgradeLightMerc.text = string.Format(upgradeString, PlayerPartyManager.GetMercenaryUpgradeCost(Armor.ArmorLevel.Light));
        txtBtnUpgradeMediumMerc.text = string.Format(upgradeString, PlayerPartyManager.GetMercenaryUpgradeCost(Armor.ArmorLevel.Medium));
    }

    /// <summary>
    /// Updates the state of buy/hire/upgrade buttons.
    /// </summary>
    void ManageButtons()
    {
        Weapon chosenWeapon = PrefabManager.Weapons[PlayerInventoryManager.PlayerChosenWeaponIndex];
        Armor chosenHeadArmor = PrefabManager.HeadArmors[PlayerInventoryManager.PlayerChosenHeadArmorIndex];
        Armor chosenTorsoArmor = PrefabManager.TorsoArmors[PlayerInventoryManager.PlayerChosenTorsoArmorIndex];
        Armor chosenHandArmor = PrefabManager.HandArmors[PlayerInventoryManager.PlayerChosenHandArmorIndex];
        Armor chosenLegArmor = PrefabManager.LegArmors[PlayerInventoryManager.PlayerChosenLegArmorIndex];

        bool allItemsArePurchased = chosenWeapon.IsPurchasedByPlayer()
            && chosenHeadArmor.IsPurchasedByPlayer()
            && chosenTorsoArmor.IsPurchasedByPlayer()
            && chosenHandArmor.IsPurchasedByPlayer()
            && chosenLegArmor.IsPurchasedByPlayer();

        // --- Buttons to gray out ---
        btnFight.interactable = allItemsArePurchased;

        btnBuyWeapon.interactable = PlayerInventoryManager.CanBuyItem(chosenWeapon);
        btnBuyHeadArmor.interactable = PlayerInventoryManager.CanBuyItem(chosenHeadArmor);
        btnBuyTorsoArmor.interactable = PlayerInventoryManager.CanBuyItem(chosenTorsoArmor);
        btnBuyHandArmor.interactable = PlayerInventoryManager.CanBuyItem(chosenHandArmor);
        btnBuyLegArmor.interactable = PlayerInventoryManager.CanBuyItem(chosenLegArmor);

        btnHireBasicMerc.interactable = PlayerPartyManager.CanHireMercenary(Armor.ArmorLevel.None);
        btnHireLightMerc.interactable = PlayerPartyManager.CanHireMercenary(Armor.ArmorLevel.Light);
        btnHireMediumMerc.interactable = PlayerPartyManager.CanHireMercenary(Armor.ArmorLevel.Medium);
        btnHireHeavyMerc.interactable = PlayerPartyManager.CanHireMercenary(Armor.ArmorLevel.Heavy);

        btnUpgradeBasicMerc.interactable = PlayerPartyManager.CanUpgradeMercenary(Armor.ArmorLevel.None);
        btnUpgradeLightMerc.interactable = PlayerPartyManager.CanUpgradeMercenary(Armor.ArmorLevel.Light);
        btnUpgradeMediumMerc.interactable = PlayerPartyManager.CanUpgradeMercenary(Armor.ArmorLevel.Medium);

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
        mannequinAgent.InitializeAgent(
            PrefabManager.Weapons[PlayerInventoryManager.PlayerChosenWeaponIndex]
          , PrefabManager.HeadArmors[PlayerInventoryManager.PlayerChosenHeadArmorIndex]
          , PrefabManager.TorsoArmors[PlayerInventoryManager.PlayerChosenTorsoArmorIndex]
          , PrefabManager.HandArmors[PlayerInventoryManager.PlayerChosenHandArmorIndex]
          , PrefabManager.LegArmors[PlayerInventoryManager.PlayerChosenLegArmorIndex]);

        mannequinAgent.transform.parent = mannequinContainer;
        mannequinAgent.transform.localPosition = Vector3.zero;
        mannequinAgent.transform.localScale = Vector3.one * mannequinAgentSize;
    }

    void PlayButtonSound()
    {
        SoundEffectManager.PlayButtonSound(Camera.main.transform.position);
    }
}
