using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _firePoint;
    [SerializeField] private Transform _weaponPivot;
    [SerializeField] private WeaponData _currentWeapon;

    [Header("Rotation")]
    [SerializeField] private float _rotateSpeed = 720f;

    private float _nextFireTime;
    private Transform _currentTarget;
    private int _bulletBonus;

    public WeaponData CurrentWeapon => _currentWeapon;
    public int BulletBonus => _bulletBonus;
    private int _enemyMask;
    private readonly Collider[] _hitBuffer = new Collider[32];

    void Awake()
    {
        _enemyMask = LayerMask.GetMask("Enemy");
    }

    private void Update()
    {
        _currentTarget = FindNearestEnemy();

        if (_currentTarget != null)
        {
            RotateToward(_currentTarget);
            TryShoot();
        }
    }

    public void EquipWeapon(WeaponData newWeapon)
    {
        _currentWeapon = newWeapon;
        _nextFireTime = 0f;
    }

    public void AddBulletBonus(int bonus)
    {
        _bulletBonus += bonus;
    }

    private void TryShoot()
    {
        if (_currentWeapon == null) return;
        if (Time.time < _nextFireTime) return;

        float asMultiplier = 1 + AugmentManager.AttackSpeedPercent / 100f;
        _nextFireTime = Time.time + _currentWeapon.fireRate / asMultiplier;

        ShootBullets();
    }

    private void ShootBullets()
    {
        int count = _currentWeapon.bulletCount + _bulletBonus;
        float spread = _currentWeapon.spreadAngle;

        Vector3 pos = _firePoint.position;
        Quaternion baseRot = _weaponPivot.rotation;
        ObjectPool pool = ObjectPool.Instance;
        string tag = _currentWeapon.bulletTag;

        float startAngle = -spread * (count - 1) * 0.5f;
        Quaternion step = Quaternion.Euler(0f, spread, 0f);
        Quaternion rot = baseRot * Quaternion.Euler(0f, startAngle, 0f);

        for (int i = 0; i < count; i++)
        {
            pool.SpawnFromPool(tag, pos, rot);
            rot *= step;
        }
    }

    private Transform FindNearestEnemy()
    {
        float range = _currentWeapon != null ? _currentWeapon.detectRange : 10f;
        int count = Physics.OverlapSphereNonAlloc(transform.position, range, _hitBuffer, _enemyMask);

        Transform nearest = null;
        float minDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            float dist = Vector3.Distance(transform.position, _hitBuffer[i].transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = _hitBuffer[i].transform;
            }
        }
        return nearest;
    }

    private void RotateToward(Transform target)
    {
        Vector3 dir = (target.position - _weaponPivot.position).normalized;
        dir.y = 0f;
        if (dir == Vector3.zero) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        // Xoay WeaponPivot thay vì transform (Player)
        _weaponPivot.rotation = Quaternion.RotateTowards(
            _weaponPivot.rotation,
            targetRot,
            _rotateSpeed * Time.deltaTime
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (_currentWeapon == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _currentWeapon.detectRange);
    }
}