using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[SerializeField]
[CreateAssetMenu(fileName = "new State", menuName = "New State", order = 0)]
public class stateSO : ScriptableObject
{
    public GameStates states;
    public enum GameStates
    {
        Start,
        InGame,
        Menu,
        GameOver,
    }

    public bool canMenu;
    public bool canMove;
}
