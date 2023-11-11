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
/// 
/// Since this is a UI script, many of the fields are meant to be set using Unity's Inspector menu.
/// For this reason, these fields were not given commented documentation, as there are many of them.
/// Some of public methods may also not have been given commented documentation, as they're just
/// callbacks for the UI widgets.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    public Slider sliderCameraSensitivity;
    public TextMeshProUGUI txtCameraSensitivity;

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
            SetDefaultQuality();
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

        sliderCameraSensitivity.wholeNumbers = false;
        sliderCameraSensitivity.minValue = StaticVariables.PlayerCameraRotationSpeedMin;
        sliderCameraSensitivity.value = StaticVariables.PlayerCameraRotationSpeed;
        sliderCameraSensitivity.maxValue = StaticVariables.PlayerCameraRotationSpeedMax;
        txtCameraSensitivity.text = "Camera Sensitivity: " + StaticVariables.PlayerCameraRotationSpeed.ToString("0.00");

        sliderSound.wholeNumbers = false;
        sliderSound.minValue = StaticVariables.SoundSettingMin;
        sliderSound.value = StaticVariables.SoundSetting;
        sliderSound.maxValue = StaticVariables.SoundSettingMax;
        txtSound.text = "Sound: " + StaticVariables.SoundSetting.ToString("0.00");

        sliderDifficulty.wholeNumbers = false;
        sliderDifficulty.minValue = StaticVariables.DifficultySettingMin;
        sliderDifficulty.value = StaticVariables.DifficultySetting;
        sliderDifficulty.maxValue = StaticVariables.DifficultySettingMax;
        txtDifficulty.text = "Difficulty: " + (StaticVariables.DifficultySetting * 100f).ToString("0") + "%";

        sliderFieldOfView.wholeNumbers = true;
        sliderFieldOfView.minValue = StaticVariables.PlayerCameraFieldOfViewMin;
        sliderFieldOfView.value = StaticVariables.PlayerCameraFieldOfView;
        sliderFieldOfView.maxValue = StaticVariables.PlayerCameraFieldOfViewMax;
        txtFieldOfView.text = "Field of View: " + StaticVariables.PlayerCameraFieldOfView;
    }

    /// <summary>
    /// Starts the game by navigating to the InformationMenuScene.
    /// It also unpauses the game (in case it was paused the last time).
    /// It also invokes <see cref="HordeGameLogic.StartNewHordeGame"/> to start a new Horde game.
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

        SetDefaultQuality();
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
    /// Callback method when the camera sensitivity slider's value has been changed.
    /// It updates the player's camera sensitivity value by changing <see cref="StaticVariables.PlayerCameraRotationSpeed"/>.
    /// </summary>
    public void OnSliderValueChanged_CameraSensitivity()
    {
        float val = sliderCameraSensitivity.value;
        txtCameraSensitivity.text = "Camera Sensitivity: " + val.ToString("0.00");
        StaticVariables.PlayerCameraRotationSpeed = val;
    }

    /// <summary>
    /// Callback method when the sound slider's value has been changed.
    /// It changes the overall sound (ie, master volume) of the game, value by changing
    /// <see cref="StaticVariables.SoundSetting"/>.
    /// </summary>
    public void OnSliderValueChanged_Sound()
    {
        float val = sliderSound.value;
        txtSound.text = "Sound: " + val.ToString("0.00");
        StaticVariables.SoundSetting = val;

        AudioListener.volume = StaticVariables.SoundSetting;
    }

    /// <summary>
    /// Callback method when the difficulty slider's value has been changed.
    /// It changes the difficulty of the game, value by changing
    /// <see cref="StaticVariables.DifficultySetting"/>.
    /// </summary>
    public void OnSliderValueChanged_Difficulty()
    {
        float val = sliderDifficulty.value;
        txtDifficulty.text = "Difficulty: " + (val * 100f).ToString("0") + "%";
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
    /// Invoked by <see cref="InitMainMenuUI"/> and <see cref="OnButtonClick_SetDefaultQuality"/>.
    /// The default quality is set when the game starts for the first time.
    /// Then, it can be set again using the "set default quality" button.
    /// Since the button uses the <see cref="OnButtonClick_SetDefaultQuality"/> which plays a button sound,
    /// we separate the code so that the same button sound is not heard when invoked by <see cref="InitMainMenuUI"/>.
    /// </summary>
    void SetDefaultQuality()
    {
        QualitySettings.SetQualityLevel(StaticVariables.DefaultQualitySetting, true);

        UpdateQualitySettingWidgets();
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
