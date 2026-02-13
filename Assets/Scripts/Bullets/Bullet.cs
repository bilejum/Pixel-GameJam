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

    // 标记此子弹是否已经被分裂过，防止重复分裂
    [HideInInspector] public bool hasSplit = false; // 新加标记位

    private void Awake()
    {
        AudioManager.Instance.PlaySFX(bulletSound);
        // 子弹生成时，直接给它一个向前的速度

    }

    private void Start()
    {
        GetComponent<Rigidbody2D>().velocity = _direction * speed;
        Destroy(gameObject, lifeTime);
    }

    // 碰撞检测逻辑可以在这里扩展
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null || _shooter == null) return;

        // 1. 过滤：如果是掉落物，或者是另一颗子弹，直接无视
        if (collision.GetComponent<LootTile>() != null) return;
        if (collision.GetComponent<Bullet>() != null) return;

        Transform shooterRoot = _shooter.transform;
        Transform targetRoot = collision.transform.root;

        // 情况 A：撞到了友军/自己
        if (targetRoot == shooterRoot)
        {
            // 如果是紫色方块，触发分裂（这里逻辑可以保留，但不要 Destroy）
            if (collision.TryGetComponent<PurpleTile>(out PurpleTile purple))
            {
                // 分裂逻辑由 PurpleTile 触发，这里子弹继续飞行穿过
            }
            return; // 关键：碰到自己人直接返回，不销毁，不扣血
        }

        // 情况 B：撞到了敌军
        var hitTile = collision.GetComponent<Tile>();
        if (hitTile != null)
        {
            hitTile.Health -= damage;
            if (hitEffectPrefab != null) Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
