using System.Collections.Generic;
using UnityEngine;

public enum State { Move, Fight }

public class BaseEnemy : Ship
{
    private EnemyManager _enemyManager;
    private Ship _target;

    [SerializeField] private State _currentState = State.Move;

    [Header("AI设置")]
    [SerializeField] private float _attackRange = 10f; // 进入此范围开始射击
    [SerializeField] private float _stopRange = 5f;   // 距离太近则停止推进
    [SerializeField] private float _fireRate = 1f;
    private float _fireTimer;
    protected override void Awake()
    {
        base.Awake();
        rb.gravityScale = 0f;
        rb.drag = 1f;           // 线性阻力，防止无限滑行
        rb.angularDrag = 2f;    // 旋转阻力，防止疯狂自转
    }

    private void Start()
    {
        _enemyManager = EnemyManager.Instance;
        if (_enemyManager != null)
        {
            _enemyManager._enemiesList.Add(this);
            _target = _enemyManager.target;
        }

        Debug.Log($"敌人最大弹药量{_ammoCapacity},当前弹药量{_ammoAmount}");
    }

    private void FixedUpdate()
    {
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
        if (_currentState == State.Fight && _target !=null)
        {
            HandleAttack();
        }

        CheckAmmoRestore();
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
        _fireTimer += Time.deltaTime;
        if (_fireTimer >= _fireRate && _ammoAmount >0)
        {
            // 简单判断下是否对准了玩家，没对准不瞎射
            float dotProduct = Vector2.Dot(transform.up, (_target.transform.position - transform.position).normalized);
            if (dotProduct > 0.95f)
            {
                Instantiate(_bullet, _firePoint.position, _firePoint.rotation);
                _fireTimer = 0;
                _ammoAmount -= 1;
                Debug.Log($"敌人最大弹药量{_ammoCapacity},当前弹药量{_ammoAmount}");
            }
        }
    }

}