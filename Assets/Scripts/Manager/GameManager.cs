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
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        State = GameState.Game;

       if(State is GameState.Game) { 
        //鼠标锁定在游戏窗口
        Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }

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
                //EnemyManager.Instance.DisAbleEnemies();
                Time.timeScale = 0f;
                CameraHandler.Instance.SwitchCinemachineUpdateMode(true);
            }
            else if (State == GameState.Build)
            {
                UIManager.Instance.SwitchBuildingUI(false);
                _playerShip.canMove = true;
                State = GameState.Game;
                //EnemyManager.Instance.EnableEnemies();
                Time.timeScale = 1f;
                CameraHandler.Instance.SwitchCinemachineUpdateMode(false);
                _playerShip._ghostTile.SetActive(false);
                _playerShip._selectedTile = null;
            }
        }
    }
}
