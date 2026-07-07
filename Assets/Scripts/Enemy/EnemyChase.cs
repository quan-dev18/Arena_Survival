using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyChase : EnemyBase
{
    [Header("Chase Settings")]
    [SerializeField] private float _attackRange    = 1.5f;

    [Header("Knockback")]
    [SerializeField] private float _knockbackForce = 20f;

    [Header("Death")]
    [SerializeField] private float _fadeDuration = 0.5f;

    [Header("Exp Orb Drop")]
    [SerializeField] private int _minOrbs = 1;
    [SerializeField] private int _maxOrbs = 10;
    [SerializeField] private float _expOrbValue = 10f;

    [SerializeField] protected string _poolTag = "Enemy";

    protected NavMeshAgent   _agent;
    protected Transform      _target;
    private SlashComboAttack _combo;

    [SerializeField] private Animator _animator;
    private MeshRenderer _meshRenderer;
    private MaterialPropertyBlock _mpb;
    private Coroutine _deathRoutine;
    private KnockbackReceiver _targetKnockback;
    private Color _originalColor;

    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    protected override void Awake()
    {
        base.Awake();
        _agent = GetComponent<NavMeshAgent>();
        _combo = GetComponent<SlashComboAttack>();
        _animator = GetComponent<Animator>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _mpb = new MaterialPropertyBlock();
        _meshRenderer.GetPropertyBlock(_mpb);
        _originalColor = _meshRenderer.sharedMaterial.GetColor(BaseColor);
    }

    protected override void Start()
    {
        base.Start();
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
            _agent.speed = moveSpeed;
            _agent.ResetPath();
        }

        if (_deathRoutine != null)
        {
            StopCoroutine(_deathRoutine);
            _deathRoutine = null;
        }

        ResetAlpha();
    }

    private void ResetAlpha()
    {
        if (_meshRenderer == null) return;
        Color color = _originalColor;
        color.a = 1f;
        _mpb.SetColor(BaseColor, color);
        _meshRenderer.SetPropertyBlock(_mpb);
    }

    private void FindTarget()
    {
        GameObject playerChild = GameObject.FindWithTag("Player");
        if (playerChild != null)
        {
            Transform root = playerChild.transform.root;
            _target = root;
            _targetKnockback = root.GetComponent<KnockbackReceiver>();
        }
        else
        {
            Debug.LogWarning("EnemyChase: None Player lives");
        }
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

    public virtual void ApplyAttackSpeedMultiplier(float multiplier)
    {
        _combo?.ApplySpeedMultiplier(multiplier);
    }

    protected virtual void Attack()
    {
        _agent.isStopped = true;
        if (_combo == null) return;

        float finalDamage = damage * Random.Range(1f, 1f + damageVariance);
        _combo.TryAttack(_target, finalDamage, _targetKnockback, _knockbackForce);
    }

    public override void TakeDamage(float amount)
    {
        _animator.SetTrigger("isHurted");
        base.TakeDamage(amount);
    }

    protected override void Die()
    {
        _animator.SetBool("OnDie", true);
        _agent.isStopped = true;
        EnemySpawner.Instance?.OnEnemyDied();
        _deathRoutine = StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return null;

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        float safetyTimer = 0f;
        const float maxWait = 5f;

        while (stateInfo.normalizedTime < 1f && safetyTimer < maxWait)
        {
            stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            safetyTimer += Time.deltaTime;
            yield return null;
        }

        float elapsed = 0f;
        _meshRenderer.GetPropertyBlock(_mpb);
        Color fadeColor = _originalColor;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _fadeDuration;
            Color color = fadeColor;
            color.a = Mathf.Lerp(1f, 0f, t);
            _mpb.SetColor(BaseColor, color);
            _meshRenderer.SetPropertyBlock(_mpb);
            yield return null;
        }

        _deathRoutine = null;
        SpawnExpOrbs();
        ObjectPool.Instance.ReturnToPool(_poolTag, gameObject);
    }

    private void SpawnExpOrbs()
    {
        int count = Random.Range(_minOrbs, _maxOrbs + 1);
        ObjectPool pool = ObjectPool.Instance;

        for (int i = 0; i < count; i++)
        {
            Vector3 offset = Random.insideUnitSphere * 1f;
            offset.y = 0f;
            GameObject orb = pool.SpawnFromPool("ExpOrb", transform.position + offset, Quaternion.identity);
            if (orb != null)
            {
                orb.GetComponent<ExpOrb>().xpValue = _expOrbValue;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}