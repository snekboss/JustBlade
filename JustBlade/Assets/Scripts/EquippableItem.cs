using UnityEngine;

/// <summary>
/// A class which designates the attached game object as an <see cref="EquippableItem"/>.
/// This class contains common fields which are used by other derived classes such as
/// <see cref="Weapon"/> and <see cref="Armor"/>.
/// </summary>
public class EquippableItem : MonoBehaviour
{
    /// <summary>
    /// The shown name in the game (such as menus etc.), to be set in the Inspector.
    /// </summary>
    public string shownName;
    /// <summary>
    /// Purchase cost, to be set in the Inspector.
    /// </summary>
    public int purchaseCost;
    bool isPurchasedByPlayer = false;
    /// <summary>
    /// True if this is a starter item (ie, the player doesn't have to buy it), to be set in the Inspector.
    /// </summary>
    public bool isStarterItem;

    /// <summary>
    /// Sets the <see cref="EquippableItem"/>'s <see cref="isPurchasedByPlayer"/> field to true if
    /// the item was purchased by the player.
    /// When the value of the field <see cref="isPurchasedByPlayer"/> is changed at runtime,
    /// Unity applies this change to the prefab permanently.
    /// However, since it is a private field, this change is not detected by the Inspector
    /// (nor the source control which I'm using).
    /// When the field was public, Unity's Inspector recognized the changes to this field as a change to the prefab
    /// itself, which was then recognized as a change by my source control tool.
    /// I'm not sure if this only happens in Unity Editor or if it also happens in the actual main game.
    /// Regardless, when the change happens in Unity Editor, I've made the field private in order to avoid this issue.
    /// </summary>
    /// <param name="value">True if is purchased by player; false otherwise.</param>
    public void SetPurchasedByPlayer(bool value)
    {
        isPurchasedByPlayer = value;
    }

    /// <summary>
    /// Check if this <see cref="EquippableItem"/> was purchased by the player.
    /// </summary>
    /// <returns>True if this equippable item was purchased by the player; false otherwise.</returns>
    public bool IsPurchasedByPlayer() { return isPurchasedByPlayer; }
}
