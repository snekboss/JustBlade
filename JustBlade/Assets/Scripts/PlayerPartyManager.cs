using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A static class which manages player's party of mercenaries.
/// It governs buying and upgrading of troops, as well as getting troop count of each <see cref="Armor.ArmorLevel"/>.
/// </summary>
public static class PlayerPartyManager
{
    public static readonly int MaxNumberOfMercenaries = 10;

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
    static Dictionary<Armor.ArmorLevel, int> mercCountByArmorLevel;

    public static void InitializePlayerParty()
    {
        MercCountByArmorLevel[Armor.ArmorLevel.None] = 0;
        MercCountByArmorLevel[Armor.ArmorLevel.Light] = 0;
        MercCountByArmorLevel[Armor.ArmorLevel.Medium] = 0;
        MercCountByArmorLevel[Armor.ArmorLevel.Heavy] = 0;
    }

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

    public static int GetMercenaryHireCost(Armor.ArmorLevel mercArmorLevel)
    {
        return PrefabManager.MercenaryDataByArmorLevel[mercArmorLevel].hireCost;
    }

    public static int GetMercenaryUpgradeCost(Armor.ArmorLevel mercArmorLevel)
    {
        return PrefabManager.MercenaryDataByArmorLevel[mercArmorLevel].upgradeCost;
    }

    public static int GetMercenaryCount(Armor.ArmorLevel mercArmorLevel)
    {
        return MercCountByArmorLevel[mercArmorLevel];
    }

    public static void HireMercenary(Armor.ArmorLevel mercArmorLevel)
    {
        MercCountByArmorLevel[mercArmorLevel]++;
        PlayerInventoryManager.RemovePlayerGold(PrefabManager.MercenaryDataByArmorLevel[mercArmorLevel].hireCost);

        PlayerStatisticsTracker.NumTotalMercenariesHired++;
    }

    public static void KillMercenary(Armor.ArmorLevel mercArmorLevel)
    {
        DisbandMercenary(mercArmorLevel);

        PlayerStatisticsTracker.MercenariesTotalDeathCount++;
    }

    static void DisbandMercenary(Armor.ArmorLevel mercArmorLevel)
    {
        if (MercCountByArmorLevel[mercArmorLevel] > 0)
        {
            MercCountByArmorLevel[mercArmorLevel]--;
        }
    }

    public static void UpgradeMercenary(Armor.ArmorLevel mercToBeUpgraded)
    {
        if (mercToBeUpgraded == Armor.ArmorLevel.Heavy)
        {
            // Do not upgrade, as Heavy is already the highest upgrade level.
            return;
        }

        int mercToBeUpgradedInt = (int)mercToBeUpgraded;
        int mercToBeUpgradedToInt = mercToBeUpgradedInt + 1;

        Armor.ArmorLevel mercToBeUpgradedTo = (Armor.ArmorLevel)mercToBeUpgradedToInt;

        DisbandMercenary(mercToBeUpgraded);

        MercCountByArmorLevel[mercToBeUpgradedTo]++;
        PlayerInventoryManager.RemovePlayerGold(PrefabManager.MercenaryDataByArmorLevel[mercToBeUpgraded].upgradeCost);

        PlayerStatisticsTracker.NumTotalMercenaryUpgrades++;
  }

    public static bool CanHireMercenary(Armor.ArmorLevel mercArmorLevel)
    {
        return (PlayerPartyIsFull() == false)
            && (PlayerInventoryManager.PlayerGold >= PrefabManager.MercenaryDataByArmorLevel[mercArmorLevel].hireCost);
    }

    public static bool CanUpgradeMercenary(Armor.ArmorLevel mercArmorLevel)
    {
        if (mercArmorLevel == Armor.ArmorLevel.Heavy)
        {
            return false; // Heavy is the max upgrade level.
        }

        return (GetMercenaryCount(mercArmorLevel) > 0)
            && (PlayerInventoryManager.PlayerGold >= PrefabManager.MercenaryDataByArmorLevel[mercArmorLevel].upgradeCost);
    }

    public static bool PlayerPartyIsFull() { return NumTotalMercenaries == MaxNumberOfMercenaries; }

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
}
