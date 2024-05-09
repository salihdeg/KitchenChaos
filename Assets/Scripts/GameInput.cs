using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    private const string PLAYER_PREFS_BINDINGS = "InputBindings";

    public event System.EventHandler OnInteractAction;
    public event System.EventHandler OnInteractAlternateAction;
    public event System.EventHandler OnPause;
    public event System.EventHandler OnBindingRebind;

    public enum Binding
    {
        Move_Up,
        Move_Down,
        Move_Left,
        Move_Right,
        Interact,
        InteractAlternate,
        Pause,
        Gamepad_Interact,
        Gamepad_InteractAlternate,
        Gamepad_Pause,
    }

    private PlayerInputActions _playerInputActions;

    private void Awake()
    {
        Instance = this;

        _playerInputActions = new PlayerInputActions();

        if (PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS))
        {
            _playerInputActions.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));
        }

        _playerInputActions.Enable();

        _playerInputActions.Player.Interact.performed += Interact_Performed;
        _playerInputActions.Player.InteractAlternate.performed += InteractAlternate_performed;
        _playerInputActions.Player.Pause.performed += Pause_Performed;
    }

    private void OnDestroy()
    {
        _playerInputActions.Player.Interact.performed -= Interact_Performed;
        _playerInputActions.Player.InteractAlternate.performed -= InteractAlternate_performed;
        _playerInputActions.Player.Pause.performed -= Pause_Performed;

        _playerInputActions.Dispose();
    }

    private void InteractAlternate_performed(InputAction.CallbackContext obj)
    {
        OnInteractAlternateAction?.Invoke(this, System.EventArgs.Empty);
    }

    private void Interact_Performed(InputAction.CallbackContext context)
    {
        OnInteractAction?.Invoke(this, System.EventArgs.Empty);
    }

    private void Pause_Performed(InputAction.CallbackContext context)
    {
        OnPause?.Invoke(this, System.EventArgs.Empty);
    }

    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = _playerInputActions.Player.Move.ReadValue<Vector2>();

        inputVector = inputVector.normalized;
        return inputVector;
    }

    public string GetBindingText(Binding binding)
    {
        switch (binding)
        {
            default:
            case Binding.Move_Up:
                return _playerInputActions.Player.Move.bindings[1].ToDisplayString();
            case Binding.Move_Down:
                return _playerInputActions.Player.Move.bindings[2].ToDisplayString();
            case Binding.Move_Left:
                return _playerInputActions.Player.Move.bindings[3].ToDisplayString();
            case Binding.Move_Right:
                return _playerInputActions.Player.Move.bindings[4].ToDisplayString();
            case Binding.Interact:
                return _playerInputActions.Player.Interact.bindings[0].ToDisplayString();
            case Binding.InteractAlternate:
                return _playerInputActions.Player.InteractAlternate.bindings[0].ToDisplayString();
            case Binding.Pause:
                return _playerInputActions.Player.Pause.bindings[0].ToDisplayString();
            // GAMEPAD INPUTS
            case Binding.Gamepad_Interact:
                return _playerInputActions.Player.Interact.bindings[1].ToDisplayString();
            case Binding.Gamepad_InteractAlternate:
                return _playerInputActions.Player.InteractAlternate.bindings[1].ToDisplayString();
            case Binding.Gamepad_Pause:
                return _playerInputActions.Player.Pause.bindings[1].ToDisplayString();
        }
    }

    public void RebindBinding(Binding binding, System.Action onActionRebound)
    {
        _playerInputActions.Player.Disable();

        InputAction inputAction;
        int bindingIndex;

        switch (binding)
        {
            default:
            case Binding.Move_Up:
                inputAction = _playerInputActions.Player.Move;
                bindingIndex = 1;
                break;
            case Binding.Move_Down:
                inputAction = _playerInputActions.Player.Move;
                bindingIndex = 2;
                break;
            case Binding.Move_Left:
                inputAction = _playerInputActions.Player.Move;
                bindingIndex = 3;
                break;
            case Binding.Move_Right:
                inputAction = _playerInputActions.Player.Move;
                bindingIndex = 4;
                break;
            case Binding.Interact:
                inputAction = _playerInputActions.Player.Interact;
                bindingIndex = 0;
                break;
            case Binding.InteractAlternate:
                inputAction = _playerInputActions.Player.InteractAlternate;
                bindingIndex = 0;
                break;
            case Binding.Pause:
                inputAction = _playerInputActions.Player.Pause;
                bindingIndex = 0;
                break;
            // GAMEPAD INPUTS
            case Binding.Gamepad_Interact:
                inputAction = _playerInputActions.Player.Interact;
                bindingIndex = 1;
                break;
            case Binding.Gamepad_InteractAlternate:
                inputAction = _playerInputActions.Player.InteractAlternate;
                bindingIndex = 1;
                break;
            case Binding.Gamepad_Pause:
                inputAction = _playerInputActions.Player.Pause;
                bindingIndex = 1;
                break;
        }

        inputAction.PerformInteractiveRebinding(bindingIndex)
            .OnComplete(callback =>
            {
                callback.Dispose();
                _playerInputActions.Player.Enable();
                onActionRebound();

                string inputJson = _playerInputActions.SaveBindingOverridesAsJson();
                PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, inputJson);
                PlayerPrefs.Save();

                OnBindingRebind?.Invoke(this, System.EventArgs.Empty);
            })
            .Start();
    }
}
