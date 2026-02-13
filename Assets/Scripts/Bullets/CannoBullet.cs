using UnityEngine;

public class CannoBullet : Bullet
{
    private bool hasExploded = false;

    [Header("范围伤害设置")]
    [SerializeField] private float explosionRadius = 10f;
    [SerializeField] private bool isDamageAttenuation = true;

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 基础检查
        if (collision == null || _shooter == null || hasExploded) return;

        // 2. 过滤掉落物和子弹（防止子弹互相撞炸）
        if (collision.GetComponent<LootTile>() != null) return;
        if (collision.GetComponent<Bullet>() != null) return;

        Transform shooterRoot = _shooter.transform;
        Transform targetRoot = collision.transform.root;

        // 3. 碰到自己人：直接穿透，不触发爆炸
        if (targetRoot == shooterRoot)
        {
            // 如果需要紫色方块分裂逻辑，可以在这里调用
            return;
        }

        // 4. 碰到非友军物体：立即执行爆炸并销毁
        ExecuteExplosion();
        Destroy(gameObject);
    }

    private void ExecuteExplosion()
    {
        if (hasExploded) return;
        hasExploded = true;

        // 范围检测伤害
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            // 爆炸也不会伤到自己人
            if (_shooter != null && hitCollider.transform.root == _shooter.transform) continue;

            var hitTile = hitCollider.GetComponent<Tile>();
            if (hitTile != null)
            {
                float finalDamage = damage;
                if (isDamageAttenuation)
                {
                    float distance = Vector2.Distance(transform.position, hitCollider.transform.position);
                    float damagePercent = Mathf.Clamp01(1 - (distance / explosionRadius));
                    finalDamage *= damagePercent;
                }
                hitTile.Health -= finalDamage;
            }
        }

        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    private void OnDestroy()
    {
        // 如果是自然死亡（空爆），则触发爆炸
        if (!hasExploded)
        {
            ExecuteExplosion();
        }
    }
}