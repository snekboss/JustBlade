using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemShop
{
    // TODO: move below somewhere else (or remove entirely)
    public static int CurrentRoundNumber = 1;
    public static int MaximumRoundNumber = 6;

    public static int MaxNumAgentsInEachTeamMultiplier = 2;
    public static bool IsPlayerEliminated = false;
    public static bool IsTournamentEnded { get { return IsPlayerEliminated || CurrentRoundNumber > MaximumRoundNumber; } }
    public static bool IsFinalRound { get { return CurrentRoundNumber == MaximumRoundNumber; } }

    public static int TotalOpponentsBeatenByPlayer;
    public static bool PlayerWasBestedInThisMelee;
    public static int MaxNumAgentsInEachTeam
    {
        get
        {
            if (CurrentRoundNumber == MaximumRoundNumber)
            {
                return 1;
            }

            return (MaximumRoundNumber - CurrentRoundNumber) * MaxNumAgentsInEachTeamMultiplier;
        }
    }

    public static void StartNewTournament()
    {
        IsPlayerEliminated = false;
        PlayerWasBestedInThisMelee = false;
        TotalOpponentsBeatenByPlayer = 0;
        CurrentRoundNumber = 1;
    }
    // TODO: move above somewhere else (or remove entirely)

    // Item shop stuff below
    static int playerChosenWeaponIndex;
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


    static int playerChosenHeadArmorIndex;
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

    static int playerChosenTorsoArmorIndex;
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

    static int playerChosenHandArmorIndex;
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

    static int playerChosenLegArmorIndex;
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

    public static void BuyEquippableItem(EquippableItem item)
    {
        PlayerGold -= item.purchaseCost;
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
    // Item shop stuff above

    // TODO: Move below to PlayerData or something
    public static int PlayerGold = 10000;
    public static int DefaultPlayerGold;
    // TODO: Move above to PlayerData or something


    // TODO: move below to TroopShop

    public static int GetMercenaryHireCost(Armor.ArmorLevel mercArmorLevel)
    {
        return PrefabManager.MercenaryDataByArmorLevel[mercArmorLevel].hireCost;
    }

    public static int GetMercenaryUpgradeCost(Armor.ArmorLevel mercArmorLevel)
    {
        return PrefabManager.MercenaryDataByArmorLevel[mercArmorLevel].upgradeCost;
    }

    static Dictionary<Armor.ArmorLevel, int> mercCountByArmorLevel;
    static Dictionary<Armor.ArmorLevel, int> MercCountByArmorLevel
    {
        get
        {
            if (mercCountByArmorLevel == null)
            {
                mercCountByArmorLevel = new Dictionary<Armor.ArmorLevel, int>();
                MercCountByArmorLevel.Add(Armor.ArmorLevel.None, 0);
                MercCountByArmorLevel.Add(Armor.ArmorLevel.Light, 0);
                MercCountByArmorLevel.Add(Armor.ArmorLevel.Medium, 0);
                MercCountByArmorLevel.Add(Armor.ArmorLevel.Heavy, 0);
            }

            return mercCountByArmorLevel;
        }
    }

    public static int GetMercenaryCount(Armor.ArmorLevel mercArmorLevel)
    {
        return MercCountByArmorLevel[mercArmorLevel];
    }

    public static void HireMercenary(Armor.ArmorLevel mercArmorLevel)
    {
        MercCountByArmorLevel[mercArmorLevel]++;
        PlayerGold -= PrefabManager.MercenaryDataByArmorLevel[mercArmorLevel].hireCost;
    }

    public static void DisbandMercenary(Armor.ArmorLevel mercArmorLevel)
    {
        if (MercCountByArmorLevel[mercArmorLevel] > 0)
        {
            MercCountByArmorLevel[mercArmorLevel]--;
        }
    }

    public static void UpgradeMercenary(Armor.ArmorLevel mercArmorLevel)
    {
        if (mercArmorLevel == Armor.ArmorLevel.Heavy)
        {
            // Do not upgrade, as Heavy is already the highest upgrade level.
            return;
        }

        int mercToBeUpgradedInt = (int)mercArmorLevel;
        int mercToBeUpgradedToInt = mercToBeUpgradedInt + 1;

        Armor.ArmorLevel mercToBeUpgradedTo = (Armor.ArmorLevel)mercToBeUpgradedToInt;

        DisbandMercenary(mercArmorLevel);

        MercCountByArmorLevel[mercToBeUpgradedTo]++;
        PlayerGold -= PrefabManager.MercenaryDataByArmorLevel[mercToBeUpgradedTo].upgradeCost;
    }

    public static bool CanHireMercenary(Armor.ArmorLevel mercArmorLevel)
    {
        return (PlayerPartyIsFull() == false)
            && (PlayerGold >= PrefabManager.MercenaryDataByArmorLevel[mercArmorLevel].hireCost);
    }

    public static bool CanUpgradeMercenary(Armor.ArmorLevel mercArmorLevel)
    {
        if (mercArmorLevel == Armor.ArmorLevel.Heavy)
        {
            return false; // Heavy is the max upgrade level.
        }

        return (GetMercenaryCount(mercArmorLevel) > 0)
            && (PlayerGold >= PrefabManager.MercenaryDataByArmorLevel[mercArmorLevel].upgradeCost);
    }

    public static void HireBasicMercenary()
    {
        HireMercenary(Armor.ArmorLevel.None);
    }

    public static void HireLightMercenary()
    {
        HireMercenary(Armor.ArmorLevel.Light);
    }

    public static void HireMediumMercenary()
    {
        HireMercenary(Armor.ArmorLevel.Medium);
    }

    public static void HireHeavyMercenary()
    {
        HireMercenary(Armor.ArmorLevel.Heavy);
    }

    public static void UpgradeBasicMercenary()
    {
        UpgradeMercenary(Armor.ArmorLevel.None);
    }

    public static void UpgradeLightMercenary()
    {
        UpgradeMercenary(Armor.ArmorLevel.Light);
    }

    public static void UpgradeMediumMercenary()
    {
        UpgradeMercenary(Armor.ArmorLevel.Medium);
    }

    public static readonly int MaxNumberOfMercenaries = 10;

    public static int NumTotalMercenaries
    {
        get
        {
            return GetMercenaryCount(Armor.ArmorLevel.None)
                + GetMercenaryCount(Armor.ArmorLevel.Light)
                + GetMercenaryCount(Armor.ArmorLevel.Medium)
                + GetMercenaryCount(Armor.ArmorLevel.Heavy);
        }
    }

    public static bool PlayerPartyIsFull() { return NumTotalMercenaries == MaxNumberOfMercenaries; }

    // TODO: Move above to TroopShop
}
