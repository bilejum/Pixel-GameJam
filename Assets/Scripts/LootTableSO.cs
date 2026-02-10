using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootItem
{
    public Tile itemPrefab; // 掉落的预制体

    [Range(0, 100)]
    public int dropWeight; // 权重：权重越高，掉落几率越大
}

[CreateAssetMenu(fileName = "New Loot Table", menuName = "ScriptableObject/LootTable")]
public class LootTableSO : ScriptableObject
{
    public List<LootItem> lootItems = new List<LootItem>();

    public Tile GetRandomItem()
    {
        // 1. 计算总权重
        int totalWeight = 0;
        foreach (var item in lootItems)
        {
            totalWeight += item.dropWeight;
        }

        // 2. 在总权重范围内取随机数
        int randomNumber = Random.Range(0, totalWeight);

        // 3. 筛选物品
        int cumulativeWeight = 0;
        foreach (var item in lootItems)
        {
            cumulativeWeight += item.dropWeight;
            if (randomNumber < cumulativeWeight)
            {
                return item.itemPrefab;
            }
        }

        return null;
    }
}
