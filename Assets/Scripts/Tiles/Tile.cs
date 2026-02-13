using Cinemachine;
using System;
using UnityEngine;

//这个Type指的是方块的功能类别，譬如能否攻击
public enum TileType
{
    Attack,
    Core,
    Blue,
    Other,
}

public abstract class Tile : MonoBehaviour
{
    [Header("血量相关")]
    [SerializeField] protected float _health = 100f;
    [SerializeField] protected float _maxHealth = 100f;
    [SerializeField] protected float _healthRecoveyRate =0.1f; //这是百分比

    [Header("杂项")]
    [SerializeField] public Color _color;

    [SerializeField] protected GameObject _destroyEffectPrefab;

    public Vector3Int _coordinate;

    protected Ship _ship;


    [SerializeField] public TileType _tileType;


    [SerializeField] protected Material _material;

    protected TileEffect _tileEffect;

    private CinemachineImpulseSource _impulseSource;

    [Header("开火设置")]
    [SerializeField] protected float _energyConsume;

    // 隐藏在 Inspector 中，因为这是运行时动态赋值的
    [HideInInspector] public ItemData originItemData;

    // ... 原有变量 ...
    [HideInInspector] public Tile sourcePrefab; // 记录来源

    public string Info;
    public float Health
    {
        get
        {
            return _health;
        }
        set
        {
            if (value < _health)
            {
                _ship.SetShipState = ShipState.Fight;

                if (_ship is PlayerShip && _impulseSource != null)
                {
                    // 产生震动：可以根据伤害大小调整震动强度
                    _impulseSource.GenerateImpulse();
                }
            }



            _health = value;

            var healthPercent = _health / _maxHealth;
            _tileEffect.PlayDissolve(1- healthPercent);

            // 逻辑处理：血量归零销毁物体
            if (_health <= 0)
            {
                //把自己从坐标里删了
                _ship.DeleteTileInGrid(_coordinate);

                if (_destroyEffectPrefab != null)
                {
                    // 在子弹当前的位置，以默认旋转角度生成特效
                    var newDestroyEffect = Instantiate(_destroyEffectPrefab, transform.position, Quaternion.identity);
                    var main = newDestroyEffect.GetComponent<ParticleSystem>().main;
                    var colorOvertime = newDestroyEffect.GetComponent<ParticleSystem>().colorOverLifetime;
                    colorOvertime.color = _color;

                    OnTileDeath();
                }

            }
        }
    }

    protected virtual void Awake()
    {
        _ship = transform.parent.parent.GetComponent<Ship>();
        _tileEffect = GetComponent<TileEffect>();

        //_material = Resources.Load<Material>("Shaders/Shader Graphs_DissolveShader");
        //_spriteRenderer.material = _material;

        _impulseSource = GetComponentInParent<CinemachineImpulseSource>();
    }

    protected virtual void Update()
    {
        //脱战自动回血
        if(_ship.GetShipState is ShipState.Noncombat && _health < _maxHealth)
        {
            Health += _maxHealth * _healthRecoveyRate * Time.deltaTime;
        }
    }

    protected virtual void OnTileDeath()
    {
        // 普通方块直接死
        Destroy(gameObject);
    }

}