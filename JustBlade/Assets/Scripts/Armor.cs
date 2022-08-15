using UnityEngine;

public class Armor : MonoBehaviour
{
    public enum ArmorType 
    { 
        Head = 0,
        Torso,
        Hand,
        Leg
    }

    public enum ArmorLevel
    {
        Light = 1,
        Medium,
        Heavy,
    }

    public SkinnedMeshRenderer skinnedMeshRenderer;
    public ArmorType armorType;
    public ArmorLevel armorLevel;
    public bool coversTheEntireBodyPart; // hide corresponding body part if true
}
