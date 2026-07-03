using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput _playerInput;
    public Vector2 Move { get; private set; }

    private InputActionMap _actionMap;
    private InputAction _moveAction;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _actionMap = _playerInput.currentActionMap;

        _moveAction = _actionMap.FindAction("Move");

        _moveAction.performed += OnMove;

        _moveAction.canceled += OnMove;
    }

    private void OnDestroy()
    {
        _moveAction.performed -= OnMove;

        _moveAction.canceled -= OnMove;

    }

    private void OnEnable()  { _actionMap?.Enable(); }
    private void OnDisable() { _actionMap?.Disable(); }

    void OnMove(InputAction.CallbackContext ctx)  => Move = ctx.ReadValue<Vector2>();
}