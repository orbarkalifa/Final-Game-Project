using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuTransition : TransitionBase
{
    private bool UserPressedMenu;
    private GameStateChannel gameStateChannel;

    protected override void Awake()
    {
        base.Awake();

        // Initialize Input Actions
        // Get the GameStateChannel
        var beacon = FindObjectOfType<Beacon>();
        gameStateChannel = beacon.gameStateChannel;
        
        gameStateChannel.OnMenuClicked += CheckIfMenuWasPressed;
    }
    public override bool ShouldTransition()
    {
        if(!sourceState.CheckIfCurrent()) return false;
        bool canTransition = (UserPressedMenu && gameStateChannel.GetCurrentGameState().Estate.canMenu);
        UserPressedMenu = false; 
        return canTransition;
    }

    private void CheckIfMenuWasPressed()
    {
        if(!sourceState.CheckIfCurrent())return;
        UserPressedMenu = true; 
    }
    
    private void OnDestroy()
    {
        gameStateChannel.OnMenuClicked -= CheckIfMenuWasPressed;
    }
}