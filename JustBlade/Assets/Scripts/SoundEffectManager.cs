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
    static readonly string[] UnarmoredBlunt = { "unarmored_blunt_1" };
    static readonly string[] ArmoredCut = { "armored_cut_1" };
    static readonly string[] ArmoredBlunt = { "armored_blunt_1" };
    static readonly string[] ObjectHit = { "object_hit_1" };
    static readonly string[] WoodenBlock = { "wooden_block_1" };
    static readonly string[] MetalBlock = { "metal_block_1" };

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

        PlayAndDestroy sound = null;
        switch (weaponSoundType)
        {
            case Weapon.WeaponAttackSoundType.Cut:
                if (isUnarmored)
                {
                    sound = GameObject.Instantiate(PrefabManager.SoundsByName[GetRandomSound(UnarmoredCut)]);
                }
                else
                {
                    sound = GameObject.Instantiate(PrefabManager.SoundsByName[GetRandomSound(ArmoredCut)]);
                }
                break;
            case Weapon.WeaponAttackSoundType.Blunt:
                if (isUnarmored)
                {
                    sound = GameObject.Instantiate(PrefabManager.SoundsByName[GetRandomSound(UnarmoredBlunt)]);
                }
                else
                {
                    sound = GameObject.Instantiate(PrefabManager.SoundsByName[GetRandomSound(ArmoredBlunt)]);
                }
                break;
            default:
                break;
        }

        Vector3 soundPlayWorldPos = attacker.EqMgr.equippedWeapon.transform.position;
        sound.PlayAndSelfDestruct(soundPlayWorldPos);
    }

    public static void PlayObjectHitSound(Vector3 soundPlayWorldPos)
    {
        PlayAndDestroy sound = GameObject.Instantiate(PrefabManager.SoundsByName[GetRandomSound(ObjectHit)]);
        sound.PlayAndSelfDestruct(soundPlayWorldPos);
    }

    public static void PlayDefendBlockedSound(Agent defender)
    {
        Vector3 soundPlayWorldPos = defender.EqMgr.equippedWeapon.transform.position;
        PlayAndDestroy sound = null;
        switch (defender.EqMgr.equippedWeapon.blockSoundType)
        {
            case Weapon.WeaponDefendSoundType.Wood:
                sound = GameObject.Instantiate(PrefabManager.SoundsByName[GetRandomSound(WoodenBlock)]);
                break;
            case Weapon.WeaponDefendSoundType.Metal:
                sound = GameObject.Instantiate(PrefabManager.SoundsByName[GetRandomSound(MetalBlock)]);
                break;
            default:
                break;
        }

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
