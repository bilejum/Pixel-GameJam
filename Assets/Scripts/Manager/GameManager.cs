using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public enum GameState
{
    Game,
    Build
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState State;

    public PlayerShip _playerShip;

    private void Awake()
    {
        Instance = this;
        State = GameState.Game;
    }
    private void Update()
    {
        SwitchBuildUI();
    }

    private void SwitchBuildUI()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            //如果处于游戏状态，则进入建造UI
            if (State == GameState.Game)
            {
                UIManager.Instance.SwitchBuildingUI(true);
                _playerShip.canMove = false;
                State = GameState.Build;
            }
            else if (State == GameState.Build)
            {
                UIManager.Instance.SwitchBuildingUI(false);
                _playerShip.canMove = true;
                State = GameState.Game;
            }
        }
    }
}
