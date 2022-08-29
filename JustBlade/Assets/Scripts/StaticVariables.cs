using UnityEngine;

/// <summary>
/// A singleton class that contains some static variables which can be accessed throughout the game.
/// The main behind using a singleton approach is to ensure that Unity's <see cref="LayerMask.NameToLayer(string)"/> method does not complain.
/// It also contains constant names of some strings, as well as some of user options which are meant to be
/// kept in memory until the application is terminated.
/// </summary>
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

    /// <summary>
    /// These are in sync with Unity's <see cref="QualitySettings.names"/>.
    /// </summary>
    enum QualitySetting
    {
        VeryLow = 0,
        Low,
        Medium,
        High,
        VeryHigh,
        Ultra,
    }

    public static float PlayerCameraRotationSpeed = 45.0f;
    public static float PlayerCameraFieldOfView = 60.0f;
    public static float ThirdPersonCameraOffsetYcur = 0.3f;
    public static float ThirdPersonCameraOffsetZcur = 1.0f;
    public static bool IsCameraModeFirstPerson = false;
    public static bool IsGamePaused = false;
    public static int DefaultQualitySetting = (int)(QualitySetting.High);
}

