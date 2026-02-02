using System;
using UnityEngine;

public abstract class Tile : MonoBehaviour
{
    [SerializeField] protected float _health = 100f;
    protected Color _color;

    protected Ship _ship;

    protected SpriteRenderer _spriteRenderer;
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
        Debug.Log($"{_color.ToString()},已安装!");
    }
}