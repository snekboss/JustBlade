/// <summary>
/// A class which designates the attached game object as a <see cref="MercenaryAgentData"/>.
/// It contains references to prefabs which can be used to initialize friendly <see cref="Agent"/>s in the Horde game mode.
/// The initialization of is mainly done by <see cref="HordeGameLogic"/>, as the agents are spawned.
/// Friendly agents use <see cref="MercenaryAgentData"/>; while enemy agents use <see cref="InvaderAgentData"/>.
/// </summary>
public class MercenaryAgentData : HordeAgentData
{
	/// <summary>
	/// Overall <see cref="Armor.ArmorLevel"/> of the mercenary, set in the Inspector.
	/// </summary>
	public Armor.ArmorLevel mercArmorLevel;
	/// <summary>
	/// Hire cost of the mercenary, set in the Inspector.
	/// </summary>
	public int hireCost;
	/// <summary>
	/// Upgrade cost of the mercenary, set in the Inspector.
	/// </summary>
	public int upgradeCost;
}
