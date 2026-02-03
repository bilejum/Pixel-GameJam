using System;
using UnityEngine;

public class RedTile : Tile
{
    public override void Init()
    {
        _color = Color.red;
        base.Init();

        //出生增加弹药容量
        _ship._ammoCapacity += 1;
    }

    private void OnDestroy()
    {
        if (_ship != null) _ship._ammoCapacity -= 1;
    }
}