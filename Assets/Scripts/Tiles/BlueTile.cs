using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueTile : Tile
{
    public override void Init()
    {
        base.Init();
        //增加扭矩力和速度
        _ship.ThrustForce *= 1.1f;
        _ship.TurnTorque *= 1.1f;

    }

    private void OnDestroy()
    {
        _ship.ThrustForce /= 1.1f;
        _ship.TurnTorque /= 1.1f;
    }
}
