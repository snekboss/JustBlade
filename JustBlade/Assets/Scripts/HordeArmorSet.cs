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

	public Armor GetRandomArmorFromList(List<Armor> armorList)
    {
        if (armorList == null || armorList.Count == 0)
        {
			return null;
        }

		int randomIndex = Random.Range(0, armorList.Count);
		return armorList[randomIndex];
    }

	public void ProvideRequestedArmorSet(out Armor headArmorPrefab
		, out Armor torsoArmorPrefab
		, out Armor handArmorPrefab
		, out Armor legArmorPrefab)
    {
		headArmorPrefab = GetRandomArmorFromList(headArmorPrefabs);
		torsoArmorPrefab = GetRandomArmorFromList(torsoArmorPrefabs);
		handArmorPrefab = GetRandomArmorFromList(handArmorPrefabs);
		legArmorPrefab = GetRandomArmorFromList(legArmorPrefabs);
	}
}
