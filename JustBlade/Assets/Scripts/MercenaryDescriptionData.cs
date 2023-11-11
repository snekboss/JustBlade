using UnityEngine;

/// <summary>
/// A script which is meant to be attached as a component to friendly agents in the Horde game mode.
/// When the friend of a player is killed, the appropriate number in <see cref="PlayerPartyManager"/>
/// regarding the number of remaining mercenaries must be informed.
/// In order to identify a certain <see cref="Agent"/> to see whether it's friendly or not, this
/// script is attached to the <see cref="Agent"/> as a component (which is done by <see cref="HordeGameLogic"/>.
/// </summary>
public class MercenaryDescriptionData : MonoBehaviour
{
    /// <summary>
    /// The overall <see cref="Armor.ArmorLevel"/> of the mercenary agent.
    /// </summary>
    public Armor.ArmorLevel mercArmorLevel;

    /// <summary>
    /// Initialize the <see cref="MercenaryDescriptionData"/> based on the <see cref="MercenaryAgentData"/> prefab.
    /// </summary>
    /// <param name="mercData">The <see cref="MercenaryAgentData"/> from which is component is initialized.</param>
    public void InitializeFromMercenaryData(MercenaryAgentData mercData)
    {
        mercArmorLevel = mercData.mercArmorLevel;
    }
}
