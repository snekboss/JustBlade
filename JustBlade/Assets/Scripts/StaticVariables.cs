using UnityEngine;

/// <summary>
/// TODO: The method mentioned below doesn't complain? :D
/// A singleton class that contains some static variables which can be accessed throughout the game.
/// The main behind using a singleton approach is to ensure that Unity's <see cref="LayerMask.NameToLayer(string)"/> method does not complain.
/// It also contains constant names of some strings, as well as some of user options which are meant to be
/// kept in memory until the application is terminated.
/// </summary>
public class StaticVariables
{
    public const string DefaultLayerName = "Default";
    public const string AgentLayerName = "Agent";
    public const string WeaponLayerName = "Weapon";
    public const string LimbLayerName = "Limb";
    public const string NoCollisionLayerName = "NoCollision";

    public static LayerMask DefaultLayer { get; private set; } = LayerMask.NameToLayer(DefaultLayerName);
    public static LayerMask AgentLayer { get; private set; } = LayerMask.NameToLayer(AgentLayerName);
    public static LayerMask WeaponLayer { get; private set; } = LayerMask.NameToLayer(WeaponLayerName);
    public static LayerMask LimbLayer { get; private set; } = LayerMask.NameToLayer(LimbLayerName);
    public static LayerMask NoCollisionLayer { get; private set; } = LayerMask.NameToLayer(NoCollisionLayerName);

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

    public const float PlayerCameraRotationSpeedMin = 0.1f;
    public const float PlayerCameraRotationSpeedMax = 10.0f;
    public static float PlayerCameraRotationSpeed = 1.0f;

    public const int PlayerCameraFieldOfViewMin = 45;
    public const int PlayerCameraFieldOfViewMax = 110;
    public static int PlayerCameraFieldOfView = 60;

    public static float ThirdPersonCameraOffsetYcur = 0.5f;
    public static float ThirdPersonCameraOffsetZcur = 1.5f;
    public static bool IsCameraModeFirstPerson = false;
    public static bool IsGamePaused = false;

    public const float SoundSettingMin = 0f;
    public const float SoundSettingMax = 1f;
    public static float SoundSetting = SoundSettingMax;

    public const float DifficultySettingMin = 0.5f;
    public const float DifficultySettingMax = 1.5f;
    public static float DifficultySetting = 1f;

    public static int DefaultQualitySetting = (int)(QualitySetting.High);
}

