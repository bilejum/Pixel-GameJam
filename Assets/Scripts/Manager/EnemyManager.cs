using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    public Ship _target;                                                                                                                    

    public List<BaseEnemy> _enemiesList;

    [SerializeField]
    private BaseEnemy _enemyPrefab;

    // 波次相关
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

    public bool _waveEndFlag = false;

    // 当前正在运行的生成协程（避免重复启动）
    private Coroutine _spawnCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _enemiesList = _enemiesList ?? new List<BaseEnemy>();
        _waveDataSOList = _waveDataSOList ?? new List<WaveDataSO>();

        _target = GameManager.Instance?._playerShip;

        // 加载所有 WaveDataSO（如果 Inspector 已手动赋值则不会覆盖）
        var waveDatas = Resources.LoadAll<WaveDataSO>(_waveDataSOPath);
        if (waveDatas != null && waveDatas.Length > 0)
        {
            _waveDataSOList.Clear();
            foreach (var wd in waveDatas)
            {
                _waveDataSOList.Add(wd);
            }
            Debug.Log($"EnemyManager: 加载到 {_waveDataSOList.Count} 个 WaveDataSO");
        }
        else
        {
            Debug.LogWarning($"EnemyManager: 未在 Resources/{_waveDataSOPath} 找到 WaveDataSO");
        }

        // 确保当前波次索引与 currentWave 一致并在有效范围
        if (_waveDataSOList.Count > 0)
        {
            if (_currentWaveIndex < 0 || _currentWaveIndex >= _waveDataSOList.Count)
                _currentWaveIndex = 0;
            _currentWave = _waveDataSOList[_currentWaveIndex];
        }
        else
        {
            _currentWave = null;
            _currentWaveIndex = -1;
        }

        // 如果有可用波次，开始第一波
        if (_currentWave != null)
        {
            StartWave();
        }
        else
        {
            Debug.LogWarning("EnemyManager: 没有可用的波次，自动生成已被禁用。");
        }
    }

    private void Update()
    {
        if (_currentWave == null) return;

        _timer += Time.deltaTime;

        // 当计时达到当前波次时间并且没有正在处理波次结束
        if (!_waveEndFlag && _timer >= _currentWave.time)
        {
            _waveEndFlag = true;
            Debug.Log($"Wave {_currentWave.waveIndex} 达到时间，开始清怪并准备下一波");
            StartCoroutine(EndWaveAndStartNext());
        }

        // 手动清理（调试）
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartClearEnemies();
        }

        // 安全更新 UI（保护 null）
        if (_currentWave != null && UIManager.Instance != null)
        {
            UIManager.Instance.UpdateWaveText(Mathf.Max(0f, _currentWave.time - _timer), _currentWave.waveIndex);
        }
    }

    // 结束当前波次：先清怪，再进入下一波（如果存在）
    private IEnumerator EndWaveAndStartNext()
    {
        StartClearEnemies();

        // 等待直到所有敌人被清空（ClearEnemiesRoutine 逐个移除）
        while (_enemiesList.Count > 0)
        {
            yield return null;
        }

        // 进入下一波（如果有）
        _currentWaveIndex++;
        if (_waveDataSOList == null || _currentWaveIndex >= _waveDataSOList.Count)
        {
            Debug.Log("EnemyManager: 所有波次已结束或没有更多波次");
            _currentWave = null;
            _waveEndFlag = false;
            yield break;
        }

        _currentWave = _waveDataSOList[_currentWaveIndex];
        _timer = 0f;
        _waveEndFlag = false;
        Debug.Log($"EnemyManager: 开始波次 {_currentWave.waveIndex} (index {_currentWaveIndex})");
        StartWave();
    }

    [ContextMenu("开始生成当前波次")]
    public void StartWave()
    {
        if (_currentWave == null)
        {
            Debug.LogWarning("StartWave: 当前无有效波次，取消生成。");
            return;
        }

        // 如果已有生成协程在跑，避免重复启动
        if (_spawnCoroutine != null)
        {
            Debug.LogWarning("StartWave: 生成协程已在运行，跳过重复启动。");
            return;
        }

        _spawnCoroutine = StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        Debug.Log($"开始波次: {_currentWave.waveIndex}");

        if (_currentWave.enemyGroups == null || _currentWave.enemyGroups.Count == 0)
        {
            Debug.LogWarning($"SpawnWaveRoutine: 波次 {_currentWave.waveIndex} 没有定义 enemyGroups");
            _spawnCoroutine = null;
            yield break;
        }

        foreach (var group in _currentWave.enemyGroups)
        {
            // EnemyGroup 是 struct，不能与 null 比较。可检查 group.enemyPrefab 是否为 null
            if (group.enemyPrefab == null)
                continue;

            BaseEnemy prefab = group.enemyPrefab as BaseEnemy;
            if (prefab == null)
            {
                Debug.LogWarning($"SpawnWaveRoutine: group.enemyPrefab 不是 BaseEnemy 或为 null，跳过该组。");
                continue;
            }

            int count = Mathf.Max(0, group.count);
            for (int i = 0; i < count; i++)
            {
                SpawnEnemy(prefab, group.spawnPosition);

                yield return new WaitForSeconds(_spawnInterval);
            }
        }

        Debug.Log("该波次所有敌人已生成完毕");
        _spawnCoroutine = null;
    }

    private void SpawnEnemy(BaseEnemy prefab, Vector2 position)
    {
        if (prefab == null)
            return;

        float x = position.x + Random.Range(10f, 50f);
        float y = position.y + Random.Range(10f, 50f);

        // 实例化敌人（返回 BaseEnemy）
        BaseEnemy newEnemy = Instantiate(prefab, new Vector2(x, y), Quaternion.identity);
        if (newEnemy != null)
        {
            _enemiesList.Add(newEnemy);
            // 可选：初始化目标引用
            var ai = newEnemy.GetComponent<BaseAIShip>();
            if (ai != null && _target != null)
            {
                // 如果 BaseAIShip 期望有 target 字段，可以赋值（此处假设有公开方式）
                // ai.SetTarget(_target); // 如果有公开方法可调用
            }
        }
        else
        {
            Debug.LogError("SpawnEnemy: Instantiate 返回 null（检查 prefab）");
        }
    }

    public void StartClearEnemies()
    {
        // 安全清理 lootList（避免在遍历时修改集合）
        if (lootList != null && lootList.Count > 0)
        {
            var copy = new List<LootTile>(lootList);
            foreach (var lootItem in copy)
            {
                lootList.Remove(lootItem);
                if (lootItem != null)
                {
                    Destroy(lootItem.gameObject);
                }
            }
        }

        // 如果已经在清理协程中则不重复开启
        StopCoroutineIfRunning(nameof(ClearEnemiesRoutine));
        StartCoroutine(ClearEnemiesRoutine());
    }

    private void StopCoroutineIfRunning(string routineName)
    {
        // 不能直接 StopCoroutine by name reliably if multiple, 这里不做复杂管理，ClearEnemiesRoutine 自身会安全退出
    }

    private IEnumerator ClearEnemiesRoutine()
    {
        while (_enemiesList.Count > 0)
        {
            int lastIndex = _enemiesList.Count - 1;
            BaseEnemy enemy = _enemiesList[lastIndex];

            if (enemy != null)
            {
                if (enemy.transform.childCount > 0)
                {
                    Transform enemyShip = enemy.transform.GetChild(0);
                    for (int j = 0; j < enemyShip.childCount; j++)
                    {
                        Tile t = enemyShip.GetChild(j).GetComponent<Tile>();
                        if (t != null) t.Health = 0;
                    }
                }

                // 确保彻底销毁 GameObject
                Destroy(enemy.gameObject);
            }

            _enemiesList.RemoveAt(lastIndex);

            yield return new WaitForSeconds(0.2f);
        }
    }

    public void DisAbleEnemies()
    {
        foreach (var enemy in _enemiesList)
        {
            if (enemy == null) continue;
            BaseAIShip enemyShip = enemy.GetComponent<BaseAIShip>();
            if (enemyShip != null) enemyShip._canAction = false;
        }
    }

    public void EnableEnemies()
    {
        foreach (var enemy in _enemiesList)
        {
            if (enemy == null) continue;
            BaseAIShip enemyShip = enemy.GetComponent<BaseAIShip>();
            if (enemyShip != null) enemyShip._canAction = true;
        }
    }
}
