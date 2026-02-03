using UnityEngine;

public class InstructArrow : MonoBehaviour
{
    [Header("缩放设置")]
    [SerializeField] private float _baseSize = 1.0f;
    [SerializeField] private float _sizeReference = 5f;

    [Header("位移偏移设置")]
    [SerializeField] private float _baseOffset = 1.5f;
    [SerializeField] private float _offsetMultiplier = 0.5f;

    [Header("动画设置")]
    [Range(1f, 20f)]
    [SerializeField] private float _smoothSpeed = 10f; // 丝滑程度，值越大越快，越小越迟钝

    private void Update()
    {
        float camSize = Camera.main.orthographicSize;

        // --- 1. 计算目标值 (Target Values) ---
        float targetScaleValue = (camSize / _sizeReference) * _baseSize;
        float targetYOffset = _baseOffset + (camSize * _offsetMultiplier);

        // --- 2. 使用 Lerp 进行平滑处理 ---
        // 缩放平滑
        float newScale = Mathf.Lerp(transform.localScale.x, targetScaleValue, Time.deltaTime * _smoothSpeed);
        transform.localScale = Vector3.one * newScale;

        // 位置平滑
        float newY = Mathf.Lerp(transform.localPosition.y, targetYOffset, Time.deltaTime * _smoothSpeed);
        transform.localPosition = new Vector3(0, newY, 0);
    }
}