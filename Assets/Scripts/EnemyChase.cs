using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyChase : EnemyBase
{
    [Header("Chase Settings")]
    [SerializeField] private float _attackRange    = 1.5f;
    [SerializeField] private float _attackCooldown = 1f;

    private NavMeshAgent _agent;
    private Transform _target;
    private float _attackTimer;

    protected override void Start()
    {
        base.Start();
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = moveSpeed;
        FindTarget();
        _attackTimer = _attackCooldown; // Fix: không attack ngay khi spawn
    }

    // Gọi lại khi re-spawn từ pool
    public override void OnSpawn()
    {
        base.OnSpawn();
        _attackTimer = _attackCooldown;
        FindTarget();

        if (_agent != null)
        {
            _agent.isStopped = false;
            _agent.ResetPath();
        }
    }

    private void FindTarget()
    {
        // Tự tìm Player qua tag — không cần drag tay
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            _target = player.transform;
        else
            Debug.LogWarning("EnemyChase: Không tìm thấy Player! Kiểm tra tag.");
    }

    private void Update()
    {
        if (_target == null) return;

        _attackTimer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, _target.position);

        if (dist <= _attackRange)
            Attack();
        else
            Chase();
    }

    private void Chase()
    {
        _agent.isStopped = false;
        _agent.speed = moveSpeed;
        _agent.SetDestination(_target.position);
    }

    private void Attack()
    {
        _agent.isStopped = true;

        // Luôn nhìn về phía player khi attack
        Vector3 dir = (_target.position - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);

        if (_attackTimer <= 0f)
        {
            _target.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            _attackTimer = _attackCooldown;
            Debug.Log($"{name} attacked player for {damage} damage!");
        }
    }

    protected override void Die()
    {
        _agent.isStopped = true;
        EnemySpawner.Instance?.OnEnemyDied();
        ObjectPool.Instance.ReturnToPool("Enemy", gameObject);
    }
    

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}