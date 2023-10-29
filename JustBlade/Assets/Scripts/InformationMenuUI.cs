using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// A script which designates the attached game object as InformationMenuUI.
/// It contains the logic of the controls of the Information Menu UI.
/// The information menu screen is shown during breaks from combat.
/// It is also shown when the player is dead or when the player has beaten the game.
/// </summary>
public class InformationMenuUI : MonoBehaviour
{
    public GameObject screenGameOver;
    public GameObject screenRoundInfo;

    public TextMeshProUGUI txtGameOverHeader;
    public TextMeshProUGUI txtGameOverBody;
    public TextMeshProUGUI txtInformationBody;

    public Button btnReturnToMainMenu;
    public Button btnNext;

    /// <summary>
    /// Unity's Start method.
    /// In this case, it is used to make the mouse cursor visible, and invoke <see cref="UpdateTexts"/>.
    /// </summary>
    void Start()
    {
        Cursor.visible = true;

        UpdateTexts();
    }

    /// <summary>
    /// Returns to the main menu by loading MainMenuScene.
    /// </summary>
    public void OnButtonClick_ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    /// <summary>
    /// Navigates to the gear selection menu by loading GearSelectionMenuScene.
    /// </summary>
    public void OnButtonClick_Next()
    {
        SceneManager.LoadScene("GearSelectionMenuScene");
    }

    /// <summary>
    /// This method updates the necessary UI elements when the game is over.
    /// </summary>
    void UpdateGameOverScreenTexts()
    {
        screenGameOver.SetActive(HordeGameLogic.IsGameEnded);

        string gameOverHeaderStr = "";
        string gameOverBodyStr = "";
        string NL = Environment.NewLine;

        string playerStats = string.Format(
            "Player kill count: {0}" + NL
          + "Player mercenary kill count: {1}" + NL
          + "Total kill count: {2}" + NL
          + NL
          + "Total number of mercenaries hired: {3}" + NL
          + "Total number mercenary upgrades were made: {4}" + NL
          + "Total number of mercenaries died: {5}" + NL
          + NL
          + "Total damage player has inflicted: {6}" + NL
          + "Total damage taken by player: {7}" + NL
          + "Total number of attacks successfully blocked by player: {8}" + NL
          + NL
          + "Total gold earned: {9}" + NL
          + "Total gold spent: {10}" + NL
          + "Remaining gold: {11}"
          , PlayerStatisticsTracker.PlayerTotalKillCount
          , PlayerStatisticsTracker.MercenariesTotalKillCount
          , PlayerStatisticsTracker.PlayerTotalKillCount + PlayerStatisticsTracker.MercenariesTotalKillCount
          , PlayerStatisticsTracker.NumTotalMercenariesHired
          , PlayerStatisticsTracker.NumTotalMercenaryUpgrades
          , PlayerStatisticsTracker.MercenariesTotalDeathCount
          , PlayerStatisticsTracker.PlayerTotalDamageDealt
          , PlayerStatisticsTracker.PlayerTotalDamageTaken
          , PlayerStatisticsTracker.PlayerTotalSuccessfulBlocks
          , PlayerStatisticsTracker.PlayerTotalGoldEarned
          , PlayerStatisticsTracker.PlayerTotalGoldSpent
          , PlayerInventoryManager.PlayerGold
          ); ;

        if (HordeGameLogic.IsPlayerDied)
        {
            gameOverHeaderStr = "Game Over";

            double numBeatenWavesDouble = Convert.ToDouble(HordeGameLogic.NumberOfWavesBeaten);
            double numTotalWavesDouble = Convert.ToDouble(HordeGameLogic.TotalNumberOfWaves);

            double percentage = numBeatenWavesDouble / numTotalWavesDouble;

            gameOverBodyStr = "";

            if (percentage < 0.25d)
            {
                gameOverBodyStr += "Your journey ends here, having repelled only a few of the enemy hordes. Your courage was commendable, but the enemy was relentless.";
            }
            else if (percentage < 0.5d)
            {
                gameOverBodyStr += "You fought valiantly, repelling several of the enemy hordes. Your strength wavered, and the enemy seized their chance.";
            }
            else if (percentage < 0.75d)
            {
                gameOverBodyStr += "Many enemy hordes fell before you. Yet, in the end, their numbers overwhelmed you.";
            }
            else /*if (percentage < 1.0d)*/
            {
                gameOverBodyStr += "You stood on the brink of victory, having repelled most of the enemy hordes. But in the final moments, fate turned against you.";
            }

            gameOverBodyStr += "Remember, every defeat is a step towards victory. Stand up, fight again, and claim your glory!";
        }
        else
        {
            gameOverHeaderStr = "Congratulations!";

            gameOverBodyStr = string.Format(
                "Against all odds, you have repelled all {0} waves of the enemy hordes. Their mightiest have fallen before you, and the enemy, recognizing your indomitable spirit, have withdrawn their forces. You stand victorious."
                , HordeGameLogic.TotalNumberOfWaves);
        }

        gameOverBodyStr += NL + NL + playerStats;

        txtGameOverHeader.text = gameOverHeaderStr;
        txtGameOverBody.text = gameOverBodyStr;
    }

    /// <summary>
    /// This method updates the necessary UI elements while the game isn't over,
    /// in order to show information to the player.
    /// </summary>
    void UpdateRoundInfoScreenTexts()
    {
        screenRoundInfo.SetActive(HordeGameLogic.IsGameEnded == false);

        string infoStr = "";

        string NL = Environment.NewLine;

        if (HordeGameLogic.IsGameHasJustBegun)
        {
            infoStr += "In the aftermath of a crushing defeat, you, once a respected commander, have been stripped of your titles, wealth, and possessions. Your liege lord, in a final act of mercy, has spared your life but exiled you to a forsaken outpost on the edge of the realm. This desolate place, under constant threat from relentless enemy hordes, is your new home."
                + NL + NL + "Armed with nothing more than a simple axe and a short spear, and clad in mere rags offering no protection, you are all that stands between the outpost and its doom. Your starting gold is meager, barely enough to hire a handful of mercenaries or purchase rudimentary equipment."
                + NL + NL + "Yet, within this dire situation lies an opportunity for redemption. With each wave of enemies you repel, your wealth will grow. The gold earned from these victories can be used to strengthen your position - hire hardened soldiers, acquire formidable weapons and impenetrable armor."
                + NL + NL + "Your past may be marked by shame and defeat, but your future holds the promise of glory and honor. Stand firm, fight bravely, and reclaim the respect you once commanded. The battlefield awaits!";

        }
        else
        {
            infoStr += "Victory is yours, for now. The fallen enemies have left behind a bounty of gold. Use this respite wisely. Equip yourself with stronger weapons and armor, hire or upgrade mercenaries to stand by your side. Remember, the challenges ahead will only grow tougher.";

            if (HordeGameLogic.IsBossBattleNext)
            {
                infoStr += NL + NL + "Prepare yourself, for the battle ahead will not be against mere foot soldiers. The upcoming challenge brings with it a formidable adversary - a boss battle. This enemy will be stronger, tougher, and more relentless than any you've faced before. Gather your strength and steel your resolve. The true test of your mettle is about to begin.";
            }

            infoStr += NL + NL + "You've grown stronger, surviving wave after wave of enemies. Your health has increased, your strikes deal more damage, and you've become more resilient to enemy attacks. Your movement speed on the battlefield may have also improved slightly.";


            int health = PlayerCharacteristicProgressionTracker.PlayerCharSet.MaximumHealth;
            // Multiplier values will be written in percentage increases.
            // This will hopefully help the player understand what's happening.
            float dmgMulti = 
                100f * (PlayerCharacteristicProgressionTracker.PlayerCharSet.ExtraDamageInflictionMultiplier - 1f);
            float dmgTakenMulti = 
                100f * (1f - PlayerCharacteristicProgressionTracker.PlayerCharSet.DamageTakenMultiplier);
            float speedMulti = 100f * (PlayerCharacteristicProgressionTracker.PlayerCharSet.ExtraMovementSpeedLimitMultiplier - 1f);

            string precisionStr = "0.0";

            string statStr = string.Format(
                 "Maximum health: {0}" + NL 
               + "Damage bonus: +{1}%" + NL
               + "Damage resistance bonus: +{2}%" + NL
               + "Movement speed bonus: +{3}%"
               , health
               , dmgMulti.ToString(precisionStr)
               , dmgTakenMulti.ToString(precisionStr)
               , speedMulti.ToString(precisionStr));

            infoStr += NL + NL + statStr;
        }


        txtInformationBody.text = infoStr;
    }

    /// <summary>
    /// Updates all texts shown on the screen by invoking both <see cref="UpdateGameOverScreenTexts"/> and <see cref="UpdateRoundInfoScreenTexts"/>.
    /// </summary>
    void UpdateTexts()
    {
        UpdateGameOverScreenTexts();
        UpdateRoundInfoScreenTexts();
    }
}
