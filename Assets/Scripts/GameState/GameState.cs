using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class GameState : MonoBehaviour
{
    [FormerlySerializedAs("stateSO")]
    public stateSO Estate;
    [SerializeField] public GameState nextState;
    private List<TransitionBase> transitions = new();
    public bool wasTransitionInto = false;
    public bool inTransition = false;
    private GameStateChannel gameStateChannel;

    private void Awake()
    {
        gameStateChannel = FindObjectOfType<Beacon>().gameStateChannel;
        foreach (var transition in GetComponentsInChildren<TransitionBase>())
        {
            transitions.Add(transition);
            Debug.Log(transition.TargetState);
        }
    }

    private void Update()
    { 
        if (!CheckIfCurrent())
            return;
        nextState = null;
        foreach (var transition in transitions.Where(x => x.ShouldTransition()))
        {
            if (transition.TargetState)
            {
                nextState = transition.TargetState;
                break;
            }
        }

        if (!inTransition && !wasTransitionInto && nextState)
        {
            inTransition = true;
            StateEnter(nextState);
            nextState = null;
            inTransition = false;
        }

        if (wasTransitionInto)
        {
            wasTransitionInto = false;
        }
    }
    
    public bool CheckIfCurrent()
    {
        return gameStateChannel.GetCurrentGameState() == this;;
    }
    public void StateEnter(GameState next)
    {
        wasTransitionInto = true;
        Debug.Log($"game state is entered {next}");
        gameStateChannel.StateEntered(next);    
    }
}