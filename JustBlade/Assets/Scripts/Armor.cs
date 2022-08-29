using UnityEngine;

/// <summary>
/// A class which designates the attached game object as an Armor.
/// The game object to which this is attached is required to have a <see cref="SkinnedMeshRenderer"/>,
/// which also needs to be compatible with the skeleton rig of the character model which I made.
/// </summary>
public class Armor : MonoBehaviour
{
    /// <summary>
    /// The type of armor.
    /// If the armor is a helmet, then it is worn on the head, so it is a Head armor.
    /// The others are similar.
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

    public string shownName; // The shown name in the game, such as menus etc.
    public SkinnedMeshRenderer skinnedMeshRenderer;
    public ArmorType armorType;
    public ArmorLevel armorLevel;
    public bool coversTheEntireBodyPart; // hide corresponding body part if true
}
