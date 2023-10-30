using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class which designates the attached game object as a Weapon.
/// Weapons collide with scene geometry, as well as <see cref="Limb"/>s to damage other <see cref="Agent"/>s.
/// This game's weapon damage system is very similar to Mordhau's, which is another video game.
/// Hence, there are many fields such as <see cref="SwingDmgHeadHeavy"/> etc. which save the weapon damage values.
/// The reason behind having separate fields is so that the name of every damage type is shown in the Inspector value nicely.
/// If another data structure were used to save the damage information, it would have required some serializing in order to show them
/// in the Inpsector menu (that is, if it worked as nicely as the current solution in the first place...).
/// </summary>
public class Weapon : EquippableItem
{
    /// <summary>
    /// This is a switch which is only used in the Inspector Menu of the Unity Editor.
    /// The switch is required to stop the Unity Editor spamming the console window with useless warnings.
    /// When set to true, the changes made to the weapon parameters in the Inspector Menu are shown in the scene view.
    /// See also <see cref="OnValidate"/>.
    /// </summary>
    public bool EDIT_MODE;

    /// <summary>
    /// Used to determine the direction in which the hitbox collider of the weapon extends.
    /// </summary>
    public enum ColliderDirection
    {
        AxisX = 0,
        AxisY = 1,
        AxixZ = 2,
    }

    /// <summary>
    /// The weapon type determines the animations used by the agent.
    /// </summary>
    public enum WeaponType
    {
        TwoHanded = 0,
        Polearm = 1,
    }

    public enum WeaponAttackSoundType
    {
        Cut = 0,
        Blunt,
    }

    public enum WeaponDefendSoundType
    {
        Wood = 0,
        Metal,
    }

    public WeaponAttackSoundType swingSoundType;
    public WeaponAttackSoundType stabSoundType;
    public WeaponDefendSoundType blockSoundType;

    public GameObject weaponVisual; // the mesh of the weapon
    [Range(0.01f, 20.0f)]
    public float weaponLength;

    [Range(0.001f, 3.0f)]
    public float weaponRadius;

    public ColliderDirection colDirection;
    public bool isInverseDirection; // flips the direction of the collider

    public WeaponType weaponType;

    // Damage values
    // Swing damage
    // Head
    public int SwingDmgHeadNaked;
    public int SwingDmgHeadLight;
    public int SwingDmgHeadMed;
    public int SwingDmgHeadHeavy;

    // Torso
    public int SwingDmgTorsoNaked;
    public int SwingDmgTorsoLight;
    public int SwingDmgTorsoMed;
    public int SwingDmgTorsoHeavy;

    // Leg
    public int SwingDmgLegNaked;
    public int SwingDmgLegLight;
    public int SwingDmgLegMed;
    public int SwingDmgLegHeavy;

    // Stab damage
    // Head
    public int StabDmgHeadNaked;
    public int StabDmgHeadLight;
    public int StabDmgHeadMed;
    public int StabDmgHeadHeavy;

    // Torso
    public int StabDmgTorsoNaked;
    public int StabDmgTorsoLight;
    public int StabDmgTorsoMed;
    public int StabDmgTorsoHeavy;

    // Leg
    public int StabDmgLegNaked;
    public int StabDmgLegLight;
    public int StabDmgLegMed;
    public int StabDmgLegHeavy;

    public int AverageSwingDamage
    {
        get
        {
            int sum = SwingDmgHeadNaked + SwingDmgHeadLight + SwingDmgHeadMed + SwingDmgHeadHeavy
                + SwingDmgTorsoNaked + SwingDmgTorsoLight + SwingDmgTorsoMed + SwingDmgTorsoHeavy
                + SwingDmgLegNaked + SwingDmgLegLight + SwingDmgLegMed + SwingDmgLegHeavy;
            float avgFloat = (float)(sum) / 12.0f;
            return Convert.ToInt32(avgFloat);
        }
    }

    public int AverageStabDamage
    {
        get 
        { 
            int sum = StabDmgHeadNaked + StabDmgHeadLight + StabDmgHeadMed + StabDmgHeadHeavy
                + StabDmgTorsoNaked + StabDmgTorsoLight + StabDmgTorsoMed + StabDmgTorsoHeavy
                + StabDmgLegNaked + StabDmgLegLight + StabDmgLegMed + StabDmgLegHeavy;
            float avgFloat = (float)(sum) / 12.0f;
            return Convert.ToInt32(avgFloat);
        }
    }

    Rigidbody rbody;
    BoxCollider col;
    Agent ownerAgent;

    bool isDmgAlreadyApplied; // whether or not any damage was applied, the weapon is only allowed to hit one thing per swing.

    Vector3 ColDirectionVec
    {
        get
        {
            if (colDirection == ColliderDirection.AxisX)
            {
                return isInverseDirection ? Vector3.left : Vector3.right;
            }
            else if (colDirection == ColliderDirection.AxisY)
            {
                return isInverseDirection ? Vector3.down : Vector3.up;
            }
            else
            {
                return isInverseDirection ? Vector3.back : Vector3.forward;
            }
        }
    }

    Vector3 ColDimensionVec
    {
        get
        {
            if (colDirection == ColliderDirection.AxisX)
            {
                return new Vector3(weaponLength, weaponRadius, weaponRadius);
            }
            else if (colDirection == ColliderDirection.AxisY)
            {
                return new Vector3(weaponRadius, weaponLength, weaponRadius);
            }
            else
            {
                return new Vector3(weaponRadius, weaponRadius, weaponLength);
            }
        }

    }

    /// <summary>
    /// Lets this weapon object know which agent owns it, by setting the <see cref="ownerAgent"/> field.
    /// </summary>
    /// <param name="ownerAgent">The agent which owns this weapon.</param>
    public void InitializeOwnerAgent(Agent ownerAgent)
    {
        this.ownerAgent = ownerAgent;
    }

    /// <summary>
    /// Toggles the collision of this weapon by swapping its layer back and forth:
    /// - Use "Weapon" layer to enable collision.
    /// - Use "NoCollision" layer to disable collision.
    /// </summary>
    /// <param name="canCollide">True collision is to be enabled; false otherwise.</param>
    public void SetCollisionAbility(bool canCollide)
    {
        if (canCollide)
        {
            if (gameObject.layer == StaticVariables.Instance.NoCollisionLayer)
            {
                gameObject.layer = StaticVariables.Instance.WeaponLayer;
            }
        }
        else
        {
            if (gameObject.layer == StaticVariables.Instance.WeaponLayer)
            {
                gameObject.layer = StaticVariables.Instance.NoCollisionLayer;
            }
        }
    }

    /// <summary>
    /// Initializes the collider hitbox parameters based on <see cref="weaponLength"/> and <see cref="weaponRadius"/>.
    /// </summary>
    void InitializeColliderParameters()
    {
        col = gameObject.GetComponent<BoxCollider>();

        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
        }

        col.isTrigger = true;

        Vector3 posStart = weaponVisual.transform.localPosition;
        col.center = posStart + (ColDirectionVec * weaponLength / 2);

        col.size = ColDimensionVec;

        gameObject.layer = StaticVariables.Instance.WeaponLayer;
    }

    /// <summary>
    /// Initializes the rigidbody of this weapon.
    /// The weapon will not be affected by gravity; and is kinematic.
    /// If a rigidbody doesn't exist, a default one is generated.
    /// </summary>
    void InitializeRigidbodyParameters()
    {
        rbody = gameObject.GetComponent<Rigidbody>();
        if (rbody == null)
        {
            rbody = gameObject.AddComponent<Rigidbody>();
        }

        rbody.isKinematic = true;
        rbody.useGravity = false;
    }

    /// <summary>
    /// Unity's Awake method.
    /// In this case, it initialies the weapon hitbox by invoking
    /// <see cref="InitializeColliderParameters"/> and <see cref="InitializeRigidbodyParameters"/>.
    /// </summary>
    void Awake()
    {
        InitializeColliderParameters();
        InitializeRigidbodyParameters();
    }

    /// <summary>
    /// Unity's Update method.
    /// In this case, it keeps track of whether the weapon is allowed to apply damage during collision with <see cref="Limb"/>s.
    /// If the <see cref="ownerAgent"/> is currently attacking, then the weapon is allowed to apply damage ONCE.
    /// If the <see cref="ownerAgent"/> is dead, then this method returns without doing anything.
    /// </summary>
    void Update()
    {
        if (ownerAgent == null || ownerAgent.IsDead)
        {
            return;
        }

        if (isDmgAlreadyApplied && ownerAgent.AnimMgr.IsAttacking == false)
        {
            // If the ownerAgent has stopped attacking, reset this switch so that the agent can apply damage next time they decide to attack.
            isDmgAlreadyApplied = false;
        }
    }

    /// <summary>
    /// A UnityEditor method.
    /// It is used to see the visual changes of the weapon hitbox collider in scene view of the editor.
    /// The changes to the weapon hitbox collider are applied only if <see cref="EDIT_MODE"/> is set to true.
    /// </summary>
    void OnValidate()
    {
        if (EDIT_MODE)
        {
            InitializeColliderParameters();
        }
    }

    /// <summary>
    /// Unity's OnTriggerStay method.
    /// In this case, it contains the logic behind any <see cref="Weapon"/> object making collision with other objects in the game.
    /// </summary>
    /// <param name="other">The collider of the object which entered the trigger area of this weapon object.</param>
    void OnTriggerStay(Collider other)
    {
        if (isDmgAlreadyApplied || ownerAgent == null || ownerAgent.IsDead || ownerAgent.AnimMgr.IsAttacking == false)
        {
            return;
        }

        // Check if the agent hit a limb.
        if (other.gameObject.layer == StaticVariables.Instance.LimbLayer)
        {
            Limb defenderLimb = other.gameObject.GetComponent<Limb>();
            if (defenderLimb.OwnerAgent == ownerAgent)
            {
                // Ignore if agent hit their own limbs.
                return;
            }

            // At this point, we know we hit another agent's limb.
            Agent attacker = ownerAgent;
            Agent defender = defenderLimb.OwnerAgent;

            // If the defender agent is already dead, then ignore.
            if (defender.IsDead)
            {
                return;
            }

            // Check if the defender is friendly. If so, don't damage the friend.
            bool isFriendly =
                (attacker.IsFriendOfPlayer == true && defender.IsFriendOfPlayer == true) 
             || (attacker.IsFriendOfPlayer == false && defender.IsFriendOfPlayer == false);

            if (isFriendly)
            {
                // Bounce the attack so that the agents don't use one another as meat shields.
                attacker.AnimMgr.SetIsAttackBounced(true);
                isDmgAlreadyApplied = true;

                SoundEffectManager.PlayObjectHitSound(transform.position);
                return;
            }

            // Check if the defender was able to block the attacker's attack.
            // If so, set the correct "bounce" and "block" animations.
            if (CombatMechanics.IsDefenderAbleToBlock(attacker, defender))
            {
                attacker.AnimMgr.SetIsAttackBounced(true);
                defender.AnimMgr.SetIsDefBlocked(true);
                isDmgAlreadyApplied = true;

                SoundEffectManager.PlayDefendBlockedSound(defender);

                if (defender.IsPlayerAgent)
                {
                    PlayerStatisticsTracker.PlayerTotalSuccessfulBlocks++;
                }

                return;
            }
            else
            {
                // At this point, we know that the defender could not defend, and therefore got hit.
                // So, apply damage.
                CombatMechanics.ApplyDamageToDefender(attacker, defender, defenderLimb.limbType);
                isDmgAlreadyApplied = true;

                SoundEffectManager.PlayWeaponSoundOnStruckAgent(attacker, defender, defenderLimb.limbType);
                return;
            }
        }

        // Check if the agent hit scene geometry.
        if (other.gameObject.layer == StaticVariables.Instance.DefaultLayer)
        {
            // Bounce the attack and return.
            ownerAgent.AnimMgr.SetIsAttackBounced(true);
            isDmgAlreadyApplied = true;
            SoundEffectManager.PlayObjectHitSound(transform.position);
            return;
        }
    }
}
