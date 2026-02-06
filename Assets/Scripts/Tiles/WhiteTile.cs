using UnityEngine;

public class WhiteTile : Tile
{
    protected override void Awake()
    {
        _color = Color.white;
        _tileType = TileType.Core;
        base.Awake();
    }

    private void OnDestroy()
    {
        _ship.CoreDestory();
    }
}