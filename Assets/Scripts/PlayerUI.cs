using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Health Bar")]
    [SerializeField] private Image _healthFill;
    [SerializeField] private PlayerHealth _playerHealth;

    [Header("Health Colors")]
    [SerializeField] private Color _highHealthColor = new Color(0.13f, 0.84f, 0.2f);
    [SerializeField] private Color _midHealthColor = new Color(0.95f, 0.85f, 0.1f);
    [SerializeField] private Color _lowHealthColor = new Color(0.84f, 0.13f, 0.13f);
    [SerializeField] [Range(0, 1)] private float _lowThreshold = 0.25f;
    [SerializeField] [Range(0, 1)] private float _midThreshold = 0.6f;
    [SerializeField] [Range(0, 1)] private float _pulseThreshold = 0.35f;

    [Header("Billboard")]
    [SerializeField] private bool _faceCamera = true;
    [SerializeField] private Transform _camera;

    private Material _healthMaterial;
    private static readonly int PulseEnabled = Shader.PropertyToID("_PulseEnabled");

    private void Awake()
    {
        if (_healthFill == null)
            _healthFill = GetComponentInChildren<Image>();

        if (_playerHealth == null)
            _playerHealth = GetComponentInParent<PlayerHealth>();

        if (_camera == null && Camera.main != null)
            _camera = Camera.main.transform;

        if (_healthFill != null)
            _healthMaterial = _healthFill.material;

        if (_playerHealth != null)
            UpdateHealthBar(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
    }

    private void OnEnable()
    {
        if (_playerHealth != null)
            _playerHealth.OnHealthChanged += UpdateHealthBar;
    }

    private void OnDisable()
    {
        if (_playerHealth != null)
            _playerHealth.OnHealthChanged -= UpdateHealthBar;
    }

    private void LateUpdate()
    {
        if (!_faceCamera || _camera == null) return;
        transform.rotation = _camera.rotation;
    }

    private void UpdateHealthBar(float current, float max)
    {
        if (_healthFill == null) return;

        float percent = Mathf.Clamp01(current / max);
        _healthFill.fillAmount = percent;
        _healthFill.color = GetGradientColor(percent);

        if (_healthMaterial != null)
            _healthMaterial.SetFloat(PulseEnabled, percent < _pulseThreshold ? 1f : 0f);
    }

    private Color GetGradientColor(float percent)
    {
        if (percent < _lowThreshold)
            return Color.Lerp(_lowHealthColor, _midHealthColor, percent / _lowThreshold);
        if (percent < _midThreshold)
            return Color.Lerp(_midHealthColor, _highHealthColor, (percent - _lowThreshold) / (_midThreshold - _lowThreshold));
        return _highHealthColor;
    }
}
