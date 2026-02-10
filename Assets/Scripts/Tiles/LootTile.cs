using System.Collections;
using UnityEngine;

public class LootTile : MonoBehaviour
{
    public Tile tilePrefab;
    private bool isBeingCollected = false; // 防止重复触发

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 使用 NameToLayer 是正确的
        if (!isBeingCollected && collision.transform.root.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // 开启吸取协程，传入玩家的 Transform
            StartCoroutine(CollectRoutine(collision.transform.root));
        }
    }

    private IEnumerator CollectRoutine(Transform playerTransform)
    {
        isBeingCollected = true;
        float speed = 2f; // 初始速度
        float acceleration = 1.5f; // 加速度，让过程有“吸进去”的感觉

        // 当物体距离玩家大于一个很小的值时，持续飞向玩家
        while (Vector3.Distance(transform.position, playerTransform.position) > 0.2f)
        {
            // 每一帧都重新计算朝向玩家的方向
            Vector3 direction = (playerTransform.position - transform.position).normalized;

            // 速度随时间增加
            speed += acceleration;

            // 移动
            transform.position += direction * speed * Time.deltaTime;

            // 可选：添加缩小动画，看起来更像被吸收了
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * 5f);

            yield return null; // 等待下一帧
        }

        // 真正到达位置后，加入背包并销毁自己
        InventoryManager.Instance.AddItem(tilePrefab, 1);
        Destroy(gameObject);
    }
}