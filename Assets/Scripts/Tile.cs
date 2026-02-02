using UnityEngine;

public class Tile : MonoBehaviour
{
    // 1. 定义一个私有的“后备字段”，用于存储实际数据
    // 使用 [SerializeField] 可以让它在 Unity 检查面板中显示
    [SerializeField] private float health = 100f;

    // 2. 定义公共属性，用于逻辑控制
    public float Health
    {
        get
        {
            return health; // 返回私有字段的值
        }
        set
        {
            health = value; // 修改私有字段的值，不会导致循环调用

            // 逻辑处理：血量归零销毁物体
            if (health <= 0)
            {
                Destroy(gameObject);
            }

            //怎么你一进我就动不了了！！！俺没动！
        }
    }
}