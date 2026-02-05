using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private PlayerShip _playerShip;

    [SerializeField] private TextMeshProUGUI _ammoText;

    [SerializeField] private List<Tile> tilesDic;

    public RectTransform _buildingUI;

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
    public void UpdateAmmoText(int ammoAmount, int AmmoCapacity)
    {
        if (_ammoText == null) return;
        _ammoText.text = $"Ammo: {ammoAmount}/{AmmoCapacity}";
    }

    public void SwitchColor(string tileColor)
    {
        switch (tileColor)
        {
            case "White":
                _playerShip.SelectedTile(tilesDic[0]);
                break;
            case "Red":
                _playerShip.SelectedTile(tilesDic[1]);
                break;
            case "Blue":
                _playerShip.SelectedTile(tilesDic[2]);
                break;
            default:
                break;
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


}
