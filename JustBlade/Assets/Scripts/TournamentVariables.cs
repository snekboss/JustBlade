using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TournamentVariables
{
    public static int CurrentRoundNumber = 1;
    public static int MaximumRoundNumber = 6;

    public static int MaxNumAgentsInEachTeamMultiplier = 2;
    public static int MaxNumAgentsInEachTeam {
        get 
        {
            if (CurrentRoundNumber == MaximumRoundNumber)
            {
                return 1;
            }

            return (MaximumRoundNumber - CurrentRoundNumber) * MaxNumAgentsInEachTeamMultiplier;
        }
    }

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

    public static bool IsPlayerEliminated = false;

    public static bool IsTournamentEnded { get { return IsPlayerEliminated || CurrentRoundNumber > MaximumRoundNumber; } }
    public static bool IsFinalRound { get { return CurrentRoundNumber == MaximumRoundNumber; } }

    public static int TotalOpponentsBeatenByPlayer;
    public static bool PlayerWasBestedInThisMelee;

    public static readonly int BasicMercenaryHireCost = 500;
    public static readonly int LightMercenaryHireCost = 1000;
    public static readonly int MediumMercenaryHireCost = 2000;
    public static readonly int HeavyMercenaryHireCost = 3000;

    public static readonly int BasicMercenaryUpgradeCost = 500;
    public static readonly int LightMercenaryUpgradeCost = 1000;
    public static readonly int MediumMercenaryUpgradeCost = 1000;

    public static int NumBasicMercenaries;
    public static int NumLightMercenaries;
    public static int NumMediumMercenaries;
    public static int NumHeavyMercenaries;

    public static readonly int MaxNumberOfMercenaries = 10;

    public static int NumTotalMercenaries
    {
        get { return NumBasicMercenaries + NumLightMercenaries + NumMediumMercenaries + NumHeavyMercenaries; }
    }

    public static int PlayerGold = 99999;
    public static int DefaultPlayerGold;

    public static void StartNewTournament()
    {
        IsPlayerEliminated = false;
        PlayerWasBestedInThisMelee = false;
        TotalOpponentsBeatenByPlayer = 0;
        CurrentRoundNumber = 1;
    }

    public static void BuyEquippableItem(EquippableItem item)
    {
        PlayerGold -= item.purchaseCost;
        item.isPurchasedByPlayer = true;
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

    public static void HireBasicMercenary() 
    {
        PlayerGold -= BasicMercenaryHireCost;
        NumBasicMercenaries++;
    }
    public static void HireLightMercenary() 
    {
        PlayerGold -= LightMercenaryHireCost;
        NumLightMercenaries++;
    }
    public static void HireMediumMercenary()
    {
        PlayerGold -= MediumMercenaryHireCost;
        NumMediumMercenaries++;
    }

    public static void HireHeavyMercenary()
    {
        PlayerGold -= HeavyMercenaryHireCost;
        NumHeavyMercenaries++;
    }

    public static void UpgradeBasicMercenary() 
    {
        PlayerGold -= BasicMercenaryUpgradeCost;
        NumBasicMercenaries--;
        NumLightMercenaries++;
    }
    public static void UpgradeLightMercenary() 
    {
        PlayerGold -= LightMercenaryUpgradeCost;
        NumLightMercenaries--;
        NumMediumMercenaries++;
    }
    public static void UpgradeMediumMercenary()
    {
        PlayerGold -= MediumMercenaryUpgradeCost;
        NumMediumMercenaries--;
        NumHeavyMercenaries++;
    }

}
