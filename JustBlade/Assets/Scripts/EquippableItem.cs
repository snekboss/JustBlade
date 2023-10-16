using UnityEngine;

public class EquippableItem : MonoBehaviour
{
    public string shownName; // The shown name in the game, such as menus etc.
    public int purchaseCost;
    bool isPurchasedByPlayer;
    public bool isStarterItem;

    public void BePurchasedByPlayer()
    {
        isPurchasedByPlayer = true;
    }

    public bool IsPurchasedByPlayer() { return isPurchasedByPlayer; }
}
