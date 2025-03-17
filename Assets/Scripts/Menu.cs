using Scriptable.Scripts;
using UnityEngine;

public class Menu : MonoBehaviour
{

    [SerializeField]
    private GameObject menuPanel;
    private GameStateChannel GmaGameStateChannel;

    private void Awake()
    {
        if (menuPanel == null)
        {
            Debug.LogError("MenuPanel is not assigned! Assign it in the Inspector.");
        }
        
        GmaGameStateChannel = FindObjectOfType<Beacon>().gameStateChannel;
        if (GmaGameStateChannel == null)
        {
            Debug.LogError("gameStateChannel not found in the scene!");
            return;
        }
        menuPanel.SetActive(false);
        Debug.Log("subscribed");
        GmaGameStateChannel.StateEnter += ToggleMenu;
        GmaGameStateChannel.StateExit += ToggleMenu;
    }


    public void ToggleMenu(GameState state)
    {
        if(state.Estate.states != stateSO.GameStates.Menu)
            return;
        Debug.Log("ToggleMenu");
        // You could check here if the current state allows for the menu
        bool isActive = menuPanel.activeSelf;
        menuPanel.SetActive(!isActive);

        // Pause/unpause
        Time.timeScale = menuPanel.activeSelf ? 0f : 1f;
    }


}

