using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour, IKitchenObjectParent
{
    public event System.EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : System.EventArgs
    {
        public BaseCounter selectedCounter;
    }
    public event System.EventHandler OnPickedSomething;
    public static event System.EventHandler OnAnyPlayerSpawned;
    public static event System.EventHandler OnAnyPickedSomething;

    public static Player LocalInstance { get; private set; }

    [SerializeField] private LayerMask _countersLayerMask;
    [SerializeField] private LayerMask _collisionsLayerMask;

    [SerializeField] private float _moveSpeed = 7f;
    [SerializeField] private Transform _kitchenObjectHoldPoint;
    [SerializeField] private List<Vector3> _spawnPositionsList;
    [SerializeField] private PlayerVisual _playerVisual;

    private bool _isWalking;
    private Vector3 _lastInteractDir;
    private BaseCounter _selectedCounter;
    private KitchenObject _kitchenObject;

    private void Awake()
    {
        //Instance = this;
    }

    private void Start()
    {
        GameInput.Instance.OnInteractAction += GameInput_OnInteraction;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;

        PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
        _playerVisual.SetPlayerColor(KitchenGameMultiplayer.Instance.GetPlayerColor(playerData.colorId));
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }
        transform.position = _spawnPositionsList[KitchenGameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)];
        OnAnyPlayerSpawned?.Invoke(this, System.EventArgs.Empty);

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if(clientId == OwnerClientId && HasKitchenObject())
        {
            KitchenObject.DestroyKitchenObject(GetKitchenObject());
        }
    }

    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
    }

    private void GameInput_OnInteractAlternateAction(object sender, System.EventArgs e)
    {
        if (!GameManager.Instance.IsGamePlaying()) return;

        if (_selectedCounter != null)
        {
            _selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteraction(object sender, System.EventArgs e)
    {
        if (!GameManager.Instance.IsGamePlaying()) return;

        if (_selectedCounter != null)
        {
            _selectedCounter.Interact(this);
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        HandleMovement();
        HandleInteractions();
    }

    public bool IsWalking()
    {
        return _isWalking;
    }

    private void HandleInteractions()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if (moveDir != Vector3.zero)
        {
            _lastInteractDir = moveDir;
        }
        float interactDistance = 2f;
        bool hit = Physics.Raycast(transform.position, _lastInteractDir, out RaycastHit raycastHit, interactDistance, _countersLayerMask);
        if (hit)
        {
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                if (baseCounter != _selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
    }

    private void HandleMovement()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float moveDistance = _moveSpeed * Time.deltaTime;
        float playerRadius = 0.7f;
        float playerHeight = 2f;
        //bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance, _collisionsLayerMask);
        bool canMove = !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDir, Quaternion.identity, moveDistance, _collisionsLayerMask);

        if (!canMove)
        {
            // Cannot move towards moveDir

            // Attempt only X movement
            Vector3 moveDirX = new Vector3(moveDir.x, 0f, 0f).normalized;
            canMove =
                (moveDir.x < -0.5f || moveDir.x > +0.5f)
                && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirX, Quaternion.identity, moveDistance, _collisionsLayerMask);

            if (canMove)
            {
                // Can move only on the X
                moveDir = moveDirX;
            }
            else
            {
                // Cannot move only on the X

                // Attempt only Z movement
                Vector3 moveDirZ = new Vector3(0f, 0f, moveDir.z).normalized;
                canMove =
                    ((moveDir.z < -0.5f || moveDir.z > +0.5f))
                    && !Physics.BoxCast(transform.position, Vector3.one * playerRadius, moveDirZ, Quaternion.identity, moveDistance, _collisionsLayerMask);

                if (canMove)
                {
                    // Can move only on the Z
                    moveDir = moveDirZ;
                }
                else
                {
                    // Cannot move in any direction
                }
            }
        }
        if (canMove)
        {
            transform.position += moveDir * _moveSpeed * Time.deltaTime;
        }

        _isWalking = moveDir != Vector3.zero;

        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, rotateSpeed * Time.deltaTime);
    }

    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        _selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = _selectedCounter
        });
    }

    public Transform GetKitchernObjectFollowTransform()
    {
        return _kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        _kitchenObject = kitchenObject;
        if (kitchenObject != null)
        {
            OnPickedSomething?.Invoke(this, System.EventArgs.Empty);
            OnAnyPickedSomething?.Invoke(this, System.EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject()
    {
        return _kitchenObject;
    }

    public void ClearKitchenObject()
    {
        _kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return _kitchenObject != null;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
