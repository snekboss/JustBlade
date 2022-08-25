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

    public Button btnReturnToMainMenu;

    bool isPaused;

    void InitInGameUI()
    {
        screenPauseMenu.SetActive(false);
        screenPlayerInfo.SetActive(false);

        OnSlider_ValueChanged();
    }

    public void OnSlider_ValueChanged()
    {
        int val = Convert.ToInt32(sliderMouseSensitivity.value);
        txtMouseSensitivity.text = "Mouse Sensitivity: " + val;
        PlayerAgent.PlayerCameraRotationSpeed = val;
    }

    public void OnClick_ButtonReturnToMainMenu()
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
