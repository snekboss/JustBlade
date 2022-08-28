using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    PlayerAgent playerAgent;

    public GameObject screenPauseMenu;
    public GameObject screenPlayerInfo;
    public GameObject healthBar;

    public Slider sliderMouseSensitivity;
    public TextMeshProUGUI txtMouseSensitivity;

    public Slider sliderFieldOfView;
    public TextMeshProUGUI txtFieldOfView;

    public Button btnReturnToMainMenu;

    bool isPaused;

    void InitInGameUI()
    {
        screenPauseMenu.SetActive(false);
        screenPlayerInfo.SetActive(false);

        sliderMouseSensitivity.value = StaticVariables.PlayerCameraRotationSpeed;
        sliderFieldOfView.value = StaticVariables.PlayerCameraFieldOfView;

        OnSliderValueChanged_MouseSensitivity();
        OnSliderValueChanged_FieldOfView();
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

    public void OnButtonClick_ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    void Start()
    {
        InitInGameUI();
    }

    void HandlePausingTheGame()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            isPaused = !isPaused;

            screenPauseMenu.SetActive(isPaused);

            if (isPaused)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }
        }
    }

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

    void UpdateHealthBar()
    {
        if (playerAgent == null)
        {
            return;
        }

        float healthRatio = Mathf.Clamp01((float)(playerAgent.Health) / (float)(Agent.MaximumHealth));
        float y = healthBar.transform.localScale.y;
        float z = healthBar.transform.localScale.z;
        healthBar.transform.localScale = new Vector3(healthRatio, y, z);
    }

    void Update()
    {
        HandlePausingTheGame();

        FindPlayerAgentInScene();
        UpdateHealthBar();
    }
}
