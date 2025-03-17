using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TransitionBase : MonoBehaviour
{
    [SerializeField] protected GameState sourceState;
    [SerializeField] protected GameState targetState;
    public GameState TargetState => targetState;

    protected virtual void Awake()
    {
        sourceState = GetComponentInParent<GameState>();
        if(sourceState == null )
        {
            Debug.LogError($"Unable to find source state in {name}");
        }
        if(targetState == null )
        {
            Debug.LogError($"Unable to find target state in {name}");
        }
    }

    public abstract bool ShouldTransition();
}