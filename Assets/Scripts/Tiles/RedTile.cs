using System;
using UnityEngine;

/// <summary>
/// 红色方块，本身作为最基础炮台使用，从方块内部发射
/// </summary>

public class RedTile : Tile
{
    [SerializeField] private Bullet bullet;

    [Header("开火设置")]
    [SerializeField] protected AudioSource _FireSound;
    protected float _weaponShootCDTimer;
    [SerializeField] protected float _weaponShootCD = 0.5f;
    [SerializeField] private float _recoilForce = 5f; // 基础后坐力


    private void Awake()
    {
        _FireSound = GetComponent<AudioSource>();
    }

    public override void Init()
    {
        _color = Color.red;
        _tileType = TileType.Attack;
        base.Init();
    }
    private void Update()
    {
        _weaponShootCDTimer += Time.deltaTime;
    }

    public void Shoot(Vector2 direction)
    {
        if (_weaponShootCDTimer >= _weaponShootCD)
        {
            var newBullet = Instantiate<Bullet>(bullet, transform.position, Quaternion.identity);
            newBullet._direction = direction;
            newBullet._shooter = _ship;
            _weaponShootCDTimer = 0;
            _ship.ApplyRecoil(direction,1f,_recoilForce);
            _FireSound.Play();
        }
    }
}