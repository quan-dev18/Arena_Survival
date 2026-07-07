using UnityEngine;

public class Bullet : MonoBehaviour
{
    public static float EnemySpeedMultiplier = 1f;

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
    private int _wallLayer;

    // Cache ObjectPool de tranh tra Instance moi frame
    private void Awake()
    {
        _pool = ObjectPool.Instance;
        _playerLayer = LayerMask.NameToLayer("Player");
        _enemyLayer = LayerMask.NameToLayer("Enemy");
        _wallLayer = LayerMask.NameToLayer("Wall");
    }

    private void OnEnable()
    {
        _timer = 0f;
    }

    private void Update()
    {
        float speed = _isEnemyBullet ? _speed * EnemySpeedMultiplier : _speed;
        float moveDistance = speed * Time.deltaTime;

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
            int mask = 1 << _playerLayer | 1 << _wallLayer;
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, rayDistance, mask))
            {
                if (hit.collider.gameObject.layer == _playerLayer && hit.collider.GetComponentInParent<PlayerHealth>() is {} playerHealth)
                    playerHealth.TakeDamage(CalcDamage());
                _pool.ReturnToPool(_poolTag, gameObject);
                return true;
            }
        }
        else
        {
            int mask = 1 << _enemyLayer | 1 << _wallLayer;
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, rayDistance, mask))
            {
                if (hit.collider.gameObject.layer != _wallLayer)
                {
                    hit.collider.GetComponent<KnockbackReceiver>()?.ApplyKnockback(transform.forward, 10f);
                    hit.collider.GetComponent<EnemyBase>()?.TakeDamage(CalcDamage());
                }
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
