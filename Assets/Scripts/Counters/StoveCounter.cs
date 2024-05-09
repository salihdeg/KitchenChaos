using Unity.Netcode;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgress
{
    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned
    }

    public event System.EventHandler<OnStateChangedEventArgs> OnStateChanged;
    public event System.EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    public class OnStateChangedEventArgs : System.EventArgs
    {
        public State state;
    }

    [SerializeField] private FryingRecipeSO[] _fryingRecipeSOs;
    [SerializeField] private BurningRecipeSO[] _burningRecipeSOs;

    private NetworkVariable<State> _state = new NetworkVariable<State>(State.Idle);
    private NetworkVariable<float> _fryingTimer = new NetworkVariable<float>(0f);
    private NetworkVariable<float> _burningTimer = new NetworkVariable<float>(0f);
    private FryingRecipeSO _fryingRecipeSO;
    private BurningRecipeSO _burningRecipeSO;

    public override void OnNetworkSpawn()
    {
        _fryingTimer.OnValueChanged += FryingTimer_OnValueChanged;
        _burningTimer.OnValueChanged += BurningTimer_OnValueChanged;
        _state.OnValueChanged += State_OnValueChanged;
    }

    private void FryingTimer_OnValueChanged(float previousValue, float newValue)
    {
        float fryingTimerMax = _fryingRecipeSO != null ? _fryingRecipeSO.fryingTimerMax : 1f;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        { progressNormalized = _fryingTimer.Value / fryingTimerMax });
    }

    private void BurningTimer_OnValueChanged(float previousValue, float newValue)
    {
        float burningTimerMax = _burningRecipeSO != null ? _burningRecipeSO.burningTimerMax : 1f;
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        { progressNormalized = _burningTimer.Value / burningTimerMax });
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = _state.Value });
        if ((_state.Value == State.Idle) || (_state.Value == State.Burned))
        {
            OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs { progressNormalized = 0f });
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        if (HasKitchenObject())
        {
            switch (_state.Value)
            {
                case State.Idle:
                    break;
                case State.Frying:

                    _fryingTimer.Value += Time.deltaTime;

                    if (_fryingTimer.Value > _fryingRecipeSO.fryingTimerMax)
                    {
                        // Fried
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                        KitchenObject.SpawnKitchenObject(_fryingRecipeSO.output, this);

                        _state.Value = State.Fried;
                        _burningTimer.Value = 0f;
                        SetBurningRecipeSOClientRPC(
                            KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(GetKitchenObject().GetKitchenObjectSO()));
                    }
                    break;
                case State.Fried:

                    _burningTimer.Value += Time.deltaTime;

                    if (_burningTimer.Value > _burningRecipeSO.burningTimerMax)
                    {
                        // Burned
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());
                        KitchenObject.SpawnKitchenObject(_burningRecipeSO.output, this);

                        _state.Value = State.Burned;
                    }
                    break;
                case State.Burned:
                    break;
                default:
                    break;
            }
        }
    }

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            // There is no KitchenObject here
            if (player.HasKitchenObject())
            {
                // Player is carrying something
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO()))
                {
                    // Player carrying something that can be Fried
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);

                    InteractLogicPlaceObjectOnCounterServerRPC(
                        KitchenGameMultiplayer.Instance.GetKitchenObjectSOIndex(kitchenObject.GetKitchenObjectSO()));
                }
            }
            else
            {
                // Player not carrying anythnig
            }
        }
        else
        {
            // There is a KitchenObject here
            if (!player.HasKitchenObject())
            {
                // Player is not carring anythnig
                GetKitchenObject().SetKitchenObjectParent(player);

                SetStateIdleServerRPC();
            }
            else
            {
                // Player is carrying something
                if (player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject))
                {
                    // Player is holding a Plate
                    if (plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                    {
                        KitchenObject.DestroyKitchenObject(GetKitchenObject());

                        SetStateIdleServerRPC();
                    }
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetStateIdleServerRPC()
    {
        _state.Value = State.Idle;
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRPC(int kitchenObjectSOIndex)
    {
        _fryingTimer.Value = 0f;
        _state.Value = State.Frying;
        SetFryingRecipeSOClientRPC(kitchenObjectSOIndex);
    }

    [ClientRpc]
    private void SetFryingRecipeSOClientRPC(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        _fryingRecipeSO = GetFryingRecipeSOWithInput(kitchenObjectSO);
    }

    [ClientRpc]
    private void SetBurningRecipeSOClientRPC(int kitchenObjectSOIndex)
    {
        KitchenObjectSO kitchenObjectSO = KitchenGameMultiplayer.Instance.GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        _burningRecipeSO = GetBurningRecipeSOWithInput(kitchenObjectSO);
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);

        return fryingRecipeSO != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);

        return fryingRecipeSO != null
            ? fryingRecipeSO.output
            : null;
    }

    private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        for (int i = 0; i < _fryingRecipeSOs.Length; i++)
        {
            if (_fryingRecipeSOs[i].input == inputKitchenObjectSO)
            {
                return _fryingRecipeSOs[i];
            }
        }
        return null;
    }

    private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        for (int i = 0; i < _burningRecipeSOs.Length; i++)
        {
            if (_burningRecipeSOs[i].input == inputKitchenObjectSO)
            {
                return _burningRecipeSOs[i];
            }
        }
        return null;
    }

    public bool IsFried()
    {
        return _state.Value == State.Fried;
    }
}
