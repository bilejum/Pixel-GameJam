using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ship : MonoBehaviour
{
    protected Rigidbody2D rb;

    [SerializeField] protected Bullet _bullet;

    protected Grid _grid;

    protected Dictionary<Vector3Int, Tile> _tileGrid;

    [Header("动力设置")]
    [SerializeField] protected float _thrustForce = 100f; // 推进力
    [SerializeField] protected float _turnTorque = 15f;   // 转向力矩

    //对外暴露的加速度和扭矩力变量
    public float ThrustForce
    {
        get
        {
            return _thrustForce;
        }
        set
        {
            _thrustForce = value;
            if (_thrustForce <= 0)
            {
                _thrustForce = 0f;
            }

        }
    }
    public float TurnTorque
    {
        get
        {
            return _turnTorque;
        }
        set
        {
            _turnTorque = value;
            if (_turnTorque <= 0)
            {
                _turnTorque = 0f;
            }

        }
    }

    protected virtual void Awake()
    {
        _grid = GetComponentInChildren<Grid>();
        rb = GetComponent<Rigidbody2D>();
        _tileGrid = new Dictionary<Vector3Int, Tile>();
    }

    public void DeleteTileInGrid(Vector3Int cellPos)
    {
        _tileGrid.Remove(cellPos);
    }

    public void CoreDestory()
    {
        Destroy(this.gameObject);
    }

    //应用后坐力
    public void ApplyRecoil(Vector2 direction, float multiplier, float recoilForce)
    {
        // 后坐力的方向与子弹发射方向完全相反
        Vector2 recoilDir = -direction;

        // 计算总冲量：基础后坐力 * 炮管强化倍率
        float totalForce = recoilForce * multiplier;

        // 使用 ForceMode2D.Impulse (瞬间冲击力)
        // 这样不需要持续施加力，更符合爆炸发射的感觉
        rb.AddForce(recoilDir * totalForce, ForceMode2D.Impulse);
    }

}

