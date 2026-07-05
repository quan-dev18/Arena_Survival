using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputManager _input;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 10f;

    [Header("Rotation")]
    [SerializeField] private float _rotationSpeed = 15f;

    [Header("Gravity")]
    [SerializeField] private float _gravity = -20f;

    private CharacterController _controller;
    private Camera _mainCamera;
    private float _verticalVelocity;

    [SerializeField] private Animator _animator;

    public float MoveSpeed => _moveSpeed;

    public void SetMoveSpeed(float speed)
    {
        _moveSpeed = speed;
    }

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _mainCamera = Camera.main;
        if(_animator == null) _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        ApplyGravity();
        Move();
    }

    private void Move()
    {
        Vector2 input = _input.Move;
        bool isMoving = input != Vector2.zero;
        _animator.SetBool("OnMove", isMoving);
        if (!isMoving) return;
        
        // Lấy hướng camera, bỏ trục Y
        Vector3 camForward = _mainCamera.transform.forward;
        Vector3 camRight   = _mainCamera.transform.right;
        camForward.y = 0f;
        camRight.y   = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // Tính hướng di chuyển theo camera
        Vector3 moveDir = camForward * input.y + camRight * input.x;

        _controller.Move(moveDir * _moveSpeed * Time.deltaTime);

        // Xoay nhân vật về hướng di chuyển
        Quaternion targetRot = Quaternion.LookRotation(moveDir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            _rotationSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (_controller.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;

        _verticalVelocity += _gravity * Time.deltaTime;
        _controller.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
    }
}