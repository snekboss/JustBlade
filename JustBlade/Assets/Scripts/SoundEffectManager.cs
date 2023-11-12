using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// A static class which manages the playing of the sound effects in the game.
/// Note that the readonly strings defined in this class must be in-sync with the sound effect prefabs
/// located in "Resources/SoundEffects".
/// The sound effect prefabs in this folder are objects of type <see cref="PlayAndDestroy"/>,
/// and their names are determined by <see cref="PlayAndDestroy.soundName"/>, which must be unique.
/// The class and its fields are static, because the instance based alternative would have us involve
/// managing game objects from scene to scene, since Unity destroys all contents of an open scene before
/// transitioning to another one. For a game of this size, I think the static class approach is sufficient.
/// </summary>
public static class SoundEffectManager
{
    // These strings must be in-sync with the sound effect prefabs located in Resources/SoundEffects.

    static readonly string[] UnarmoredCut = { "unarmored_cut_1" };
    static readonly string[] UnarmoredBlunt = { "unarmored_blunt_1", };
    static readonly string[] ArmoredCut = {
        "armored_cut_1",
        "armored_cut_2",
        "armored_cut_3",
        "armored_cut_4",
    };
    static readonly string[] ArmoredBlunt = {
        //"armored_blunt_1", // this one doesn't sound as meaty as the others.
        "armored_blunt_2",
        "armored_blunt_3",
        "armored_blunt_4",
        "armored_blunt_5",
    };
    static readonly string[] ObjectHit = { "object_hit_1" };
    static readonly string[] WoodenBlock = { "wooden_block_1" };
    static readonly string[] MetalBlock = { "metal_block_1", "metal_block_2" };
    static readonly string[] FootstepGrass = { 
        "footstep_grass_1", 
        "footstep_grass_2", 
        "footstep_grass_3", 
        "footstep_grass_4", 
        "footstep_grass_5",
    };
    static readonly string[] Hurt = {
        "hurt_1",
        "hurt_2",
        "hurt_3",
        "hurt_4",
        "hurt_5",
        "hurt_6",
        "hurt_7",
        "hurt_8",
    };
    static readonly string[] Grunt = {
        "grunt_1",
        "grunt_2",
        "grunt_3",
        "grunt_4",
        "grunt_5",
        "grunt_6",
        "grunt_7",
        "grunt_8",
        "grunt_9",
        "grunt_10",
        "grunt_11",
    };
    static readonly string[] Death = {
        "death_1",
        "death_2",
        "death_3",
        "death_4",
        "death_5",
        "death_6",
        "death_7",
    };
    static readonly string[] Whiff = { "whiff_1", "whiff_2" };
    static readonly string[] Coin = {
        "coin_1",
        "coin_2",
        "coin_3",
    };
    static readonly string[] Button = { "button_1" };

    /// <summary>
    /// Plays a sound for when an agent is struck by a weapon.
    /// The sound is played at the world position of the attacker's <see cref="Weapon"/>.
    /// The sound is based on the <see cref="Weapon.WeaponAttackSoundType"/>, and the <see cref="Armor.ArmorLevel"/>
    /// of the <see cref="Limb.LimbType"/> which was struck.
    /// </summary>
    /// <param name="attacker">Attacker agent.</param>
    /// <param name="defender">Defender agent.</param>
    /// <param name="limbType">Limb that was struck.</param>
    public static void PlayWeaponSoundOnStruckAgent(Agent attacker, Agent defender, Limb.LimbType limbType)
    {
        Armor.ArmorLevel limbArmorLevel = Armor.ArmorLevel.None;

        switch (limbType)
        {
            case Limb.LimbType.Head:
                limbArmorLevel = defender.EqMgr.HeadArmorLevel;
                break;
            case Limb.LimbType.Torso:
                limbArmorLevel = defender.EqMgr.TorsoArmorLevel;
                break;
            case Limb.LimbType.Legs:
                limbArmorLevel = defender.EqMgr.LegArmorLevel;
                break;
            default:
                break;
        }

        bool isUnarmored = limbArmorLevel == Armor.ArmorLevel.None || limbArmorLevel == Armor.ArmorLevel.Light;

        bool attackerIsStabbing = attacker.AnimMgr.IsAttackingFromDown;

        Weapon.WeaponAttackSoundType weaponSoundType =
            attackerIsStabbing ? attacker.EqMgr.EquippedWeapon.stabSoundType : attacker.EqMgr.EquippedWeapon.swingSoundType;

        Vector3 soundPlayWorldPos = attacker.EqMgr.EquippedWeapon.transform.position;

        switch (weaponSoundType)
        {
            case Weapon.WeaponAttackSoundType.Cut:
                if (isUnarmored)
                {
                    PlaySound(UnarmoredCut, soundPlayWorldPos);
                }
                else
                {
                    PlaySound(ArmoredCut, soundPlayWorldPos);
                }
                break;
            case Weapon.WeaponAttackSoundType.Blunt:
                if (isUnarmored)
                {
                    PlaySound(UnarmoredBlunt, soundPlayWorldPos);
                }
                else
                {
                    PlaySound(ArmoredBlunt, soundPlayWorldPos);
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Plays a sound for when an agent has struck an object on the scene.
    /// This is just a "generic object hit sound effect" for general use.
    /// </summary>
    /// <param name="soundPlayWorldPos">World position to play the sound.</param>
    public static void PlayObjectHitSound(Vector3 soundPlayWorldPos)
    {
        PlaySound(ObjectHit, soundPlayWorldPos);
    }

    /// <summary>
    /// Plays a sound for when an agent has successfully defended against an attack.
    /// The sound depends on the defender's <see cref="Weapon.WeaponDefendSoundType"/>.
    /// The sound is played at the world coordinates of the defender's weapon.
    /// </summary>
    /// <param name="defender"></param>
    public static void PlayDefendBlockedSound(Agent defender)
    {
        Vector3 soundPlayWorldPos = defender.EqMgr.EquippedWeapon.transform.position;
        switch (defender.EqMgr.EquippedWeapon.blockSoundType)
        {
            case Weapon.WeaponDefendSoundType.Wood:
                PlaySound(WoodenBlock, soundPlayWorldPos);
                break;
            case Weapon.WeaponDefendSoundType.Metal:
                PlaySound(MetalBlock, soundPlayWorldPos);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Plays a sound for when an agent is hurt.
    /// </summary>
    /// <param name="soundPlayWorldPos">World position to play the sound.</param>
    public static void PlayHurtSound(Vector3 soundPlayWorldPos) 
    {
        PlaySound(Hurt, soundPlayWorldPos);
    }

    /// <summary>
    /// Plays a sound for when an agent is meant to grunt.
    /// For example, agents grunt when they are attacking.
    /// </summary>
    /// <param name="soundPlayWorldPos">World position to play the sound.</param>
    public static void PlayGruntSound(Vector3 soundPlayWorldPos) 
    {
        PlaySound(Grunt, soundPlayWorldPos);
    }

    /// <summary>
    /// Plays a sound for when an agent is dead.
    /// </summary>
    /// <param name="soundPlayWorldPos">World position to play the sound.</param>
    public static void PlayDeathSound(Vector3 soundPlayWorldPos) 
    {
        PlaySound(Death, soundPlayWorldPos);
    }

    /// <summary>
    /// Plays a sound for when a weapon has been swung.
    /// </summary>
    /// <param name="soundPlayWorldPos">World position to play the sound.</param>
    public static void PlayWhiffSound(Vector3 soundPlayWorldPos)
    {
        PlaySound(Whiff, soundPlayWorldPos);
    }

    /// <summary>
    /// Plays a sound for when an agent's footstep sound is meant to be played.
    /// </summary>
    /// <param name="soundPlayWorldPos">World position to play the sound.</param>
    public static void PlayFootstepSound(Vector3 soundPlayWorldPos)
    {
        PlaySound(FootstepGrass, soundPlayWorldPos);
    }

    /// <summary>
    /// Plays a coin sound.
    /// It is used when the player earns/loses gold.
    /// </summary>
    /// <param name="soundPlayWorldPos">World position to play the sound.</param>
    public static void PlayCoinSound(Vector3 soundPlayWorldPos)
    {
        PlaySound(Coin, soundPlayWorldPos);
    }

    /// <summary>
    /// Plays a button sound.
    /// </summary>
    /// <param name="soundPlayWorldPos">World position to play the sound.</param>
    public static void PlayButtonSound(Vector3 soundPlayWorldPos)
    {
        PlaySound(Button, soundPlayWorldPos);
    }

    /// <summary>
    /// Plays a random sound from a given array of sound names.
    /// If the game's sound level is 0, the sound is not played (ie, the sound effect game object
    /// is not instantiated, to save performance).
    /// </summary>
    /// <param name="soundArray">An array of sound names, found in <see cref="SoundEffectManager"/>.</param>
    /// <param name="soundPlayWorldPos">World position to play the sound.</param>
    static void PlaySound(string[] soundArray, Vector3 soundPlayWorldPos)
    {
        if (StaticVariables.SoundSetting == 0)
        {
            return;
        }

        PlayAndDestroy sound = GameObject.Instantiate(PrefabManager.SoundsByName[GetRandomSound(soundArray)]);
        sound.PlayAndSelfDestruct(soundPlayWorldPos);
    }

    /// <summary>
    /// Gets a random sound name from a given array of sound names.
    /// If there's only one sound name, then that one is returned without invoking any randomizer methods.
    /// </summary>
    /// <param name="soundNames">An array of sound names to choose from.</param>
    /// <returns>A random sound name.</returns>
    static string GetRandomSound(string[] soundNames)
    {
        if (soundNames.Length == 1)
        {
            return soundNames[0];
        }

        int randIndex = Random.Range(0, soundNames.Length);
        return soundNames[randIndex];
    }
}
