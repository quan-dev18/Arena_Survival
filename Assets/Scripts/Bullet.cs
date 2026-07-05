using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _damageVariance = 0.3f;
    [SerializeField] private float _lifetime = 3f;

    [Header("Target")]
    [SerializeField] private bool _isEnemyBullet;
    [SerializeField] private string _poolTag = "Bullet";

    private float _timer;
    private ObjectPool _pool;
    private int _playerLayer;
    private int _enemyLayer;

    // Cache ObjectPool de tranh tra Instance moi frame
    private void Awake()
    {
        _pool = ObjectPool.Instance;
        _playerLayer = LayerMask.NameToLayer("Player");
        _enemyLayer = LayerMask.NameToLayer("Enemy");
    }

    private void OnEnable()
    {
        _timer = 0f;
    }

    private void Update()
    {
        float moveDistance = _speed * Time.deltaTime;

        if (RaycastHitTarget(moveDistance))
            return;

        transform.Translate(Vector3.forward * moveDistance);

        _timer += Time.deltaTime;
        if (_timer >= _lifetime)
            _pool.ReturnToPool(_poolTag, gameObject);
    }

    private bool RaycastHitTarget(float moveDistance)
    {
        float rayDistance = moveDistance + 0.2f;

        if (_isEnemyBullet)
        {
            // Enemy bullet: raycast rong, chi damage khi tag Player + co PlayerHealth
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, rayDistance, 1 << _playerLayer | 1 << _enemyLayer))
            {
                if (hit.collider.CompareTag("Player") && hit.collider.TryGetComponent<PlayerHealth>(out var playerHealth))
                {
                    playerHealth.TakeDamage(CalcDamage());
                    _pool.ReturnToPool(_poolTag, gameObject);
                    return true;
                }
            }
        }
        else
        {
            // Player bullet: chi raycast Enemy
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, rayDistance, 1 << _enemyLayer))
            {
                hit.collider.GetComponent<KnockbackReceiver>()?.ApplyKnockback(transform.forward, 10f);
                hit.collider.GetComponent<EnemyBase>()?.TakeDamage(CalcDamage());
                _pool.ReturnToPool(_poolTag, gameObject);
                return true;
            }
        }

        return false;
    }

    private float CalcDamage()
    {
        return _damage * (1 + AugmentManager.DamagePercent / 100f) * Random.Range(1f, 1f + _damageVariance);
    }
}
