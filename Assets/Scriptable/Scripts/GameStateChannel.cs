using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Game State Channel", menuName = "Channels/Game State")]
public class GameStateChannel : ScriptableObject
{
    public Action<GameState> StateEnter;
    public Action<GameState> StateExit;
    public Func<GameState> GetCurrentState;
    public event Action OnMenuClicked;
    public void MenuClicked()
    {
        OnMenuClicked?.Invoke();
    }
    
    public void StateEntered(GameState gameState)
    {
        StateEnter?.Invoke(gameState);
    }

    public void StateExited(GameState gameState)
    {
        StateExit?.Invoke(gameState);
        
    }

    public GameState GetCurrentGameState()
    {
        return GetCurrentState?.Invoke();
    }
}
