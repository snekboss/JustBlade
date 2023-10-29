using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerCharacteristicProgressionTracker
{
    const int HealthPerWaveBeaten = 5;
    const float ExtraDamagePerWaveBeaten = 0.01f;
    const float DamageTakenMultiplierPerWaveBeaten = 0.01f;
    const float ExtraMovementSpeedMultiplierPerWaveBeaten = 0.005f;

    public static int ProgressedHealth 
    { 
        get 
        {
            return CharacteristicManager.DefaultMaximumHealth + 
                (HealthPerWaveBeaten * HordeGameLogic.NumberOfWavesBeaten);
        }
    }
    public static float ProgressedModelSize 
    { 
        get 
        { 
            // Player's model size is never changed.
            return CharacteristicManager.DefaultAgentSizeMultiplier;
        }
    }
    public static float ProgressedExtraDamage 
    { 
        get 
        { 
            return CharacteristicManager.DefaultExtraDamageMultiplier + 
                (ExtraDamagePerWaveBeaten * HordeGameLogic.NumberOfWavesBeaten);
        }
    }
    public static float ProgressedDamageTakenMultiplier 
    { 
        get 
        {
            // This value is a "damage taken" multiplier, and thus subtracted.
            return CharacteristicManager.DefaultDamageTakenMultiplier -
                (DamageTakenMultiplierPerWaveBeaten * HordeGameLogic.NumberOfWavesBeaten);
        }
    }
    public static float ProgressedExtraMovementSpeed
    {
        get
        {
            return CharacteristicManager.DefaultExtraMovementSpeedLimitMultiplier +
                (ExtraMovementSpeedMultiplierPerWaveBeaten * HordeGameLogic.NumberOfWavesBeaten);
        }
    }
    public static int ProgressedPoise 
    { 
        get 
        { 
            // Player's poise value is never changed.
            return CharacteristicManager.DefaultMaximumPoise;
        }
    }
}
