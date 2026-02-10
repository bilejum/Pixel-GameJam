using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Utils
{
    public static Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10;

        return Camera.main.ScreenToWorldPoint(mousePos);
    }


    // Create Text in the World
    public static TextMesh CreateworldText(Transform parent, string text, Vector3 localPosition, int fontsize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
    {
        GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.text = text;
        textMesh.fontSize = fontsize;
        textMesh.color = color;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        textMesh.transform.localScale = new Vector3(0.1f, 0.1f, 0);
        return textMesh;
    }


    public static Vector2 GetDirectionToMouse(UnityEngine.Transform transform)
    {
        // 获取鼠标世界坐标
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        // 计算方向：从飞船位置指向鼠标位置
        Vector2 directionToMouse = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;

        return directionToMouse;
    }

    public static bool IsPointerOverUI()
    {
        // 1. 检查当前是否有UI被射线命中
        if (EventSystem.current == null) return false;

        // 2. 创建指针事件数据，检测当前鼠标位置的UI
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        // 3. 检测命中的UI对象
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // 4. 有命中结果则说明点击在UI上
        return results.Count > 0;
    }

}
