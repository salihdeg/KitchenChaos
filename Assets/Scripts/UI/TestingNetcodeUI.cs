using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestingNetcodeUI : MonoBehaviour
{
    [SerializeField] private Button _startHostButton, _startClientButton;

    private void Awake()
    {
        _startHostButton.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.StartHost();
            Debug.Log("HOST");
            Hide();
        });
        _startClientButton.onClick.AddListener(() =>
        {
            KitchenGameMultiplayer.Instance.StartClient();
            Debug.Log("CLIENT");
            Hide();
        });
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
}
