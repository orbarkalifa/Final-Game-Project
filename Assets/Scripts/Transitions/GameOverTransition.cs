using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class GameOverTransition : TransitionBase
{
    [SerializeField]private MainCharacter Player;
    private GameStateChannel gameStateChannel;


    protected override void Awake()
    {
        base.Awake();
        gameStateChannel = FindObjectOfType<Beacon>().gameStateChannel;
        if(!Player)
        {
            Debug.LogError("Player GameObject not found in game over transition");
        }
        if(!gameStateChannel)
            Debug.LogError("gamestatechannel not found in game over transition");
    }

    public override bool ShouldTransition()
    {
        if(!sourceState.CheckIfCurrent()) return false;
        return (Player.currentHits <= 0 && Player.doneLoading);
    }
}
