using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : BaseAIShip
{
    private EnemyManager _enemyManager;
    protected override void Start()
    {
        _enemyManager = EnemyManager.Instance;
        if (_enemyManager != null)
        {
            _enemyManager._enemiesList.Add(this);
            _target = _enemyManager._target;
        }
    }

    private void OnDestroy()
    {
        if (_enemyManager != null)
        {
            _enemyManager._enemiesList.Remove(this);
        }
    }

}
