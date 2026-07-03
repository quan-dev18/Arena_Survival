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

    private void Update()
    {
        _currentTarget = FindNearestEnemy();

        if (_currentTarget != null)
        {
            RotateToward(_currentTarget);
            TryShoot();
        }
    }

    // Đổi vũ khí runtime (gọi từ item pickup, UI, etc.)
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

        _nextFireTime = Time.time + _currentWeapon.fireRate * AugmentManager.FireRateMultiplier;

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
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            _currentWeapon != null ? _currentWeapon.detectRange : 10f,
            LayerMask.GetMask("Enemy")
        );

        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = hit.transform;
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