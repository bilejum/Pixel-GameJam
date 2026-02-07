using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    public Ship target;

    public List<BaseEnemy> _enemiesList;

    [SerializeField] private BaseEnemy enemyPrefab;

    [SerializeField] private bool _spawnEnemyFlag = false;

    private void Awake()
    {
        Instance = this;
        _enemiesList = new List<BaseEnemy>();

        target = GameManager.Instance._playerShip;
    }


    private float timer;
    private void Update()
    {
        if (_spawnEnemyFlag)
        {
            timer += Time.deltaTime;
            if (timer > 1f)
            {
                SpawnEnemies();
                timer = 0f;
            }
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearEnemies();
        }
    }

    private void SpawnEnemies()
    {
        var x = Random.Range(-100, 100);
        var y = Random.Range(-100, 100);

        Instantiate(enemyPrefab, new Vector3(x, y, 0), Quaternion.identity);
    }

    public void ClearEnemies()
    {
        foreach (var enemy in _enemiesList)
        {
            var enemyShip = enemy.transform.GetChild(0);
            var enemyShipIndex = enemy.transform.childCount;
            for (int i = 0; i < enemyShipIndex; i++)
            {
                enemyShip.GetChild(i).GetComponent<Tile>().Health = 0;
            }
        }
    }

    public void StopAllEnemies()
    {

    }
}
