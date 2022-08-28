using UnityEngine;

public class StaticVariables
{
    static StaticVariables instance;
    public static StaticVariables Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new StaticVariables();
            }

            return instance;
        }
    }

    public const string DefaultLayerName = "Default";
    public const string AgentLayerName = "Agent";
    public const string WeaponLayerName = "Weapon";
    public const string LimbLayerName = "Limb";
    public const string NoCollisionLayerName = "NoCollision";

    public LayerMask DefaultLayer { get; private set; } = LayerMask.NameToLayer(DefaultLayerName);
    public LayerMask AgentLayer { get; private set; } = LayerMask.NameToLayer(AgentLayerName);
    public LayerMask WeaponLayer { get; private set; } = LayerMask.NameToLayer(WeaponLayerName);
    public LayerMask LimbLayer { get; private set; } = LayerMask.NameToLayer(LimbLayerName);
    public LayerMask NoCollisionLayer { get; private set; } = LayerMask.NameToLayer(NoCollisionLayerName);

    public const string HumanHeadName = "human_head";
    public const string HumanTorsoName = "human_body_with_shorts";
    public const string HumanHandsName = "human_hands";
    public const string HumanLegsName = "human_feet";

    public static float PlayerCameraRotationSpeed = 45.0f;
    public static float PlayerCameraFieldOfView = 60.0f;
    public static float ThirdPersonCameraOffsetYcur = 0.3f;
    public static float ThirdPersonCameraOffsetZcur = 1.0f;
    public static bool IsCameraModeFirstPerson = false;
}

