using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatesCounterVisual : MonoBehaviour
{
    [SerializeField] private PlatesCounter _platesCounter;
    [SerializeField] private Transform _counterTopPoint;
    [SerializeField] private Transform _platesVisualPrefab;

    private List<GameObject> _plateVisualGamebjects;

    private void Awake()
    {
        _plateVisualGamebjects = new List<GameObject>();
    }

    private void Start()
    {
        _platesCounter.OnPlatetSpawned += PlatesCounter_OnPlatetSpawned;
        _platesCounter.OnPlateRemoved += PlatesCounter_OnPlateRemoved;
    }

    private void PlatesCounter_OnPlateRemoved(object sender, System.EventArgs e)
    {
        GameObject plateGameObject = _plateVisualGamebjects[_plateVisualGamebjects.Count - 1];
        _plateVisualGamebjects.Remove(plateGameObject);
        Destroy(plateGameObject);
    }

    private void PlatesCounter_OnPlatetSpawned(object sender, System.EventArgs e)
    {
        Transform plateVisualTransform = Instantiate(_platesVisualPrefab, _counterTopPoint);

        float plateOffsetY = 0.1f;
        plateVisualTransform.localPosition = new Vector3(0f, plateOffsetY * _plateVisualGamebjects.Count, 0f);

        _plateVisualGamebjects.Add(plateVisualTransform.gameObject);
    }
}
