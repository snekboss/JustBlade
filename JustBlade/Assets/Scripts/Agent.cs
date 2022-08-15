using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Agent : MonoBehaviour
{
    EquipmentManager eqMgr;
    AnimationManager animMgr;

    public EquipmentManager EqMgr
    {
        get
        {
            if (eqMgr == null)
            {
                eqMgr = GetComponent<EquipmentManager>();
            }

            return eqMgr;
        }
    }
    public AnimationManager AnimMgr
    {
        get
        {
            if (animMgr == null)
            {
                animMgr = GetComponent<AnimationManager>();
            }

            return animMgr;
        }
    }

    public float lookAngleX;

    public enum CombatDirection
    {
        Up = 0,
        Right,
        Down,
        Left
    }

    protected void LateUpdate()
    {
        animMgr.LateUpdateAnimations();
    }
}
