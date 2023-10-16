using UnityEngine;

public class EquippableItem : MonoBehaviour
{
    public string shownName; // The shown name in the game, such as menus etc.
    public int purchaseCost;
    bool isPurchasedByPlayer = false;
    public bool isStarterItem;

    public void SetPurchasedByPlayer(bool value)
    {
        isPurchasedByPlayer = value;
    }

    public bool IsPurchasedByPlayer() { return isPurchasedByPlayer; }
}
