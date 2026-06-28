using UnityEngine;
using UnityEngine.VFX;

public class SlashComboAttack : MonoBehaviour
{
    [Header("Combo Settings")]
    [SerializeField] private float _hitInterval      = 0.35f;
    [SerializeField] private float _comboCooldown    = 1.2f;
    [SerializeField] private float _speedMultiplier  = 0.85f;
    [SerializeField] private float _minCooldown      = 0.35f;
    [SerializeField] private float _damageDelay      = 0.08f;

    [Header("VFX")]
    [SerializeField] private VisualEffect[] _slashVFX;

    private int _comboStep;
    private float _hitIntervalTimer;
    private float _cooldownTimer;
    private bool _inCombo;
    private float _currentCooldown;

    private int ComboLength => _slashVFX is { Length: > 0 } ? _slashVFX.Length : 2;

    public void TryAttack(Transform target, float damage, KnockbackReceiver knockback = null, float knockbackForce = 20f)
    {
        if (target == null) return;

        _hitIntervalTimer -= Time.deltaTime;
        _cooldownTimer    -= Time.deltaTime;

        if (_cooldownTimer > 0f) return;

        if (!_inCombo)
        {
            _inCombo          = true;
            _comboStep        = 0;
            _hitIntervalTimer = 0f;
        }

        if (_hitIntervalTimer > 0f) return;

        LookAt(target);
        PlaySlashVFX(_comboStep);
        StartCoroutine(DelayedDamage(target, damage, knockback, knockbackForce, _comboStep));

        _comboStep++;

        if (_comboStep >= ComboLength)
        {
            _currentCooldown = Mathf.Max(_currentCooldown * _speedMultiplier, _minCooldown);
            _cooldownTimer   = _currentCooldown;
            _comboStep       = 0;
            _inCombo         = false;
        }
        else
        {
            _hitIntervalTimer = _hitInterval;
        }
    }

    public void ResetCombo()
    {
        _comboStep        = 0;
        _hitIntervalTimer = 0f;
        _cooldownTimer    = 0f;
        _inCombo          = false;
        _currentCooldown  = _comboCooldown;
        StopAllCoroutines();

        if (_slashVFX == null) return;
        foreach (var vfx in _slashVFX)
            if (vfx != null) vfx.Stop();
    }

    private void Awake()
    {
        _currentCooldown = _comboCooldown;
        if (_slashVFX == null) return;
        foreach (var vfx in _slashVFX)
            if (vfx != null) vfx.Stop();
    }

    private void LookAt(Transform target)
    {
        Vector3 dir = (target.position - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    private void PlaySlashVFX(int step)
    {
        if (_slashVFX == null || _slashVFX.Length == 0) return;
        VisualEffect vfx = _slashVFX[step % _slashVFX.Length];
        if (vfx == null) return;
        vfx.Reinit();
        vfx.Play();
    }

    private System.Collections.IEnumerator DelayedDamage(Transform target, float damage, KnockbackReceiver knockback, float knockbackForce, int step)
    {
        yield return new WaitForSeconds(_damageDelay);

        if (target == null) yield break;

        target.GetComponent<PlayerHealth>()?.TakeDamage(damage);

        Vector3 kbDir = step % 2 == 0 ? transform.right : (-transform.right + transform.forward).normalized;
        knockback?.ApplyKnockback(kbDir, knockbackForce);
    }
}
