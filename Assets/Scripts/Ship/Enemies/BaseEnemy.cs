using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseEnemy : BaseAIShip
{

    public static Action OnEnemyKilled;

    public System.Action OnEnemyDestroyed;

    private EnemyManager _enemyManager;

    [SerializeField]private LootTile _lootTile;

    public bool systemKill = false;

    public LootTableSO lootTable; // 在 Inspector 中拖入创建好的配置文件
    protected override void Start()
    {
        _enemyManager = EnemyManager.Instance;
        if (_enemyManager != null)
        {
            _enemyManager._enemiesList.Add(this);
            _target = _enemyManager._target;
        }

        _lootTile = Resources.Load<LootTile>("Prefabs/Tiles/LootTile");
    }

    private void OnDestroy()
    {
        if (_enemyManager != null)
        {
            _enemyManager._enemiesList.Remove(this);
            Tile itemToDrop = lootTable.GetRandomItem();

            if (itemToDrop != null && !systemKill)
            {
                var newLootTile = Instantiate(_lootTile, transform.position, Quaternion.identity);
                newLootTile.tilePrefab = itemToDrop;
                newLootTile.GetComponent<SpriteRenderer>().color = itemToDrop._color;
                _enemyManager.lootList.Add(newLootTile);
            }
        }

        OnEnemyKilled?.Invoke();
        OnEnemyDestroyed?.Invoke();
    }

}
