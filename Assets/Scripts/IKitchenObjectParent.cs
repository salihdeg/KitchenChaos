using Unity.Netcode;
using UnityEngine;

public interface IKitchenObjectParent
{
    public Transform GetKitchernObjectFollowTransform();

    public void SetKitchenObject(KitchenObject kitchenObject);

    public KitchenObject GetKitchenObject();

    public void ClearKitchenObject();

    public bool HasKitchenObject();

    public NetworkObject GetNetworkObject();
}
