using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HordeArmorSet : MonoBehaviour
{
	public List<Armor> headArmorPrefabs;
	public List<Armor> torsoArmorPrefabs;
	public List<Armor> handArmorPrefabs;
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
