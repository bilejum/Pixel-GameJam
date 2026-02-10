using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueTile : Tile
{

    [SerializeField] private float _addThrustForceAmount;
    [SerializeField] private float _addTurnTorqueAmount;
    [SerializeField] private float _addStrafeForceAmount;

    protected override void Awake()
    {
        //增加扭矩力和速度
        _tileType = TileType.Other;
        base.Awake();

        _ship.ThrustForce += _addThrustForceAmount;
        _ship.TurnTorque += _addTurnTorqueAmount;
        _ship.StrafeForce += _addStrafeForceAmount;
    }

    private void OnDestroy()
    {
        _ship.ThrustForce -= _addThrustForceAmount;
        _ship.TurnTorque -= _addTurnTorqueAmount;
        _ship.StrafeForce -= _addStrafeForceAmount;
    }
}
