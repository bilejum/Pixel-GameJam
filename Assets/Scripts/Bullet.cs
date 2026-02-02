using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 3f; // 3秒后自动销毁，防止内存爆掉

    public float damage = 20f;

    void Start()
    {
        // 子弹生成时，直接给它一个向前的速度
        GetComponent<Rigidbody2D>().velocity = transform.up * speed;
        Destroy(gameObject, lifeTime);
    }

    // 碰撞检测逻辑可以在这里扩展
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("hit");
        var hitTile = collision.GetComponent<Tile>();
        hitTile.Health -= damage;
        Debug.Log($"damage :{hitTile.Health}");
        
        Destroy(gameObject);
    }
}
