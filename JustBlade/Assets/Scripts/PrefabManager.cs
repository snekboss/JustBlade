using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    static List<Armor> LoadArmors(string path)
    {
        List<Armor> ret = new List<Armor>();

        UnityEngine.Object[] objs = Resources.LoadAll(path);
        for (int i = 0; i < objs.Length; i++)
        {
            GameObject armorGO = objs[i] as GameObject;
            Armor armor = armorGO.GetComponent<Armor>();
            ret.Add(armor);
        }

        return ret;
    }

    static List<Weapon> LoadWeapons(string path)
    {
        List<Weapon> ret = new List<Weapon>();

        UnityEngine.Object[] objs = Resources.LoadAll(path);
        for (int i = 0; i < objs.Length; i++)
        {
            GameObject weaponGO = objs[i] as GameObject;
            Weapon weapon = weaponGO.GetComponent<Weapon>();
            ret.Add(weapon);
        }

        return ret;
    }
}
