using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput _playerInput;
    public Vector2 Move { get; private set; }

    public event System.Action OnPause;

    private InputActionMap _actionMap;
    private InputAction _moveAction;
    private InputAction _pauseAction;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _actionMap = _playerInput.currentActionMap;

        _moveAction = _actionMap.FindAction("Move");

        _pauseAction = _actionMap.FindAction("OnPause");

        _pauseAction.performed += OnPausePerformed;

        _moveAction.performed += OnMove;
        _moveAction.canceled += OnMove;
    }

    private void OnDestroy()
    {
        _moveAction.performed -= OnMove;
        _moveAction.canceled -= OnMove;

        _pauseAction.performed -= OnPausePerformed;
    }

    private void OnEnable()  { _actionMap?.Enable(); }
    private void OnDisable() { _actionMap?.Disable(); }

    void OnMove(InputAction.CallbackContext ctx)  => Move = ctx.ReadValue<Vector2>();
    void OnPausePerformed(InputAction.CallbackContext _) => OnPause?.Invoke();
}