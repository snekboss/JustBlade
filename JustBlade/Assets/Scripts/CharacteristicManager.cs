using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class which must be attached to the game objects which are also <see cref="Agent"/>s.
/// It governs the characteristics of the attached <see cref="Agent"/>.
/// Characteristics are values such as Health, Movement Speed, Extra damage multiplier, etc.
/// If you wanted to add traditional RPG statistics such as "strength", "dexterity", etc.,
/// you would put those values in this class.
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
    public float debugExtraDamageInflictionMultiplier;
    public float debugDamageTakenMultiplier;
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
    public const float DefaultExtraDamageInflictionMultiplier = 1f;
    public const float DefaultDamageTakenMultiplier = 1f;
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

    /// <summary>
    /// The maximum health of the agent.
    /// </summary>
    public int MaximumHealth { get; private set; }

    /// <summary>
    /// The current health of the agent, which is different from <see cref="MaximumHealth"/>.
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

    /// <summary>
    /// True if the agent is dead, false otherwise.
    /// </summary>
    public bool IsDead { get; protected set; } = false;

    /// <summary>
    /// Movement speed limit multiplier of this agent after all calculations have been taken into account.
    /// This is different from <see cref="CurrentMovementSpeed"/>. This is the movement speed *limit*.
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

    /// <summary>
    /// The current movement speed of this agent. This is different from <see cref="MovementSpeedLimit"/>.
    /// This value is the *current* movement speed. The way it is set is done differently by different
    /// types of <see cref="Agent"/>s. For example, <see cref="AiAgent"/>s rely on Unity's NavMeshAgent;
    /// whereas the <see cref="PlayerAgent"/> uses inputs.
    /// </summary>
    public float CurrentMovementSpeed { get; set; }

    /// <summary>
    /// If the <see cref="MovementSpeedLimit"/> of this agent is less than the <see cref="DefaultMovementSpeedLimit"/>,
    /// then we consider this agent to be "over encumbered".
    /// Over encumbered agents do not get movement speed penalty when they're moving backwards.
    /// This feature is currently imposed on the <see cref="PlayerAgent"/>.
    /// Because, for the <see cref="AiAgent"/>s, the movement speed is dictated by Unity's NavMeshAgent system.
    /// Also, players are smart, and the Ai is not, so this is a nerf to the player power.
    /// </summary>
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

    /// <summary>
    /// Sets the extra damage infliction multiplier for this agent.
    /// For values above 1, the agent deals extra damage.
    /// </summary>
    public float ExtraDamageInflictionMultiplier { get; set; } = DefaultExtraDamageInflictionMultiplier;

    /// <summary>
    /// Sets the damage taken multiplier for this agent.
    /// For example, if the damage taken multiplier is 0.9, the agent takes 10% less damage.
    /// </summary>
    public float DamageTakenMultiplier { get; set; } = DefaultDamageTakenMultiplier;

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

    /// <summary>
    /// The current poise value of this agent. This value is different from <see cref="MaximumPoise"/>.
    /// </summary>
    public int CurrentPoise { get { return currentPoise; } }
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
    /// Also, resets poise back to <see cref="MaximumPoise"/> if all poise is depleted.
    /// </summary>
    public void DecrementPoise()
    {
        currentPoise--;
        if (currentPoise < 0)
        {
            currentPoise = MaximumPoise;
        }
    }

    /// <summary>
    /// Initializes the characteristics manager of this <see cref="Agent"/>.
    /// You can initialize the the characteristics of the agent here.
    /// If a value is not provided, the default value is used.
    /// </summary>
    /// <param name="maximumHealth">Maximum health of this agent.</param>
    /// <param name="modelSizeMultiplier">A multiplier for the agent character's model size.</param>
    /// <param name="extraMovementSpeedLimitMultiplier">Extra movement speed limit multiplier.</param>
    /// <param name="extraDamageInflictionMultiplier">Extra damage infliction multiplier.</param>
    /// <param name="damageTakenMultiplier">Damage taken multiplier.</param>
    /// <param name="maximumPoise">Maximum poise.</param>
    public void InitializeCharacteristicsManager(int maximumHealth = DefaultMaximumHealth
        , float modelSizeMultiplier = DefaultAgentSizeMultiplier
        , float extraMovementSpeedLimitMultiplier = DefaultExtraMovementSpeedLimitMultiplier
        , float extraDamageInflictionMultiplier = DefaultExtraDamageInflictionMultiplier
        , float damageTakenMultiplier = DefaultDamageTakenMultiplier
        , int maximumPoise = DefaultMaximumPoise)
    {
        MaximumHealth = maximumHealth;
        Health = MaximumHealth;

        AgentSizeMultiplier = modelSizeMultiplier;
        ExtraMovementSpeedLimitMultiplier = extraMovementSpeedLimitMultiplier;
        ExtraDamageInflictionMultiplier = extraDamageInflictionMultiplier;
        DamageTakenMultiplier = damageTakenMultiplier;

        MaximumPoise = maximumPoise;
        currentPoise = MaximumPoise;
    }

    /// <summary>
    /// Applies damage to the <see cref="Health"/> of the owner <see cref="Agent"/>.
    /// If the agent is still alive, this method invokes <see cref="Agent.OnThisAgentDamaged(Agent, int)"/>.
    /// If the Health goes below zero, the agent dies.
    /// When the agent is dead, this method invokes <see cref="Agent.OnThisAgentDeath(Agent)"/>.
    /// </summary>
    /// <param name="attacker">The agent whom damaged this agent.</param>
    /// <param name="amount">The amount by which the health should be damaged.</param>
    public void ApplyDamage(Agent attacker, int amount)
    {
        int difficultyAmount = amount;
        if (StaticVariables.DifficultySetting <= 1f && OwnerAgent.IsFriendOfPlayer)
        {
            // To use the DifficultySetting as a "damage taken" multiplier,
            // make sure it is not greater than 1f to avoid taking increased damage.
            difficultyAmount = System.Convert.ToInt32(difficultyAmount * StaticVariables.DifficultySetting);
        }
        Health -= difficultyAmount;
        ownerAgent.AudioMgr.PlayHurtSound();

        if (Health <= 0)
        {
            IsDead = true;
            OwnerAgent.OnThisAgentDeath(attacker);
            return;
        }

        OwnerAgent.OnThisAgentDamaged(attacker, difficultyAmount);
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
        debugExtraDamageInflictionMultiplier = ExtraDamageInflictionMultiplier;
        debugDamageTakenMultiplier = DamageTakenMultiplier;
        debugMaximumPoise = maximumPoise;
        debugPoise = currentPoise;
        debugFinalMovementSpeedLimit = MovementSpeedLimit;
    }
#endif
}
