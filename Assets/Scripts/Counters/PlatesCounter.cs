using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    public event System.EventHandler OnPlatetSpawned;
    public event System.EventHandler OnPlateRemoved;

    [SerializeField] private KitchenObjectSO _plateKitchenObjectSO;

    [SerializeField] private float _spawnPlateTimerMax = 4f;
    [SerializeField] private int _platesSpawnAmountMax = 4;
    private float _spawnPlateTimer;
    private int _platesSpawnAmount;

    private void Update()
    {
        if (!IsServer) return;

        _spawnPlateTimer += Time.deltaTime;
        if (_spawnPlateTimer > _spawnPlateTimerMax)
        {
            _spawnPlateTimer = 0;
            if (GameManager.Instance.IsGamePlaying() && _platesSpawnAmount < _platesSpawnAmountMax)
            {
                SpawnPlateServerRPC();
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlateServerRPC()
    {
        SpawnPlateClientRPC();
    }

    [ClientRpc]
    private void SpawnPlateClientRPC()
    {
        _platesSpawnAmount++;

        OnPlatetSpawned?.Invoke(this, System.EventArgs.Empty);
    }

    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject())
        {
            // Player is empty handed
            if (_platesSpawnAmount > 0)
            {
                // There's at least one plate here
                _platesSpawnAmount--;
                KitchenObject.SpawnKitchenObject(_plateKitchenObjectSO, player);

                InteractLogicServerRPC();
            }
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
        OnPlateRemoved?.Invoke(this, System.EventArgs.Empty);
    }
}
