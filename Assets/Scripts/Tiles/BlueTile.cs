using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueTile : Tile
{

    [SerializeField] private float _addThrustForceAmount;
    [SerializeField] private float _addTurnTorqueAmount;
    [SerializeField] private float _addStrafeForceAmount;
    [SerializeField] private float _adddashForceAmount;

    protected override void Awake()
    {
        //增加扭矩力和速度
        _tileType = TileType.Blue;
        base.Awake();

        _ship.ThrustForce += _addThrustForceAmount;
        _ship.TurnTorque += _addTurnTorqueAmount;
        _ship.StrafeForce += _addStrafeForceAmount;
        _ship._dashForce += _adddashForceAmount;
    }

    private void OnDestroy()
    {
        _ship.ThrustForce -= _addThrustForceAmount;
        _ship.TurnTorque -= _addTurnTorqueAmount;
        _ship.StrafeForce -= _addStrafeForceAmount;
        _ship._dashForce -= _adddashForceAmount;
    }
}
