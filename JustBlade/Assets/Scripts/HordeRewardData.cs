using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HordeRewardData : MonoBehaviour
{
	public int minGoldReward;
	public int maxGoldReward;

	public void CopyDataFromPrefab(HordeRewardData prefab)
	{
		minGoldReward = prefab.minGoldReward;
		maxGoldReward = prefab.maxGoldReward;
	}

	public int GetRandomGoldAmountWithinRange()
	{
		float difficultyMulti = 1f / StaticVariables.DifficultySetting;
		int randomGold = Random.Range(minGoldReward, maxGoldReward);
		int difficultyGold = System.Convert.ToInt32(difficultyMulti * randomGold);

		return difficultyGold;
	}
}
