using Unity.Netcode;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
    public event System.EventHandler OnPlayerGrabbedObject;

    [SerializeField] private KitchenObjectSO _kitchenObjectSO;

    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject())
        {
            // Player is not carrying anything
            // Spawn KitchenObject on Player
            KitchenObject.SpawnKitchenObject(_kitchenObjectSO, player);

            // Animation
            InteractLogicServerRPC();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRPC()
    {
        InteractLogicClientRPC();
    }

    [ClientRpc]
    private void InteractLogicClientRPC()
    {
        OnPlayerGrabbedObject?.Invoke(this, System.EventArgs.Empty);
    }
}
