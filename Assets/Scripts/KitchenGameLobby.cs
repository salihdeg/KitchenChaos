using Unity.Services.Authentication;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

public class KitchenGameLobby : MonoBehaviour
{
    public static KitchenGameLobby Instance { get; private set; }

    private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";

    public event System.EventHandler OnCreateLobbyStarted;
    public event System.EventHandler OnCreateLobbyFailed;
    public event System.EventHandler OnJoinStarted;
    public event System.EventHandler OnJoinFailed;
    public event System.EventHandler OnQuickJoinFailed;
    public event System.EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    public class OnLobbyListChangedEventArgs : System.EventArgs
    {
        public List<Lobby> lobbyList;
    }

    private Lobby _joinedLobby;
    private float _heartbeatTimer;
    private float _listLobbiesTimer;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeUnityAuthentication();
    }

    private async void InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions options = new InitializationOptions();
            options.SetProfile(Random.Range(0, 10000).ToString());
            await UnityServices.InitializeAsync(options);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    private void Update()
    {
        HandleHeartbeat();
        HandlePeriodicListLobbies();
    }

    private async void HandleHeartbeat()
    {
        if (!IsLobbyHost()) return;

        _heartbeatTimer -= Time.deltaTime;
        if (_heartbeatTimer <= 0f)
        {
            Debug.Log("HEARTBEAT!");
            await LobbyService.Instance.SendHeartbeatPingAsync(_joinedLobby.Id);
            float _heartbeatTimerMax = 20f;
            _heartbeatTimer = _heartbeatTimerMax;
        }
    }

    private void HandlePeriodicListLobbies()
    {
        if (!AuthenticationService.Instance.IsSignedIn) return;
        if (SceneManager.GetActiveScene().name != Loader.Scene.LobbyScene.ToString()) return;
        if (_joinedLobby != null) return;

        _listLobbiesTimer -= Time.deltaTime;
        if (_listLobbiesTimer <= 0f)
        {
            float listLobbiesTimerMax = 3f;
            _listLobbiesTimer = listLobbiesTimerMax;
            ListLobbies();
        }
    }

    private bool IsLobbyHost()
    {
        return _joinedLobby != null && (_joinedLobby.HostId == AuthenticationService.Instance.PlayerId);
    }

    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
            {
                new QueryFilter (QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
            }
            };
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);
            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs { lobbyList = queryResponse.Results });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(KitchenGameMultiplayer.MAX_PLAYER_AMOUNT - 1);
            return allocation;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }

    private async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }

    private async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            return await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return default;
        }
    }

    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        OnCreateLobbyStarted?.Invoke(this, System.EventArgs.Empty);
        try
        {
            _joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, KitchenGameMultiplayer.MAX_PLAYER_AMOUNT,
                new CreateLobbyOptions { IsPrivate = isPrivate });

            Allocation allocation = await AllocateRelay();

            string relayJoinCode = await GetRelayJoinCode(allocation);
            await LobbyService.Instance.UpdateLobbyAsync(_joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    {KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            });

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

            KitchenGameMultiplayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        }
        catch (LobbyServiceException e)
        {
            OnCreateLobbyFailed?.Invoke(this, System.EventArgs.Empty);
            Debug.Log(e);
        }
    }

    public async void QuickJoin()
    {
        OnJoinStarted?.Invoke(this, System.EventArgs.Empty);
        try
        {
            _joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            string relayJoinCode = _joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

            KitchenGameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            OnQuickJoinFailed?.Invoke(this, System.EventArgs.Empty);
            Debug.Log(e);
        }
    }

    public async void JoinWithCode(string lobbyCode)
    {
        OnJoinStarted?.Invoke(this, System.EventArgs.Empty);
        try
        {
            _joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            string relayJoinCode = _joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
            KitchenGameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            OnJoinFailed?.Invoke(this, System.EventArgs.Empty);
            Debug.Log(e);
        }
    }

    public async void JoinWithId(string lobbyId)
    {
        OnJoinStarted?.Invoke(this, System.EventArgs.Empty);
        try
        {
            _joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            string relayJoinCode = _joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
            KitchenGameMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException e)
        {
            OnJoinFailed?.Invoke(this, System.EventArgs.Empty);
            Debug.Log(e);
        }
    }

    public async void DeleteLobby()
    {
        if (_joinedLobby == null) return;

        try
        {
            await LobbyService.Instance.DeleteLobbyAsync(_joinedLobby.Id);
            _joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void LeaveLobby()
    {
        if (_joinedLobby == null) return;

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            _joinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void KickPlayer(string playerId)
    {
        if (!IsLobbyHost()) return;

        try
        {
            await LobbyService.Instance.RemovePlayerAsync(_joinedLobby.Id, playerId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public Lobby GetLobby()
    {
        return _joinedLobby;
    }
}
