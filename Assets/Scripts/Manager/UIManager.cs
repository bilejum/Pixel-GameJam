using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

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

    [SerializeField]
    private GameObject _crosshair;

    [SerializeField]
    public CanvasGroup blackScreenGroup;
    public float fadeDuration = 1.5f; // 变黑持续时间

    public CanvasGroup loseScreen;

    public TextMeshProUGUI scoreText;

    public Slider _energyBar;

    private float _targetFillAmount; // 记录目标比例
    public float lerpSpeed = 5f; // 平滑速度

    //左侧栏目
    public GameObject _Info;
    public GameObject Infotext;

    [Header("动画设置")]
    [SerializeField]
    private float _fadeDuration = 0.5f; // 过渡时间

    public RectTransform notification;
    public RectTransform notificationText;
    

    private void OnEnable()
    {
        // 订阅分数改变信号
        GameManager.OnScoreChanged += UpdateScoreUI;
    }

    private void OnDisable()
    {
        GameManager.OnScoreChanged -= UpdateScoreUI;
    }

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

        // --- 必须加上这一段，否则触发器没有索引，或者根本没激活 ---
        for (int i = 0; i < _backPackUIList.Count; i++)
        {
            // 尝试获取或添加触发器组件
            var trigger = _backPackUIList[i].gameObject.GetComponent<SlotTooltipTrigger>();
            if (trigger == null)
            {
                trigger = _backPackUIList[i].gameObject.AddComponent<SlotTooltipTrigger>();
            }
            trigger.Setup(i); // 核心：告诉脚本它是第几个格子
        }

        // 场景加载后，如果黑屏是满的，就慢慢消失
        if (blackScreenGroup.alpha > 0)
        {
            StartCoroutine(FadeFromBlackRoutine());
        }
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

    public void StartFadeToBlack()
    {
        StartCoroutine(FadeRoutine(0, 1));
    }

    private IEnumerator FadeRoutine(float startAlpha, float endAlpha)
    {
        float elapsed = 0;
        blackScreenGroup.blocksRaycasts = true;
        float startMusicVol = AudioManager.Instance.musicSource.volume;

        // 初始状态确保 LoseScreen 是透明的
        loseScreen.alpha = 0;
        loseScreen.blocksRaycasts = false;
        loseScreen.blocksRaycasts = true; // 变黑完成后，允许点击失败面板上的按钮

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeDuration);

            // 1. 背景变黑：从 0 到 1
            blackScreenGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, progress);

            // 2. 失败文字浮现：同样从 0 到 1 (注意这里不再用 1 - ...)
            loseScreen.alpha = progress;

            // 3. 音量淡出
            AudioManager.Instance.musicSource.volume = Mathf.Lerp(startMusicVol, 0f, progress);
            AudioManager.Instance.sfxSource.volume = Mathf.Lerp(1f, 0f, progress);

            yield return null;
        }

        blackScreenGroup.alpha = endAlpha;
        loseScreen.alpha = 1; // 最终完全显示
    }

    private IEnumerator FadeFromBlackRoutine()
    {
        float elapsed = 0;
        while (elapsed < 1f)
        {
            elapsed += Time.unscaledDeltaTime;
            blackScreenGroup.alpha = 1 - (elapsed / 1f);
            yield return null;
        }
        blackScreenGroup.alpha = 0;
        blackScreenGroup.blocksRaycasts = false;
    }

    private void UpdateScoreUI(int newScore)
    {
        // 更新 UI 文本显示
        scoreText.text = "SCORE: " + newScore.ToString();
    }

    public void UpdateEnergy(float energyValue, float maxEnergy)
    {
        // 1. 安全计算目标比例
        if (maxEnergy > 0)
        {
            _targetFillAmount = energyValue / maxEnergy;
        }
        else
        {
            _targetFillAmount = 0f;
        }
        _energyBar.value = Mathf.Lerp(
            _energyBar.value,
            _targetFillAmount,
            Time.unscaledDeltaTime * lerpSpeed
        );
    }

    public void AdjustEnergy(float energy, float maxEnergy)
    {
        _energyText.text = $"{energy}/{maxEnergy}";
    }

    // 在 UIManager 类中添加

    public void ShowTooltip(int index)
    {
        // 1. 范围检查
        if (index >= 0 && index < InventoryManager.Instance.itemList.Count)
        {
            var item = InventoryManager.Instance.itemList[index];

            // 2. 确保 item 和 tilePrefab 都不为空
            if (item != null && item.tilePrefab != null)
            {
                // 3. 从 Prefab 上获取 Tile 组件（因为 Tile 是抽象类，GetComponent 依然有效）
                Tile tileScript = item.tilePrefab.GetComponent<Tile>();

                if (tileScript != null)
                {
                    _Info.SetActive(true);

                    // 4. 获取 TextMeshProUGUI 组件并赋值
                    var textComponent = Infotext.GetComponent<TextMeshProUGUI>();
                    if (textComponent != null)
                    {
                        // 这里直接读取你在 Tile 类里定义的 Info 变量
                        textComponent.transform.parent.gameObject.SetActive(true);
                        textComponent.text = tileScript.Info;
                    }
                }
            }
        }
    }

    public void HideTooltip()
    {
        _Info.SetActive(false);
    }

    // UIManager.cs 内部

    public void ToggleDeleteMode()
    {
        if (_playerShip == null)
            return;

        // 1. 切换删除模式状态
        _playerShip.deleteMode = !_playerShip.deleteMode;

        // 2. 逻辑处理：如果进入了删除模式，通常要清除当前选中的方块（不显示 ghostTile）
        if (_playerShip.deleteMode)
        {
            Debug.Log("进入删除模式");
            _playerShip._selectedTile = null;

            // 隐藏 GhostTile（你可以直接在 UIManager 调用或在 PlayerShip 里处理）
            if (_playerShip._ghostTile != null)
                _playerShip._ghostTile.SetActive(false);

            // 视觉反馈：改变鼠标图标或按钮颜色（可选）
            // buttonImage.color = Color.red;
        }
        else
        {
            Debug.Log("退出删除模式");
            // buttonImage.color = Color.white;
        }
    }
}
