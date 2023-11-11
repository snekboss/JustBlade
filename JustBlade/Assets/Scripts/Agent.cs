using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// An abstract class which is meant to be inherited by anything that is designated as Agent.
/// It contains references to fields which are used by all agents.
/// Most of this class' methods are either abstract or virtual, so refer to the derived classes for more information.
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

    const float AgentDespawnTime = 5;

    /// <summary>
    /// True if the agent has less than or equal to zero <see cref="CharacteristicManager.Health"/>.
    /// </summary>
    public bool IsDead { get { return CharMgr.IsDead; } }
    /// <summary>
    /// This is set to true by <see cref="PlayerAgent"/> in <see cref="PlayerAgent.Awake"/>;
    /// false by every other agent.
    /// </summary>
    public bool IsPlayerAgent { get; protected set; }
    /// <summary>
    /// True if the agent is a friend of the player (ie, will fight for the player); false otherwise.
    /// It also invokes <see cref="Agent.InitializeFriendlinessIndicator"/> so that the friendly agents
    /// get an indicator above their heads to determine whether they're friendly to the player or not.
    /// </summary>
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
    bool isFriendOfPlayer;

    /// <summary>
    /// Access the <see cref="EquipmentManager"/> of this agent.
    /// </summary>
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

    /// <summary>
    /// Access the <see cref="AnimationManager"/> of this agent.
    /// </summary>
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

    /// <summary>
    /// Access the <see cref="LimbManager"/> of this agent.
    /// </summary>
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

    /// <summary>
    /// Access the <see cref="CharacteristicManager"/> of this agent.
    /// </summary>
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
    /// Access the <see cref="AgentAudioManager"/> of this agent.
    /// </summary>
    public AgentAudioManager AudioMgr
    {
        get
        {
            if (audioMgr == null)
            {
                audioMgr = GetComponent<AgentAudioManager>();
            }

            return audioMgr;
        }
    }
    AgentAudioManager audioMgr;

    /// <summary>
    /// Every agent has this (including the player) so that they can avoid one another.
    /// </summary>
    protected NavMeshAgent nma;


    /// <summary>
    /// An delegate for when an agent dies.
    /// </summary>
    /// <param name="victim">Victim agent.</param>
    /// <param name="killer">Killer agent.</param>
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
    /// Invoked by <see cref="CharacteristicManager.ApplyDamage(Agent, int)"/> when this <see cref="Agent"/> dies.
    /// It starts the <see cref="AgentDespawnCoroutine"/> to despawn the agent game object.
    /// It also plays a death sound and animation.
    /// Finally, it invokes the <see cref="OnDeath"/> event if it has any subscribers.
    /// </summary>
    /// <param name="killer"></param>
    public void OnThisAgentDeath(Agent killer)
    {
        AnimMgr.PlayDeathAnimation();
        AudioMgr.PlayDeathSound();
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

    /// <summary>
    /// A method to initialize the friendliness indicator of <see cref="AiAgent"/>s.
    /// These indicators are visual game objects that get different colors for friendly and enemy
    /// <see cref="AiAgent"/>s, to help the player distinguish friend from foe.
    /// </summary>
    protected virtual void InitializeFriendlinessIndicator() { }

    /// <summary>
    /// A virtual method mainly used by <see cref="AiAgent"/>s to set their preference towards
    /// using vertical attacks (up/down) more often rather than using all directions equally likely.
    /// They take the distance to their closest friend into account when making this decision.
    /// </summary>
    /// <param name="distanceToClosestFriend">Distance to the closest friend of this agent.</param>
    public virtual void ToggleCombatDirectionPreference(float distanceToClosestFriend) { }

    /// <summary>
    /// A virtual method mainly used by <see cref="AiAgent"/>s to consider a nearby enemy.
    /// An <see cref="AiAgent"/> will change their targetted enemy based on certain conditions,
    /// and this method is meant to provide an enemy to consider for targetting.
    /// Refer to <see cref="AiAgent.ConsiderNearbyEnemy(Agent)"/> for more details.
    /// </summary>
    /// <param name="nearbyEnemy"></param>
    public virtual void ConsiderNearbyEnemy(Agent nearbyEnemy) { }

    /// <summary>
    /// A method mainly used by <see cref="PlayerAgent"/> to see if he is moving backwards,
    /// based on a local movement direction vector.
    /// If so, this information can then be used to apply a movement speed penalty for moving backwards.
    /// Currently, only the <see cref="PlayerAgent"/> uses this, as the <see cref="AiAgent"/>s' navigation
    /// are handled by Unity's <see cref="NavMeshAgent"/> system.
    /// </summary>
    /// <param name="localMoveDir">A movement direction vector
    /// with respect to the local space of this agent.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Initialize the equipment and characteristics of this <see cref="Agent"/>.
    /// After the instantiation of any agent, this method should be used to initialize such things.
    /// All agents will spawn with their equipment and characteristics.
    /// Currently, these things cannot be changed after the spawning has occured.
    /// This is one of the two methods required to fully initialize an Agent.
    /// The other one is <see cref="Agent.InitializePosition(Vector3)"/>.
    /// </summary>
    /// <param name="weaponPrefab">A reference to the <see cref="Weapon"/> prefab.</param>
    /// <param name="headArmorPrefab">A reference to the <see cref="Armor"/> 
    /// prefab of <see cref="Armor.ArmorType.Head"/>.</param>
    /// <param name="torsoArmorPrefab">A reference to the <see cref="Armor"/> 
    /// prefab of <see cref="Armor.ArmorType.Torso"/>.</param>
    /// <param name="handArmorPrefab">A reference to the <see cref="Armor"/> 
    /// prefab of <see cref="Armor.ArmorType.Hand"/>.</param>
    /// <param name="legArmorPrefab">A reference to the <see cref="Armor"/> 
    /// prefab of <see cref="Armor.ArmorType.Leg"/>.</param>
    /// <param name="characteristicPrefab">A reference to the specific <see cref="CharacteristicSet"/>
    /// prefab to initialize the <see cref="CharacteristicManager"/> of this agent.
    /// The default values in <see cref="CharacteristicManager"/> will be used if this argument is null.</param>
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
                , characteristicPrefab.ExtraDamageInflictionMultiplier
                , characteristicPrefab.DamageTakenMultiplier
                , characteristicPrefab.MaximumPoise);
        }

        AudioMgr.InitializeAgentAudioManager();
    }

    /// <summary>
    /// A method to initialize the position of a newly spawned <see cref="Agent"/>.
    /// All agents could potentially use Unity's <see cref="NavMeshAgent"/>, for various reasons.
    /// When using <see cref="Object.Instantiate(Object)"/> to spawn an agent, if the spawn position is not
    /// recognized by Unity's <see cref="NavMesh"/>, then Unity complains.
    /// To avoid these complaints, the <see cref="NavMeshAgent"/> component of agents should be disabled by default.
    /// This method method is used to set the position of a newly spawned agent, and then renable its
    /// <see cref="NavMeshAgent"/> component.
    /// </summary>
    /// <param name="worldPos"></param>
    public virtual void InitializePosition(Vector3 worldPos) { }

    /// <summary>
    /// A virtual method which tells whether the <see cref="Agent"/> is grounded or not.
    /// Mainly used by <see cref="PlayerAgent"/>, as the other agents currently do not jump.
    /// To be grounded means to be touching the ground.
    /// The meanings of <see cref="IsGrounded"/> and <see cref="IsFalling"/> are different.
    /// For example, if an agent is NOT grounded, this doesn't necessarily mean he is falling.
    /// The state of "falling" is based on a timer, to keep the animations from playing too early.
    /// </summary>
    /// <returns>True if the agent is touching the ground; false otherwise</returns>
    public virtual bool IsGrounded() { return true; }

    /// <summary>
    /// A virtual method which tells whether the <see cref="Agent"/> is falling or not.
    /// Mainly used by <see cref="PlayerAgent"/>, as the other agents currently do not jump/fall.
    /// Firstly, only <see cref="PlayerAgent"/> can jump anyway (the others cant).
    /// The meanings of <see cref="IsGrounded"/> and <see cref="IsFalling"/> are different.
    /// For example, if an agent is NOT grounded, this doesn't necessarily mean he is falling.
    /// The state of "falling" is based on a timer, to keep the animations from playing too early.
    /// </summary>
    /// <returns>True if the agent is falling; false otherwise</returns>
    public virtual bool IsFalling() { return false; }

    /// <summary>
    /// Unity's Awake method.
    /// In this case, it is mainly used to set the layer of the agent.
    /// </summary>
    public virtual void Awake()
    {
        gameObject.layer = StaticVariables.AgentLayer;
        IsPlayerAgent = false;
    }

    /// <summary>
    /// Unity's LateUpdate method.
    /// It is used to adjust the spine bone of all agents.
    /// The spine bone is connected to the pelvis bone manually.
    /// It's also rotated about its local X axis, so that the agents can look up and down while attacking.
    /// Since animations are done in Update, any animation related post processing is done in LateUpdate.
    /// Note that this method should not use <see cref="StaticVariables.IsGamePaused"/> to pause the method.
    /// Because, even when the game is paused, we would like the spine bone to be connected and rotated
    /// accordingly, in order to avoid visual discrepancies.
    /// </summary>
    protected virtual void LateUpdate()
    {
        AnimMgr.LateUpdateAnimations();
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
