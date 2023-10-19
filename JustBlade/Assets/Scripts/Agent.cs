using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An abstract class which is meant to be inherited by anything that is designated as Agent.
/// It contains references to fields which are used by all agents.
/// Most of this class' methods are either abstract or virtual, so refer to the derived classes for more info.
/// </summary>
public abstract class Agent : MonoBehaviour
{
    /// <summary>
    /// An enumeration for the directions of the attacks.
    /// These are the same across the entire project.
    /// Meaning, even in the Animator Controllers, Up is zero, etc.
    /// </summary>
    public enum CombatDirection
    {
        Up = 0,
        Right,
        Down,
        Left
    }

    public const int DefaultMaximumHealth = 100;
    public const float AgentDespawnTime = 5;
    public const float DefaultMovementSpeedLimit = 2.5f;
    public const float DefaultAgentHeight = 1.85f;
    public const float DefaultAgentRadius = 0.25f;
    public const float DefaultAgentMass = 70.0f;
    public const float DefaultAgentScale = 1f;
    public const float DefaultExtraMovementSpeedLimitMultiplier = 1f;
    public const float DefaultExtraDamageMultiplier = 1f;
    public const float DefaultExtraDamageResistanceMultiplier = 1f;
    public const int DefaultMaximumPoise = 0;


    public float AgentHeight { get { return DefaultAgentHeight * AgentScale; } }
    public float AgentRadius { get { return DefaultAgentRadius * AgentScale; } }
    public float AgentMass   { get { return DefaultAgentMass * AgentScale; } }

    /// <summary>
    /// Scales the model size of the agent.
    /// Note roviding a new value to this property causes the agent to invoke <see cref="ReinitializeParameters"/>.
    /// </summary>
    public float AgentScale
    { 
        get { return agentScale; } 
        set
        {
            agentScale = value;

            transform.localScale = Vector3.one * agentScale;

            ReinitializeParameters();
        }
    }
    float agentScale = DefaultAgentScale;

    /// <summary>
    /// Sets the health value of the given agent.
    /// Note that this property ignores <see cref="Agent.DefaultMaximumHealth"/>,
    /// and sets the current health to the provided value.
    /// Maximum health information is not stored in Agents, because they do not recover health.
    /// </summary>
    public int Health 
    {
        get { return health; }
        set 
        {
            health = value;
        }
    }
    int health = DefaultMaximumHealth;

    public bool IsDead { get; protected set; } = false;

    /// <summary>
    /// Movement speed limit multiplier of this agent after all calculations have been taken into account.
    /// </summary>
    public float MovementSpeedLimit
    { 
        get { return DefaultMovementSpeedLimit 
                * EqMgr.MovementSpeedMultiplierFromArmor 
                * ExtraMovementSpeedLimitMultiplier; }
    }
    protected float currentMovementSpeed;

    /// <summary>
    /// Sets the extra movement speed limit multiplier for this agent.
    /// Note 1: Use <see cref="MovementSpeedLimit"/> to get the final movement speed limit.
    /// Note 2: Providing a new value to this property causes the agent to invoke <see cref="ReinitializeParameters"/>.
    /// </summary>
    public float ExtraMovementSpeedLimitMultiplier 
    { 
        get { return extraMovementSpeedLimitMultiplier; }
        set
        {
            extraMovementSpeedLimitMultiplier = value;

            ReinitializeParameters();
        }
    }
    float extraMovementSpeedLimitMultiplier = DefaultExtraMovementSpeedLimitMultiplier;

    public float ExtraDamageMultiplier { get; set; } = DefaultExtraDamageMultiplier;
    public float ExtraDamageResistanceMultiplier { get; set; } = DefaultExtraDamageResistanceMultiplier;

    /// <summary>
    /// Maximum amount of poise of the agent.
    /// Each point of poise determines if a single enemy attack can be withstood without flinching.
    /// </summary>
    public int MaximumPoise 
    { 
        get { return maximumPoise; }
        set
        {
            maximumPoise = value;
            currentPoise = maximumPoise;
        }
    }
    int maximumPoise = DefaultMaximumPoise;
    int currentPoise = DefaultMaximumPoise;

    /// <summary>
    /// Determines if the agent is capable of withstanding a single incoming attack without flinching.
    /// </summary>
    /// <returns>True if the agent can withstand an attack without flinching; false otherwise.</returns>
    public bool CanPoiseThroughAttack()
    {
        return currentPoise > 0;
    }

    /// <summary>
    /// Reduces poise by one point.
    /// Also resets poise back to <see cref="MaximumPoise"/> if all poise is depleted.
    /// </summary>
    public void DecrementPoise()
    {
        currentPoise--;
        if (currentPoise < 0)
        {
            currentPoise = MaximumPoise;
        }
    }


    public bool IsPlayerAgent { get; protected set; }
    public bool isFriendOfPlayer;

    EquipmentManager eqMgr;
    AnimationManager animMgr;
    LimbManager limbMgr;

    /// <summary>
    /// An delegate for when an agent dies.
    /// </summary>
    /// <param name="victim"></param>
    /// <param name="killer"></param>
    public delegate void AgentDeathEvent(Agent victim, Agent killer);
    /// <summary>
    /// The event of this agent's death.
    /// </summary>
    public event AgentDeathEvent OnDeath;

    public delegate void ArmorSetRequestEvent(out Armor headArmorPrefab
        , out Armor torsoArmorPrefab
        , out Armor handArmorPrefab
        , out Armor legArmorPrefab);
    public event ArmorSetRequestEvent ArmorSetRequest;

    public delegate void WeaponRequestEvent(out Weapon weaponPrefab);
    public event WeaponRequestEvent WeaponRequest;

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

    /// <summary>
    /// The angle of looking up and down. Mainly used for rotating the spine bone in <see cref="AnimationManager"/>.
    /// </summary>
    public float LookAngleX { get; protected set; }

    /// <summary>
    /// Applies damage to the <see cref="Health"/> of this agent.
    /// If the Health goes below zero, the agent dies.
    /// When the agent is dead, it is automatically despawned via <see cref="AgentDespawnCoroutine"/>.
    /// </summary>
    /// <param name="attacker">The agent whom damaged this agent.</param>
    /// <param name="amount">The amount by which the health should be damaged.</param>
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

    /// <summary>
    /// Callback method for when this agent is damaged.
    /// It is a protected method, so it is meant to be used from inside Agent class.
    /// </summary>
    /// <param name="attacker">The agent whom damaged this agent.</param>
    /// <param name="amount">The amount by which the health was damaged.</param>
    protected virtual void OnDamaged(Agent attacker, int amount) { }


    /// <summary>
    /// Callback method for when any agent dies.
    /// </summary>
    /// <param name="victim">The agent who died.</param>
    /// <param name="killer">The agent who killed the victim agent.</param>
    public virtual void OnOtherAgentDeath(Agent victim, Agent killer) { }

    /// <summary>
    /// A method to reinitialize parameters, if needed.
    /// Due Unity's de-centralized scripting system, there is no way to know which script will be invoked when.
    /// This method is used to recalculate <see cref="EquipmentManager.MovementSpeedMultiplierFromArmor"/>,
    /// and it can be further overloaded by <see cref="Agent"/>'s descendants to recalculate other parameters.
    /// </summary>
    public virtual void ReinitializeParameters()
    {
        EqMgr.CalculateMovementSpeedMultiplierFromArmor();
    }

    /// <summary>
    /// A virtual method which is used mostly by <see cref="EquipmentManager"/> to request a full set of gear.
    /// It is used to invoke <see cref="ArmorSetRequestEvent"/> and <see cref="WeaponRequestEvent"/>.
    /// Descendants of <see cref="Agent"/> are free to override this method, of course.
    /// </summary>
    /// <param name="weaponPrefab">The prefab reference of the weapon.</param>
    /// <param name="headArmorPrefab">The prefab reference of the head armor.</param>
    /// <param name="torsoArmorPrefab">The prefab reference of the torso armor.</param>
    /// <param name="handArmorPrefab">The prefab reference of the hand armor.</param>
    /// <param name="legArmorPrefab">The prefab reference of the leg armor.</param>
    public virtual void RequestEquipmentSet(out Weapon weaponPrefab
        , out Armor headArmorPrefab
        , out Armor torsoArmorPrefab
        , out Armor handArmorPrefab
        , out Armor legArmorPrefab)
    {
        weaponPrefab = null;
        headArmorPrefab = null;
        torsoArmorPrefab = null;
        handArmorPrefab = null;
        legArmorPrefab = null;
        if (ArmorSetRequest != null)
        {
            ArmorSetRequest(out headArmorPrefab
                        , out torsoArmorPrefab
                        , out handArmorPrefab
                        , out legArmorPrefab);
        }

        if (WeaponRequest != null)
        {
            WeaponRequest(out weaponPrefab);
        }
    }

    /// <summary>
    /// Unity's Awake method.
    /// In this case, it is mainly used to set the layer of the agent.
    /// </summary>
    public virtual void Awake()
    {
        gameObject.layer = StaticVariables.Instance.AgentLayer;
        IsPlayerAgent = false;
    }

    /// <summary>
    /// Unity's LateUpdate method.
    /// It is used to adjust the spine bone of all agents.
    /// The spine bone is connected to the pelvis bone manually.
    /// It's also rotated about its local X axis, so that the agents can look up and down while attacking.
    /// Since animations are done in Update, any animation related post processing is done in LateUpdate.
    /// </summary>
    protected virtual void LateUpdate()
    {
        animMgr.LateUpdateAnimations();
    }

    /// <summary>
    /// A coroutine method to despawn the agent from the scene.
    /// It's best to call this when the agent is dead.
    /// </summary>
    /// <returns>Some kind of Unity coroutine magic thing.</returns>
    IEnumerator AgentDespawnCoroutine()
    {
        yield return new WaitForSeconds(AgentDespawnTime);
        Destroy(this.gameObject);
    }
}
