using UnityEngine;

public class WhiteTile : Tile
{
    protected override void Awake()
    {
        _color = Color.white;
        _tileType = TileType.Core;
        base.Awake();
    }

    protected override void OnTileDeath()
    {
        if (_ship != null)
        {
            // 这里会正确触发 PlayerShip 或 EnemyShip 的重写方法
            _ship.CoreDestory();
        }
        // 注意：不要在这里 Destroy(gameObject)，
        // 因为 Ship.CoreDestory 会把整个父物体删掉
    }
}