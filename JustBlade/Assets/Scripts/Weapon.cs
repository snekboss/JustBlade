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

    void OnValidate()
    {
        if (EDIT_MODE)
        {
            InitializeColliderParameters();
        }
    }
}