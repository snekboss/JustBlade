using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// A script which designates the attached game object as Main Menu UI.
/// It contains the logic of the controls of the Main Menu UI.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    public Slider sliderMouseSensitivity;
    public TextMeshProUGUI txtMouseSensitivity;

    public Slider sliderSound;
    public TextMeshProUGUI txtSound;

    public Slider sliderDifficulty;
    public TextMeshProUGUI txtDifficulty;

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

    /// <summary>
    /// Initializes the Main Menu UI. It involves things like:
    /// - Making the mouse cursor visible,
    /// - Setting the initial values of sliders,
    /// - Setting up the default quality settings (if the game was loaded for the first time).
    /// </summary>
    void InitMainMenuUI()
    {
        Cursor.visible = true;

        screenMainMenu.SetActive(true);
        screenKeyBindings.SetActive(false);
        screenSettings.SetActive(false);

        btnGoBack.gameObject.SetActive(false);

        InitializeSliders();

        if (isLoadingForTheFirstTime)
        {
            OnButtonClick_SetDefaultQuality(); // also plays button sound on load now.
            isLoadingForTheFirstTime = false;
        }
        else
        {
            UpdateQualitySettingWidgets();
        }
    }

    /// <summary>
    /// Initializes the slider values.
    /// </summary>
    void InitializeSliders()
    {
        // Set the values in this order or there will be bugs, because Unity invokes the callback method when you set value:
        // - minValue
        // - value
        // - maxValue

        sliderMouseSensitivity.wholeNumbers = false;
        sliderMouseSensitivity.minValue = StaticVariables.PlayerCameraRotationSpeedMin;
        sliderMouseSensitivity.value = StaticVariables.PlayerCameraRotationSpeed;
        sliderMouseSensitivity.maxValue = StaticVariables.PlayerCameraRotationSpeedMax;

        sliderSound.wholeNumbers = false;
        sliderSound.minValue = StaticVariables.SoundSettingMin;
        sliderSound.value = StaticVariables.SoundSetting;
        sliderSound.maxValue = StaticVariables.SoundSettingMax;

        sliderDifficulty.wholeNumbers = false;
        sliderDifficulty.minValue = StaticVariables.DifficultySettingMin;
        sliderDifficulty.value = StaticVariables.DifficultySetting;
        sliderDifficulty.maxValue = StaticVariables.DifficultySettingMax;

        sliderFieldOfView.wholeNumbers = true;
        sliderFieldOfView.minValue = StaticVariables.PlayerCameraFieldOfViewMin;
        sliderFieldOfView.value = StaticVariables.PlayerCameraFieldOfView;
        sliderFieldOfView.maxValue = StaticVariables.PlayerCameraFieldOfViewMax;
    }

    /// <summary>
    /// Starts the game by navigating to the TournamentInfoMenuScene.
    /// It also unpauses the game (in case it was paused the last time).
    /// </summary>
    public void OnButtonClick_StartGame()
    {
        PlayButtonSound();

        Time.timeScale = 1;

        HordeGameLogic.StartNewHordeGame();

        SceneManager.LoadScene("InformationMenuScene");
    }

    /// <summary>
    /// Navigates to the Key Bindings submenu.
    /// </summary>
    public void OnButtonClick_KeyBindings()
    {
        PlayButtonSound();

        screenMainMenu.SetActive(false);
        screenKeyBindings.SetActive(true);
        screenSettings.SetActive(false);

        btnGoBack.gameObject.SetActive(true);
    }

    /// <summary>
    /// Navigates to the Settings submenu.
    /// </summary>
    public void OnButtonClick_Settings()
    {
        PlayButtonSound();

        screenMainMenu.SetActive(false);
        screenKeyBindings.SetActive(false);
        screenSettings.SetActive(true);

        btnGoBack.gameObject.SetActive(true);
    }

    /// <summary>
    /// Exits the game.
    /// </summary>
    public void OnButtonClick_ExitGame()
    {
        PlayButtonSound();
        Application.Quit();
    }

    /// <summary>
    /// Navigates back to the main menu screen from any other submenu.
    /// </summary>
    public void OnButtonClick_GoBack()
    {
        PlayButtonSound();

        screenMainMenu.SetActive(true);
        screenKeyBindings.SetActive(false);
        screenSettings.SetActive(false);

        btnGoBack.gameObject.SetActive(false);
    }

    /// <summary>
    /// Increases the graphical quality of the game by one level.
    /// </summary>
    public void OnButtonClick_IncreaseQuality()
    {
        PlayButtonSound();

        QualitySettings.IncreaseLevel(true);

        UpdateQualitySettingWidgets();
    }

    /// <summary>
    /// Decreases the graphical quality of the game by one level.
    /// </summary>
    public void OnButtonClick_DecreaseQuality()
    {
        PlayButtonSound();

        QualitySettings.DecreaseLevel(true);

        UpdateQualitySettingWidgets();
    }

    /// <summary>
    /// Sets the graphical quality of the game to default settings based on <see cref="StaticVariables.DefaultQualitySetting"/>.
    /// </summary>
    public void OnButtonClick_SetDefaultQuality()
    {
        PlayButtonSound();

        QualitySettings.SetQualityLevel(StaticVariables.DefaultQualitySetting, true);

        UpdateQualitySettingWidgets();
    }

    /// <summary>
    /// Updates the UI elements regarding the quality settings of the game.
    /// </summary>
    void UpdateQualitySettingWidgets()
    {
        int index = QualitySettings.GetQualityLevel();

        btnDecreaseQuality.interactable = (index != 0);
        btnIncreaseQuality.interactable = (index != (QualitySettings.names.Length - 1));

        txtChosenQuality.text = QualitySettings.names[index];
    }

    /// <summary>
    /// Callback method when the mouse sensitivity slider's value has been changed.
    /// It updates the player's mouse sensitivity value by changing <see cref="StaticVariables.PlayerCameraRotationSpeed"/>.
    /// </summary>
    public void OnSliderValueChanged_MouseSensitivity()
    {
        float val = sliderMouseSensitivity.value;
        txtMouseSensitivity.text = "Mouse Sensitivity: " + val.ToString("0.00");
        StaticVariables.PlayerCameraRotationSpeed = val;
    }

    public void OnSliderValueChanged_Sound()
    {
        float val = sliderSound.value;
        txtSound.text = "Sound: " + val.ToString("0.00");
        StaticVariables.SoundSetting = val;

        AudioListener.volume = StaticVariables.SoundSetting;
    }

    public void OnSliderValueChanged_Difficulty()
    {
        float val = sliderDifficulty.value;
        txtDifficulty.text = "Difficulty: " + val.ToString("0.00");
        StaticVariables.DifficultySetting = val;
    }

    /// <summary>
    /// Callback method when the field of view slider's value has been changed.
    /// It updates the camera's field of view value by changing <see cref="StaticVariables.PlayerCameraFieldOfView"/>.
    /// </summary>
    public void OnSliderValueChanged_FieldOfView()
    {
        int val = Convert.ToInt32(sliderFieldOfView.value);
        txtFieldOfView.text = "Field of View: " + val;
        StaticVariables.PlayerCameraFieldOfView = val;

        Camera.main.fieldOfView = StaticVariables.PlayerCameraFieldOfView;
    }

    /// <summary>
    /// Unity's Start method.
    /// It invokes <see cref="InitMainMenuUI"/>.
    /// </summary>
    void Start()
    {
        InitMainMenuUI();
    }

    void PlayButtonSound()
    {
        SoundEffectManager.PlayButtonSound(Camera.main.transform.position);
    }
}
