using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Agent : MonoBehaviour
{
    public static readonly int MaximumHealth = 100;
    public int Health { get; protected set; } = MaximumHealth;

    public bool IsDead { get; protected set; } = false;

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

    public void ApplyDamage(int amount)
    {
        Health -= amount;

        if (Health <= 0)
        {
            IsDead = true;
            // TODO: Add death animation to the game.
            Debug.LogWarning("Agent \"" + name + "\" is dead, but there's no death animation, so we're just ignoring things now...");
            //animMgr.PlayDeathAnimation();
        }
    }

    public enum CombatDirection
    {
        Up = 0,
        Right,
        Down,
        Left
    }

    void LateUpdate()
    {
        animMgr.LateUpdateAnimations();
    }
}
