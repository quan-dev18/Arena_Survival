using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ExpOrb : MonoBehaviour
{
    public float xpValue = 10f;

    public Rigidbody Rb { get; private set; }

    private ObjectPool _pool;

    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        _pool = ObjectPool.Instance;
    }

    private void OnEnable()
    {
        Rb.isKinematic = false;
        Rb.linearVelocity = Vector3.zero;
        Rb.angularVelocity = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        Rb.isKinematic = true;
        PlayerExperience playerExp = collision.gameObject.GetComponentInChildren<PlayerExperience>();

        if (playerExp != null)
            playerExp.CollectOrb(this);
        else
            _pool.ReturnToPool("ExpOrb", gameObject);
    }
}
