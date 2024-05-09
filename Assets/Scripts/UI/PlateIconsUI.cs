using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateIconsUI : MonoBehaviour
{
    [SerializeField] private PlateKitchenObject _plateKitchenObject;
    [SerializeField] private Transform _iconTemplate;

    private void Awake()
    {
        _iconTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        _plateKitchenObject.OnIngredientAdded += PlateKitchenObject_OnIngredientAdded;
    }

    private void PlateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        // Destroy all children except iconTemplate
        foreach (Transform child in transform)
        {
            if (child == _iconTemplate) continue;
            Destroy(child.gameObject);
        }

        foreach (KitchenObjectSO kitchenObjectSO in _plateKitchenObject.GetKitchenObjectSOList())
        {
            Transform iconTransform = Instantiate(_iconTemplate, transform);
            iconTransform.gameObject.SetActive(true);
            iconTransform.GetComponent<PlateIconsSingleUI>().SetKitchenObjectSO(kitchenObjectSO);
        }
    }
}
