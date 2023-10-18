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
		return Random.Range(minGoldReward, maxGoldReward);
	}
}
