using UnityEngine;

/// <summary>
/// A singleton class that contains some static variables which can be accessed throughout the game.
/// These static fields are "very general", so it makes sense to have them be stored here throughout the entire game.
/// </summary>
public class StaticVariables
{
    /// <summary>
    /// The layer name "Default" for Unity's physics/collision system.
    /// </summary>
    public const string DefaultLayerName = "Default";
    /// <summary>
    /// The layer name "Agent" for Unity's physics/collision system.
    /// </summary>
    public const string AgentLayerName = "Agent";
    /// <summary>
    /// The layer name "Weapon" for Unity's physics/collision system.
    /// </summary>
    public const string WeaponLayerName = "Weapon";
    /// <summary>
    /// The layer name "Limb" for Unity's physics/collision system.
    /// </summary>
    public const string LimbLayerName = "Limb";
    /// <summary>
    /// The layer name "NoCollision" for Unity's physics/collision system.
    /// </summary>
    public const string NoCollisionLayerName = "NoCollision";

    /// <summary>
    /// A layer mask which represents the default layer.
    /// </summary>
    public static LayerMask DefaultLayer { get; private set; } = LayerMask.NameToLayer(DefaultLayerName);
    /// <summary>
    /// A layer mask which represents the agent layer.
    /// </summary>
    public static LayerMask AgentLayer { get; private set; } = LayerMask.NameToLayer(AgentLayerName);
    /// <summary>
    /// A layer mask which represents the weapon layer.
    /// </summary>
    public static LayerMask WeaponLayer { get; private set; } = LayerMask.NameToLayer(WeaponLayerName);
    /// <summary>
    /// A layer mask which represents the limb layer.
    /// </summary>
    public static LayerMask LimbLayer { get; private set; } = LayerMask.NameToLayer(LimbLayerName);
    /// <summary>
    /// A layer mask which represents the no collision layer.
    /// </summary>
    public static LayerMask NoCollisionLayer { get; private set; } = LayerMask.NameToLayer(NoCollisionLayerName);

    /// <summary>
    /// The name of human model's head game object.
    /// </summary>
    public const string HumanHeadName = "human_head";
    /// <summary>
    /// The name of human model's torso game object.
    /// </summary>
    public const string HumanTorsoName = "human_body_with_shorts";
    /// <summary>
    /// The name of human model's hand game object.
    /// </summary>
    public const string HumanHandsName = "human_hands";
    /// <summary>
    /// The name of human model's legs game object.
    /// </summary>
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

    /// <summary>
    /// Minimum rotation speed of the player controlled camera.
    /// </summary>
    public const float PlayerCameraRotationSpeedMin = 0.1f;
    /// <summary>
    /// Maximum rotation speed of the player controlled camera.
    /// </summary>
    public const float PlayerCameraRotationSpeedMax = 10.0f;
    /// <summary>
    /// The current rotation speed of the player controlled camera.
    /// </summary>
    public static float PlayerCameraRotationSpeed = 1.0f;

    /// <summary>
    /// Minimum field of view value of the player controlled camera.
    /// </summary>
    public const int PlayerCameraFieldOfViewMin = 45;
    /// <summary>
    /// Maximum field of view value of the player controlled camera.
    /// </summary>
    public const int PlayerCameraFieldOfViewMax = 110;
    /// <summary>
    /// The current field of view value of the player controlled camera.
    /// </summary>
    public static int PlayerCameraFieldOfView = 60;


    /// <summary>
    /// The current Y offset (vertical) of the player controlled camera.
    /// </summary>
    public static float ThirdPersonCameraOffsetYcur = 0.5f;
    /// <summary>
    /// The current Z offset (depth) of the player controlled camera.
    /// </summary>
    public static float ThirdPersonCameraOffsetZcur = 1.5f;
    /// <summary>
    /// True if the player's camera should be in first person view mode; false for third person view mode.
    /// </summary>
    public static bool IsCameraModeFirstPerson = false;
    /// <summary>
    /// True if the game is paused; false otherwise.
    /// </summary>
    public static bool IsGamePaused = false;

    /// <summary>
    /// Minimum sound level (master volume) setting.
    /// </summary>
    public const float SoundSettingMin = 0f;
    /// <summary>
    /// Maximum sound level (master volume) setting.
    /// </summary>
    public const float SoundSettingMax = 1f;
    /// <summary>
    /// The current sound level (master volume) setting.
    /// </summary>
    public static float SoundSetting = SoundSettingMax;

    /// <summary>
    /// Minimum difficulty setting for the game.
    /// </summary>
    public const float DifficultySettingMin = 0.25f;
    /// <summary>
    /// Maximum difficulty setting for the game.
    /// </summary>
    public const float DifficultySettingMax = 1.5f;
    /// <summary>
    /// The current difficulty setting for the game.
    /// </summary>
    public static float DifficultySetting = (DifficultySettingMin + DifficultySettingMax) / 2f;

    /// <summary>
    /// The default quality setting.
    /// See also: <see cref="QualitySetting"/>.
    /// </summary>
    public static int DefaultQualitySetting = (int)(QualitySetting.High);
}

