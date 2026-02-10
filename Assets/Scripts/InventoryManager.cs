using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class ItemData
{
    public Tile tilePrefab;
    public int count;

    public ItemData(Tile tilePrefab, int count)
    {
        this.tilePrefab = tilePrefab;
        this.count = count;
    }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance; // 单例模式方便访问

    public List<ItemData> itemList = new List<ItemData>();

    public int space = 12; // 背包格子上限

    private Color32 slotColor = new Color32(56, 56, 56, 194);
    void Awake()
    {
        Instance = this;


    }
    private void Start()
    {
        AddItem(Resources.Load<Tile>("Prefabs/Tiles/Red Tile"), 2);
        AddItem(Resources.Load<Tile>("Prefabs/Tiles/Red Tile"), 2);
        AddItem(Resources.Load<Tile>("Prefabs/Tiles/Red Tile"), 2);
        AddItem(Resources.Load<Tile>("Prefabs/Tiles/Blue Tile"), 2);
        AddItem(Resources.Load<Tile>("Prefabs/Tiles/Yellow Tile"), 2);
    }

    public void AddItem(Tile tile, int count)
    {
        // 1. 尝试在现有列表中找到相同的物品
        ItemData existingItem = itemList.Find(x => x.tilePrefab == tile);

        if (existingItem != null)
        {
            // 2. 如果找到了，直接加数量
            existingItem.count += count;
        }
        else
        {
            // 3. 如果没找到，先检查背包空间
            if (itemList.Count < space)
            {
                itemList.Add(new ItemData(tile, count));
            }
            else
            {
                Debug.LogWarning("背包已满！");
                return; // 空间满了就不执行后面的 UI 刷新
            }
        }

        // 4. 无论哪种情况（只要添加成功），都刷新 UI
        UpdateBackpackUI();
    }

    public void DeleteItem(Tile tile, int count)
    {
        // 1. 尝试在现有列表中找到相同的物品
        ItemData existingItem = itemList.Find(x => x.tilePrefab == tile);

        if (existingItem != null)
        {
            // 2. 如果找到了，直减数量
            existingItem.count -= count;
            if(existingItem.count <= 0)
            {
                itemList.Remove(existingItem);
            }
        }
        else
        {
            // 3. 如果没找到，发出错误报告
            Debug.LogWarning("delete fail");
        }

        // 4. 无论哪种情况（只要添加成功），都刷新 UI
        UpdateBackpackUI();
    }


    public void UpdateBackpackUI()
    {
        var count = 0;
        for (int i = 0; i < 12; i++)
        {
            UIManager.Instance._backPackUIList[i].image.color = slotColor;
            UIManager.Instance._backPackUIList[i].GetComponentInChildren<TextMeshProUGUI>().text = 0.ToString();
        }

        foreach (var item in itemList)
        {
            UIManager.Instance._backPackUIList[count].image.color = itemList[count].tilePrefab.GetComponent<Tile>()._color;
            UIManager.Instance._backPackUIList[count].GetComponentInChildren<TextMeshProUGUI>().text = itemList[count].count.ToString();
            count += 1;
        }

    }
}
