using UnityEngine;

/// <summary>
/// A class which designates the attached game object as a <see cref="CharacteristicSet"/>.
/// It contains characteristic values.
/// This class can be attached to game objects as a script, and then saved as prefabs.
/// Later, these prefabs can be used to initialize the <see cref="CharacteristicManager"/> of <see cref="Agent"/>s in the game.
/// The initialization of is mainly done by <see cref="HordeGameLogic"/>, as the agents are spawned.
/// <see cref="CharacteristicSet"/> is one of the prefabs which are compactly kept by <see cref="HordeAgentData"/>.
/// </summary>
[System.Serializable]
public class CharacteristicSet : MonoBehaviour
{
	/// <summary>
	/// Maximum health, to be set in the Inspector menu.
	/// </summary>
	public int MaximumHealth;
	/// <summary>
	/// Agent character's model size multiplier, to be set in the Inspector menu.
	/// </summary>
	public float ModelSizeMultiplier;
	/// <summary>
	/// Extra movement speed limit multiplier, to be set in the Inspector menu.
	/// </summary>
	public float ExtraMovementSpeedLimitMultiplier;
	/// <summary>
	/// Extra damage infliction multiplier, to be set in the Inspector menu.
	/// </summary>
	public float ExtraDamageInflictionMultiplier;
	/// <summary>
	/// Damage taken multiplier, to be set in the Inspector menu.
	/// </summary>
	public float DamageTakenMultiplier;
	/// <summary>
	/// Maximum poise, to be set in the Inspector menu.
	/// </summary>
	public int MaximumPoise;
}
