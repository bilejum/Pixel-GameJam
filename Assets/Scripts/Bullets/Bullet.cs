using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] public float speed = 20f;

    //自动销毁，防止内存爆掉
    [SerializeField] public float lifeTime = 3f;

    [SerializeField] public float damage = 20f;

    [Header("特效设置")]
    [SerializeField] protected GameObject hitEffectPrefab;

    public Vector2 _direction;

    public Ship _shooter;

    [SerializeField] private string bulletSound;
    private void Awake()
    {
        AudioManager.Instance.PlaySFX(bulletSound);
    }

    void Start()
    {
        // 子弹生成时，直接给它一个向前的速度
        GetComponent<Rigidbody2D>().velocity = _direction * speed;
        Destroy(gameObject, lifeTime);
    }

    // 碰撞检测逻辑可以在这里扩展
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. 基础安全检查
        if (collision == null || _shooter == null) return;

        // 2. 检查碰撞体是否带有 LootTile 组件，如果有，直接无视
        if (collision.GetComponent<LootTile>() != null) return;

        // 3. 原有的逻辑
        Transform shooterRoot = _shooter.transform;
        Transform targetRoot = collision.transform.root;

        if (targetRoot != shooterRoot)
        {
            // 伤害逻辑...
            var hitTile = collision.GetComponent<Tile>();
            if (hitTile != null)
            {
                hitTile.Health -= damage;
                // 只有打到 Tile 时子弹才消失
                if (hitEffectPrefab != null)
                {
                    Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                }
                Destroy(gameObject);
            }
        }
    }
}
