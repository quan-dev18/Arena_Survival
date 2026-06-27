using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(SlashComboAttack))]          // tự add nếu quên
public class EnemyChase : EnemyBase
{
    [Header("Chase Settings")]
    [SerializeField] private float _attackRange    = 1.5f;

    [Header("Knockback")]
    [SerializeField] private float _knockbackForce = 20f;

    private NavMeshAgent     _agent;
    private Transform        _target;
    private SlashComboAttack _combo;

    protected override void Start()
    {
        base.Start();
        _agent = GetComponent<NavMeshAgent>();
        _combo = GetComponent<SlashComboAttack>();
        _agent.speed = moveSpeed;
        FindTarget();
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        FindTarget();
        _combo?.ResetCombo();

        if (_agent != null)
        {
            _agent.isStopped = false;
            _agent.ResetPath();
        }
    }

    private void FindTarget()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            _target = player.transform;
        else
            Debug.LogWarning("EnemyChase: Không tìm thấy Player! Kiểm tra tag.");
    }

    private void Update()
    {
        if (_target == null) return;

        float dist = Vector3.Distance(transform.position, _target.position);

        if (dist <= _attackRange)
            Attack();
        else
            Chase();
    }

    private void Chase()
    {
        _agent.isStopped = false;
        _agent.speed     = moveSpeed;
        _agent.SetDestination(_target.position);
    }

    private void Attack()
    {
        _agent.isStopped = true;

        KnockbackReceiver kb = _target.GetComponent<KnockbackReceiver>();
        _combo.TryAttack(_target, damage, kb, _knockbackForce);
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