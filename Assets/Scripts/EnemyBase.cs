using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100;
    public float moveSpeed = 5;
    public float damage = 10;

    protected float currentHealth;

    // Gọi khi spawn từ pool (thay thế Start cho logic reset)
    public virtual void OnSpawn()
    {
        currentHealth = maxHealth;
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
            Die();
    }

    // Subclass override để return pool thay vì Destroy
    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}