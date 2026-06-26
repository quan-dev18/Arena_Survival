using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class KnockbackReceiver : MonoBehaviour
{
    [SerializeField] private float _knockbackDuration = 0.15f;

    private NavMeshAgent _agent;
    private Rigidbody _rb;
    private Coroutine _knockbackCoroutine;  // Lưu coroutine đang chạy

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody>();
    }

    public void ApplyKnockback(Vector3 hitDirection, float force)
    {
        // Nếu đang knockback → dừng cái cũ, chạy cái mới
        if (_knockbackCoroutine != null)
            StopCoroutine(_knockbackCoroutine);

        _knockbackCoroutine = StartCoroutine(KnockbackCoroutine(hitDirection, force));
    }

    private IEnumerator KnockbackCoroutine(Vector3 direction, float force)
    {
        if (_agent != null)
        {
            _agent.isStopped = true;
            _agent.updatePosition = false;
        }

        float elapsed = 0f;
        while (elapsed < _knockbackDuration)
        {
            float currentForce = Mathf.Lerp(force, 0f, elapsed / _knockbackDuration);
            Vector3 movement = direction * currentForce * Time.deltaTime;

            if (_rb != null)
                _rb.MovePosition(_rb.position + movement);
            else
                transform.position += movement;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_agent != null)
        {
            _agent.Warp(transform.position);
            _agent.updatePosition = true;
            _agent.isStopped = false;
        }

        _knockbackCoroutine = null;
    }
}