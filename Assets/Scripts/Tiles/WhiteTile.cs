using UnityEngine;

public class WhiteTile : Tile
{
    public override void Init()
    {
        _color = Color.white;
        base.Init();
    }

    private void OnDestroy()
    {
        _ship.CoreDestory();
    }
}