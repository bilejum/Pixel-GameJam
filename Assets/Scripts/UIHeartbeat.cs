using UnityEngine;

public class UIHeartbeat : MonoBehaviour
{
    [Header("设置")]
    public float speed = 10f;      // 速度
    public float amount = 0.1f;   // 幅度

    private Vector3 initialScale;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        // 使用 Sin 函数计算缩放值
        float pulse = 1.0f + Mathf.Sin(Time.time * speed) * amount;
        transform.localScale = initialScale * pulse;
    }
}