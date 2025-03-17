using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameOverScreen : MonoBehaviour
{

    [FormerlySerializedAs("menuPanel")]
    [SerializeField]
    private GameObject gameOverScreen;
    private GameStateChannel GmaGameStateChannel;

    private void Awake()
    {
        if (gameOverScreen == null)
        {
            Debug.LogError("MenuPanel is not assigned! Assign it in the Inspector.");
        }
        
        GmaGameStateChannel = FindObjectOfType<Beacon>().gameStateChannel;
        if (GmaGameStateChannel == null)
        {
            Debug.LogError("gameStateChannel not found in the scene!");
            return;
        }
        gameOverScreen.SetActive(false);
        Debug.Log("subscribed");
        GmaGameStateChannel.StateEnter += ToggleGOScreen;
        GmaGameStateChannel.StateExit += ToggleGOScreen;
    }


    public void ToggleGOScreen(GameState state)
    {
        if(state.Estate.states != stateSO.GameStates.GameOver)
            return;
        Debug.Log("Toggle GameOver Screen");
        // You could check here if the current state allows for the menu
        bool isActive = gameOverScreen.activeSelf;
        gameOverScreen.SetActive(!isActive);

        // Pause/unpause
        Time.timeScale = gameOverScreen.activeSelf ? 0f : 1f;
    }
}
