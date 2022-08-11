using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Agent : MonoBehaviour
{
    public EquipmentManager eqMgr;
    public AnimationManager animMgr;

    public enum CombatDirection
    {
        Up = 0,
        Right,
        Down,
        Left
    }
}
