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
    public static int BasicMercenaryHireCost { get { return PrefabManager.BasicMercenaryData.hireCost; } }
    public static int LightMercenaryHireCost { get { return PrefabManager.LightMercenaryData.hireCost; } }
    public static int MediumMercenaryHireCost { get { return PrefabManager.MediumMercenaryData.hireCost; } }
    public static int HeavyMercenaryHireCost { get { return PrefabManager.HeavyMercenaryData.hireCost; } }

    public static int BasicMercenaryUpgradeCost { get { return PrefabManager.BasicMercenaryData.upgradeCost; } }
    public static int LightMercenaryUpgradeCost { get { return PrefabManager.LightMercenaryData.upgradeCost; } }
    public static int MediumMercenaryUpgradeCost { get { return PrefabManager.MediumMercenaryData.upgradeCost; } }

    public static int NumBasicMercenaries { get { return numBasicMercenaries; } }
    static int numBasicMercenaries;
    public static int NumLightMercenaries { get { return numLightMercenaries; } }
    static int numLightMercenaries;
    public static int NumMediumMercenaries { get { return numMediumMercenaries; } }
    static int numMediumMercenaries;
    public static int NumHeavyMercenaries { get { return numHeavyMercenaries; } }
    static int numHeavyMercenaries;

    public static readonly int MaxNumberOfMercenaries = 10;

    public static int NumTotalMercenaries
    {
        get { return NumBasicMercenaries + NumLightMercenaries + NumMediumMercenaries + NumHeavyMercenaries; }
    }

    public static void HireBasicMercenary()
    {
        PlayerGold -= PrefabManager.BasicMercenaryData.hireCost;
        numBasicMercenaries++;
    }
    public static void HireLightMercenary()
    {
        PlayerGold -= PrefabManager.LightMercenaryData.hireCost;
        numLightMercenaries++;
    }
    public static void HireMediumMercenary()
    {
        PlayerGold -= PrefabManager.MediumMercenaryData.hireCost;
        numMediumMercenaries++;
    }

    public static void HireHeavyMercenary()
    {
        PlayerGold -= PrefabManager.HeavyMercenaryData.hireCost;
        numHeavyMercenaries++;
    }

    public static void UpgradeBasicMercenary()
    {
        PlayerGold -= PrefabManager.BasicMercenaryData.upgradeCost;
        numBasicMercenaries--;
        numLightMercenaries++;
    }
    public static void UpgradeLightMercenary()
    {
        PlayerGold -= PrefabManager.LightMercenaryData.upgradeCost;
        numLightMercenaries--;
        numMediumMercenaries++;
    }
    public static void UpgradeMediumMercenary()
    {
        PlayerGold -= PrefabManager.MediumMercenaryData.upgradeCost;
        numMediumMercenaries--;
        numHeavyMercenaries++;
    }
    public static void DisbandBasicMercenary()
    {
        if (numBasicMercenaries > 0)
        {
            numBasicMercenaries--;
        }
    }
    public static void DisbandLightMercenary()
    {
        if (numLightMercenaries > 0)
        {
            numLightMercenaries--;
        }
    }
    public static void DisbandMediumMercenary()
    {
        if (numMediumMercenaries > 0)
        {
            numMediumMercenaries--;
        }
    }
    public static void DisbandHeavyMercenary()
    {
        if (numHeavyMercenaries > 0)
        {
            numHeavyMercenaries--;
        }
    }

    public static bool PlayerPartyIsFull() { return NumTotalMercenaries == MaxNumberOfMercenaries; }

    public static bool CanHireBasicMercenary()
    {
        return !PlayerPartyIsFull() || PlayerGold >= BasicMercenaryHireCost;
    }

    public static bool CanHireLightMercenary()
    {
        return !PlayerPartyIsFull() || PlayerGold >= LightMercenaryHireCost;
    }
    public static bool CanHireMediumMercenary()
    {
        return !PlayerPartyIsFull() || PlayerGold >= MediumMercenaryHireCost;
    }
    public static bool CanHireHeavyMercenary()
    {
        return !PlayerPartyIsFull() || PlayerGold >= HeavyMercenaryHireCost;
    }

    public static bool CanUpgradeBasicMercenary()
    {
        return (NumBasicMercenaries > 0) && (PlayerGold >= BasicMercenaryUpgradeCost);
    }

    public static bool CanUpgradeLightMercenary()
    {
        return (NumLightMercenaries > 0) && (PlayerGold >= LightMercenaryUpgradeCost);
    }

    public static bool CanUpgradeMediumMercenary()
    {
        return (NumMediumMercenaries > 0) && (PlayerGold >= MediumMercenaryHireCost);
    }

    // TODO: Move above to TroopShop
}
