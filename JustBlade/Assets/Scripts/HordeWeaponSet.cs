using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class which designates the attached game object as a <see cref="HordeArmorSet"/>.
/// It contains references to list of <see cref="Weapon"/> prefabs which can be used to initialize <see cref="Agent"/>
/// in the Horde game mode.
/// WARNING: The list should be filled with at least one <see cref="Weapon"/> prefab.
/// Not doing so will result in a <see cref="System.IndexOutOfRangeException"/>.
/// The initialization of is mainly done by <see cref="HordeGameLogic"/>, as the agents are spawned.
/// <see cref="HordeWeaponSet"/> is one of the prefabs which are compactly kept by <see cref="HordeAgentData"/>.
/// </summary>
[System.Serializable]
public class HordeWeaponSet : MonoBehaviour
{
    /// <summary>
    /// List of weapons, filled in the Inspector.
    /// </summary>
    public List<Weapon> weaponPrefabs;

    /// <summary>
    /// Gets a random from this <see cref="HordeWeaponSet"/>.
    /// WARNING: It is assumed that this <see cref="HordeWeaponSet"/> contains at least one weapon prefab.
    /// If not, it will throw a <see cref="System.IndexOutOfRangeException"/>.
    /// </summary>
    /// <returns>A random weapon from the list of weapons in the <see cref="HordeWeaponSet"/>.</returns>
    public Weapon GetRandomWeapon()
    {
        // We're assuming that there's always at least one weapon to choose from.
        // Be a good user and always provide at least one weapon please.
        // There's no point in spawning without a weapon...

        int randomIndex = Random.Range(0, weaponPrefabs.Count);
        return weaponPrefabs[randomIndex];
    }
}
