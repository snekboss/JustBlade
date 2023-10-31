using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// TODO: Write summary.
/// Note that the readonly strings defined in this class must be in-sync with the sound effect prefabs
/// located in Resources/SoundEffects.
/// The sound effect prefabs in this folder are objects of type <see cref="PlayAndDestroy"/>,
/// and their names are determined by <see cref="PlayAndDestroy.soundName"/>, which must be unique.
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
        "armored_blunt_1",
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
            attackerIsStabbing ? attacker.EqMgr.equippedWeapon.stabSoundType : attacker.EqMgr.equippedWeapon.swingSoundType;

        Vector3 soundPlayWorldPos = attacker.EqMgr.equippedWeapon.transform.position;

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

    public static void PlayObjectHitSound(Vector3 soundPlayWorldPos)
    {
        PlaySound(ObjectHit, soundPlayWorldPos);
    }

    public static void PlayDefendBlockedSound(Agent defender)
    {
        Vector3 soundPlayWorldPos = defender.EqMgr.equippedWeapon.transform.position;
        switch (defender.EqMgr.equippedWeapon.blockSoundType)
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

    public static void PlayHurtSound(Vector3 soundPlayWorldPos) 
    {
        PlaySound(Hurt, soundPlayWorldPos);
    }
    public static void PlayGruntSound(Vector3 soundPlayWorldPos) 
    {
        PlaySound(Grunt, soundPlayWorldPos);
    }
    public static void PlayDeathSound(Vector3 soundPlayWorldPos) 
    {
        PlaySound(Death, soundPlayWorldPos);
    }

    public static void PlayWhiffSound(Vector3 soundPlayWorldPos)
    {
        PlaySound(Whiff, soundPlayWorldPos);
    }

    public static void PlayFootstepSound(Vector3 soundPlayWorldPos)
    {
        PlaySound(FootstepGrass, soundPlayWorldPos);
    }

    static void PlaySound(string[] soundArray, Vector3 soundPlayWorldPos)
    {
        PlayAndDestroy sound = GameObject.Instantiate(PrefabManager.SoundsByName[GetRandomSound(soundArray)]);
        sound.PlayAndSelfDestruct(soundPlayWorldPos);
    }

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
