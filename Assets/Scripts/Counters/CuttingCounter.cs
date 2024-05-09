using Unity.Netcode;
using UnityEngine;

public class CuttingCounter : BaseCounter, IHasProgress
{
    public static event System.EventHandler OnAnyCut;

    public event System.EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
    public event System.EventHandler OnCut;


    [SerializeField] private CuttingRecipeSO[] _cuttingRecipeSOs;

    private int _cuttingProgress;

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
                    // Player carrying something that can be Cut
                    KitchenObject kitchenObject = player.GetKitchenObject();
                    kitchenObject.SetKitchenObjectParent(this);
                    InteractLogicPlaceObjectOnCounterServerRPC();
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
                        //GetKitchenObject().DestroySelf();
                    }
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicPlaceObjectOnCounterServerRPC()
    {
        InteractLogicPlaceObjectOnCounterClientRPC();
    }

    [ClientRpc]
    private void InteractLogicPlaceObjectOnCounterClientRPC()
    {
        _cuttingProgress = 0;

        //CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(kitchenObject.GetKitchenObjectSO());
        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            //progressNormalized = (float)_cuttingProgress / cuttingRecipeSO.cuttingProgressMax
            progressNormalized = 0f
        }); ;
    }

    public override void InteractAlternate(Player player)
    {
        if (HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            // There is a KitchenObject here
            // Cut!
            // Say server "Hey someone is cutting something!"
            // Than server say every client "there is something cutting here!"
            // every client works the client code!
            CutObjectServerRPC();
            TestCuttingProgressDoneServerRPC();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void CutObjectServerRPC()
    {
        CutObjectClientRPC();
    }

    [ClientRpc]
    private void CutObjectClientRPC()
    {
        _cuttingProgress++;

        OnCut?.Invoke(this, System.EventArgs.Empty);
        OnAnyCut?.Invoke(this, System.EventArgs.Empty);

        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

        OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs
        {
            progressNormalized = (float)_cuttingProgress / cuttingRecipeSO.cuttingProgressMax
        });
    }

    [ServerRpc(RequireOwnership = false)]
    private void TestCuttingProgressDoneServerRPC()
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
        // If cutting progress done, spawn!
        if (_cuttingProgress >= cuttingRecipeSO.cuttingProgressMax)
        {
            KitchenObjectSO outputKitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());

            KitchenObject.DestroyKitchenObject(GetKitchenObject());

            KitchenObject.SpawnKitchenObject(outputKitchenObjectSO, this);
        }
    }

    private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);

        return cuttingRecipeSO != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO)
    {
        CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);

        return cuttingRecipeSO != null
            ? cuttingRecipeSO.output
            : null;
    }

    private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO)
    {
        for (int i = 0; i < _cuttingRecipeSOs.Length; i++)
        {
            if (_cuttingRecipeSOs[i].input == inputKitchenObjectSO)
            {
                return _cuttingRecipeSOs[i];
            }
        }
        return null;
    }

    new public static void ResetStaticData()
    {
        OnAnyCut = null;
    }
}
