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

    private void TryShoot()
    {
        if (_currentWeapon == null) return;
        if (Time.time < _nextFireTime) return;

        _nextFireTime = Time.time + _currentWeapon.fireRate;

        ShootBullets();
    }

    private void ShootBullets()
    {
        int count = _currentWeapon.bulletCount;
        float spread = _currentWeapon.spreadAngle;

        // Tính góc bắt đầu để các viên đạn đối xứng
        float startAngle = -spread * (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + spread * i;
            Quaternion rot = _firePoint.rotation * Quaternion.Euler(0, angle, 0);

            ObjectPool.Instance.SpawnFromPool(
                _currentWeapon.bulletTag,
                _firePoint.position,
                rot
            );
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