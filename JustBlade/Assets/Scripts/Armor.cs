using UnityEngine;

/// <summary>
/// A class which designates the attached game object as an <see cref="Armor"/>.
/// /// This class inherits from <see cref="EquippableItem"/>.
/// The game object to which this is attached is required to have a <see cref="SkinnedMeshRenderer"/>,
/// which also needs to be compatible with the skeleton rig of the character model which I made.
/// </summary>
public class Armor : EquippableItem
{
    /// <summary>
    /// The type of armor.
    /// If the armor is a helmet, then it is worn on the head, so it is a Head armor.
    /// Similarly for other body parts.
    /// </summary>
    public enum ArmorType 
    { 
        Head = 0,
        Torso,
        Hand,
        Leg
    }

    /// <summary>
    /// The level of the armor.
    /// None means the armor provides no protection, but it also doesn't incur the agent to any movement penalties.
    /// </summary>
    public enum ArmorLevel
    {
        None = 0,
        Light,
        Medium,
        Heavy,
    }

    /// <summary>
    /// The skinned mesh renderer this armor uses, set in the Inspector.
    /// </summary>
    public SkinnedMeshRenderer skinnedMeshRenderer;
    /// <summary>
    /// Armor type of this armor, set in the Inspector.
    /// </summary>
    public ArmorType armorType;
    /// <summary>
    /// Armor level of this armor, set in the Inspector.
    /// </summary>
    public ArmorLevel armorLevel;
    /// <summary>
    /// True if this armor covers the entire corresponding body part, set in the Inspector.
    /// </summary>
    public bool coversTheEntireBodyPart; // hide corresponding body part if true
}
