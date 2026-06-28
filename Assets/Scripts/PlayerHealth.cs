using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private Animator _animator;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private Color _flashColor = Color.red;
    [SerializeField] private float _flashDuration = 0.15f;
    private float _currentHealth;
    private MaterialPropertyBlock _mpb;
    private Color _originalColor;
    private float _flashTimer;
    private bool _isFlashing;


    private void Awake(){
        _currentHealth = _maxHealth;
        _mpb = new MaterialPropertyBlock();
        _meshRenderer.GetPropertyBlock(_mpb);
        _originalColor = _meshRenderer.sharedMaterial.GetColor("_BaseColor");
    }

    public void TakeDamage(float amount)
    {
        _animator.SetTrigger("isHurted");
       
        _currentHealth -= amount;
        _flashTimer = _flashDuration;
        _isFlashing = true;


        if (_currentHealth <= 0f)
            Debug.Log("Player died!");
    }

    private void Update()
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