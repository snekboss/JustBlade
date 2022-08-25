using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TournamentVariables
{
    public static int CurrentRoundNumber = 1;
    public static int MaximumRoundNumber = 6;

    public static int MaxNumAgentsInEachTeamMultiplier = 2;
    public static int MaxNumAgentsInEachTeam {
        get 
        {
            if (CurrentRoundNumber == MaximumRoundNumber)
            {
                return 1;
            }

            return (MaximumRoundNumber - CurrentRoundNumber + 1) * MaxNumAgentsInEachTeamMultiplier;
        }
    }

    static int playerChosenWeaponIndex;
    public static int PlayerChosenWeaponIndex
    {
        get { return playerChosenWeaponIndex; }
        set 
        {
            playerChosenWeaponIndex = value;

            // Loop the index around like a circle.
            if (playerChosenWeaponIndex >= PrefabManager.Weapons.Count)
            {
                playerChosenWeaponIndex = 0;
            }

            if (playerChosenWeaponIndex < 0)
            {
                playerChosenWeaponIndex = PrefabManager.Weapons.Count - 1;
            }
        }
    }


    static int playerChosenHeadArmorIndex;
    public static int PlayerChosenHeadArmorIndex
    {
        get { return playerChosenHeadArmorIndex; }
        set
        {
            playerChosenHeadArmorIndex = value;

            // Loop the index around like a circle.
            if (playerChosenHeadArmorIndex >= PrefabManager.HeadArmors.Count)
            {
                playerChosenHeadArmorIndex = 0;
            }

            if (playerChosenHeadArmorIndex < 0)
            {
                playerChosenHeadArmorIndex = PrefabManager.HeadArmors.Count - 1;
            }
        }
    }

    static int playerChosenTorsoArmorIndex;
    public static int PlayerChosenTorsoArmorIndex
    {
        get { return playerChosenTorsoArmorIndex; }
        set
        {
            playerChosenTorsoArmorIndex = value;

            // Loop the index around like a circle.
            if (playerChosenTorsoArmorIndex >= PrefabManager.TorsoArmors.Count)
            {
                playerChosenTorsoArmorIndex = 0;
            }

            if (playerChosenTorsoArmorIndex < 0)
            {
                playerChosenTorsoArmorIndex = PrefabManager.TorsoArmors.Count - 1;
            }
        }
    }

    static int playerChosenHandArmorIndex;
    public static int PlayerChosenHandArmorIndex
    {
        get { return playerChosenHandArmorIndex; }
        set
        {
            playerChosenHandArmorIndex = value;

            // Loop the index around like a circle.
            if (playerChosenHandArmorIndex >= PrefabManager.HandArmors.Count)
            {
                playerChosenHandArmorIndex = 0;
            }

            if (playerChosenHandArmorIndex < 0)
            {
                playerChosenHandArmorIndex = PrefabManager.HandArmors.Count - 1;
            }
        }
    }

    static int playerChosenLegArmorIndex;
    public static int PlayerChosenLegArmorIndex
    {
        get { return playerChosenLegArmorIndex; }
        set
        {
            playerChosenLegArmorIndex = value;

            // Loop the index around like a circle.
            if (playerChosenLegArmorIndex >= PrefabManager.LegArmors.Count)
            {
                playerChosenLegArmorIndex = 0;
            }

            if (playerChosenLegArmorIndex < 0)
            {
                playerChosenLegArmorIndex = PrefabManager.LegArmors.Count - 1;
            }
        }
    }

    public static bool IsPlayerEliminated = false;

    public static bool IsTournamentEnded { get { return IsPlayerEliminated || CurrentRoundNumber > MaximumRoundNumber; } }
}
