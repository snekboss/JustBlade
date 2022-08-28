using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Agent : MonoBehaviour
{
    public static readonly int MaximumHealth = 100;
    public static readonly float AgentDespawnTime = 5;
    public static readonly float DefaultMovementSpeedLimit = 2.5f;
    public static readonly float AgentHeight = 1.85f;
    public static readonly float AgentRadius = 0.25f;
    public static readonly float AgentMass = 70.0f;

    public int Health { get; protected set; } = MaximumHealth;
    public bool IsDead { get; protected set; } = false;
    protected float currentMovementSpeed;
    public float MovementSpeedLimit { get; protected set; }

    public bool isFriendOfPlayer;

    public bool IsPlayerAgent { get; protected set; }

    public delegate void AgentDeathEvent(Agent victim, Agent killer);
    public event AgentDeathEvent OnDeath;

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

    public float LookAngleX { get; protected set; }

    public void ApplyDamage(Agent attacker, int amount)
    {
        Health -= amount;

        if (Health <= 0)
        {
            IsDead = true;
            animMgr.PlayDeathAnimation();
            StartCoroutine("AgentDespawnCoroutine");

            if (OnDeath != null)
            {
                OnDeath(this, attacker);
            }
            return;
        }

        OnDamaged(attacker, amount);
    }

    protected virtual void OnDamaged(Agent attacker, int amount) { }

    public virtual void OnGearInitialized() { }

    public virtual void OnOtherAgentDeath(Agent victim, Agent killer) { }

    public virtual void InitializeMovementSpeedLimit(float movementSpeedLimit)
    {
        MovementSpeedLimit = movementSpeedLimit;
    }

    public abstract void RequestEquipmentSet(out Weapon weaponPrefab
        , out Armor headArmorPrefab
        , out Armor torsoArmorPrefab
        , out Armor handArmorPrefab
        , out Armor legArmorPrefab);

    public enum CombatDirection
    {
        Up = 0,
        Right,
        Down,
        Left
    }

    public virtual void Awake()
    {
        gameObject.layer = StaticVariables.Instance.AgentLayer;
        IsPlayerAgent = false;
    }

    protected virtual void LateUpdate()
    {
        if (StaticVariables.IsGamePaused)
        {
            return;
        }

        animMgr.LateUpdateAnimations();
    }

    IEnumerator AgentDespawnCoroutine()
    {
        yield return new WaitForSeconds(AgentDespawnTime);
        Destroy(this.gameObject);
    }
}
