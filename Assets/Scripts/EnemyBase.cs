using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100;
    public float moveSpeed = 5;
    public float damage = 10;
    public float damageVariance = 0.3f;

    protected float currentHealth;
    protected bool _isDead;

    public virtual void OnSpawn()
    {
        currentHealth = maxHealth;
        _isDead = false;
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(float amount)
    {
        if (_isDead) return;

        currentHealth -= amount;
        DamagePopUp.Create(transform.position + Vector3.up * 1.5f, Mathf.RoundToInt(amount));
        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        _isDead = true;
        Destroy(gameObject);
    }
}