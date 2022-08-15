using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Agent : MonoBehaviour
{
    EquipmentManager eqMgr;
    AnimationManager animMgr;
    LimbManager limbMgr;

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

    public LimbManager LimbMgr
    {
        get
        {
            if (limbMgr == null)
            {
                limbMgr = GetComponent<LimbManager>();
            }

            return limbMgr;
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
