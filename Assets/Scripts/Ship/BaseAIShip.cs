using System.Collections.Generic;
using UnityEngine;

public enum AIState
{
    Move,
    Fight
}

public enum Fraction
{
    Good,
    Bad
}
public class BaseAIShip : Ship
{
    protected Ship _target;

    [SerializeField] protected AIState _currentState = AIState.Move;

    [SerializeField] private Fraction _fraction;

    [Header("AI设置")]
    [SerializeField] protected float _attackRange = 70f; // 进入此范围开始射击
    [SerializeField] protected float _stopRange = 30f;   // 距离太近则停止推进
    [SerializeField] protected float _spreadAngle = 10f;

    protected Vector2 directionToTarget;

    public bool _canAction = true;

    [Header("避让设置")]
    [SerializeField] protected float _avoidanceRadius = 15f; // 检测队友的范围
    [SerializeField] protected float _avoidanceForce = 200f;  // 避让推力的强度
    private Collider2D[] _neighborResults = new Collider2D[10]; // 缓存数组，减少GC


    private Vector2 avoidance;

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
        if (_target == null) return;

        float distance = Vector2.Distance(_target.transform.position, this.transform.position);

        // 状态切换逻辑
        if (distance < _attackRange) _currentState = AIState.Fight;
        else _currentState = AIState.Move;

        HandleRotation();
        HandleMovement(distance);
    }

    protected override void Update()
    {
        if (_canAction)
        {
            base.Update();

            HandleAIAttack();
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
        if (Time.frameCount % 30 == 0)
        {
            avoidance = GetAvoidanceSteering();
        }
        // 只有当敌人基本对准了玩家，且距离不够近时，才开启推进器
        float dotProduct = Vector2.Dot(transform.up, (_target.transform.position - transform.position).normalized);

        if (dotProduct > 0.8f && distance > _stopRange)
        {
            rb.AddRelativeForce(Vector2.up * _thrustForce);
        }

        // 持续应用避让力（不受朝向限制，这样侧滑也能避开队友）
        if (avoidance != Vector2.zero)
        {
            // 使用 AddForce 让避让更平滑
            rb.AddForce(avoidance * _avoidanceForce, ForceMode2D.Force);
        }
    }

    private void HandleAIAttack()
    {
        if (_currentState == AIState.Fight && _target != null)
        {
            Vector2 toTarget = _target.transform.position - transform.position;
            float distance = toTarget.magnitude;
            directionToTarget = toTarget.normalized;

            float dotProduct = Vector2.Dot(transform.up, directionToTarget);
            if (dotProduct > 0.95f)
            {
                // 距离越远，散布越大（可选）
                float currentSpread = _spreadAngle * (1 + (distance * 0.05f));

                float randomOffset = UnityEngine.Random.Range(-currentSpread * 0.5f, currentSpread * 0.5f);
                Vector2 impreciseDir = Quaternion.Euler(0, 0, randomOffset) * directionToTarget;

                HandleAttack(impreciseDir);
            }
        }
    }

    protected Vector2 GetAvoidanceSteering()
    {
        Vector2 avoidanceVector = Vector2.zero;

        // 1. 只检测特定层级（建议给AI船只设置专门的Layer，比如 "Ship"）
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, _avoidanceRadius, _neighborResults, LayerMask.GetMask("AIDetection"));

        for (int i = 0; i < count; i++)
        {
            Collider2D other = _neighborResults[i];
            if (other.gameObject == this.gameObject) continue; // 排除自己

            // 2. 计算避让力：距离越近，推力越大
            Vector2 diff = (Vector2)transform.position - (Vector2)other.transform.position;
            float distance = diff.magnitude;

            if (distance < _avoidanceRadius && distance > 0)
            {
                // 使用反平方律或线性衰减：1/d 让距离极近时产生极大推力
                avoidanceVector += diff.normalized / distance;
            }
        }

        return avoidanceVector;
    }

}