using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    public Ship _target;

    public List<BaseEnemy> _enemiesList;

    [SerializeField]
    private BaseEnemy _enemyPrefab;

    //波次相关
    [SerializeField]
    private bool _spawnEnemyFlag = false;
    public List<WaveDataSO> _waveDataSOList;

    [SerializeField]
    private string _waveDataSOPath = "ScriptableObjects";

    [SerializeField]
    private WaveDataSO _currentWave;

    [SerializeField]
    private int _currentWaveIndex = 0;
    public float _spawnInterval = 0.5f; // 每个敌人生成的间隔

    private float _timer;

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

        _enemiesList = new List<BaseEnemy>();

        _target = GameManager.Instance._playerShip;

        var waveDatas = Resources.LoadAll(_waveDataSOPath);

        foreach (WaveDataSO waveData in waveDatas)
        {
            _waveDataSOList.Add(waveData);
        }

        _currentWave = _waveDataSOList[_currentWaveIndex];

        StartWave();
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _currentWave.time)
        {
            //ClearEnemies();
            _currentWaveIndex += 1;
            _currentWaveIndex = _currentWaveIndex % _waveDataSOList.Count + 1;

            _currentWave = _waveDataSOList[_currentWaveIndex];
            StartWave();
            _timer = 0;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearEnemies();
        }

        UIManager.Instance.UpdateWaveText(_currentWave.time - _timer, _currentWave.waveIndex);
    }

    [ContextMenu("开始生成当前波次")] // 让你可以在 Inspector 里右键点击脚本手动测试
    public void StartWave()
    {
        if (_currentWave != null)
        {
            StartCoroutine(SpawnWaveRoutine());
        }
    }

    private IEnumerator SpawnWaveRoutine()
    {
        Debug.Log($"开始波次: {_currentWave.waveIndex}");

        foreach (var group in _currentWave.enemyGroups)
        {
            // 按照每个 Group 定义的数量生成敌人
            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.enemyPrefab, group.spawnPosition);

                // 等待一小会儿再生成下一个，防止重叠
                yield return new WaitForSeconds(_spawnInterval);
            }
        }

        Debug.Log("该波次所有敌人已生成完毕");
    }

    private void SpawnEnemy(Ship prefab, Vector2 position)
    {
        if (prefab == null)
            return;

        float x = position.x + Random.Range(10, 20);
        float y = position.y + Random.Range(10, 20);

        // 实例化敌人
        Ship newEnemy = Instantiate(prefab, new Vector2(x, y), Quaternion.identity);
        _enemiesList.Add((BaseEnemy)newEnemy);
    }

    public void ClearEnemies()
    {
        // 从 count - 1 开始倒序循环
        for (int i = _enemiesList.Count - 1; i >= 0; i--)
        {
            BaseEnemy enemy = _enemiesList[i];
            if (enemy != null)
            {
                var enemyShip = enemy.transform.GetChild(0);
                var enemyShipIndex = enemy.transform.childCount;
                for (int j = 0; j < enemyShipIndex; j++)
                {
                    enemyShip.GetChild(j).GetComponent<Tile>().Health = 0;
                }
            }
        }
        // 循环结束后清空列表
        _enemiesList.Clear();
    }

    //public void ClearEnemies()
    //{
    //    foreach (var enemy in _enemiesList)
    //    {
    //        var enemyShip = enemy.transform.GetChild(0);
    //        var enemyShipIndex = enemy.transform.childCount;
    //        for (int i = 0; i < enemyShipIndex; i++)
    //        {
    //            enemyShip.GetChild(i).GetComponent<Tile>().Health = 0;
    //        }
    //    }
    //}

    public void DisAbleEnemies()
    {
        foreach (var enemy in _enemiesList)
        {
            BaseAIShip enemyShip = enemy.GetComponent<BaseAIShip>();
            enemyShip._canAction = false;
        }
    }

    public void EnableEnemies()
    {
        foreach (var enemy in _enemiesList)
        {
            BaseAIShip enemyShip = enemy.GetComponent<BaseAIShip>();
            enemyShip._canAction = true;
        }
    }
}
