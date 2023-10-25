using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class which must be attached to the game objects which are also <see cref="Agent"/>s.
/// It governs the characteristics of the attached <see cref="Agent"/>.
/// </summary>
public class CharacteristicManager : MonoBehaviour
{
#if UNITY_EDITOR
    public bool debugShowData = false;

    public float debugAgentWorldHeight;
    public float debugAgentWorldRadius;
    public float debugAgentMass;

    public int debugMaximumHealth;
    public int debugHealth;
    public float debugAgentScale;
    public float debugExtraMovementSpeedLimitMultiplier;
    public float debugExtraDamageMultiplier;
    public float debugExtraDamageResistanceMultiplier;
    public int debugMaximumPoise;
    public int debugPoise;
    public float debugFinalMovementSpeedLimit;
#endif

    public const int DefaultMaximumHealth = 100;
    public const float DefaultMovementSpeedLimit = 2.5f;
    public const float DefaultAgentHeight = 1.85f;
    public const float DefaultAgentRadius = 0.25f;
    public const float DefaultAgentMass = 70.0f;
    public const float DefaultAgentSizeMultiplier = 1f;
    public const float DefaultExtraMovementSpeedLimitMultiplier = 1f;
    public const float DefaultExtraDamageMultiplier = 1f;
    public const float DefaultExtraDamageResistanceMultiplier = 1f;
    public const int DefaultMaximumPoise = 0;

    public const float MovingBackwardsAngleMin = 40f;
    public const float MovingBackwardsAngleMax = 140f;
    public const float PlayerMovingBackwardsSpeedPenaltyMultiplier = 0.5f; // specifically for player only.

    Agent ownerAgent;
    public Agent OwnerAgent
    {
        get
        {
            if (ownerAgent == null)
            {
                ownerAgent = GetComponent<Agent>();
            }

            return ownerAgent;
        }
    }

    /// <summary>
    /// Height of the agent in world space, which takes <see cref="AgentSizeMultiplier"/> into account.
    /// </summary>
    public float AgentWorldHeight { get { return DefaultAgentHeight * AgentSizeMultiplier; } }
    /// <summary>
    /// Radius of the agent in world space, which takes <see cref="AgentSizeMultiplier"/> into account.
    /// </summary>
    public float AgentWorldRadius { get { return DefaultAgentRadius * AgentSizeMultiplier; } }
    /// <summary>
    /// Mass of the agent in world space, which takes <see cref="AgentSizeMultiplier"/> into account.
    /// </summary>
    public float AgentWorldMass { get { return DefaultAgentMass * AgentSizeMultiplier; } }

    /// <summary>
    /// Scales the model size of the agent.
    /// </summary>
    public float AgentSizeMultiplier
    {
        get { return agentScale; }
        set
        {
            agentScale = value;

            transform.localScale = Vector3.one * agentScale;
        }
    }
    float agentScale = DefaultAgentSizeMultiplier;

    public int MaximumHealth { get; private set; }

    /// <summary>
    /// Sets the health value of the given agent.
    /// Note that this property also sets the current health to the provided value.
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
        get
        {
            return DefaultMovementSpeedLimit
              * OwnerAgent.EqMgr.MovementSpeedMultiplierFromArmor
              * ExtraMovementSpeedLimitMultiplier;
        }
    }
    public float CurrentMovementSpeed { get; set; }

    public bool IsOverEncumbered { get { return MovementSpeedLimit < DefaultMovementSpeedLimit; } }

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

    public void InitializeCharacteristicsManager(int maximumHealth = DefaultMaximumHealth
        , float modelSizeMultiplier = DefaultAgentSizeMultiplier
        , float extraMovementSpeedLimitMultiplier = DefaultExtraMovementSpeedLimitMultiplier
        , float extraDamageMultiplier = DefaultExtraDamageMultiplier
        , float extraDamageResistanceMultiplier = DefaultExtraDamageResistanceMultiplier
        , int maximumPoise = DefaultMaximumPoise)
    {
        MaximumHealth = maximumHealth;
        Health = MaximumHealth;

        AgentSizeMultiplier = modelSizeMultiplier;
        ExtraMovementSpeedLimitMultiplier = extraMovementSpeedLimitMultiplier;
        ExtraDamageMultiplier = extraDamageMultiplier;
        ExtraDamageResistanceMultiplier = extraDamageResistanceMultiplier;

        MaximumPoise = maximumPoise;
        currentPoise = MaximumPoise;
    }

    public void ApplyDamage(Agent attacker, int amount)
    {
        Health -= amount;

        if (Health <= 0)
        {
            IsDead = true;
            OwnerAgent.OnThisAgentDeath(attacker);
            return;
        }

        OwnerAgent.OnThisAgentDamaged(attacker, amount);
    }

#if UNITY_EDITOR
    void Update()
    {
        if (debugShowData == false)
        {
            return;
        }

        debugAgentWorldHeight = AgentWorldHeight;
        debugAgentWorldRadius = AgentWorldRadius;
        debugAgentMass = AgentWorldMass;
        debugMaximumHealth = MaximumHealth;
        debugHealth = Health;
        debugAgentScale = AgentSizeMultiplier;
        debugExtraMovementSpeedLimitMultiplier = ExtraMovementSpeedLimitMultiplier;
        debugExtraDamageMultiplier = ExtraDamageMultiplier;
        debugExtraDamageResistanceMultiplier = ExtraDamageResistanceMultiplier;
        debugMaximumPoise = maximumPoise;
        debugPoise = currentPoise;
        debugFinalMovementSpeedLimit = MovementSpeedLimit;
    }
#endif
}
