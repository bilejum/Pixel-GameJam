using UnityEngine;

public class PurpleTile : Tile
{
    [Header("分裂设置")]
    [SerializeField] private float _splitAngle = 30f; // 斜向发射的角度偏移
    [SerializeField] private float _energyCostPerSplit = 5f; // 每次分裂消耗母舰能量（可选）

    protected override void Awake()
    {
        base.Awake();
        // 设置为 Other 或者是你自定义的 Utility 类型
        _tileType = TileType.Other;
    }

    // 确保你的方块上有 Collider2D 并且勾选了 Is Trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Bullet>(out Bullet b))
        {
            // 如果子弹已经分裂过，或者能量不足，直接跳过
            if (b.hasSplit || (_ship != null && _ship._energy < _energyCostPerSplit)) return;

            SplitBullet(b);
        }
    }

    private void SplitBullet(Bullet original)
    {
        // 1. 获取原方向
        Vector2 baseDir = original._direction.normalized;

        // 2. 计算三个方向：正前，左偏，右偏
        Vector2 forwardDir = baseDir;
        Vector2 leftDir = RotateVector(baseDir, _splitAngle);
        Vector2 rightDir = RotateVector(baseDir, -_splitAngle);

        Vector2[] targetDirections = { forwardDir, leftDir, rightDir };

        // 3. 生成新子弹
        foreach (Vector2 dir in targetDirections)
        {
            // 让新子弹生成在方块中心稍微靠前一点的位置
            Vector3 spawnPos = transform.position + (Vector3)dir.normalized * 0.5f;
            Bullet newBullet = Instantiate(original, spawnPos, Quaternion.identity);
            newBullet._direction = dir;
            newBullet._shooter = original._shooter;

            // 【关键】标记新子弹为已分裂状态，防止它们再次触发当前的 PurpleTile
            newBullet.hasSplit = true;

        }
        // 4. 消耗母舰能量并销毁原子弹
        if (_ship != null) _ship.ConsumeEnergy(_energyCostPerSplit);
        Destroy(original.gameObject);

        // 5. 视觉反馈：可以调用之前 Tile 里的效果或震动
        // _tileEffect.PlayDissolve(0.1f); // 模拟一点点消耗感
    }

    // 旋转向量的工具函数
    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;

        return new Vector2((cos * tx) - (sin * ty), (sin * tx) + (cos * ty));
    }
}