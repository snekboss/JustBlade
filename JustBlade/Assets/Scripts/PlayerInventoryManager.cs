using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A static class which manages the player's inventory and shopping.
/// The inventory of the player and the shop are integrated, hence they're in the same class.
/// </summary>
public static class PlayerInventoryManager
{
    public static int DefaultPlayerGold = 1000;
    public static int PlayerGold { get; private set; }
    
    /// <summary>
    /// Adds gold to player. The argument must be a positive value.
    /// </summary>
    /// <param name="amount">Positive gold amount.</param>
    public static void AddPlayerGold(int amount)
    {
        if (amount < 0)
        {
            return;
        }

        PlayerStatisticsTracker.PlayerTotalGoldEarned += amount;
        PlayerGold += amount;

        SoundEffectManager.PlayCoinSound(Camera.main.transform.position);
    }

    /// <summary>
    /// Removes gold from player. The argument must be a positive value.
    /// </summary>
    /// <param name="amount">Positive gold amount.</param>
    public static void RemovePlayerGold(int amount)
    {
        if (amount < 0)
        {
            return;
        }

        PlayerStatisticsTracker.PlayerTotalGoldSpent += amount;
        PlayerGold -= amount;

        SoundEffectManager.PlayCoinSound(Camera.main.transform.position);
    }

    public static void InitializePlayerInventory()
    {
        PlayerGold = DefaultPlayerGold;

        PlayerChosenWeaponIndex = 0;
        PlayerChosenHeadArmorIndex = 0;
        PlayerChosenTorsoArmorIndex = 0;
        PlayerChosenHandArmorIndex = 0;
        PlayerChosenLegArmorIndex = 0;

        ResetPurchaseStatus(PrefabManager.Weapons);
        ResetPurchaseStatus(PrefabManager.HeadArmors);
        ResetPurchaseStatus(PrefabManager.TorsoArmors);
        ResetPurchaseStatus(PrefabManager.HandArmors);
        ResetPurchaseStatus(PrefabManager.LegArmors);
    }

    public static int PlayerChosenWeaponIndex
    {
        get { return playerChosenWeaponIndex; }
        set
        {
            playerChosenWeaponIndex = value;

            // Loop the index around like a circle.
            if (playerChosenWeaponIndex >= PrefabManager.Weapons.Count)
            {
                playerChosenWeaponIndex = 0;
            }

            if (playerChosenWeaponIndex < 0)
            {
                playerChosenWeaponIndex = PrefabManager.Weapons.Count - 1;
            }
        }
    }
    static int playerChosenWeaponIndex;

    public static int PlayerChosenHeadArmorIndex
    {
        get { return playerChosenHeadArmorIndex; }
        set
        {
            playerChosenHeadArmorIndex = value;

            // Loop the index around like a circle.
            if (playerChosenHeadArmorIndex >= PrefabManager.HeadArmors.Count)
            {
                playerChosenHeadArmorIndex = 0;
            }

            if (playerChosenHeadArmorIndex < 0)
            {
                playerChosenHeadArmorIndex = PrefabManager.HeadArmors.Count - 1;
            }
        }
    }
    static int playerChosenHeadArmorIndex;

    public static int PlayerChosenTorsoArmorIndex
    {
        get { return playerChosenTorsoArmorIndex; }
        set
        {
            playerChosenTorsoArmorIndex = value;

            // Loop the index around like a circle.
            if (playerChosenTorsoArmorIndex >= PrefabManager.TorsoArmors.Count)
            {
                playerChosenTorsoArmorIndex = 0;
            }

            if (playerChosenTorsoArmorIndex < 0)
            {
                playerChosenTorsoArmorIndex = PrefabManager.TorsoArmors.Count - 1;
            }
        }
    }
    static int playerChosenTorsoArmorIndex;

    public static int PlayerChosenHandArmorIndex
    {
        get { return playerChosenHandArmorIndex; }
        set
        {
            playerChosenHandArmorIndex = value;

            // Loop the index around like a circle.
            if (playerChosenHandArmorIndex >= PrefabManager.HandArmors.Count)
            {
                playerChosenHandArmorIndex = 0;
            }

            if (playerChosenHandArmorIndex < 0)
            {
                playerChosenHandArmorIndex = PrefabManager.HandArmors.Count - 1;
            }
        }
    }
    static int playerChosenHandArmorIndex;

    public static int PlayerChosenLegArmorIndex
    {
        get { return playerChosenLegArmorIndex; }
        set
        {
            playerChosenLegArmorIndex = value;

            // Loop the index around like a circle.
            if (playerChosenLegArmorIndex >= PrefabManager.LegArmors.Count)
            {
                playerChosenLegArmorIndex = 0;
            }

            if (playerChosenLegArmorIndex < 0)
            {
                playerChosenLegArmorIndex = PrefabManager.LegArmors.Count - 1;
            }
        }
    }
    static int playerChosenLegArmorIndex;

    public static void BuyEquippableItem(EquippableItem item)
    {
        RemovePlayerGold(item.purchaseCost);
        item.SetPurchasedByPlayer(true);
    }
    public static bool CanBuyItem(EquippableItem item)
    {
        return PlayerGold >= item.purchaseCost;
    }
    public static void BuyChosenWeapon()
    {
        BuyEquippableItem(PrefabManager.Weapons[PlayerChosenWeaponIndex]);
    }

    public static void BuyChosenHeadArmor()
    {
        BuyEquippableItem(PrefabManager.HeadArmors[PlayerChosenHeadArmorIndex]);
    }

    public static void BuyChosenTorsoArmor()
    {
        BuyEquippableItem(PrefabManager.TorsoArmors[PlayerChosenTorsoArmorIndex]);
    }

    public static void BuyChosenHandArmor()
    {
        BuyEquippableItem(PrefabManager.HandArmors[PlayerChosenHandArmorIndex]);
    }
    public static void BuyChosenLegArmor()
    {
        BuyEquippableItem(PrefabManager.LegArmors[playerChosenLegArmorIndex]);
    }

    /// <summary>
    /// Resets the purchase status of a collection of <see cref="EquippableItem"/>.
    /// It's mainly meant to be used for resetting the purchase status of the prefabs
    /// in <see cref="PrefabManager"/> when starting a new game.
    /// To make use of built-in covariance support, the argument is an <see cref="IEnumerable{EquippableItem}"/>.
    /// </summary>
    /// <param name="equippableItemCollection">A collection of <see cref="EquippableItem"/> prefabs. </param>
    static void ResetPurchaseStatus(IEnumerable<EquippableItem> equippableItemCollection)
    {
        foreach (var eqItem in equippableItemCollection)
        {
            eqItem.SetPurchasedByPlayer(eqItem.isStarterItem);
        }
    }
}
