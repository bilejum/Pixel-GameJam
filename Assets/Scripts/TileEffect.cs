using UnityEngine;
using System.Collections;

public class TileEffect : MonoBehaviour
{
    private Material _instancedMaterial; // 存一下方块自己的材质
    private float _currentDissolve = 0f; // 当前消融进度
    public float dissolveSpeed = 0.8f;   // 消融速度

    void Awake()
    {
        // 获取当前物体 SpriteRenderer 组件里的材质
        // 使用 .material 会自动创建该材质的副本，这样改动就不会影响到其他方块
        _instancedMaterial = GetComponent<SpriteRenderer>().material;
    }

    // 当方块被打中或者死亡时调用这个方法
    [ContextMenu("Test Dissolve")] // 这行代码能让你在编辑器里右键组件手动触发测试
    public void PlayDissolve(float currentDissolve)
    {
        StopAllCoroutines(); // 防止重复触发
        StartCoroutine(DissolveRoutine(currentDissolve));
    }

    IEnumerator DissolveRoutine(float currentDissolve)
    {
        _currentDissolve = currentDissolve;

        _instancedMaterial.SetFloat("_DissolveAmount", _currentDissolve);

        yield return null; // 等待下一帧
    }
}
