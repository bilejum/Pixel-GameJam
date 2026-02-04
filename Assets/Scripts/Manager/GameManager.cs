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

    [SerializeField] private PlayerShip _playerShip;

    private void Awake()
    {
        State = GameState.Game;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            //如果处于游戏状态，则进入建造UI
            if(State == GameState.Game)
            {
                UIManager.Instance._buildingUI.gameObject.SetActive(true);
                CameraHandler.Instance.ZoomInToShip(4);
                _playerShip.canMove = false;
                State = GameState.Build;
            }
            else if(State == GameState.Build)
            {
                UIManager.Instance._buildingUI.gameObject.SetActive(false);
                CameraHandler.Instance.ZoomInToShip(20);
                _playerShip.canMove = true;
                State = GameState.Game;
            }
        }
    }
}
