using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A static class which manages the player's inventory and shopping.
/// The inventory of the player and the shop are integrated, hence they're in the same class.
/// This is because "buying" items is just setting the <see cref="EquippableItem.isPurchasedByPlayer"/> to true.
/// Creating a proper inventory and shop system would have required new user interfaces to be implemented,
/// as they would no longer be integrated to one another.
/// The shopping and management of the inventory is done by simply tracking the indices of the list of prefabs
/// which are in the <see cref="PrefabManager"/>. For example, to find out which <see cref="Weapon"/> the player
/// is currently considering using/buying, use <see cref="PrefabManager.Weapons"/> and <see cref="PlayerChosenWeaponIndex"/>.
/// Similarly for other equipment in the <see cref="PrefabManager"/>.
/// The class and its fields are static, because there is only one <see cref="PlayerAgent"/>, and
/// the instance based alternative would have us involve managing game objects from scene to scene, since
/// Unity destroys all contents of an open scene before transitioning to another one.
/// For a game of this size, I think the static class approach is sufficient.
/// </summary>
public static class PlayerInventoryManager
{
    public static int DefaultPlayerGold = 1000;
    /// <summary>
    /// How much gold the player currently has.
    /// </summary>
    public static int PlayerGold { get; private set; }
    
    /// <summary>
    /// Adds gold to the player. The argument must be a positive value.
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
    /// Removes gold from the player. The argument must be a positive value.
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

    /// <summary>
    /// Initializes the player's inventory.
    /// This involves resetting the player's gold, and "forgetting" anything which the player
    /// has bought in a different playthrough.
    /// </summary>
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

    /// <summary>
    /// Returns the index of which weapon we have "chosen" in the <see cref="PrefabManager.Weapons"/>.
    /// </summary>
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

    /// <summary>
    /// Returns the index of which head armor we have "chosen" in the <see cref="PrefabManager.HeadArmors"/>.
    /// </summary>
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

    /// <summary>
    /// Returns the index of which torso armor we have "chosen" in the <see cref="PrefabManager.TorsoArmors"/>.
    /// </summary>
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

    /// <summary>
    /// Returns the index of which hand armor we have "chosen" in the <see cref="PrefabManager.HandArmors"/>.
    /// </summary>
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

    /// <summary>
    /// Returns the index of which leg armor we have "chosen" in the <see cref="PrefabManager.LegArmors"/>.
    /// </summary>
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

    /// <summary>
    /// Buys an <see cref="EquippableItem"/> by setting its <see cref="EquippableItem.isPurchasedByPlayer"/> to true.
    /// </summary>
    /// <param name="item">Item to be bought.</param>
    public static void BuyEquippableItem(EquippableItem item)
    {
        RemovePlayerGold(item.purchaseCost);
        item.SetPurchasedByPlayer(true);
    }

    /// <summary>
    /// Returns true if the player can afford to buy a particular <see cref="EquippableItem"/>.
    /// </summary>
    /// <param name="item">Item to check if it can be afforded.</param>
    /// <returns>True if the item can be afforded; false otherwise.</returns>
    public static bool CanBuyItem(EquippableItem item)
    {
        return PlayerGold >= item.purchaseCost;
    }

    /// <summary>
    /// Buys the chosen weapon.
    /// </summary>
    public static void BuyChosenWeapon()
    {
        BuyEquippableItem(PrefabManager.Weapons[PlayerChosenWeaponIndex]);
    }

    /// <summary>
    /// Buys the head armor.
    /// </summary>
    public static void BuyChosenHeadArmor()
    {
        BuyEquippableItem(PrefabManager.HeadArmors[PlayerChosenHeadArmorIndex]);
    }

    /// <summary>
    /// Buys the torso armor.
    /// </summary>
    public static void BuyChosenTorsoArmor()
    {
        BuyEquippableItem(PrefabManager.TorsoArmors[PlayerChosenTorsoArmorIndex]);
    }

    /// <summary>
    /// Buys the hand armor.
    /// </summary>
    public static void BuyChosenHandArmor()
    {
        BuyEquippableItem(PrefabManager.HandArmors[PlayerChosenHandArmorIndex]);
    }

    /// <summary>
    /// Buys the leg armor.
    /// </summary>
    public static void BuyChosenLegArmor()
    {
        BuyEquippableItem(PrefabManager.LegArmors[playerChosenLegArmorIndex]);
    }

    /// <summary>
    /// Resets the purchase status of a collection of <see cref="EquippableItem"/>.
    /// It's mainly meant to be used for resetting the purchase status of the prefabs
    /// in <see cref="PrefabManager"/> when starting a new game.
    /// To make use of built-in covariance support of C#, the argument is an <see cref="IEnumerable{EquippableItem}"/>.
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
