using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class which designates the attached game object as a <see cref="HordeArmorSet"/>.
/// It contains references to list of <see cref="Armor"/> prefabs which can be used to initialize <see cref="Agent"/>
/// in the Horde game mode.
/// The initialization of is mainly done by <see cref="HordeGameLogic"/>, as the agents are spawned.
/// <see cref="HordeArmorSet"/> is one of the prefabs which are compactly kept by <see cref="HordeAgentData"/>.
/// </summary>
[System.Serializable]
public class HordeArmorSet : MonoBehaviour
{
    /// <summary>
    /// A list of head armor prefabs, filled in the Inspector menu.
    /// </summary>
	public List<Armor> headArmorPrefabs;
    /// <summary>
    /// A list of torso armor prefabs, filled in the Inspector menu.
    /// </summary>
	public List<Armor> torsoArmorPrefabs;
    /// <summary>
    /// A list of hand armor prefabs, filled in the Inspector menu.
    /// </summary>
	public List<Armor> handArmorPrefabs;
    /// <summary>
    /// A list of leg armor prefabs, filled in the Inspector menu.
    /// </summary>
	public List<Armor> legArmorPrefabs;

	Armor GetRandomArmorFromList(List<Armor> armorList)
    {
        if (armorList == null || armorList.Count == 0)
        {
			return null;
        }

		int randomIndex = Random.Range(0, armorList.Count);
		return armorList[randomIndex];
    }

    /// <summary>
    /// Gets a random <see cref="Armor"/> contained in this armor set.
    /// </summary>
    /// <param name="armorType">The <see cref="Armor.ArmorType"/> to choose from.</param>
    /// <returns>An <see cref="Armor"/> of the selected <see cref="Armor.ArmorType"/>.</returns>
	public Armor GetRandomArmor(Armor.ArmorType armorType)
    {
		List<Armor> chosenList = null;
        switch (armorType)
        {
            case Armor.ArmorType.Head:
                chosenList = headArmorPrefabs;
                break;
            case Armor.ArmorType.Torso:
                chosenList = torsoArmorPrefabs;
                break;
            case Armor.ArmorType.Hand:
                chosenList = handArmorPrefabs;
                break;
            case Armor.ArmorType.Leg:
                chosenList = legArmorPrefabs;
                break;
            default:
#if UNITY_EDITOR
                Debug.Log("Error.");
#endif
                break;
        }

        return GetRandomArmorFromList(chosenList);
	}
}
