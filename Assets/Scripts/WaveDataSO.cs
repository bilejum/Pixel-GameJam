using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct EnemyGroup
{
    public Ship enemyPrefab;
    public int count;
    public Vector2 spawnPosition;
}

[CreateAssetMenu(menuName ="ScriptableObject/WavaDataSO")]
public class WaveDataSO : ScriptableObject
{
    public int waveIndex;
    public List<EnemyGroup> enemyGroups;
    public float time;
}
