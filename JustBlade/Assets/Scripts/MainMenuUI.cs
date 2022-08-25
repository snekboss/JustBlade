using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public Slider sliderMouseSensitivity;
    public TextMeshProUGUI txtMouseSensitivity;

    public GameObject screenMainMenu;
    public GameObject screenControls;

    public Button btnStartGame;
    public Button btnControls;
    public Button btnExitGame;
    public Button btnGoBack;

    public void OnClick_ButtonStartGame()
    {
        PlayerAgent.PlayerCameraRotationSpeed = sliderMouseSensitivity.value;
        Time.timeScale = 1;

        TournamentVariables.IsPlayerEliminated = false;
        TournamentVariables.CurrentRoundNumber = 1;

        SceneManager.LoadScene("TournamentInfoMenuScene");
    }

    public void OnClick_ButtonControls()
    {
        screenMainMenu.SetActive(false);
        screenControls.SetActive(true);

        btnGoBack.gameObject.SetActive(true);
    }

    public void OnClick_ButtonExitGame()
    {
        Application.Quit();
    }

    public void OnClick_ButtonGoBack()
    {
        screenMainMenu.SetActive(true);
        screenControls.SetActive(false);

        btnGoBack.gameObject.SetActive(false);
    }

    public void OnSlider_ValueChanged()
    {
        int val = Convert.ToInt32(sliderMouseSensitivity.value);
        txtMouseSensitivity.text = "Mouse Sensitivity: " + val;
        PlayerAgent.PlayerCameraRotationSpeed = val;
    }

    void InitMainMenuUI()
    {
        screenMainMenu.SetActive(true);
        screenControls.SetActive(false);

        btnGoBack.gameObject.SetActive(false);

        sliderMouseSensitivity.value = PlayerAgent.PlayerCameraRotationSpeed;

        OnSlider_ValueChanged();
    }

    void Start()
    {
        InitMainMenuUI();
    }
}
