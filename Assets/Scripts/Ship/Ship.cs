using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// 主要用于判断是否脱战
/// </summary>
public enum ShipState
{
    Fight,
    Noncombat
}
public abstract class Ship : MonoBehaviour
{
    protected Rigidbody2D rb;

    protected Grid _grid;

    protected Dictionary<Vector3Int, Tile> _tileGrid;

    [Header("动力设置")]
    [SerializeField] protected float _thrustForce = 0f; // 推进力
    [SerializeField] protected float _turnTorque = 0f;   // 转向力矩
    [SerializeField] protected float _strafeForce = 0f; // 横向移动力度
    [SerializeField]
    public float _dashForce = 50f; // 冲刺力度

    public float _energy = 0f;
    public float _maxEnergy = 0f;

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
    public float StrafeForce { get => _strafeForce; set => _strafeForce = value; }

    [Header("脱战设置")]
    private ShipState _shipState = ShipState.Noncombat;
    public float _noncombatCD = 10f;
    private float _nocombatTimer;

    public ShipState SetShipState
    {
        set
        {
            if (value is ShipState.Fight)
            {
                _shipState = ShipState.Fight;
                _nocombatTimer = 0;
            }
        }
    }

    public ShipState GetShipState
    {
        get
        {
            return _shipState;
        }

    }



    protected virtual void Awake()
    {

        _grid = GetComponentInChildren<Grid>();
        rb = GetComponent<Rigidbody2D>();
        _tileGrid = new Dictionary<Vector3Int, Tile>();

        // 修复：如果该 Ship 的 prefab 已经包含若干 Tile（作为子对象），
        // 需要在 Awake 时把这些 Tile 注册到 _tileGrid，这样敌机 prefab 上的枪塔才能被调度到 HandleAttack。
        var existingTiles = GetComponentsInChildren<Tile>(includeInactive: true);
        foreach (var t in existingTiles)
        {
            // 如果 Tile 已有坐标且未被注册，则加入字典；
            // 若坐标未设置（默认 Vector3Int.zero），仍尝试注册，后续逻辑可以根据需要调整坐标管理策略。
            if (!_tileGrid.ContainsKey(t._coordinate))
            {
                _tileGrid[t._coordinate] = t;
            }
        }
    }

    protected virtual void Update()
    {
        //进入脱战判断，被打的时候会重置脱战计时
        if (_shipState is ShipState.Fight)
        {
            _nocombatTimer += Time.deltaTime;
            //脱战CD
            if (_nocombatTimer >= _noncombatCD)
            {
                _shipState = ShipState.Noncombat;
                _nocombatTimer = 0f;
            }
        }
    }

    //轮询调度所有可以攻击的Tile
    protected virtual void HandleAttack(Vector2 direction)
    {
        foreach (var tile in _tileGrid.Values)
        {
            if (tile._tileType is TileType.Attack)
            {
                var attackTile = tile as BaseAttackTile;
                attackTile.Shoot(direction);
            }
        }
    }

    public void DeleteTileInGrid(Vector3Int cellPos)
    {
        _tileGrid.Remove(cellPos);
    }

    public void CoreDestory()
    {
        AudioManager.Instance.PlaySFX("Kill");
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

    public void ConsumeEnergy(float amount)
    {
        _energy -= amount;
    }

}

