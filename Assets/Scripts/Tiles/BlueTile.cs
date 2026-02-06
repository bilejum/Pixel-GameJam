using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueTile : Tile
{

    [SerializeField] private float _addThrustForceAmount;
    [SerializeField] private float _addTurnTorqueAmount;
    public override void Init()
    {
        base.Init();
        //增加扭矩力和速度
        _ship.ThrustForce += _addThrustForceAmount;
        _ship.TurnTorque += _addTurnTorqueAmount;

        _tileType = TileType.Other;

    }

    private void OnDestroy()
    {
        _ship.ThrustForce -= _addThrustForceAmount;
        _ship.TurnTorque -= _addTurnTorqueAmount;
    }
}
