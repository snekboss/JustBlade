using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// A script which designates the attached game object as In Game UI.
/// It contains the logic of the controls of the In Game UI.
/// The InGameUI shows certain information about the status of the <see cref="PlayerAgent"/>.
/// This includes things like remaining health and gold of the player; as well as the
/// order status of the player (ie, whether the player has ordered his mercenaries to "hold position" or "attack").
/// Since this is a UI script, many of the fields are meant to be set using Unity's Inspector menu.
/// For this reason, these fields were not given commented documentation, as there are many of them.
/// Some of public methods may also not have been given commented documentation, as they're just
/// callbacks for the UI widgets.
/// </summary>
public class InGameUI : MonoBehaviour
{
    PlayerAgent playerAgent;

    public GameObject screenPauseMenu;
    public GameObject screenPlayerInfo;
    public GameObject healthBar;

    public Slider sliderCameraSensitivity;
    public TextMeshProUGUI txtCameraSensitivity;

    public Slider sliderSound;
    public TextMeshProUGUI txtSound;

    public Slider sliderFieldOfView;
    public TextMeshProUGUI txtFieldOfView;

    public TextMeshProUGUI txtPlayerGold;
    public TextMeshProUGUI txtToggleAiStatus;

    public Button btnReturnToMainMenu;

    /// <summary>
    /// Initializes the In Game UI.
    /// It also pauses the game, in case it was paused.
    /// It also initialies the slider values based on values like <see cref="StaticVariables.PlayerCameraRotationSpeed"/>
    /// and <see cref="StaticVariables.PlayerCameraFieldOfView"/>.
    /// </summary>
    void InitInGameUI()
    {
        StaticVariables.IsGamePaused = false;

        //Cursor.visible = false;
        StaticVariables.ShowCursor(false);

        screenPauseMenu.SetActive(false);
        screenPlayerInfo.SetActive(false);

        InitializeSliders();
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

        sliderFieldOfView.wholeNumbers = true;
        sliderFieldOfView.minValue = StaticVariables.PlayerCameraFieldOfViewMin;
        sliderFieldOfView.value = StaticVariables.PlayerCameraFieldOfView;
        sliderFieldOfView.maxValue = StaticVariables.PlayerCameraFieldOfViewMax;
        txtFieldOfView.text = "Field of View: " + StaticVariables.PlayerCameraFieldOfView;
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
    /// Quits the Horde game mode, and loads the MainMenuScene.
    /// Does not ask for confirmation.
    /// </summary>
    public void OnButtonClick_ReturnToMainMenu()
    {
        PlayButtonSound();
        SceneManager.LoadScene("MainMenuScene");
    }

    /// <summary>
    /// Unity's Start method.
    /// In this case, it is used to invoke <see cref="InitInGameUI"/>.
    /// </summary>
    void Start()
    {
        InitInGameUI();
    }

    /// <summary>
    /// Handles the pausing of the game. It does this by:
    /// - Showing/hiding the pause menu UI,
    /// - Showing/hiding the mouse cursor,
    /// - Setting the <see cref="Time.timeScale"/> to 0 (when paused) and 1 (when unpaused).
    /// </summary>
    void HandlePausingTheGame()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            StaticVariables.IsGamePaused = !StaticVariables.IsGamePaused;

            screenPauseMenu.SetActive(StaticVariables.IsGamePaused);

            if (StaticVariables.IsGamePaused)
            {
                Time.timeScale = 0;
                //Cursor.visible = true;
                StaticVariables.ShowCursor(true);
            }
            else
            {
                Time.timeScale = 1;
                //Cursor.visible = false;
                StaticVariables.ShowCursor(false);

            }
        }
    }

    /// <summary>
    /// In order to update the UI elements which involve the <see cref="playerAgent"/>'s health,
    /// it needs a reference to it first.
    /// Since the player agent may not spawn immediately, it needs to check every frame until it can find the player agent.
    /// Once the player is found, the In Game UI logic script is activated.
    /// This method handles the logic of this edge case.
    /// </summary>
    void FindPlayerAgentInScene()
    {
        if (playerAgent != null)
        {
            return;
        }

        playerAgent = FindObjectOfType<PlayerAgent>();

        if (playerAgent != null)
        {
            screenPlayerInfo.SetActive(true);
        }
    }

    /// <summary>
    /// Updates the UI elements based on the <see cref="PlayerAgent"/>'s health value by keeping track of it.
    /// </summary>
    void UpdateHealthBar()
    {
        if (playerAgent == null)
        {
            return;
        }

        float healthRatio =
            Mathf.Clamp01((float)(playerAgent.CharMgr.Health) / (float)(playerAgent.CharMgr.MaximumHealth));
        float y = healthBar.transform.localScale.y;
        float z = healthBar.transform.localScale.z;
        healthBar.transform.localScale = new Vector3(healthRatio, y, z);
    }

    /// <summary>
    /// Upgrades the texts on the screen in the game.
    /// Examples of these texts are gold, player order status (ie, "holding position" vs "attacking).
    /// </summary>
    void UpdateTexts()
    {
        if (playerAgent == null)
        {
            return;
        }

        string NL = Environment.NewLine;
        txtPlayerGold.text = "Gold: " + PlayerInventoryManager.PlayerGold.ToString();
        string mercOrderText = playerAgent.IsPlayerOrderingToHoldPosition ? "Holding position" : "Attacking";
        txtToggleAiStatus.text = string.Format("Mercenaries: {0}" + NL + "(Press Q to toggle)", mercOrderText);
    }

    /// <summary>
    /// Unity's Update method.
    /// In this case, it handles the logic of the In Game UI.
    /// </summary>
    void Update()
    {
        HandlePausingTheGame();

        FindPlayerAgentInScene();
        UpdateHealthBar();
        UpdateTexts();
    }

    void PlayButtonSound()
    {
        SoundEffectManager.PlayButtonSound(Camera.main.transform.position);
    }
}
