using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A static class which holds references to the prefabs under the "Resources" folder of this project.
/// The prefabs are loaded once via lazy initialization, and remain in memory throughout the life cycle of the game application.
/// The prefab references are accessible from anywhere throughout the life cycle of the game.
/// </summary>
public static class PrefabManager
{
    static List<Armor> headArmors;
    public static List<Armor> HeadArmors
    {
        get
        {
            if (headArmors == null)
            {
                headArmors = SortByStarterItemness(LoadArmors("Armors/Head Armors"));
            }

            return headArmors;
        }
    }

    static List<Armor> torsoArmors;
    public static List<Armor> TorsoArmors
    {
        get
        {
            if (torsoArmors == null)
            {
                torsoArmors = SortByStarterItemness(LoadArmors("Armors/Torso Armors"));
            }

            return torsoArmors;
        }
    }

    static List<Armor> handArmors;
    public static List<Armor> HandArmors
    {
        get
        {
            if (handArmors == null)
            {
                handArmors = SortByStarterItemness(LoadArmors("Armors/Hand Armors"));
            }

            return handArmors;
        }
    }

    static List<Armor> legArmors;
    public static List<Armor> LegArmors
    {
        get
        {
            if (legArmors == null)
            {
                legArmors = SortByStarterItemness(LoadArmors("Armors/Leg Armors"));
            }

            return legArmors;
        }
    }

    static List<Weapon> weapons;
    public static List<Weapon> Weapons
    {
        get
        {
            if (weapons == null)
            {
                weapons = SortByStarterItemness(LoadWeapons("Weapons"));
            }

            return weapons;
        }
    }

    static Dictionary<Armor.ArmorLevel, MercenaryAgentData> mercenaryDataByArmorLevel;

    public static Dictionary<Armor.ArmorLevel, MercenaryAgentData> MercenaryDataByArmorLevel
    {
        get
        {
            if (mercenaryDataByArmorLevel == null)
            {
                mercenaryDataByArmorLevel = LoadMercenaryAgentData("MercenaryAgentData");
            }

            return mercenaryDataByArmorLevel;
        }
    }

    /// <summary>
    /// Loads armors based on a given path, under "Resources" folder of this project.
    /// </summary>
    /// <param name="path">The name of a subfolder under "Resources".</param>
    /// <returns>List of <see cref="Armor"/> prefabs.</returns>
    static List<Armor> LoadArmors(string path)
    {
        List<Armor> ret = new List<Armor>();

        UnityEngine.Object[] objs = Resources.LoadAll(path);
        for (int i = 0; i < objs.Length; i++)
        {
            GameObject armorGO = objs[i] as GameObject;
            Armor armor = armorGO.GetComponent<Armor>();
            armor.SetPurchasedByPlayer(armor.isStarterItem);
            ret.Add(armor);
        }

        return ret;
    }

    /// <summary>
    /// Loads weapons based on a given path, under "Resources" folder of this project.
    /// </summary>
    /// <param name="path">The name of a subfolder under "Resources".</param>
    /// <returns>List of <see cref="Weapon"/> prefabs.</returns>
    static List<Weapon> LoadWeapons(string path)
    {
        List<Weapon> ret = new List<Weapon>();

        UnityEngine.Object[] objs = Resources.LoadAll(path);
        for (int i = 0; i < objs.Length; i++)
        {
            GameObject weaponGO = objs[i] as GameObject;
            Weapon weapon = weaponGO.GetComponent<Weapon>();
            weapon.SetPurchasedByPlayer(weapon.isStarterItem);
            ret.Add(weapon);
        }

        return ret;
    }

    /// <summary>
    /// Loads all <see cref="MercenaryAgentData"/> on a given path, under "Resources" folder of this project.
    /// Note that <see cref="MercenaryAgentData"/> related code are hardcoded, and therefore
    /// there must be exactly one <see cref="MercenaryAgentData"/> loaded per <see cref="Armor.ArmorLevel"/>.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    static Dictionary<Armor.ArmorLevel, MercenaryAgentData> LoadMercenaryAgentData(string path)
    {
        UnityEngine.Object[] objs = Resources.LoadAll(path);
        string nameOfArmorLevel = nameof(Armor.ArmorLevel);
        string nameOfMercData = nameof(MercenaryAgentData);

#if UNITY_EDITOR
        Debug.Assert(objs.Length == 4, "There needs to be exactly 4 " + nameOfMercData + " under MercenaryAgentData folder.");
#endif

        Dictionary<Armor.ArmorLevel, MercenaryAgentData> ret = new Dictionary<Armor.ArmorLevel, MercenaryAgentData>();

        for (int i = 0; i < objs.Length; i++)
        {
            GameObject dataGO = objs[i] as GameObject;
            MercenaryAgentData mercData = dataGO.GetComponent<MercenaryAgentData>();

#if UNITY_EDITOR
            Debug.Assert(ret.ContainsKey(mercData.mercArmorLevel) == false, nameOfArmorLevel
                    + " "
                    + mercData.mercArmorLevel.ToString()
                    + " has already been encountered. Please make sure that all "
                    + nameOfMercData + " are loaded exactly once, and correspond to exactly one "
                    + nameOfArmorLevel + ".");
#endif

            ret.Add(mercData.mercArmorLevel, mercData);
        }

        return ret;
    }

    static List<Weapon> SortByStarterItemness(
        IEnumerable<Weapon> weaponCollection)
    {
        return weaponCollection.OrderByDescending(item => item.isStarterItem)
            .ThenBy(item => item.purchaseCost)
            .ToList();
    }

    static List<Armor> SortByStarterItemness(
        IEnumerable<Armor> armorCollection)
    {
        return armorCollection.OrderByDescending(item => item.isStarterItem)
            .ThenBy(item => item.purchaseCost)
            .ToList();
    }
}
