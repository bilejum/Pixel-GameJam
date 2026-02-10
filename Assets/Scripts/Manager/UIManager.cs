using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using VInspector.Libs;

public class UIManager : MonoBehaviour
{
    //设置UIManager单例
    public static UIManager Instance { get; private set; }

    private PlayerShip _playerShip;

    public RectTransform _buildingUI;

    //建造菜单缩进大小
    [SerializeField]
    private float _buildingUIZoomInAmont = 6;

    [SerializeField]
    private float _buildingUIZoomOutAmont = 40;
    private float _lastZoomAmount;

    //速度表
    [SerializeField]
    private RectTransform _GaugePointer;

    [SerializeField]
    private float _smoothSpeed = 5f; // 平滑速度

    //能量条
    [SerializeField]
    private TextMeshProUGUI _energyText;

    //波次
    [SerializeField]
    private TextMeshProUGUI _waveTimeText;

    [SerializeField]
    private TextMeshProUGUI _waveIndexText;

    public List<Button> _backPackUIList;

    [SerializeField]
    private GameObject _backPackUI;

    [SerializeField] private GameObject _crosshair;

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
    }

    private void Start()
    {
        _playerShip = GameManager.Instance._playerShip;
    }

    public void SelectedSlot(Button button)
    {
        // 获取当前点击的格子索引
        int index = _backPackUIList.IndexOf(button);

        // 1. 安全检查：索引是否在有效范围内
        // 如果 items 列表只有 3 个元素，你点第 4 个格子（index 3）就会报越界错误
        if (index >= 0 && index < InventoryManager.Instance.itemList.Count)
        {
            var selectedItem = InventoryManager.Instance.itemList[index];

            // 2. 检查该位置的数据对象是否存在
            if (selectedItem != null && selectedItem.tilePrefab != null)
            {
                Debug.Log(selectedItem.tilePrefab);
                var tileprefab = selectedItem.tilePrefab;
                // 在这里执行后续逻辑，比如显示物品详情或使用物品
                _playerShip.SelectedTile(selectedItem);
            }
            else
            {
                Debug.Log("该格子数据为空");
            }
        }
        else
        {
            Debug.Log("这个格子超出当前数据范围，确实没东西");
        }
    }

    public void DeleteMode()
    {
        if (!_playerShip.deleteMode)
        {
            Debug.Log("enter delete");
            _playerShip.deleteMode = true;
            _playerShip._selectedTile = null;
        }
        else
        {
            _playerShip.deleteMode = false;
        }
    }

    public void AdjustGaugePointer(float thrustforce)
    {
        // 1. 限制输入值在 0-200 之间，防止指针转飞了
        float clampedForce = Mathf.Clamp(thrustforce, 0f, 200f);

        // 2. 计算目标角度 (0->90, 100->0, 200->-90)
        float targetAngle = 90f - (clampedForce / 200f * 180f);

        // 3. 创建目标旋转（UI 通常绕 Z 轴旋转）
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

        // 4. 应用平滑过度动画
        // 注意：如果这个方法是在 Update 里调用的，Lerp 会非常平滑
        _GaugePointer.rotation = Quaternion.Lerp(
            _GaugePointer.rotation,
            targetRotation,
            Time.deltaTime * _smoothSpeed
        );
    }

    public void AdjustEnergy(float energy, float maxEnergy)
    {
        _energyText.text = $"{energy}/{maxEnergy}";
    }

    public void SwitchBuildingUI(bool buildingUIFlag)
    {
        if (buildingUIFlag)
        {
            _lastZoomAmount = CameraHandler.Instance.GetOrthographicSize();
            _buildingUI.gameObject.SetActive(buildingUIFlag);
            CameraHandler.Instance.ZoomInToShip(_buildingUIZoomInAmont);
        }
        else
        {
            _buildingUI.gameObject.SetActive(buildingUIFlag);
            _buildingUIZoomOutAmont = _lastZoomAmount;
            CameraHandler.Instance.ZoomInToShip(_buildingUIZoomOutAmont);
        }
    }

    public void UpdateWaveText(float time, int currentWave)
    {
        _waveTimeText.text = time.ToString("F0");

        _waveIndexText.text = "Wave" + currentWave.ToString();
    }
}
