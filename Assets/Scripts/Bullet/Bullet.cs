using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] public float speed = 20f;

    //自动销毁，防止内存爆掉
    [SerializeField] public float lifeTime = 3f;

    [SerializeField] public float damage = 20f;

    [Header("特效设置")]
    [SerializeField] private GameObject hitEffectPrefab;

    public Vector2 _direction;

    public Ship _shooter;

    void Start()
    {
        // 子弹生成时，直接给它一个向前的速度
        GetComponent<Rigidbody2D>().velocity = _direction * speed;
        Destroy(gameObject, lifeTime);
    }

    // 碰撞检测逻辑可以在这里扩展
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.transform.parent.parent != _shooter.transform)
        {
            var hitTile = collision.GetComponent<Tile>();
            hitTile.Health -= damage;
            //Debug.Log($"damage :{hitTile.Health}");

            if (hitEffectPrefab != null)
            {
                // 在子弹当前的位置，以默认旋转角度生成特效
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
