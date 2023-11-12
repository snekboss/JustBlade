using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A static class which manages player's party of mercenaries.
/// It governs buying and upgrading of troops, as well as getting troop count of each <see cref="Armor.ArmorLevel"/>.
/// Due to time constraints, the player can only choose to buy from a 4 HARDCODED types of mercenaries in his party.
/// The data of these mercenaries are found under the "Resources/MercenaryAgentData" folder.
/// These mercenaries are categorized by their <see cref="Armor.ArmorLevel"/>. For example, a
/// <see cref="Armor.ArmorLevel.Medium"/> mercenary has medium level armor in all slots of equipment, and therefore
/// it is categorized as such. Similarly for the others.
/// The class and its fields are static, because there is only one <see cref="PlayerAgent"/>, and
/// the instance based alternative would have us involve managing game objects from scene to scene, since
/// Unity destroys all contents of an open scene before transitioning to another one.
/// For a game of this size, I think the static class approach is sufficient.
/// </summary>
public static class PlayerPartyManager
{
    /// <summary>
    /// Maximum party size for the player.
    /// The player cannot have mercenaries more than this number.
    /// </summary>
    public const int MaxPartySize = 16;
    const int BasePartySize = 3;
    const int PartySizeIncreasePerWavesBeaten = 1;


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

    /// <summary>
    /// The current party size of the player's party.
    /// </summary>
    public static int PartySize
    {
        get
        {
            int curSize = BasePartySize + HordeGameLogic.NumberOfWavesBeaten * PartySizeIncreasePerWavesBeaten;
            return Mathf.Clamp(curSize, BasePartySize, MaxPartySize);
        }
    }

    /// <summary>
    /// Initializes the player's party.
    /// This involves clearing the player's party from all mercenaries from any other playthrough, and start anew.
    /// </summary>
    public static void InitializePlayerParty()
    {
        MercCountByArmorLevel[Armor.ArmorLevel.None] = 0;
        MercCountByArmorLevel[Armor.ArmorLevel.Light] = 0;
        MercCountByArmorLevel[Armor.ArmorLevel.Medium] = 0;
        MercCountByArmorLevel[Armor.ArmorLevel.Heavy] = 0;
    }

    /// <summary>
    /// The total number of all mercenaries currently in the player's party.
    /// </summary>
    public static int NumMercenariesInParty
    {
        get
        {
            return GetMercenaryCount(Armor.ArmorLevel.None)
                + GetMercenaryCount(Armor.ArmorLevel.Light)
                + GetMercenaryCount(Armor.ArmorLevel.Medium)
                + GetMercenaryCount(Armor.ArmorLevel.Heavy);
        }
    }

    /// <summary>
    /// Gets the hire cost of a mercenary of a particular category (ie, <see cref="Armor.ArmorLevel"/>).
    /// </summary>
    /// <param name="mercArmorLevel">Armor level of the mercenary.</param>
    /// <returns>An integer, hire cost in terms of gold.</returns>
    public static int GetMercenaryHireCost(Armor.ArmorLevel mercArmorLevel)
    {
        return PrefabManager.MercenaryDataByArmorLevel[mercArmorLevel].hireCost;
    }

    /// <summary>
    /// Gets the upgrade cost of a mercenary of a particular category (ie, <see cref="Armor.ArmorLevel"/>).
    /// </summary>
    /// <param name="mercArmorLevel">Armor level of the mercenary.</param>
    /// <returns>An integer, upgrade cost in terms of gold.</returns>
    public static int GetMercenaryUpgradeCost(Armor.ArmorLevel mercArmorLevel)
    {
        return PrefabManager.MercenaryDataByArmorLevel[mercArmorLevel].upgradeCost;
    }

    /// <summary>
    /// Gets the number of mercenaries of a particular category (ie, <see cref="Armor.ArmorLevel"/>)
    /// which are currently in the player's party.
    /// </summary>
    /// <param name="mercArmorLevel">Armor level of the mercenary.</param>
    /// <returns>An integer, the number of mercenaries of such an armor level.</returns>
    public static int GetMercenaryCount(Armor.ArmorLevel mercArmorLevel)
    {
        return MercCountByArmorLevel[mercArmorLevel];
    }

    /// <summary>
    /// Hires a mercenary of a particular category (ie, <see cref="Armor.ArmorLevel"/>).
    /// </summary>
    /// <param name="mercArmorLevel">Armor level of the mercenary.</param>
    public static void HireMercenary(Armor.ArmorLevel mercArmorLevel)
    {
        MercCountByArmorLevel[mercArmorLevel]++;
        PlayerInventoryManager.RemovePlayerGold(PrefabManager.MercenaryDataByArmorLevel[mercArmorLevel].hireCost);

        PlayerStatisticsTracker.NumTotalMercenariesHired++;
    }

    /// <summary>
    /// Kills a mercenary of a particular category (ie, <see cref="Armor.ArmorLevel"/>).
    /// The number of mercenaries of such an armor level in the player's party will be reduced.
    /// It also keeps track of how many of the player's mercenaries have died.
    /// See also: <see cref="PlayerStatisticsTracker"/>.
    /// </summary>
    /// <param name="mercArmorLevel">Armor level of the mercenary.</param>
    public static void KillMercenary(Armor.ArmorLevel mercArmorLevel)
    {
        DisbandMercenary(mercArmorLevel);

        PlayerStatisticsTracker.MercenariesTotalDeathCount++;
    }

    /// <summary>
    /// Disbands a mercenary of a particular category (ie, <see cref="Armor.ArmorLevel"/>).
    /// This is different from killing a mercenary, in the sense that "disbanding" is not considered "killing",
    /// and hence the relevant statistic will not be tracked by this method.
    /// </summary>
    /// <param name="mercArmorLevel">Armor level of the mercenary.</param>
    static void DisbandMercenary(Armor.ArmorLevel mercArmorLevel)
    {
        if (MercCountByArmorLevel[mercArmorLevel] > 0)
        {
            MercCountByArmorLevel[mercArmorLevel]--;
        }
    }

    /// <summary>
    /// Upgrades a mercenary of a particular category (ie, <see cref="Armor.ArmorLevel"/>).
    /// </summary>
    /// <param name="mercArmorLevel">Armor level of the mercenary.</param>
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

    /// <summary>
    /// Checks if a mercenary of a particular category (ie, <see cref="Armor.ArmorLevel"/>) can be hired.
    /// </summary>
    /// <param name="mercArmorLevel">Armor level of the mercenary</param>
    /// <returns>True if the mercenary can be hired; false otherwise.</returns>
    public static bool CanHireMercenary(Armor.ArmorLevel mercArmorLevel)
    {
        return (PlayerPartyIsFull() == false)
            && (PlayerInventoryManager.PlayerGold >= PrefabManager.MercenaryDataByArmorLevel[mercArmorLevel].hireCost);
    }

    /// <summary>
    /// Checks if a mercenary of a particular category (ie, <see cref="Armor.ArmorLevel"/>) can be upgraded.
    /// </summary>
    /// <param name="mercArmorLevel">Armor level of the mercenary</param>
    /// <returns>True if the mercenary can be upgraded; false otherwise.</returns>
    public static bool CanUpgradeMercenary(Armor.ArmorLevel mercArmorLevel)
    {
        if (mercArmorLevel == Armor.ArmorLevel.Heavy)
        {
            return false; // Heavy is the max upgrade level.
        }

        return (GetMercenaryCount(mercArmorLevel) > 0)
            && (PlayerInventoryManager.PlayerGold >= PrefabManager.MercenaryDataByArmorLevel[mercArmorLevel].upgradeCost);
    }

    /// <summary>
    /// True if the player's party is full; false otherwise.
    /// </summary>
    /// <returns></returns>
    public static bool PlayerPartyIsFull() { return NumMercenariesInParty == PartySize; }

    /// <summary>
    /// Hires a basic mercenary (ie, a mercenary of category <see cref="Armor.ArmorLevel.None"/>).
    /// This method is primarily used by <see cref="GearSelectionUI"/>, so that the user interface
    /// need not know the details of such things.
    /// </summary>
    public static void HireBasicMercenary()
    {
        HireMercenary(Armor.ArmorLevel.None);
    }

    /// <summary>
    /// Hires a light mercenary (ie, a mercenary of category <see cref="Armor.ArmorLevel.Light"/>).
    /// This method is primarily used by <see cref="GearSelectionUI"/>, so that the user interface
    /// need not know the details of such things.
    public static void HireLightMercenary()
    {
        HireMercenary(Armor.ArmorLevel.Light);
    }

    /// <summary>
    /// Hires a medium mercenary (ie, a mercenary of category <see cref="Armor.ArmorLevel.Medium"/>).
    /// This method is primarily used by <see cref="GearSelectionUI"/>, so that the user interface
    /// need not know the details of such things.
    public static void HireMediumMercenary()
    {
        HireMercenary(Armor.ArmorLevel.Medium);
    }

    /// <summary>
    /// Hires a heavy mercenary (ie, a mercenary of category <see cref="Armor.ArmorLevel.Heavy"/>).
    /// This method is primarily used by <see cref="GearSelectionUI"/>, so that the user interface
    /// need not know the details of such things.
    public static void HireHeavyMercenary()
    {
        HireMercenary(Armor.ArmorLevel.Heavy);
    }

    /// <summary>
    /// Upgrades a basic mercenary (ie, a mercenary of category <see cref="Armor.ArmorLevel.None"/>).
    /// This method is primarily used by <see cref="GearSelectionUI"/>, so that the user interface
    /// need not know the details of such things.
    public static void UpgradeBasicMercenary()
    {
        UpgradeMercenary(Armor.ArmorLevel.None);
    }

    /// <summary>
    /// Ugrades a light mercenary (ie, a mercenary of category <see cref="Armor.ArmorLevel.Light"/>).
    /// This method is primarily used by <see cref="GearSelectionUI"/>, so that the user interface
    /// need not know the details of such things.
    public static void UpgradeLightMercenary()
    {
        UpgradeMercenary(Armor.ArmorLevel.Light);
    }

    /// <summary>
    /// Ugrades a medium mercenary (ie, a mercenary of category <see cref="Armor.ArmorLevel.Medium"/>).
    /// This method is primarily used by <see cref="GearSelectionUI"/>, so that the user interface
    /// need not know the details of such things.
    public static void UpgradeMediumMercenary()
    {
        UpgradeMercenary(Armor.ArmorLevel.Medium);
    }
}
