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
        UpdateTexts();
    }

    public void OnClick_ButtonReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    public void OnClick_ButtonNext()
    {
        SceneManager.LoadScene("GearSelectionMenuScene");
    }

    void UpdateGameOverScreenTexts()
    {
        screenGameOver.SetActive(TournamentVariables.IsTournamentEnded);

        txtGameOverHeader.text = "game over or congrats";

        string NL = Environment.NewLine;
        txtGameOverBody.text = "you lost or you won." + NL + NL + "oh and btw here are some stats like num opponents beaten etc.";
    }

    void UpdateRoundInfoScreenTexts()
    {
        screenRoundInfo.SetActive(!TournamentVariables.IsTournamentEnded);

        txtRoundInfoBody.text = "You were bested in this melee, but the master of ceremonies allowed you to blah..." +
            "or don't write that and just write You are at round X of Y etc.";
    }

    void UpdateTexts()
    {
        UpdateGameOverScreenTexts();
        UpdateRoundInfoScreenTexts();
    }
}
