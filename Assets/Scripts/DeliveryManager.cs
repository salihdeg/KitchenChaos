using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeliveryManager : NetworkBehaviour
{
    public static DeliveryManager Instance { get; private set; }

    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;

    [SerializeField] private RecipeListSO _recipeListSO;
    private List<RecipeSO> _waitingRecipeSOList;

    private float _spawnRecipeTimer = 4f;
    private float _spawnRecipeTimerMax = 4f;
    private int _waitingRecipesMax = 4;
    private int _successfulRecipesAmount;

    private void Awake()
    {
        Instance = this;

        _waitingRecipeSOList = new List<RecipeSO>();
    }

    private void Update()
    {
        if (!IsServer) return;

        _spawnRecipeTimer -= Time.deltaTime;
        if (_spawnRecipeTimer <= 0f)
        {
            _spawnRecipeTimer = _spawnRecipeTimerMax;

            if (GameManager.Instance.IsGamePlaying() && _waitingRecipeSOList.Count < _waitingRecipesMax)
            {
                int waitingRecipeSOIndex = UnityEngine.Random.Range(0, _recipeListSO.recipeSOList.Count);
                SpawnNewWaitingRecipeClientRPC(waitingRecipeSOIndex);
            }
        }
    }

    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRPC(int waitingRecipeSOIndex)
    {
        RecipeSO waitingRecipeSO = _recipeListSO.recipeSOList[waitingRecipeSOIndex];
        _waitingRecipeSOList.Add(waitingRecipeSO);

        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < _waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = _waitingRecipeSOList[i];

            if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                // Has the same number of ingredients
                bool plateContentsMatchesRecipe = true;
                foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList)
                {
                    // Cycling through all ingredients in the Recipe
                    bool ingredientFound = false;
                    foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        // Cycling through all ingredients in the Plate
                        if (plateKitchenObjectSO == recipeKitchenObjectSO)
                        {
                            // Ingredient matches!
                            ingredientFound = true;
                            break;
                        }
                    }
                    if (!ingredientFound)
                    {
                        // This Recipe ingredient was not found on the Plate!
                        plateContentsMatchesRecipe = false;
                    }
                }

                if (plateContentsMatchesRecipe)
                {
                    // Player delivered the correct recipe!
                    DeliverCorrectRecipeServerRPC(i);
                    return;
                }
            }
        }

        // No matches found!
        // Player did not deliver a correct recipe!
        DeliverIncorrectRecipeServerRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverIncorrectRecipeServerRPC()
    {
        DeliverIncorrectRecipeClientRPC();
    }

    [ClientRpc]
    private void DeliverIncorrectRecipeClientRPC()
    {
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverCorrectRecipeServerRPC(int waitingRecipeSOListIndex)
    {
        DeliverCorrectRecipeClientRPC(waitingRecipeSOListIndex);
    }

    [ClientRpc]
    private void DeliverCorrectRecipeClientRPC(int waitingRecipeSOListIndex)
    {
        _successfulRecipesAmount++;
        _waitingRecipeSOList.RemoveAt(waitingRecipeSOListIndex);

        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
    }

    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return _waitingRecipeSOList;
    }

    public int GetSuccessfulRecipesAmount()
    {
        return _successfulRecipesAmount;
    }
}
