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

    public const int MaximumHealth = 100;
    public const float AgentDespawnTime = 5;
    public const float DefaultMovementSpeedLimit = 2.5f;
    public const float AgentHeight = 1.85f;
    public const float AgentRadius = 0.25f;
    public const float AgentMass = 70.0f;

    public int Health { get; protected set; } = MaximumHealth;
    public bool IsDead { get; protected set; } = false;

    protected float currentMovementSpeed;
    public float MovementSpeedLimit { get; protected set; }
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
    /// Callback method for when <see cref="EquipmentManager"/> initializes all of the equipment (aka: gear), of this agent.
    /// </summary>
    public virtual void OnGearInitialized() { }

    /// <summary>
    /// Callback method for when any agent dies.
    /// </summary>
    /// <param name="victim">The agent who died.</param>
    /// <param name="killer">The agent who killed the victim agent.</param>
    public virtual void OnOtherAgentDeath(Agent victim, Agent killer) { }

    /// <summary>
    /// A method for the <see cref="EquipmentManager"/> to initialize the movement speed of this agent.
    /// It must be done via the <see cref="EquipmentManager"/>
    /// because there's no way to know whether or not the equipment was initialized beforehand.
    /// </summary>
    /// <param name="movementSpeedLimit">The maximum achievable movement speed of this agent.</param>
    public virtual void InitializeMovementSpeedLimit(float movementSpeedLimit)
    {
        MovementSpeedLimit = movementSpeedLimit;
    }

    /// <summary>
    /// An abstract method which is used mostly by <see cref="EquipmentManager"/> to request a full set of gear.
    /// Every agent requests gear in a different way, hence the abstractness.
    /// </summary>
    /// <param name="weaponPrefab">The prefab reference of the weapon.</param>
    /// <param name="headArmorPrefab">The prefab reference of the head armor.</param>
    /// <param name="torsoArmorPrefab">The prefab reference of the torso armor.</param>
    /// <param name="handArmorPrefab">The prefab reference of the hand armor.</param>
    /// <param name="legArmorPrefab">The prefab reference of the leg armor.</param>
    public abstract void RequestEquipmentSet(out Weapon weaponPrefab
        , out Armor headArmorPrefab
        , out Armor torsoArmorPrefab
        , out Armor handArmorPrefab
        , out Armor legArmorPrefab);

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
