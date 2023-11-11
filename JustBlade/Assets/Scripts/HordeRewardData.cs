using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class which designates the attached game object as a <see cref="HordeRewardData"/>.
/// It contains minimum and maximum gold rewards dropped by enemies in Horde game mode.
/// The reward is given from this range.
/// It is one of the prefabs which are compactly kept by <see cref="InvaderAgentData"/>.
/// </summary>
[System.Serializable]
public class HordeRewardData : MonoBehaviour
{
	/// <summary>
	/// Minimum gold dropped by slain enemy, set in the Inspector.
	/// </summary>
	public int minGoldReward;
	/// <summary>
	/// Maximum gold dropped by slain enemy, set in the Inspector.
	/// </summary>
	public int maxGoldReward;

	/// <summary>
	/// Copies reward data from a given prefab, to this instance of <see cref="HordeRewardData"/>.
	/// <see cref="HordeRewardData"/> prefabs are instantiated, and attached as components to enemies.
	/// Later, <see cref="HordeGameLogic"/> can get these components from enemies, retrieve the reward data,
	/// and reward the player appropriately.
	/// </summary>
	/// <param name="prefab"></param>
	public void CopyDataFromPrefab(HordeRewardData prefab)
	{
		minGoldReward = prefab.minGoldReward;
		maxGoldReward = prefab.maxGoldReward;
	}

	/// <summary>
	/// Gets a random gold reward within the range of <see cref="minGoldReward"/> and <see cref="maxGoldReward"/>.
	/// </summary>
	/// <returns>A gold reward value within the range.</returns>
	public int GetRandomGoldAmountWithinRange()
	{
		float difficultyMulti = 1f / StaticVariables.DifficultySetting;
		int randomGold = Random.Range(minGoldReward, maxGoldReward);
		int difficultyGold = System.Convert.ToInt32(difficultyMulti * randomGold);

		return difficultyGold;
	}
}
