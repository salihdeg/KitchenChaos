using Unity.Netcode;
using UnityEngine;

public class BaseCounter : NetworkBehaviour, IKitchenObjectParent
{
    public static event System.EventHandler OnAnyObjectPlacedHere;

    [SerializeField] private Transform _counterTopPoint;

    private KitchenObject _kitchenObject;

    public virtual void Interact(Player player)
    {
        Debug.LogError("BaseCounter.Interact();");
    }

    public virtual void InteractAlternate(Player player)
    {
        //Debug.LogError("BaseCounter.InteractAlternate();");
    }

    public Transform GetKitchernObjectFollowTransform()
    {
        return _counterTopPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        _kitchenObject = kitchenObject;
        if (kitchenObject != null)
            OnAnyObjectPlacedHere?.Invoke(this, System.EventArgs.Empty);
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

    public static void ResetStaticData()
    {
        OnAnyObjectPlacedHere = null;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
