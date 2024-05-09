using Unity.Netcode;
using UnityEngine;

public class KitchenObject : NetworkBehaviour
{
    [SerializeField] private KitchenObjectSO _kitchenObjectSO;

    private IKitchenObjectParent _kitchenObjectParent;
    private FollowTransform _followTransform;

    protected virtual void Awake()
    {
        _followTransform = GetComponent<FollowTransform>();
    }

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return _kitchenObjectSO;
    }

    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {
        SetKitchenObjectParentServerRPC(kitchenObjectParent.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetKitchenObjectParentServerRPC(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        SetKitchenObjectParentClientRPC(kitchenObjectParentNetworkObjectReference);
    }

    [ClientRpc]
    private void SetKitchenObjectParentClientRPC(NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
        IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();

        if (_kitchenObjectParent != null)
        {
            _kitchenObjectParent.ClearKitchenObject();
        }

        _kitchenObjectParent = kitchenObjectParent;

        if (kitchenObjectParent.HasKitchenObject())
        {
            Debug.LogError("IKitchenObjectParent already has a KitchenObject");
        }
        kitchenObjectParent.SetKitchenObject(this);

        _followTransform.SetTargetTransform(kitchenObjectParent.GetKitchernObjectFollowTransform());
    }

    public IKitchenObjectParent GetKitchenObjectParent()
    {
        return _kitchenObjectParent;
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void ClearKitchenObjectOnParent()
    {
        _kitchenObjectParent.ClearKitchenObject();
    }

    public bool TryGetPlate(out PlateKitchenObject plateKitchenObject)
    {
        if (this is PlateKitchenObject)
        {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }
        else
        {
            plateKitchenObject = null;
            return false;
        }
    }

    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        KitchenGameMultiplayer.Instance.SpawnKitchenObject(kitchenObjectSO, kitchenObjectParent);
    }

    public static void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        KitchenGameMultiplayer.Instance.DestroyKitchenObject(kitchenObject);
    }
}
