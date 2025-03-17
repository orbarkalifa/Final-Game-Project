using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scriptable.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public GameState currentState;
    public GameState previousState;
    public GameState defaultState;

    private List<GameState> states = new();
    private GameStateChannel gameStateChannel;
    private  UIChannelSo UIChannel;

    void Awake()
    {
        
        // This ensures only one instance if you use the typical Singleton pattern
        if (FindObjectsOfType<GameStateManager>().Length > 1) 
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        
        
        var beacon = FindObjectOfType<Beacon>();
        UIChannel = beacon.UIChannel;
        gameStateChannel = beacon.gameStateChannel;
        gameStateChannel.StateEnter += StateEnter;
        gameStateChannel.GetCurrentState += GetCurrentState;

        if (defaultState == null)
        {
            Debug.LogError("Default state is not assigned! Assign it in the Inspector.");
            return;
        }
        if (gameStateChannel == null)
        {
            Debug.LogError("GameStateChannel is not assigned or missing in the scene.");
            return;
        }
        
        foreach (var state in GetComponentsInChildren<GameState>())
        {
            states.Add(state);
        }

        if (currentState == null)
        {
            currentState = defaultState;
            defaultState.StateEnter(defaultState);
        }

        SceneManager.sceneLoaded += AnnounceStateOnSceneLoaded;
    }


    private void AnnounceStateOnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        var state = currentState;
        if (state == null) 
        {
            state = states.FirstOrDefault(x => x.CheckIfCurrent());
        }
        gameStateChannel.StateEntered(state);
    }

    private GameState GetCurrentState()
    {
        return currentState;
    }

    private void StateEnter(GameState state)
    {
        Debug.Log(state);
        previousState = currentState;
        currentState = state;
        gameStateChannel.StateExited(previousState);
    }
    private void OnDestroy()
    {
        gameStateChannel.StateEnter -= StateEnter;
        gameStateChannel.GetCurrentState -= GetCurrentState;
    }

    public void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        StateEnter(previousState);
    }
}