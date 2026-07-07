using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform _target;

    [Header("Isometric Offset")]
    [SerializeField] private Vector3 _offset = new Vector3(0f, 10f, -7f);
    [SerializeField] private float _pitch = 45f;   // góc nghiêng dọc (X)
    [SerializeField] private float _yaw   = 45f;   // góc xoay ngang (Y) — 45° = isometric

    [Header("Smoothing")]
    [SerializeField] private float _positionSmooth = 8f;
    [SerializeField] private float _rotationSmooth = 6f;

    private Vector3 _currentVelocity;   // dùng cho SmoothDamp

    private void LateUpdate()
    {
        if (_target == null) return;

        FollowTarget();
    }

    private void FollowTarget()
    {
        // Tính rotation isometric cố định
        Quaternion targetRot = Quaternion.Euler(_pitch, _yaw, 0f);

        // Tính vị trí = target + offset xoay theo góc isometric
        Vector3 targetPos = _target.position + targetRot * _offset;

        // Di chuyển mượt theo vị trí
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref _currentVelocity,
            1f / _positionSmooth);

        // Xoay mượt về góc isometric
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            _rotationSmooth * Time.deltaTime);
    }

    // Hiển thị offset trong Scene view để dễ chỉnh
    private void OnDrawGizmosSelected()
    {
        if (_target == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(_target.position, transform.position);
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}