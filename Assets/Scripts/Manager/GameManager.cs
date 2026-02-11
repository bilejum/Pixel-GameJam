using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum GameState
{
    Game,
    Build,
    End
}
public class GameManager : MonoBehaviour
{
    public static Action<int> OnScoreChanged;

    public static GameManager Instance { get; private set; }
    public GameState State;

    public PlayerShip _playerShip;

    public int currentScore = 0;


    private void OnEnable()
    {
        Debug.Log("[GameManager] OnEnable subscribing to PlayerShip.OnPlayerDeath");


        PlayerShip.OnPlayerDeath += HandlePlayerDeath;

        BaseEnemy.OnEnemyKilled += AddScore;
    }

    private void OnDisable()
    {
        Debug.Log("[GameManager] OnDisable unsubscribing from PlayerShip.OnPlayerDeath");

        BaseEnemy.OnEnemyKilled -= AddScore;
        PlayerShip.OnPlayerDeath -= HandlePlayerDeath;
    }


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

        if (State is GameState.Game)
        {
            //鼠标锁定在游戏窗口
            Cursor.lockState = CursorLockMode.Confined;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }

        _playerShip = FindAnyObjectByType<PlayerShip>();

    }
    private void Update()
    {
        if (State is GameState.End)
        {

        }

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


    private void HandlePlayerDeath()
    {
        //if (State == GameState.End) return; // 防止重复触发

        Debug.Log("GameManager 收到死亡信号，开始处理结算...");

        State = GameState.End;

        // 1. 锁定玩家操作
        _playerShip.canMove = false;

        // 2. 停止时间（或者减速）
        Time.timeScale = 0.5f;

        // 3. 呼叫 UI 管理器执行“变黑”效果
        // 假设你把上一步写的 FadeToBlack 放在了 UIManager 里
        UIManager.Instance.StartFadeToBlack();

        //4.清理残留（比如你之前的敌人清理逻辑）
        EnemyManager.Instance.StartClearEnemies();
    }

    public void RestartGame()
    {
        // 1. 核心：重置时间缩放
        // 这一点至关重要！因为你在失败时把 timeScale 改成了 0.3 或 0
        // 如果不重置，新场景加载后依然是卡死的
        Time.timeScale = 1f;

        // 2. 获取当前活动的场景名称并重新加载
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);

        // 如果你的 AudioManager 在淡出后音量为 0，记得在这里或在新场景的 Awake 里还原
        // AudioManager.Instance.musicSource.volume = 1f;
    }

    private void AddScore()
    {
        currentScore += 1;
        // 发出信号，告诉 UI 分数变了，并把当前分数传过去
        OnScoreChanged?.Invoke(currentScore);
    }

    public void QuitGame()
    {
        // 1. 如果是在 Unity 编辑器里运行
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // 2. 如果是打包后的正式程序
            Application.Quit();
#endif

        Debug.Log("游戏已退出");
    }
}
