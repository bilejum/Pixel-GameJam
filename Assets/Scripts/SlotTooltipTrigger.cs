using UnityEngine;
using UnityEngine.EventSystems; // 必须引用事件系统接口

public class SlotTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private int _buttonIndex;

    // 初始化方法，由 UIManager 或背包生成器调用
    public void Setup(int index)
    {
        _buttonIndex = index;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 鼠标移入时调用单例显示信息
        UIManager.Instance.ShowTooltip(_buttonIndex);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 鼠标移出时隐藏
        UIManager.Instance.HideTooltip();
    }
}