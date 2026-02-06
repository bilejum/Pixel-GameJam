using UnityEngine;

public class WhiteTile : Tile
{
    public override void Init()
    {
        _color = Color.white;
        _tileType = TileType.Core;
        base.Init();

    }

    private void OnDestroy()
    {
        _ship.CoreDestory();
    }
}