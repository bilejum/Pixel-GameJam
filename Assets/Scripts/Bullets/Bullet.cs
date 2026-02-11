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
        if (collision.GetComponent<LootTile>() != null) return;

        Transform shooterRoot = _shooter.transform;
        Transform targetRoot = collision.transform.root;

        // 情况 A：撞到了敌军（原逻辑）
        if (targetRoot != shooterRoot)
        {
            var hitTile = collision.GetComponent<Tile>();
            if (hitTile != null)
            {
                hitTile.Health -= damage;
                if (hitEffectPrefab != null) Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
        // 情况 B：撞到了友军（新增逻辑：分裂检测）
        else
        {
            // 如果撞到的是自己飞船上的 PurpleTile
            if (collision.TryGetComponent<PurpleTile>(out PurpleTile purple))
            {
                // 这里什么都不用做，让 PurpleTile 自己的 OnTriggerEnter2D 去处理分裂
                // 或者为了保险，在这里手动调用：purple.ManualSplit(this);
            }
        }
    }
}
