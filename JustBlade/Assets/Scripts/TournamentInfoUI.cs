using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TournamentInfoUI : MonoBehaviour
{
    public GameObject screenGameOver;
    public GameObject screenRoundInfo;

    public TextMeshProUGUI txtGameOverHeader;
    public TextMeshProUGUI txtGameOverBody;
    public TextMeshProUGUI txtRoundInfoBody;

    public Button btnReturnToMainMenu;
    public Button btnNext;

    void Start()
    {
        Cursor.visible = true;

        UpdateTexts();
    }

    public void OnButtonClick_ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void OnButtonClick_Next()
    {
        SceneManager.LoadScene("GearSelectionMenuScene");
    }

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

    void UpdateTexts()
    {
        UpdateGameOverScreenTexts();
        UpdateRoundInfoScreenTexts();
    }
}
