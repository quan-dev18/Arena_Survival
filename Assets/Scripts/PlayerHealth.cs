using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private Animator _animator;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private Color _flashColor = Color.red;
    [SerializeField] private float _flashDuration = 0.15f;
    private float _currentHealth;
    private Rigidbody _rb;
    private MaterialPropertyBlock _mpb;
    private Color _originalColor;
    private float _flashTimer;
    private bool _isFlashing;
    private bool _isDead;

    public float MaxHealth => _maxHealth;
    public float CurrentHealth => _currentHealth;
    public System.Action<float, float> OnHealthChanged;

    public void SetMaxHealth(float maxHealth)
    {
        float ratio = _currentHealth / _maxHealth;
        _maxHealth = maxHealth;
        _currentHealth = _maxHealth * ratio;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    public void Heal(float amount)
    {
        _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    private void Awake()
    {
        _currentHealth = _maxHealth;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        _rb = GetComponent<Rigidbody>();
        _mpb = new MaterialPropertyBlock();
        _meshRenderer.GetPropertyBlock(_mpb);
        _originalColor = _meshRenderer.sharedMaterial.GetColor("_BaseColor");
        _rb.isKinematic = true;
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        _animator.SetTrigger("isHurted");
        DamagePopUp.Create(transform.position + Vector3.up * 1.5f, Mathf.RoundToInt(amount),Color.black);

        _currentHealth -= amount;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        _flashTimer = _flashDuration;
        _isFlashing = true;

        if (_currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        _isDead = true;
        _animator.SetBool("OnDie", true);
        _animator.SetBool("OnMove", false);
        _rb.isKinematic = false;
        _rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
        _mpb.SetColor("_BaseColor", Color.red);
        _meshRenderer.SetPropertyBlock(_mpb);
        GameManager.Instance?.OnPlayerDied();
    }

    private void Update()
    {
        if (_isDead) return;

        ApplyHealRegen();
        UpdateFlash();
    }

    private void ApplyHealRegen()
    {
        float regen = _maxHealth * AugmentManager.HealRegenPercent / 100f * Time.deltaTime;
        if (regen > 0f)
        {
            _currentHealth = Mathf.Min(_currentHealth + regen, _maxHealth);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
    }

    private void UpdateFlash()
    {
        if (!_isFlashing) return;

        _flashTimer -= Time.deltaTime;
        float t = Mathf.Clamp01(_flashTimer / _flashDuration);
        Color current = Color.Lerp(_flashColor, _originalColor, 1f - t);
        _mpb.SetColor("_BaseColor", current);
        _meshRenderer.SetPropertyBlock(_mpb);

        if (_flashTimer <= 0f)
        {
            _isFlashing = false;
            _mpb.SetColor("_BaseColor", _originalColor);
            _meshRenderer.SetPropertyBlock(_mpb);
        }
    }
}