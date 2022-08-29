using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// A script which designates the attached game object as Tournament Info Menu UI.
/// It contains the logic of the controls of the Tournament Info Menu UI.
/// The tournament info screen is shown before and after a tournament round.
/// It is also shown when the player is eliminated from the tournament (game over) and when the player wins the tournament.
/// </summary>
public class TournamentInfoUI : MonoBehaviour
{
    public GameObject screenGameOver;
    public GameObject screenRoundInfo;

    public TextMeshProUGUI txtGameOverHeader;
    public TextMeshProUGUI txtGameOverBody;
    public TextMeshProUGUI txtRoundInfoBody;

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
    /// This method is invoked when the game is over.
    /// The game could be over in one of two ways:
    /// - The player is eliminated from the tournament.
    /// - The player wins the tournament.
    /// In either case, this method handles the text which is shown on the screen.
    /// </summary>
    void UpdateGameOverScreenTexts()
    {
        screenGameOver.SetActive(TournamentVariables.IsTournamentEnded);

        string gameOverHeaderStr = "";
        string gameOverBodyStr = "";
        string NL = Environment.NewLine;

        string playerStats = "Total number of opponents beaten: " + TournamentVariables.TotalOpponentsBeatenByPlayer;

        if (TournamentVariables.IsPlayerEliminated)
        {
            gameOverHeaderStr = "Game Over";

            gameOverBodyStr = "You have been eliminated from the tournament." + NL + NL + playerStats;
        }
        else
        {
            gameOverHeaderStr = "Congratulations!";

            gameOverBodyStr = "You have won the tournament!" + NL + NL + playerStats;
        }

        txtGameOverHeader.text = gameOverHeaderStr;
        txtGameOverBody.text = gameOverBodyStr;
    }

    /// <summary>
    /// This method is invoked before each tournament round.
    /// It is also invoked after each round (unless the game is over).
    /// It is used to the text which is shown on the screen, which gives information about the current tournament round.
    /// </summary>
    void UpdateRoundInfoScreenTexts()
    {
        screenRoundInfo.SetActive(!TournamentVariables.IsTournamentEnded);

        string infoStr = "";

        if (TournamentVariables.CurrentRoundNumber == 1)
        {
            infoStr += "Welcome to the Melee Tournament. ";
        }

        string bestedStr = "";
        if (TournamentVariables.PlayerWasBestedInThisMelee)
        {
            bestedStr = "You were bested in this melee, but since you managed to beat enough opponents, the master of ceremonies allowed you to proceed to the next round. ";
        }

        string participantStr = TournamentVariables.IsFinalRound ? "participant" : "participants";
        infoStr +=
            string.Format("You are at around {0} of {1}. There will be {2} {3} in each team. Click Next to proceed to the gear selection menu."
            , TournamentVariables.CurrentRoundNumber
            , TournamentVariables.MaximumRoundNumber
            , TournamentVariables.MaxNumAgentsInEachTeam
            , participantStr);

        txtRoundInfoBody.text = bestedStr + infoStr;
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
