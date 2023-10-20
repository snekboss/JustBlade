using UnityEngine;

public class MercenaryDescriptionData : MonoBehaviour
{
    public Armor.ArmorLevel mercArmorLevel;
    public void InitializeFromMercenaryData(MercenaryData mercData)
    {
        mercArmorLevel = mercData.mercArmorLevel;
    }
}
