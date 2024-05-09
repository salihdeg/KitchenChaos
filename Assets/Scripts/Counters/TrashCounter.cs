using Unity.Netcode;

public class TrashCounter : BaseCounter
{
    public static event System.EventHandler OnAnyObjectTrashed;

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject())
        {
            KitchenObject.DestroyKitchenObject(player.GetKitchenObject());

            InteractLogicServerRPC();
        }
    }

    new public static void ResetStaticData()
    {
        OnAnyObjectTrashed = null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRPC()
    {
        InteractLogicClientRPC();
    }

    [ClientRpc]
    private void InteractLogicClientRPC()
    {
        OnAnyObjectTrashed?.Invoke(this, System.EventArgs.Empty);
    }
}
