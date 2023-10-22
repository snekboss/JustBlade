using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HordeWeaponSet : MonoBehaviour
{
    public List<Weapon> weaponPrefabs;

    public Weapon GetRandomWeapon()
    {
        // We're assuming that there's always at least one weapon to choose from.
        // Be a good user and always provide at least one weapon please.
        // There's no point in spawning without a weapon...

        int randomIndex = Random.Range(0, weaponPrefabs.Count);
        return weaponPrefabs[randomIndex];
    }
}
