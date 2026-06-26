using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _lifetime = 3f;

    private float _timer;

    private void OnEnable()
    {
        _timer = 0f;
    }

    private void Update()
    {
        float moveDistance = _speed * Time.deltaTime ;
        float rayDistance = moveDistance + 0.2f;
        // Raycast trước khi move — tránh xuyên qua enemy
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, rayDistance, LayerMask.GetMask("Enemy", "Wall")))
        {
            hit.collider.GetComponent<KnockbackReceiver>()?.ApplyKnockback(transform.forward,10f);
            hit.collider.GetComponent<EnemyBase>()?.TakeDamage(_damage);
            
            ReturnToPool();
            return;
        }

        transform.Translate(Vector3.forward * moveDistance);

        _timer += Time.deltaTime;
        if (_timer >= _lifetime)
            ReturnToPool();
    }

    private void ReturnToPool()
    {
        ObjectPool.Instance.ReturnToPool("Bullet", gameObject);
    }
}