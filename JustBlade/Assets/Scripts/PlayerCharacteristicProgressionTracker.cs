using UnityEngine;

/// <summary>
/// A static class to track the player's progression.
/// Every agent has characteristics which are managed by the <see cref="CharacteristicManager"/>.
/// In the Horde game mode, many agents have advanced characteristics.
/// In order to allow the player "keep up" with the stronger agents, an "automatic leveling up" system
/// is introduced, which is done by this class.
/// The leveling up process is done automatically to avoid creating yet another user interface.
/// Note that the agents in the horde game mode use <see cref="CharacteristicSet"/> prefab objects
/// to receive their characteristic information. There is no such prefab for the <see cref="PlayerAgent"/>,
/// so a GAME OBJECT (not a prefab) is created at runtime each time the player's <see cref="CharacteristicSet"/>
/// is requested. See <see cref="PlayerCharSet"/> for more info.
/// </summary>
public static class PlayerCharacteristicProgressionTracker
{
    const int HealthPerWaveBeaten = 5;
    const float ExtraDamageInflictionMultiplierPerWaveBeaten = 0.01f;
    const float DamageTakenMultiplierPerWaveBeaten = 0.01f;
    const float ExtraMovementSpeedMultiplierPerWaveBeaten = 0.005f;

    /// <summary>
    /// Gets the <see cref="CharacteristicSet"/> of the 
    /// <see cref="PlayerAgent"/> based on <see cref="HordeGameLogic.NumberOfWavesBeaten"/>.
    /// There is no prefab for player's characteristics, therefore a game object is instantiated
    /// at runtime if it doesn't exist. Note that Unity destroys all game objects during
    /// scene transition. This will cause the aforementioned game object to be instantiated again.
    /// However, it'll only happen once per scene, so it's not a big deal.
    /// </summary>
    public static CharacteristicSet PlayerCharSet 
    { 
        get 
        {
            if (playerCharSet == null)
            {
                GameObject go = new GameObject("Player Characteristic Set Game Object");
                playerCharSet = go.AddComponent<CharacteristicSet>();

                // Below values are never changed for the player.
                playerCharSet.ModelSizeMultiplier = CharacteristicManager.DefaultAgentSizeMultiplier;
                playerCharSet.MaximumPoise = CharacteristicManager.DefaultMaximumPoise;


                // Below values are determined per wave beaten.
                playerCharSet.MaximumHealth = CharacteristicManager.DefaultMaximumHealth +
                (HealthPerWaveBeaten * HordeGameLogic.NumberOfWavesBeaten);

                playerCharSet.ExtraDamageInflictionMultiplier = 
                    CharacteristicManager.DefaultExtraDamageInflictionMultiplier +
                (ExtraDamageInflictionMultiplierPerWaveBeaten * HordeGameLogic.NumberOfWavesBeaten);

                // This is a "damage taken" multiplier, and thus it there is subtraction.
                playerCharSet.DamageTakenMultiplier =
                    CharacteristicManager.DefaultDamageTakenMultiplier -
                (DamageTakenMultiplierPerWaveBeaten * HordeGameLogic.NumberOfWavesBeaten);

                playerCharSet.ExtraMovementSpeedLimitMultiplier =
                    CharacteristicManager.DefaultExtraMovementSpeedLimitMultiplier +
                (ExtraMovementSpeedMultiplierPerWaveBeaten * HordeGameLogic.NumberOfWavesBeaten);

            }

            return playerCharSet;
        }       
    }
    static CharacteristicSet playerCharSet;
}
