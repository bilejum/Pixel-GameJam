using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 黄色方块，属于生产能源的方块，可以增加最大能量，并且还能发电！
/// </summary>
public class YellowTile : Tile
{
    private float _chargeTimer;
    [SerializeField] private float _chargeCD;

    [SerializeField] private float _chargeAmount;
    [SerializeField] private float _maxCharge;

    protected override void Awake()
    {
        _color = Color.yellow;
        _tileType = TileType.Other;
        base.Awake();
    }

    private void Start()
    {
        _ship._maxEnergy += _maxCharge;
    }

    protected override void Update()
    {
        base.Update();
        _chargeTimer += Time.deltaTime;
        if (_chargeTimer >= _chargeCD && _ship._energy <_ship._maxEnergy)
        {
            _ship._energy += _chargeAmount;
            _chargeTimer = 0f;
        }
    }


    private void OnDestroy()
    {
        _ship._maxEnergy -= _maxCharge;
    }
}
