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

    public List<LootTile> lootList = new List<LootTile>();

    public bool startSpawnEnemyFlag;
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
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _currentWave.time)
        {
            StartClearEnemies();
            _currentWaveIndex += 1;
            //_currentWaveIndex = _currentWaveIndex % _waveDataSOList.Count + 1;

            _currentWave = _waveDataSOList[_currentWaveIndex];
            StartWave();
            _timer = 0;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            StartClearEnemies();
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

        float x = position.x + Random.Range(10, 50);
        float y = position.y + Random.Range(10, 50);

        // 实例化敌人
        Ship newEnemy = Instantiate(prefab, new Vector2(x, y), Quaternion.identity);
        _enemiesList.Add((BaseEnemy)newEnemy);
    }

    public void StartClearEnemies()
    {
        foreach (var lootItem in lootList)
        {
            lootList.Remove(lootItem);
            Destroy(lootItem.gameObject);
        }
        // 开启协程，不要直接在普通函数里写循环
        StartCoroutine(ClearEnemiesRoutine());
    }

    private IEnumerator ClearEnemiesRoutine()
    {
        while (_enemiesList.Count > 0)
        {
            // 始终取当前列表的最后一个元素（最安全，不会越界）
            int lastIndex = _enemiesList.Count - 1;
            BaseEnemy enemy = _enemiesList[lastIndex];

            if (enemy != null)
            {
                // ... 处理逻辑 ...
                if (enemy.transform.childCount > 0)
                {
                    Transform enemyShip = enemy.transform.GetChild(0);
                    for (int j = 0; j < enemyShip.childCount; j++)
                    {
                        Tile t = enemyShip.GetChild(j).GetComponent<Tile>();
                        if (t != null) t.Health = 0;
                    }
                }
            }

            // 处理完一个就删一个，这样即便外部也删，也不会影响这里的逻辑
            _enemiesList.RemoveAt(lastIndex);

            yield return new WaitForSeconds(0.2f);
        }
    }
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
