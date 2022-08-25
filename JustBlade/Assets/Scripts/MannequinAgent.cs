using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MannequinAgent : Agent
{
    void Start()
    {
        Vector3 lookDir = Vector3.zero - transform.position;
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
    }

    public override void RequestEquipmentSet(out Weapon weaponPrefab
        , out Armor headArmorPrefab
        , out Armor torsoArmorPrefab
        , out Armor handArmorPrefab
        , out Armor legArmorPrefab)
    {
        weaponPrefab = PrefabManager.Weapons[TournamentVariables.PlayerChosenWeaponIndex];

        headArmorPrefab = PrefabManager.HeadArmors[TournamentVariables.PlayerChosenHeadArmorIndex];
        torsoArmorPrefab = PrefabManager.TorsoArmors[TournamentVariables.PlayerChosenTorsoArmorIndex];
        handArmorPrefab = PrefabManager.HandArmors[TournamentVariables.PlayerChosenHandArmorIndex];
        legArmorPrefab = PrefabManager.LegArmors[TournamentVariables.PlayerChosenLegArmorIndex];
    }

    void Update()
    {
        AnimMgr.UpdateAnimations(Vector2.zero, 0, true, false, false);
    }
}
