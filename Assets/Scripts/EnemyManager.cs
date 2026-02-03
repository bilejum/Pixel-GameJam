using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    public Ship target;

    public List<BaseEnemy> _enemiesList;

    private void Awake()
    {
        Instance = this;
        _enemiesList = new List<BaseEnemy>();
    }
}
