using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SoundEffectManager
{
    // These strings must be in-sync with the sound effect prefabs located in Resources/SoundEffects.

    static readonly string UnarmoredCut = "UnarmoredCut".ToLower();
    static readonly string UnarmoredBlunt = "UnarmoredBlunt".ToLower();
    static readonly string ArmoredCut = "ArmoredCut".ToLower();
    static readonly string ArmoredBlunt = "ArmoredBlunt".ToLower();
    static readonly string ObjectHit = "ObjectHit".ToLower();
    static readonly string WoodenBlock = "WoodenBlock".ToLower();
    static readonly string MetalBlock = "MetalBlock".ToLower();

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
                    sound = GameObject.Instantiate(PrefabManager.SoundsByName[UnarmoredCut]);
                }
                else
                {
                    sound = GameObject.Instantiate(PrefabManager.SoundsByName[ArmoredCut]);
                }
                break;
            case Weapon.WeaponAttackSoundType.Blunt:
                if (isUnarmored)
                {
                    sound = GameObject.Instantiate(PrefabManager.SoundsByName[UnarmoredBlunt]);
                }
                else
                {
                    sound = GameObject.Instantiate(PrefabManager.SoundsByName[ArmoredBlunt]);
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
        PlayAndDestroy sound = GameObject.Instantiate(PrefabManager.SoundsByName[ObjectHit]);
        sound.PlayAndSelfDestruct(soundPlayWorldPos);
    }

    public static void PlayDefendBlockedSound(Agent defender)
    {
        Vector3 soundPlayWorldPos = defender.EqMgr.equippedWeapon.transform.position;
        PlayAndDestroy sound = null;
        switch (defender.EqMgr.equippedWeapon.blockSoundType)
        {
            case Weapon.WeaponDefendSoundType.Wood:
                sound = GameObject.Instantiate(PrefabManager.SoundsByName[WoodenBlock]);
                break;
            case Weapon.WeaponDefendSoundType.Metal:
                sound = GameObject.Instantiate(PrefabManager.SoundsByName[MetalBlock]);
                break;
            default:
                break;
        }

        sound.PlayAndSelfDestruct(soundPlayWorldPos);
    }
}
