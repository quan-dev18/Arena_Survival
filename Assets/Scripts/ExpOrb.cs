using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ExpOrb : MonoBehaviour
{
    public float xpValue = 10f;
    public Rigidbody Rb { get; private set; }
    public bool IsCollected { get; private set; } 

    private ObjectPool _pool;

    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        _pool = ObjectPool.Instance;
    }

    private void OnEnable()
    {
        //reset pool
        IsCollected = false;
        Rb.isKinematic = false;
        Rb.linearVelocity = Vector3.zero;
        Rb.angularVelocity = Vector3.zero;
    }

    public bool TryMarkCollected()
    {
        if (IsCollected) return false;
        IsCollected = true;
        return true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        //block double
        if (!TryMarkCollected()) return;

        Rb.isKinematic = true;
        PlayerExperience playerExp = collision.gameObject.GetComponentInChildren<PlayerExperience>();

        if (playerExp != null)
            playerExp.CollectOrb(this);
        else
            _pool.ReturnToPool("ExpOrb", gameObject);
    }
}
