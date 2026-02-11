using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannoBullet : Bullet
{
    private bool hasExploded = false; // 防止重复爆炸

    [Header("范围伤害设置")]
    [SerializeField] private float explosionRadius = 10f; // 爆炸范围半径
    [SerializeField] private bool isDamageAttenuation = true; // 是否开启伤害衰减


    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 安全检查：确保 collision 和 shooter 存在
        if (collision == null || _shooter == null) return;
        if (collision.GetComponent<LootTile>() != null) return;

        // 2. 优化判断：避免使用 parent.parent，建议给发射者和敌人设置不同的 Layer 或 Tag
        // 这里暂时保留你的逻辑但加上空值保护
        Transform shooterRoot = _shooter.transform;
        Transform targetRoot = collision.transform.root; // 直接找最上级父物体，更安全

        if (targetRoot != shooterRoot)
        {
            // 开启范围检测
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

            foreach (var hitCollider in hitColliders)
            {
                // 过滤掉发射者
                if (hitCollider.transform.root == shooterRoot) continue;

                // 获取组件时增加空值检查
                var hitTile = hitCollider.GetComponent<Tile>();
                if (hitTile != null)
                {
                    hitTile.Health -= damage;
                }
            }

            // 生成特效并销毁
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }

            // 确保销毁逻辑在最后
            Destroy(gameObject);
        }
    }

}
