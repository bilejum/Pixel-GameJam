using Cinemachine;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerShip : Ship
{
    //玩家死亡信号
    public static Action OnPlayerDeath;


    private Vector3Int _cellPos;

    public Tile _selectedTile;

    private ItemData _selectedItemData;

    [Header("手感优化")]
    [SerializeField]
    private float _linearDrag = 1f; // 线性阻力（空气阻力感）

    [SerializeField]
    private float _angularDrag = 5f; // 旋转阻力（防止无限自转）

    [SerializeField]
    private float _dashCooldown = 1f; // 冲刺冷却时间

    [Range(0, 1)]
    [SerializeField]
    private float _brakingStrength = 0.95f; // 不按键时的减速力度

    private float _dashTimer;

    //控制输入值(0,1)
    private float _thrustInput;
    private float _turnInput;

    public bool deleteMode = false;

    //是否可以移动，主要用于UI切换
    public bool canMove = true;

    private Vector2 directionToMouse;

    public GameObject _ghostTile;

    //从这开始
    protected override void Awake()
    {
        base.Awake();
        rb.drag = _linearDrag; // 设置阻力
        rb.angularDrag = _angularDrag; // 设置旋转阻力
    }

    private void Start()
    {
        InitShip();
    }

    protected override void Update()
    {
        base.Update();

        _thrustInput = Input.GetAxisRaw("Vertical");
        _turnInput = Input.GetAxisRaw("Horizontal");

        // 冲刺冷却计时
        if (_dashTimer > 0)
            _dashTimer -= Time.deltaTime;

        // 检测冲刺输入 (空格键)
        if (Input.GetKeyDown(KeyCode.Space) && _dashTimer <= 0 && canMove)
        {
            PerformDash();
        }

        if (GameManager.Instance.State is GameState.Build)
        {
            MoveGhostTile();
        }

        //这一行必须有
        _cellPos = _grid.WorldToCell(Utils.GetMouseWorldPos());
        if (Input.GetMouseButtonDown(0) && GameManager.Instance.State is GameState.Build)
        {
            if (Utils.IsPointerOverUI())
                return;
            if (!deleteMode)
            {
                //如果可以放置，则播放正确音效
                if (CanSetTile(_cellPos))
                {
                    SetTile(_cellPos, _selectedTile);
                    AudioManager.Instance.PlaySFX("Correct");
                }
                //反之播放错误音效
                else
                {
                    AudioManager.Instance.PlaySFX("Error");
                }
            }
            else if (deleteMode)
            {
                DeleteTile(_cellPos);
            }
        }

        if (Input.GetMouseButton(0) && GameManager.Instance.State is GameState.Game)
        {
            HandleAttack(directionToMouse);
        }

        //修改速度表
        //UIManager.Instance.AdjustGaugePointer(_thrustForce * _thrustInput);

        ////修改能量条
        //UIManager.Instance.AdjustEnergy(_energy, _maxEnergy);

        directionToMouse = Utils.GetDirectionToMouse(transform);
    }

    private void FixedUpdate()
    {
        // 2. 在 FixedUpdate 中应用物理力
        if (canMove)
        {
            HandleMovement();
            ApplyBraking(); // 模拟 Reassembly 的自动制动
        }
    }

    //初始化飞船，目前仅有放置核心方块（白色方块）功能
    private void InitShip()
    {
        Tile whiteTile = Resources.Load<Tile>("Prefabs/Tiles/White Tile");
        SetTile(Vector3Int.zero, whiteTile);
    }

    public void SelectedTile(ItemData itemData)
    {
        _selectedTile = itemData.tilePrefab;
        _selectedItemData = itemData;
        _ghostTile.SetActive(true);
        _ghostTile.GetComponent<SpriteRenderer>().color = _selectedTile._color;
    }

    private void MoveGhostTile()
    {
        if (_ghostTile == null) return;
        var spriteRender = _ghostTile.GetComponent<SpriteRenderer>();

        var lerp = Vector3.Lerp(
            _ghostTile.transform.localPosition,
            _grid.CellToLocal(_cellPos),
            Time.unscaledDeltaTime * 20
        );
        _ghostTile.transform.localPosition = lerp;

        if (_selectedItemData == null) return;
        if (_selectedItemData.count <= 0)
        {
            _ghostTile.SetActive(false);
        }

    }

    public void SetTile(Vector3Int cellPos, Tile tile)
    {
        if (tile == null)
        {
            Debug.Log("SetTile 失败: 传入的 tile prefab 为空！");
            return;
        }
        Vector3 localPos = _grid.CellToLocal(cellPos);

        localPos = localPos + new Vector3(1f, 1f, 0);

        Tile newTile = Instantiate<Tile>(tile, transform.GetChild(0));

        newTile._coordinate = cellPos;

        newTile.transform.localPosition = localPos;

        _tileGrid[cellPos] = newTile;

        Vector2Int v2pos = new Vector2Int(0, 0);

        v2pos = (Vector2Int)cellPos;

        InventoryManager.Instance.DeleteItem(_selectedTile, 1);
        //Utils.CreateworldText(newTile.transform, v2pos.ToString(), Vector3.zero, 20, Color.black, TextAnchor.MiddleCenter, TextAlignment.Center, sortingOrder: 2);
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
        //Debug.Log(_selectedItemData.count);
        if (_selectedItemData.count <= 0)
        {
            return false;
        }

        // 1. 定义四个方向的偏移量（根据你的网格坐标系调整，这里是顶视角3D/2D通用：前后左右）
        // 如果是2D正交（上下左右），可改为 new Vector3Int(0, 1, 0)、(0, -1, 0)、(1, 0, 0)、(-1, 0, 0)
        Vector3Int[] directions = new Vector3Int[]
        {
            Vector3Int.up, // 前（y+1）
            Vector3Int.down, // 后（y-1）
            Vector3Int.right, // 右（x+1）
            Vector3Int.left, // 左（x-1）
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

    //private void HandleMovement()
    //{
    //    // 推进：使用 ForceMode2D.Force
    //    // 注意：物理方法内部会自动处理时间步长，不需要手动乘 Time.deltaTime
    //    if (Mathf.Abs(_thrustInput) > 0.01f)
    //    {
    //        rb.AddRelativeForce(Vector2.up * _thrustInput * _thrustForce);
    //    }

    //    // 2. 横向移动 (A/D) - 使用 AddRelativeForce 确保是相对于飞船自身的左右
    //    if (Mathf.Abs(_turnInput) > 0.01f)
    //    {
    //        rb.AddRelativeForce(Vector2.right * _turnInput * _strafeForce);
    //    }

    //    RotateTowardsMouse();
    //}

    private void HandleMovement()
    {
        // 1. 纵向与横向移动
        Vector2 moveVector = new Vector2(_turnInput * _strafeForce, _thrustInput * _thrustForce);
        rb.AddRelativeForce(moveVector);

        // 2. 核心：Reassembly 式转向
        RotateTowardsMouseEnhanced();
    }

    //private void RotateTowardsMouse()
    //{
    //    // 计算叉积：判断鼠标在飞船“机头”的哪一边
    //    // transform.up 是飞船当前的正前方
    //    float angleDiff = Vector3.Cross(transform.up, directionToMouse).z;

    //    // 应用扭矩：根据角度差施加旋转力
    //    rb.AddTorque(angleDiff * _turnTorque);

    //    // --- 优化：角速度补偿 (防止晃动) ---
    //    // 如果你觉得飞船转过头了停不下来，可以加这一段：
    //    // 当飞船接近目标方向时，自动衰减角速度，减少摆动
    //    float dot = Vector2.Dot(transform.up, directionToMouse);
    //    if (dot > 0.98f)
    //    {
    //        rb.angularVelocity *= 0.8f;
    //    }
    //}

    private void RotateTowardsMouseEnhanced()
    {
        // 计算当前朝向与目标朝向的角度差
        float targetAngle =
            Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg - 90f;
        float currentAngle = rb.rotation;
        float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

        // PID 简版：扭矩 = 角度差 * 强度 - 当前角速度 * 阻尼
        // 这会让飞船在快要对准鼠标时自动反向用力，瞬间锁死方向
        float torque = angleDiff * _turnTorque * 0.1f;

        // 这里的 0.1f 是为了平衡，如果转得太慢就调大，如果乱抖就调小
        rb.AddTorque(torque - rb.angularVelocity * 0.5f, ForceMode2D.Force);
    }

    private void ApplyBraking()
    {
        // 如果没有输入，模拟推进器反向喷射来减速
        if (Mathf.Abs(_thrustInput) < 0.01f && Mathf.Abs(_turnInput) < 0.01f)
        {
            // 快速将速度降低到接近0，产生“控制感”
            rb.velocity *= _brakingStrength;
        }
    }

    private void PerformDash()
    {
        Vector2 inputDir = new Vector2(_turnInput, _thrustInput).normalized;
        Vector2 dashDir = inputDir.sqrMagnitude < 0.01f ? Vector2.up : inputDir;

        // 清除当前速度的一分部，让冲刺更有爆发感
        rb.velocity *= 0.5f;
        rb.AddRelativeForce(dashDir * _dashForce, ForceMode2D.Impulse);

        _dashTimer = _dashCooldown;
        AudioManager.Instance.PlaySFX("Dash");
    }

    public override void CoreDestory()
    {
        Debug.Log("PlayerDeath");
        base.CoreDestory();
    }

    private void OnDestroy()
    {

        OnPlayerDeath?.Invoke();
    }
}
