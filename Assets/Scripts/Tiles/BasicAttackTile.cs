using System;
using UnityEngine;



public abstract class BaseAttackTile : Tile
{
    [SerializeField] private Bullet _bullet;

    [Header("开火设置")]
    protected float _weaponShootCDTimer;
    [SerializeField] protected float _weaponShootCD = 0.5f;
    [SerializeField] private float _recoilForce = 5f; // 基础后坐力


    protected override void Awake()
    {
        _tileType = TileType.Attack;
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
        _weaponShootCDTimer += Time.deltaTime;
    }

    public void Shoot(Vector2 direction)
    {
        if (_weaponShootCDTimer >= _weaponShootCD && _ship._energy >= _energyConsume)
        {
            var newBullet = Instantiate<Bullet>(_bullet, transform.position, Quaternion.identity);
            newBullet._direction = direction;
            newBullet._shooter = _ship;
            _weaponShootCDTimer = 0;
            _ship.ApplyRecoil(direction,1f,_recoilForce);
            _ship.ConsumeEnergy(_energyConsume);
        }
    }
}