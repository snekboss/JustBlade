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

    public const float AgentDespawnTime = 5;

    public bool IsDead { get { return CharMgr.IsDead; } }
    public bool IsPlayerAgent { get; protected set; }
    public bool IsFriendOfPlayer
    {
        get
        {
            return isFriendOfPlayer;
        }
        set
        {
            isFriendOfPlayer = value;

            InitializeFriendlinessIndicator();
        }
    }
    public bool isFriendOfPlayer;

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
    EquipmentManager eqMgr;
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
    AnimationManager animMgr;

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
    LimbManager limbMgr;

    public CharacteristicManager CharMgr
    {
        get
        {
            if (charMgr == null)
            {
                charMgr = GetComponent<CharacteristicManager>();
            }

            return charMgr;
        }
    }
    CharacteristicManager charMgr;


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
        CharMgr.ApplyDamage(attacker, amount);
    }

    public void OnThisAgentDeath(Agent killer)
    {
        AnimMgr.PlayDeathAnimation();
        StartCoroutine("AgentDespawnCoroutine");

        if (OnDeath != null)
        {
            OnDeath(this, killer);
        }
    }

    /// <summary>
    /// Callback method for when this agent is damaged.
    /// </summary>
    /// <param name="attacker">The agent whom damaged this agent.</param>
    /// <param name="amount">The amount by which the health was damaged.</param>
    public virtual void OnThisAgentDamaged(Agent attacker, int amount) { }

    protected virtual void InitializeFriendlinessIndicator() { }

    public virtual void ToggleCombatDirectionPreference(float distanceToClosestFriend) { }
    public virtual void ConsiderNearbyEnemy(Agent nearbyEnemy) { }

    protected bool IsMovingBackwards(Vector3 localMoveDir)
    {
        if (localMoveDir.z > 0f)
        {
            return false;
        }

        float angle = Vector3.Angle(Vector3.right, localMoveDir);

        return (angle > CharacteristicManager.MovingBackwardsAngleMin) 
            && (angle < CharacteristicManager.MovingBackwardsAngleMax);
    }

    public virtual void InitializeAgent(Weapon weaponPrefab
        , Armor headArmorPrefab
        , Armor torsoArmorPrefab
        , Armor handArmorPrefab
        , Armor legArmorPrefab
        , CharacteristicSet characteristicPrefab = null)
    {
        EqMgr.InitializeEquipmentManager(weaponPrefab
            , headArmorPrefab
            , torsoArmorPrefab
            , handArmorPrefab
            , legArmorPrefab);

        if (characteristicPrefab == null)
        {
            CharMgr.InitializeCharacteristicsManager();
        }
        else
        {
            CharMgr.InitializeCharacteristicsManager(characteristicPrefab.MaximumHealth
                , characteristicPrefab.ModelSizeMultiplier
                , characteristicPrefab.ExtraMovementSpeedLimitMultiplier
                , characteristicPrefab.ExtraDamageMultiplier
                , characteristicPrefab.ExtraDamageResistanceMultiplier
                , characteristicPrefab.MaximumPoise);
        }
    }

    public virtual void InitializeAgent(Weapon weaponPrefab
        , Armor headArmorPrefab
        , Armor torsoArmorPrefab
        , Armor handArmorPrefab
        , Armor legArmorPrefab
        , int maximumHealth
        , float modelSizeMultiplier
        , float extraMovementSpeedMultiplier
        , float extraDamageMultiplier
        , float extraDamageResistanceMultiplier
        , int maximumPoise)
    {
        EqMgr.InitializeEquipmentManager(weaponPrefab
            , headArmorPrefab
            , torsoArmorPrefab
            , handArmorPrefab
            , legArmorPrefab);

        CharMgr.InitializeCharacteristicsManager(
            maximumHealth
            , modelSizeMultiplier
            , extraMovementSpeedMultiplier
            , extraDamageMultiplier
            , extraDamageResistanceMultiplier
            , maximumPoise);
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
