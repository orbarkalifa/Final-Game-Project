using System;
using UnityEngine;

namespace GameStateManagement
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject startMenuPanel;
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject inGameHUDPanel;

        private BeaconSO beacon;

        public void HideAllPanels()
        {
            Debug.Log("Hiding all ui panels");
            if(startMenuPanel) startMenuPanel.SetActive(false);
            if(pauseMenuPanel) pauseMenuPanel.SetActive(false);
            if(gameOverPanel) gameOverPanel.SetActive(false);
            if(inGameHUDPanel) inGameHUDPanel.SetActive(false);
        }

        public void ShowStartMenuPanel()
        {
            HideAllPanels();
            if (startMenuPanel != null)
            {
                startMenuPanel.SetActive(true);
                Debug.Log("UIManager: Start Menu Panel activated. Active state: " + startMenuPanel.activeSelf);
            }
            else
            {
                Debug.LogError("UIManager: Start Menu Panel is not assigned!");
            }
        }

        public void ShowPauseMenuPanel()
        {
            HideAllPanels();
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(true);
                Debug.Log("UIManager: Pause Menu Panel activated. Active state: " +  pauseMenuPanel.activeSelf);
            }
            else
            {
                Debug.LogError("UIManager: Pause Menu Panel is not assigned!");
            }
        }

        public void ShowGameOverPanel()
        {
            HideAllPanels();
            if(gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                Debug.Log("UIManager: Game Over Panel activated.");
            }
            else
            {
                Debug.LogError("UIManager: Game Over Panel is not assigned!");
            }
        }

        public void ShowInGameHUDPanel()
        {
            HideAllPanels();
            if (inGameHUDPanel != null)
            {
                inGameHUDPanel.SetActive(true);
                Debug.Log("UIManager: In-Game HUD Panel activated.");
            }
            else
            {
                Debug.LogError("UIManager: In-Game HUD Panel is not assigned!");
            }
        }
    }
}
