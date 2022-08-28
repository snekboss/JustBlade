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

    public Slider sliderFieldOfView;
    public TextMeshProUGUI txtFieldOfView;

    public GameObject screenMainMenu;
    public GameObject screenKeyBindings;
    public GameObject screenSettings;

    public Button btnStartGame;
    public Button btnKeyBindings;
    public Button btnSettings;
    public Button btnExitGame;
    public Button btnGoBack;

    public TextMeshProUGUI txtChosenQuality;

    public Button btnIncreaseQuality;
    public Button btnDecreaseQuality;
    public Button btnSetDefaultQuality;


    static bool isLoadingForTheFirstTime = true;

    void InitMainMenuUI()
    {
        Cursor.visible = true;

        screenMainMenu.SetActive(true);
        screenKeyBindings.SetActive(false);
        screenSettings.SetActive(false);

        btnGoBack.gameObject.SetActive(false);

        sliderMouseSensitivity.value = StaticVariables.PlayerCameraRotationSpeed;
        sliderFieldOfView.value = StaticVariables.PlayerCameraFieldOfView;

        OnSliderValueChanged_MouseSensitivity();
        OnSliderValueChanged_FieldOfView();

        if (isLoadingForTheFirstTime)
        {
            OnButtonClick_SetDefaultQuality();
            isLoadingForTheFirstTime = false;
        }
        else
        {
            UpdateQualitySettingWidgets();
        }
    }

    public void OnButtonClick_StartGame()
    {
        Time.timeScale = 1;

        TournamentVariables.IsPlayerEliminated = false;
        TournamentVariables.PlayerWasBestedInThisMelee = false;
        TournamentVariables.TotalOpponentsBeatenByPlayer = 0;
        TournamentVariables.CurrentRoundNumber = 1;

        SceneManager.LoadScene("TournamentInfoMenuScene");
    }

    public void OnButtonClick_KeyBindings()
    {
        screenMainMenu.SetActive(false);
        screenKeyBindings.SetActive(true);
        screenSettings.SetActive(false);

        btnGoBack.gameObject.SetActive(true);
    }

    public void OnButtonClick_Settings()
    {
        screenMainMenu.SetActive(false);
        screenKeyBindings.SetActive(false);
        screenSettings.SetActive(true);

        btnGoBack.gameObject.SetActive(true);
    }

    public void OnButtonClick_ExitGame()
    {
        Application.Quit();
    }

    public void OnButtonClick_GoBack()
    {
        screenMainMenu.SetActive(true);
        screenKeyBindings.SetActive(false);
        screenSettings.SetActive(false);

        btnGoBack.gameObject.SetActive(false);
    }

    public void OnButtonClick_IncreaseQuality()
    {
        QualitySettings.IncreaseLevel(true);

        UpdateQualitySettingWidgets();
    }

    public void OnButtonClick_DecreaseQuality()
    {
        QualitySettings.DecreaseLevel(true);

        UpdateQualitySettingWidgets();
    }

    public void OnButtonClick_SetDefaultQuality()
    {
        QualitySettings.SetQualityLevel(StaticVariables.DefaultQualitySetting, true);

        UpdateQualitySettingWidgets();
    }

    void UpdateQualitySettingWidgets()
    {
        int index = QualitySettings.GetQualityLevel();

        btnDecreaseQuality.interactable = (index != 0);
        btnIncreaseQuality.interactable = (index != (QualitySettings.names.Length - 1));

        txtChosenQuality.text = QualitySettings.names[index];
    }

    public void OnSliderValueChanged_MouseSensitivity()
    {
        int val = Convert.ToInt32(sliderMouseSensitivity.value);
        txtMouseSensitivity.text = "Mouse Sensitivity: " + val;
        StaticVariables.PlayerCameraRotationSpeed = val;
    }

    public void OnSliderValueChanged_FieldOfView()
    {
        int val = Convert.ToInt32(sliderFieldOfView.value);
        txtFieldOfView.text = "Field of View: " + val;
        StaticVariables.PlayerCameraFieldOfView = val;

        Camera.main.fieldOfView = StaticVariables.PlayerCameraFieldOfView;
    }

    void Start()
    {
        InitMainMenuUI();
    }
}
