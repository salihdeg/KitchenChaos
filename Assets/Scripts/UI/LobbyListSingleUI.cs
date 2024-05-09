using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListSingleUI : MonoBehaviour
{
    private Lobby _lobby;

    [SerializeField] private TextMeshProUGUI _lobbyNameText;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            KitchenGameLobby.Instance.JoinWithId(_lobby.Id);
        });
    }

    public void SetLobby(Lobby lobby)
    {
        _lobby = lobby;
        _lobbyNameText.text = _lobby.Name;
    }
}
