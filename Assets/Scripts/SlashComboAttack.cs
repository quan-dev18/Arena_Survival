using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Combo slash attack: Horizontal (L→R) → Diagonal (R→L) → lặp lại.
/// Mỗi combo hoàn thành, tốc độ tăng dần cho đến khi đạt tốc độ tối đa.
/// Attach component này vào Enemy, rồi gọi TryAttack() từ EnemyChase.
/// </summary>
public class SlashComboAttack : MonoBehaviour
{
    [Header("Combo Settings")]
    [Tooltip("Thời gian chờ giữa các đòn trong cùng 1 combo")]
    [SerializeField] private float _hitInterval     = 0.35f;

    [Tooltip("Thời gian chờ sau khi hoàn thành 1 combo trước khi bắt đầu combo mới")]
    [SerializeField] private float _comboCooldown   = 1.2f;

    [Tooltip("Mỗi lần hoàn thành combo, cooldown nhân với hệ số này (< 1 = nhanh hơn)")]
    [SerializeField] private float _speedMultiplier = 0.85f;

    [Tooltip("Cooldown tối thiểu, không giảm xuống dưới giá trị này")]
    [SerializeField] private float _minCooldown     = 0.35f;

    [Header("VFX — theo thứ tự combo")]
    [Tooltip("0 = Horizontal, 1 = Diagonal. Lặp lại nếu combo dài hơn")]
    [SerializeField] private VisualEffect[] _slashVFX;

    // ── State ──────────────────────────────────────────────────────────────
    private int   _comboStep        = 0;   // đòn hiện tại trong combo
    private float _hitIntervalTimer = 0f;  // đếm ngược đến đòn tiếp theo
    private float _cooldownTimer    = 0f;  // đếm ngược sau khi combo xong
    private bool  _inCombo          = false;
    private float _currentCooldown;        // cooldown động, giảm dần

    // Số đòn trong 1 combo = số VFX được set (mặc định 2: ngang + chéo)
    private int ComboLength => (_slashVFX != null && _slashVFX.Length > 0)
                               ? _slashVFX.Length : 2;

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>Gọi từ EnemyChase.Attack() mỗi frame khi trong tầm đánh.</summary>
    /// <param name="target">Transform của player</param>
    /// <param name="damage">Sát thương mỗi đòn</param>
    /// <param name="knockback">Component nhận knockback của player</param>
    /// <param name="knockbackForce">Lực knockback</param>
    public void TryAttack(Transform target,
                          float damage,
                          KnockbackReceiver knockback = null,
                          float knockbackForce = 20f)
    {
        if (target == null) return;

        _hitIntervalTimer -= Time.deltaTime;
        _cooldownTimer    -= Time.deltaTime;

        // Đang nghỉ giữa các combo
        if (_cooldownTimer > 0f) return;

        if (!_inCombo)
        {
            // Bắt đầu combo mới
            _inCombo          = true;
            _comboStep        = 0;
            _hitIntervalTimer = 0f;
        }

        if (_hitIntervalTimer > 0f) return;

        // ── Thực hiện đòn đánh ────────────────────────────────────────────
        LookAt(target);
        PlaySlashVFX(_comboStep);
        ApplyDamage(target, damage, knockback, knockbackForce, _comboStep);

        Debug.Log($"[Combo] {name}  step {_comboStep + 1}/{ComboLength}  " +
                  $"cooldown={_currentCooldown:F2}s");

        _comboStep++;

        if (_comboStep >= ComboLength)
        {
            // Hoàn thành 1 combo → tăng tốc và nghỉ
            _currentCooldown  = Mathf.Max(_currentCooldown * _speedMultiplier, _minCooldown);
            _cooldownTimer    = _currentCooldown;
            _comboStep        = 0;
            _inCombo          = false;
        }
        else
        {
            // Chờ trước đòn tiếp theo
            _hitIntervalTimer = _hitInterval;
        }
    }

    /// <summary>Reset về trạng thái ban đầu (gọi khi re-spawn từ pool).</summary>
    public void ResetCombo()
    {
        _comboStep        = 0;
        _hitIntervalTimer = 0f;
        _cooldownTimer    = 0f;
        _inCombo          = false;
        _currentCooldown  = _comboCooldown;

        if (_slashVFX != null)
            foreach (var vfx in _slashVFX)
                if (vfx != null) vfx.Stop();
    }

    // ── Unity ──────────────────────────────────────────────────────────────

    private void Awake()
    {
        _currentCooldown = _comboCooldown;

        if (_slashVFX != null)
            foreach (var vfx in _slashVFX)
                if (vfx != null) vfx.Stop();
    }

    // ── Helpers ────────────────────────────────────────────────────────────

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

        // Wrap nếu combo dài hơn số VFX
        int idx = step % _slashVFX.Length;
        VisualEffect vfx = _slashVFX[idx];
        if (vfx == null) return;

        vfx.Reinit();
        vfx.Play();
    }

    private void ApplyDamage(Transform target,
                             float damage,
                             KnockbackReceiver knockback,
                             float knockbackForce,
                             int step)
    {
        target.GetComponent<PlayerHealth>()?.TakeDamage(damage);

        // Hướng knockback thay đổi theo đòn
        Vector3 kbDir = GetKnockbackDirection(step);
        knockback?.ApplyKnockback(kbDir, knockbackForce);
    }

    /// <summary>
    /// Hướng knockback theo kiểu slash:
    ///   Bước chẵn (0,2,4...) = ngang → đẩy sang phải enemy
    ///   Bước lẻ  (1,3,5...) = chéo  → đẩy chéo trái-sau
    /// </summary>
    private Vector3 GetKnockbackDirection(int step)
    {
        if (step % 2 == 0)
            // Horizontal L→R: đẩy sang phải tương đối
            return transform.right;
        else
            // Diagonal R→L: đẩy chéo phải + về sau
            return (-transform.right + transform.forward).normalized;
    }

#if UNITY_EDITOR
    // Hiển thị debug combo step ở góc trên-phải object
    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2.2f,
            $"Combo {_comboStep}/{ComboLength}  CD={_currentCooldown:F2}s");
    }
#endif
}