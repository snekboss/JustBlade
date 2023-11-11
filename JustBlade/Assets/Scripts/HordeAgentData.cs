using UnityEngine;

/// <summary>
/// A class which designates the attached game object as a <see cref="HordeAgentData"/>.
/// It contains references to prefabs which can be used to initialize <see cref="Agent"/> in the Horde game mode.
/// The initialization of is mainly done by <see cref="HordeGameLogic"/>, as the agents are spawned.
/// Horde agents are <see cref="Agent"/>s which are found in the Horde game mode.
/// Horde agents can be friends or enemies to the <see cref="PlayerAgent"/>.
/// Friendly agents use <see cref="MercenaryAgentData"/>; while enemy agents use <see cref="InvaderAgentData"/>.
/// </summary>
public class HordeAgentData : MonoBehaviour
{
	/// <summary>
	/// Reference to a <see cref="CharacteristicSet"/> prefab, set in Inspector menu.
	/// </summary>
	public CharacteristicSet charSetPrefab;
	/// <summary>
	/// Reference to a <see cref="HordeArmorSet"/> prefab, set in Inspector menu.
	/// </summary>
	public HordeArmorSet armorSetPrefab;
	/// <summary>
	/// Reference to a <see cref="HordeWeaponSet"/> prefab, set in Inspector menu.
	/// </summary>
	public HordeWeaponSet weaponSetPrefab;
}
