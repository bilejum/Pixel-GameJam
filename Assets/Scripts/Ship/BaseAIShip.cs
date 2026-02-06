using System.Collections.Generic;
using UnityEngine;

public enum State { Move, Fight }

public enum Fraction
{
    Good,
    Bad
}
public class BaseAIShip : Ship
{
    protected Ship _target;

    [SerializeField] protected State _currentState = State.Move;

    [SerializeField] private Fraction _fraction;

    [Header("AI设置")]
    [SerializeField] protected float _attackRange = 10f; // 进入此范围开始射击
    [SerializeField] protected float _stopRange = 5f;   // 距离太近则停止推进
    [SerializeField] protected float _fireRate = 1f;
    private float _fireTimer;


    [Header("索敌设置")]
    public float _searchRadius = 10f; // 索敌半径（队友能感知的最大范围）

    public float _targetUpdateInterval = 0.5f; // 目标更新间隔（秒），避免每帧检测耗性能
    [SerializeField] protected float _targetUpdateTimer;

    protected Collider2D[] detectedShip = new Collider2D[10]; // 检测到的敌人（数组比List更高效）


    protected override void Awake()
    {
        base.Awake();
        rb.gravityScale = 0f;
        rb.drag = 1f;           // 线性阻力，防止无限滑行
        rb.angularDrag = 2f;    // 旋转阻力，防止疯狂自转
    }

    protected virtual void Start()
    {

    }

    private void FixedUpdate()
    {
        _targetUpdateTimer += Time.deltaTime;
        if (_targetUpdateTimer >= _targetUpdateInterval)
        {
            //Debug.Log($"已检测，敌人为 {_target}");
            UpdateTargetEnemy();
            _targetUpdateTimer = 0f;
        }


        if (_target == null) return;

        float distance = Vector2.Distance(transform.position, _target.transform.position);

        // 状态切换逻辑
        if (distance < _attackRange) _currentState = State.Fight;
        else _currentState = State.Move;

        HandleRotation();
        HandleMovement(distance);


    }

    private void Update()
    {
        if (_currentState == State.Fight && _target != null)
        {
            HandleAttack();
        }
    }

    // 处理转向：让敌人始终面朝玩家
    private void HandleRotation()
    {
        Vector2 directionToTarget = (_target.transform.position - transform.position).normalized;

        // 计算当前正前方 (up) 与 目标方向 的夹角
        // 使用 Vector3.Cross (叉积) 判断目标在左侧还是右侧
        float crossProduct = Vector3.Cross(transform.up, directionToTarget).z;

        // 如果叉积 > 0，目标在左侧，应用正力矩转向
        // 如果叉积 < 0，目标在右侧，应用负力矩转向
        rb.AddTorque(crossProduct * _turnTorque);
    }

    // 处理移动：像玩家一样推力加速
    private void HandleMovement(float distance)
    {
        // 只有当敌人基本对准了玩家，且距离不够近时，才开启推进器
        float dotProduct = Vector2.Dot(transform.up, (_target.transform.position - transform.position).normalized);

        if (dotProduct > 0.8f && distance > _stopRange)
        {
            rb.AddRelativeForce(Vector2.up * _thrustForce);
        }
    }

    private void HandleAttack()
    {
        //_fireTimer += Time.deltaTime;
        //if (_fireTimer >= _fireRate && _ammoAmount > 0)
        //{
        //    // 简单判断下是否对准了玩家，没对准不瞎射
        //    float dotProduct = Vector2.Dot(transform.up, (_target.transform.position - transform.position).normalized);
        //    if (dotProduct > 0.95f)
        //    {
        //        Instantiate(_bullet, _firePoint.position, _firePoint.rotation);
        //        _fireTimer = 0;
        //        _ammoAmount -= 1;
        //        Debug.Log($"敌人最大弹药量{_ammoCapacity},当前弹药量{_ammoAmount}");
        //}
        //}
    }

    private void UpdateTargetEnemy()
    {
        // 清空当前目标（先重置）
        _target = null;


        // 1. 球形检测：在索敌范围内找怪物（高效的物理检测）
        var detectedEnemyCount = Physics2D.OverlapCircleNonAlloc(
            transform.position,
            _searchRadius,
            detectedShip,
            LayerMask.GetMask("AIDetection")
        );

        Debug.Log(detectedShip.ToString());

        if (detectedEnemyCount == 0) return;
        foreach (var enemy in detectedShip)
        {
            if (enemy.GetComponent<BaseAIShip>()._fraction != _fraction)
            {
                _target = enemy.GetComponent<Ship>();
                return;
            }
        }

    }
}