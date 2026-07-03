using UnityEngine;

public class EnemyRanged : EnemyChase
{
    [Header("Ranged Settings")]
    [SerializeField] private Transform _firePoint;
    [SerializeField] private string _bulletTag = "Bullet2";
    [SerializeField] private float _fireRate = 1f;

    private float _fireTimer;

    public override void OnSpawn()
    {
        base.OnSpawn();
        _fireTimer = 0f;
    }

    protected override void Attack()
    {
        _agent.isStopped = true;

        transform.LookAt(_target);

        _fireTimer -= Time.deltaTime;
        if (_fireTimer > 0f) return;

        _fireTimer = _fireRate;

        Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position;
        GameObject bullet = ObjectPool.Instance.SpawnFromPool(_bulletTag, spawnPos, Quaternion.identity);
        if (bullet != null)
        {
            Vector3 dir = (_target.position - spawnPos).normalized;
            bullet.transform.forward = dir;
        }
    }
}
