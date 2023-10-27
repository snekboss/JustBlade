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

            gameOverBodyStr = "TODO: You were killed in battle.";
        }
        else
        {
            gameOverHeaderStr = "Congratulations!";

            gameOverBodyStr =
                "TODO: You have beaten the strongest forces of the enemy. They have given up, etc.";
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

        if (HordeGameLogic.IsGameHasJustBegun)
        {
            infoStr += "TODO: Write info for when you first start the game.";
        }
        else
        {
            // TODO:
            infoStr += "TODO: Write info for a usual wave.";
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
