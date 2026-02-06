using System;
using UnityEngine;

//这个Type指的是方块的功能类别，譬如能否攻击
public enum TileType
{
    Attack,
    Core,
    Other
}

public abstract class Tile : MonoBehaviour
{
    [SerializeField] protected float _health = 100f;

    [SerializeField] protected Color _color;

    [SerializeField] protected GameObject _destroyEffectPrefab;

    public Vector3Int _coordinate;

    protected Ship _ship;

    protected SpriteRenderer _spriteRenderer;

    [SerializeField] public TileType _tileType;
    public float Health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = value;

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
                }
                Destroy(gameObject);
            }

        }
    }
    private void Awake()
    {
        _ship = transform.parent.parent.GetComponent<Ship>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        Init();
        _spriteRenderer.color = _color;
    }

    public virtual void Init()
    {
        //Debug.Log($"{_color.ToString()},已安装!");
    }
}