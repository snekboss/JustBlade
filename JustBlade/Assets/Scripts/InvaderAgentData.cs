/// <summary>
/// A class which designates the attached game object as a <see cref="InvaderAgentData"/>.
/// It contains references to prefabs which can be used to initialize enemy <see cref="Agent"/>s in the Horde game mode.
/// The initialization of is mainly done by <see cref="HordeGameLogic"/>, as the agents are spawned.
/// Friendly agents use <see cref="MercenaryAgentData"/>; while enemy agents use <see cref="InvaderAgentData"/>.
/// </summary>
public class InvaderAgentData : HordeAgentData
{
    /// <summary>
    /// Reward data prefab, set in the Inspector.
    /// </summary>
    public HordeRewardData invaderRewardDataPrefab;
    /// <summary>
    /// True if invader attacks relentlessly (without defending at all), set in the Inspector.
    /// </summary>
    public bool isAggressive;
}
