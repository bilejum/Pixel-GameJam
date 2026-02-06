using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerShip : Ship
{
    private Vector3Int _cellPos;

    [SerializeField] private Tile _selectedTile;

    [Header("手感优化")]
    [SerializeField] private float _linearDrag = 1f;    // 线性阻力（空气阻力感）
    [SerializeField] private float _angularDrag = 2f;   // 旋转阻力（防止无限自转）

    //控制输入值(0,1)
    private float _thrustInput;
    private float _turnInput;

    public bool deleteMode = false;

    //是否可以移动，主要用于UI切换
    public bool canMove = true;

    private Vector2 directionToMouse;



    //从这开始
    protected override void Awake()
    {
        base.Awake();
        rb.gravityScale = 0f;           // 太空通常没重力
        rb.drag = _linearDrag;          // 设置阻力
        rb.angularDrag = _angularDrag;  // 设置旋转阻力
    }

    public void SelectedTile(Tile tile)
    {
        _selectedTile = tile;
    }
    private void Start()
    {
        InitShip();
    }

    private void Update()
    {
        _thrustInput = Input.GetAxis("Vertical");
        _turnInput = Input.GetAxis("Horizontal");

        if (Input.GetMouseButtonDown(0) && GameManager.Instance.State is GameState.Build)
        {
            _cellPos = _grid.WorldToCell(Utils.GetMouseWorldPos());
            if (!deleteMode)
            {
                if (CanSetTile(_cellPos))
                {
                    SetTile(_cellPos, _selectedTile);
                }
            }
            else if (deleteMode)
            {
                DeleteTile(_cellPos);
            }
        }

        if(Input.GetMouseButton(0) && GameManager.Instance.State is GameState.Game)
        {
            HandleAttack();
        }

        //修改速度表
        UIManager.Instance.AdjustGaugePointer(_thrustForce * _thrustInput);

        directionToMouse = Utils.GetDirectionToMouse(transform);
    }

    private void FixedUpdate()
    {
        // 2. 在 FixedUpdate 中应用物理力
        if (canMove)
        {
            HandleMovement();
        }
    }

    //初始化飞船，目前仅有放置核心方块（白色方块）功能
    private void InitShip()
    {
        Tile whiteTile = Resources.Load<Tile>("Prefabs/Tiles/White Tile");
        SetTile(Vector3Int.zero, whiteTile);
    }

    public void SetTile(Vector3Int cellPos, Tile tile)
    {
        Vector3 localPos = _grid.CellToLocal(cellPos);

        localPos = localPos + new Vector3(1f, 1f, 0);

        Tile newTile = Instantiate<Tile>(tile, transform.GetChild(0));

        newTile._coordinate = cellPos;

        newTile.transform.localPosition = localPos;

        _tileGrid[cellPos] = newTile;

        Vector2Int v2pos = new Vector2Int(0, 0);

        v2pos = (Vector2Int)cellPos;

        Utils.CreateworldText(newTile.transform, v2pos.ToString(), Vector3.zero, 20, Color.black, TextAnchor.MiddleCenter, TextAlignment.Center, sortingOrder: 2);
    }

    public void DeleteTile(Vector3Int cellPos)
    {
        if (_tileGrid.ContainsKey(cellPos))
        {
            Debug.Log("找到方块，执行摧毁");
            Destroy(_tileGrid[cellPos].gameObject);
            _tileGrid.Remove(cellPos);
        }
    }

    private bool CanSetTile(Vector3Int cellPos)
    {
        // 1. 定义四个方向的偏移量（根据你的网格坐标系调整，这里是顶视角3D/2D通用：前后左右）
        // 如果是2D正交（上下左右），可改为 new Vector3Int(0, 1, 0)、(0, -1, 0)、(1, 0, 0)、(-1, 0, 0)
        Vector3Int[] directions = new Vector3Int[]
        {
        Vector3Int.up,  // 前（y+1）
        Vector3Int.down,     // 后（y-1）
        Vector3Int.right,    // 右（x+1）
        Vector3Int.left,      // 左（x-1）
        };

        // 2. 遍历所有方向，检查相邻单元格是否存在于网格中
        foreach (var dir in directions)
        {
            Vector3Int neighborPos = cellPos + dir; // 计算相邻单元格位置
            //Debug.Log(neighborPos);
            if (_tileGrid.ContainsKey(neighborPos) && !_tileGrid.ContainsKey(cellPos))
            {
                // 只要有一个方向存在方块，就返回true（四周有方块）
                return true;
            }
        }
        // 3. 所有方向都没有方块，返回false
        return false;
    }

    private void HandleMovement()
    {
        // 推进：使用 ForceMode2D.Force
        // 注意：物理方法内部会自动处理时间步长，不需要手动乘 Time.deltaTime
        if (Mathf.Abs(_thrustInput) > 0.01f)
        {
            rb.AddRelativeForce(Vector2.up * _thrustInput * _thrustForce);
        }
        //if (Mathf.Abs(_turnInput) > 0.01f)
        //{
        //    rb.AddForce(Vector2.right * _turnInput * _thrustForce);
        //}

        // 转向：负号是因为通常 A/左 为正方向，但 Unity 顺时针旋转需要负力矩
        //if (Mathf.Abs(_turnInput) > 0.01f)
        //{
        //    rb.AddTorque(_turnInput * -_turnTorque);
        //}
        RotateTowardsMouse();
    }

    private void RotateTowardsMouse()
    {
        // 计算叉积：判断鼠标在飞船“机头”的哪一边
        // transform.up 是飞船当前的正前方
        float angleDiff = Vector3.Cross(transform.up, directionToMouse).z;

        // 应用扭矩：根据角度差施加旋转力
        rb.AddTorque(angleDiff * _turnTorque);

        // --- 优化：角速度补偿 (防止晃动) ---
        // 如果你觉得飞船转过头了停不下来，可以加这一段：
        // 当飞船接近目标方向时，自动衰减角速度，减少摆动
        float dot = Vector2.Dot(transform.up, directionToMouse);
        if (dot > 0.98f)
        {
            rb.angularVelocity *= 0.8f;
        }
    }

    //轮询调度所有可以攻击的Tile
    private void HandleAttack()
    {
        foreach (var tile in _tileGrid.Values)
        {
            if (tile._tileType is TileType.Attack)
            {
                var attackTile = tile as RedTile;
                attackTile.Shoot(directionToMouse);
            }
        }
    }
}