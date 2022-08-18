using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Agent : MonoBehaviour
{
    public static readonly float AgentDespawnTime = 5;
    public static readonly int MaximumHealth = 100;
    public static readonly float DefaultMovementSpeed = 2.5f; // TODO: Need a good value.
    public int Health { get; protected set; } = MaximumHealth;

    public float CurrentMovementSpeed { get; protected set; }
    public float MovementSpeed { get; protected set; }

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
            animMgr.PlayDeathAnimation();
            StartCoroutine("AgentDespawnCoroutine");
        }
    }

    public void InitializeMovementSpeed(float initMovSpeed)
    {
        MovementSpeed = initMovSpeed;
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

    IEnumerator AgentDespawnCoroutine()
    {
        yield return new WaitForSeconds(AgentDespawnTime);
        Destroy(this.gameObject);
    }
}
