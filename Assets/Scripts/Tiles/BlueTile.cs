using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueTile : Tile
{
    public override void Init()
    {
        base.Init();
        //增加扭矩力和速度
        _ship.ThrustForce += 50;
        _ship.TurnTorque += 20;

        _tileType = TileType.Other;

    }

    private void OnDestroy()
    {
        _ship.ThrustForce -= 50;
        _ship.TurnTorque -= 20;
    }
}
