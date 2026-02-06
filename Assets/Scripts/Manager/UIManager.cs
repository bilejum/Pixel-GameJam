using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class UIManager : MonoBehaviour
{
    //设置UIManager单例
    public static UIManager Instance { get; private set; }

    private PlayerShip _playerShip;

    public RectTransform _buildingUI;

    //建造菜单缩进大小
    [SerializeField] private float _buildingUIZoomInAmont = 6;
    [SerializeField] private float _buildingUIZoomOutAmont = 40;
    private float _lastZoomAmount;

    //速度表
    [SerializeField] private RectTransform _GaugePointer;
    [SerializeField] private float _smoothSpeed = 5f;      // 平滑速度

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            _playerShip = GameManager.Instance._playerShip;
        }
    }
    public void DeleteMode()
    {
        if (!_playerShip.deleteMode)
        {
            Debug.Log("enter delete");
            _playerShip.deleteMode = true;
            _playerShip.SelectedTile(null);
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
        _GaugePointer.rotation = Quaternion.Lerp(_GaugePointer.rotation, targetRotation, Time.deltaTime * _smoothSpeed);
    }

    public void SwitchColor(string tileColor)
    {
        if (tileColor == null) return;
        if (_playerShip == null) return;

        string LoadPath = $"Prefabs/Tiles/{tileColor} Tile";

        Tile tilePrefab = Resources.Load<Tile>(LoadPath);

        Debug.Log(tilePrefab);
        //将获取的TilePrefab传给playerShip
        _playerShip.SelectedTile(tilePrefab);
        Debug.Log($"成功加载并选择{tileColor}Tile！");
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
}
