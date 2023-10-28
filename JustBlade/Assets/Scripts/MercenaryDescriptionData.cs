using UnityEngine;

public class MercenaryDescriptionData : MonoBehaviour
{
    public Armor.ArmorLevel mercArmorLevel;
    public void InitializeFromMercenaryData(MercenaryAgentData mercData)
    {
        mercArmorLevel = mercData.mercArmorLevel;
    }
}
