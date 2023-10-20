using System.Collections;
using System.Collections.Generic;
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
                headArmors = LoadArmors("Armors/Head Armors");
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
                torsoArmors = LoadArmors("Armors/Torso Armors");
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
                handArmors = LoadArmors("Armors/Hand Armors");
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
                legArmors = LoadArmors("Armors/Leg Armors");
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
                weapons = LoadWeapons("Weapons");
            }

            return weapons;
        }
    }

    static MercenaryData basicMercenaryData;
    public static MercenaryData BasicMercenaryData
    {
        get
        {
            if (basicMercenaryData == null)
            {
                basicMercenaryData = LoadMercenaryData("BasicMercenaryData");
            }

            return basicMercenaryData;
        }
    }

    static MercenaryData lightMercenaryData;
    public static MercenaryData LightMercenaryData
    {
        get
        {
            if (lightMercenaryData == null)
            {
                lightMercenaryData = LoadMercenaryData("LightMercenaryData");
            }

            return lightMercenaryData;
        }
    }

    static MercenaryData mediumMercenaryData;
    public static MercenaryData MediumMercenaryData
    {
        get
        {
            if (mediumMercenaryData == null)
            {
                mediumMercenaryData = LoadMercenaryData("MediumMercenaryData");
            }

            return mediumMercenaryData;
        }
    }

    static MercenaryData heavyMercenaryData;
    public static MercenaryData HeavyMercenaryData
    {
        get
        {
            if (heavyMercenaryData == null)
            {
                heavyMercenaryData = LoadMercenaryData("HeavyMercenaryData");
            }

            return heavyMercenaryData;
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
    /// Loads a <see cref="MercenaryData"/> on a given path, under "Resources" folder of this project.
    /// Note that <see cref="MercenaryData"/> related code are hardcoded, and therefore
    /// there must be only one item within the folder of the given path.
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    static MercenaryData LoadMercenaryData(string path)
    {
        UnityEngine.Object[] objs = Resources.LoadAll(path);

        Debug.Assert(objs.Length == 1);

        GameObject dataGO = objs[0] as GameObject;
        MercenaryData mercData = dataGO.GetComponent<MercenaryData>();

        return mercData;
    }
}
