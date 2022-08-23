using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public bool EDIT_MODE;

    public enum ColliderDirection
    {
        AxisX = 0,
        AxisY = 1,
        AxixZ = 2,
    }

    public enum WeaponType
    {
        TwoHanded = 0,
        Polearm = 1,
    }

    public GameObject weaponVisual;
    [Range(0.01f, 20.0f)]
    public float weaponLength;

    [Range(0.001f, 3.0f)]
    public float weaponRadius;

    public ColliderDirection colDirection;
    public bool isInverseDirection;

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

    Rigidbody rbody;
    BoxCollider col;
    Agent ownerAgent;
    [SerializeField] bool isDmgAlreadyApplied;

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

    public void InitializeOwnerAgent(Agent ownerAgent)
    {
        this.ownerAgent = ownerAgent;
    }

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

    void Awake()
    {
        InitializeColliderParameters();
        InitializeRigidbodyParameters();
    }

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

    void OnValidate()
    {
        if (EDIT_MODE)
        {
            InitializeColliderParameters();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (isDmgAlreadyApplied || ownerAgent == null || ownerAgent.IsDead || ownerAgent.AnimMgr.IsAttacking == false
            || ownerAgent.AnimMgr.IsGettingHurt == true) // doesn't really work, since the Agent could just be Holding the attack...
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

            // Check if the defender was able to block the attacker's attack.
            // If so, set the correct "bounce" and "block" animations.
            if (CombatMechanics.IsDefenderAbleToBlock(attacker, defender))
            {
                attacker.AnimMgr.SetIsAttackBounced(true);
                defender.AnimMgr.SetIsDefBlocked(true);
                isDmgAlreadyApplied = true;
                return;
            }
            else
            {
                // At this point, we know that the defender could not defend, and therefore got hit.
                // So, apply damage.
                CombatMechanics.ApplyDamageToDefender(attacker, defender, defenderLimb.limbType);
                isDmgAlreadyApplied = true;
                return;
            }
        }

        // Check if the aget hit scene geometry.
        if (other.gameObject.layer == StaticVariables.Instance.DefaultLayer)
        {
            // Bounce the attack and return.
            ownerAgent.AnimMgr.SetIsAttackBounced(true);
            isDmgAlreadyApplied = true;
            return;
        }
    }
}
