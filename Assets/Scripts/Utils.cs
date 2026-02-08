using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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


}
