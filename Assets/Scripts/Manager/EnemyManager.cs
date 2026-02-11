using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    public Ship _target;
    public List<BaseEnemy> _enemiesList = new List<BaseEnemy>(); // 初始化空列表避免null

    [SerializeField] private BaseEnemy _enemyPrefab;

    // 波次核心状态（增加访问控制与显式初始化）
    [Header("波次设置")]
    [SerializeField] private bool _spawnEnemyFlag = false;
    public List<WaveDataSO> _waveDataSOList = new List<WaveDataSO>();
    [SerializeField] private string _waveDataSOPath = "ScriptableObjects";
    [SerializeField] private WaveDataSO _currentWave;
    [SerializeField] private int _currentWaveIndex = 0;
    public float _spawnInterval = 0.5f;

    private float _timer;
    private bool _isWaveSpawnCompleted = false; // 仅在敌人生成完毕后计时
    private bool _isClearingEnemies = false; // 标记清敌流程是否正在进行
    public List<LootTile> lootList = new List<LootTile>();
    public bool startSpawnEnemyFlag;
    public bool _waveEndFlag = false; // 波次结束总控标记

    // 协程引用管理（确保精准控制）
    private Coroutine _spawnCoroutine;
    private Coroutine _clearEnemiesCoroutine; // 新增清敌协程引用
    private Coroutine _endWaveCoroutine; // 新增波次结束协程引用

    private void Awake()
    {
        // 单例模式强化（防止重复初始化）
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 可选：跨场景保留

        _target = GameManager.Instance?._playerShip;

        // 波次数据加载（容错增强）
        LoadWaveData();

        // 初始化第一波（安全校验）
        if (_waveDataSOList.Count > 0)
        {
            _currentWaveIndex = Mathf.Clamp(_currentWaveIndex, 0, _waveDataSOList.Count - 1);
            _currentWave = _waveDataSOList[_currentWaveIndex];
            Debug.Log($"初始化第一波: {_currentWave.waveIndex}");
        }
        else
        {
            Debug.LogError("无波次数据，请检查ScriptableObject配置！");
        }
    }

    private void Start()
    {
        // 延迟启动第一波（避免Awake阶段依赖项未就绪）
        if (_currentWave != null)
        {
            StartCoroutine(DelayedStartFirstWave(1f));
        }
    }

    private IEnumerator DelayedStartFirstWave(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartWave();
    }

    private void LoadWaveData()
    {
        var waveDatas = Resources.LoadAll<WaveDataSO>(_waveDataSOPath);
        if (waveDatas != null && waveDatas.Length > 0)
        {
            _waveDataSOList.Clear();
            foreach (var wd in waveDatas)
            {
                if (wd != null) _waveDataSOList.Add(wd);
            }
            Debug.Log($"加载波次数据: {_waveDataSOList.Count} 个");
        }
        else
        {
            Debug.LogWarning($"未在Resources/{_waveDataSOPath}找到WaveDataSO");
        }
    }

    private void Update()
    {
        if (_currentWave == null || _waveEndFlag || !_isWaveSpawnCompleted) return;

        _timer += Time.deltaTime;

        // 波次超时判断（严格条件：仅在生成完成且未结束时触发）
        if (_timer >= _currentWave.time)
        {
            _waveEndFlag = true;
            Debug.Log($"[TIMEOUT] 波次 {_currentWave.waveIndex} 超时，启动清敌流程");
            TriggerEndWaveSequence();
        }

        // UI更新（安全校验）
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateWaveText(Mathf.Max(0f, _currentWave.time - _timer), _currentWave.waveIndex);
        }

        // 调试快捷键（保留）
        if (Input.GetKeyDown(KeyCode.C))
        {
            ForceClearEnemies();
        }
    }

    // 波次结束流程触发器（单入口，防止重复调用）
    private void TriggerEndWaveSequence()
    {
        if (_endWaveCoroutine != null)
        {
            StopCoroutine(_endWaveCoroutine);
            Debug.LogWarning("终止正在运行的波次结束协程");
        }
        _endWaveCoroutine = StartCoroutine(EndWaveAndStartNext());
    }

    // 波次结束核心流程（串行化执行，确保时序严谨）
    private IEnumerator EndWaveAndStartNext()
    {
        Debug.Log($"开始波次结束流程: {_currentWave.waveIndex}");

        // 1. 终止当前生成协程（防止新敌人）
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
            Debug.Log("终止生成协程");
        }

        // 2. 强制停止所有敌人行为
        DisAbleEnemies();

        // 3. 启动清敌流程并等待完成（关键修复）
        yield return StartCoroutine(SafeClearEnemies());

        // 4. 波次结算延迟（可选，增强玩家体验）
        yield return new WaitForSeconds(1.5f);

        // 5. 准备下一波（边界校验+状态重置）
        _currentWaveIndex++;
        if (_currentWaveIndex >= _waveDataSOList.Count)
        {
            Debug.Log("所有波次完成！游戏胜利！");
            OnAllWavesCompleted();
            yield break;
        }

        // 6. 重置状态，启动新波次
        _currentWave = _waveDataSOList[_currentWaveIndex];
        _timer = 0f;
        _waveEndFlag = false;
        _isWaveSpawnCompleted = false;
        Debug.Log($"=== 启动新波次: {_currentWave.waveIndex} (索引{_currentWaveIndex}) ===");
        StartWave();

        // 7. 释放协程引用
        _endWaveCoroutine = null;
    }

    [ContextMenu("开始当前波次")]
    public void StartWave()
    {
        if (_currentWave == null)
        {
            Debug.LogWarning("无有效波次数据，无法启动");
            return;
        }

        // 终止已有生成协程（避免重叠）
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }

        _spawnCoroutine = StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        Debug.Log($"波次生成开始: {_currentWave.waveIndex}");
        _isWaveSpawnCompleted = false;

        // 安全校验：敌人组配置
        if (_currentWave.enemyGroups == null || _currentWave.enemyGroups.Count == 0)
        {
            Debug.LogWarning($"波次 {_currentWave.waveIndex} 无敌人组配置");
            _isWaveSpawnCompleted = true;
            _spawnCoroutine = null;
            yield break;
        }

        foreach (var group in _currentWave.enemyGroups)
        {
            if (group.enemyPrefab == null) continue;

            BaseEnemy prefab = group.enemyPrefab as BaseEnemy;
            if (prefab == null)
            {
                Debug.LogWarning($"敌人预制体不是BaseEnemy类型: {group.enemyPrefab.name}");
                continue;
            }

            int spawnCount = Mathf.Max(0, group.count);
            for (int i = 0; i < spawnCount; i++)
            {
                // 波次结束时立即终止生成（关键修复）
                if (_waveEndFlag)
                {
                    Debug.Log($"波次提前结束，终止剩余{spawnCount - i}个敌人");
                    _isWaveSpawnCompleted = true;
                    _spawnCoroutine = null;
                    yield break;
                }

                SpawnEnemy(prefab, group.spawnPosition);
                yield return new WaitForSeconds(_spawnInterval);
            }
        }

        Debug.Log($"波次生成完成: {_currentWave.waveIndex}");
        _isWaveSpawnCompleted = true;
        _spawnCoroutine = null;
    }

    private void SpawnEnemy(BaseEnemy prefab, Vector2 position)
    {
        if (prefab == null) return;

        // 生成位置随机偏移（边界校验）
        float x = position.x + Random.Range(10f, 50f);
        float y = position.y + Random.Range(10f, 50f);

        BaseEnemy newEnemy = Instantiate(prefab, new Vector2(x, y), Quaternion.identity);
        if (newEnemy != null)
        {
            _enemiesList.Add(newEnemy);
            // 绑定AI目标（假设BaseAIShip有SetTarget方法）
            var aiShip = newEnemy.GetComponent<BaseAIShip>();
            if (aiShip != null && _target != null)
            {
                aiShip.SetTarget(_target); // 建议实现此方法
            }
            // 注册敌人销毁回调（关键优化：实时维护敌人列表）
            newEnemy.OnEnemyDestroyed += () => RemoveEnemyFromList(newEnemy);
        }
        else
        {
            Debug.LogError($"预制体实例化失败: {prefab.name}");
        }
    }

    // 敌人销毁回调（确保列表实时更新，避免null引用）
    private void RemoveEnemyFromList(BaseEnemy enemy)
    {
        if (_enemiesList.Contains(enemy))
        {
            _enemiesList.Remove(enemy);
            Debug.Log($"敌人移除，剩余: {_enemiesList.Count}");
        }
    }

    // 安全清敌流程（原子操作，防止并发冲突）
    public IEnumerator SafeClearEnemies()
    {
        if (_isClearingEnemies)
        {
            Debug.LogWarning("清敌流程已在进行中");
            yield break;
        }
        _isClearingEnemies = true;
        Debug.Log("启动安全清敌流程");

        // 1. 清理掉落物（深拷贝避免遍历修改）
        ClearLoots();

        // 2. 清理敌人（逐帧销毁，避免性能峰值）
        int initialCount = _enemiesList.Count;
        int clearedCount = 0;

        // 制作敌人列表快照（防止遍历中修改）
        List<BaseEnemy> enemiesToClear = new List<BaseEnemy>(_enemiesList);

        foreach (var enemy in enemiesToClear)
        {
            if (enemy != null)
            {
                // 强制销毁敌人（包含子物体）
                Destroy(enemy.gameObject);
                clearedCount++;
            }
            yield return new WaitForSeconds(0.1f); // 控制销毁速度
        }

        // 3. 强制清空列表（防止残留引用）
        _enemiesList.Clear();
        Debug.Log($"清敌完成: 共{initialCount}个，成功销毁{clearedCount}个");

        // 4. 重置清敌状态
        _isClearingEnemies = false;
    }

    // 强制清敌（调试用，立即执行）
    public void ForceClearEnemies()
    {
        if (_endWaveCoroutine != null)
        {
            StopCoroutine(_endWaveCoroutine);
            _endWaveCoroutine = null;
        }
        StartCoroutine(SafeClearEnemies());
    }

    private void ClearLoots()
    {
        if (lootList.Count == 0) return;

        List<LootTile> lootsToClear = new List<LootTile>(lootList);
        foreach (var loot in lootsToClear)
        {
            if (loot != null) Destroy(loot.gameObject);
            lootList.Remove(loot);
        }
        Debug.Log($"清理掉落物: {lootsToClear.Count}个");
    }

    public void DisAbleEnemies()
    {
        foreach (var enemy in _enemiesList)
        {
            if (enemy == null) continue;
            var aiShip = enemy.GetComponent<BaseAIShip>();
            if (aiShip != null) aiShip._canAction = false;
        }
    }

    public void EnableEnemies()
    {
        foreach (var enemy in _enemiesList)
        {
            if (enemy == null) continue;
            var aiShip = enemy.GetComponent<BaseAIShip>();
            if (aiShip != null) aiShip._canAction = true;
        }
    }

    private void OnAllWavesCompleted()
    {
        // 游戏胜利逻辑（示例）
        Debug.Log("=== 所有波次挑战成功！===");
        // UIManager.Instance.ShowVictoryUI();
    }

    // 状态重置（场景切换或重试时调用）
    public void ResetAllStates()
    {
        // 终止所有协程
        StopAllCoroutines();

        // 重置状态变量
        _currentWaveIndex = 0;
        _timer = 0f;
        _waveEndFlag = false;
        _isWaveSpawnCompleted = false;
        _isClearingEnemies = false;

        // 清空列表
        _enemiesList.Clear();
        lootList.Clear();

        // 重置协程引用
        _spawnCoroutine = null;
        _clearEnemiesCoroutine = null;
        _endWaveCoroutine = null;

        Debug.Log("EnemyManager状态已完全重置");
    }

    private void OnDestroy()
    {
        // 释放单例引用
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

