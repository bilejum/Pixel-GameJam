using UnityEngine;

public class FadeEffect : MonoBehaviour
{
    [Range(0f, 1f)]
    public float minAlpha = 0.2f;

    private SpriteRenderer _spriteRenderer;
    private Color _baseColor;
    private float _targetAlpha = 1f; // 默认满血透明度为 1

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        // 初始记录颜色

    }

    private void Start()
    {
        _baseColor = _spriteRenderer.color;
    }

    public void UpdateVisual(float healthPercent)
    {
        // 映射透明度：血量 1 -> Alpha 1, 血量 0 -> Alpha minAlpha
        _targetAlpha = Mathf.Lerp(minAlpha, 1f, healthPercent);
    }

    private void Update()
    {
        if (_spriteRenderer == null) return;

        // 直接赋值目标透明度，不要用 1 减
        _baseColor.a = _targetAlpha;
        _spriteRenderer.color = _baseColor;
    }
}