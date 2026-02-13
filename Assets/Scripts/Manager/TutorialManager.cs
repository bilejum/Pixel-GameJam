using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI 引用")]
    public RectTransform notificationRoot; // 对应你的 notification
    public TextMeshProUGUI hintText;       // 对应你的 notificationText 上的文字组件

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt("FirstTimeLogin", 1) == 1)
            StartCoroutine(TutorialRoutine());
        else
            notificationRoot.gameObject.SetActive(false);
    }

    private IEnumerator TutorialRoutine()
    {
        yield return new WaitForSeconds(2f);

        // 1. 打开背包
        ShowStep("按 TAB 键打开背包");
        yield return new WaitUntil(() => UIManager.Instance._buildingUI.gameObject.activeSelf);

        // 2. 选择并安装
        ShowStep("选择一个方块组装飞船");
        // 等待玩家点击了格子
        yield return new WaitUntil(() => GameManager.Instance._playerShip._selectedTile != null);

        ShowStep("左侧面板显示了不同方块的功能");
        // 等待玩家把方块放下去（选中的方块变回 null）
        yield return new WaitUntil(() => Input.GetMouseButton(0));


        // 3. 删除检查
        ShowStep("点击右下角红色图标进入删除模式");
        yield return new WaitUntil(() => GameManager.Instance._playerShip.deleteMode == true);

        ShowStep("Tab 退出建造模式");
        yield return new WaitUntil(() => !UIManager.Instance._buildingUI.gameObject.activeSelf);

        ShowStep("W A S D 推进（需要安装速度方块）");
        yield return new WaitForSeconds(4f);


        ShowStep("Space 冲刺");
        yield return new WaitForSeconds(4f);
        // 5. 射击与能量检查
        ShowStep("鼠标左键射击(需要安装攻击方块和能量方块)");

        yield return new WaitForSeconds(4f);

        // 6. 结束


        ShowStep("敌机将在5秒后抵达");
        yield return new WaitForSeconds(5f);

        FinishTutorial();
    }

    private void FinishTutorial()
    {
        PlayerPrefs.SetInt("FirstTimeLogin", 0);
        PlayerPrefs.Save();
        notificationRoot.gameObject.SetActive(false);
        if (EnemyManager.Instance != null) EnemyManager.Instance.UnlockAndStartFirstWave();
    }
    private void ShowStep(string message)
    {
        notificationRoot.gameObject.SetActive(true);
        hintText.text = message;

        // 如果你想加个“叮”的声音
         AudioManager.Instance.PlaySFX("Tip");
    }

    // 调试用：按下 K 键重置教程状态
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            PlayerPrefs.SetInt("FirstTimeLogin", 1);
            Debug.Log("教程已重置，下次启动生效");
        }
    }


}