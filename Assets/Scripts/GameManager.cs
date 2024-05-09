using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    public static GameManager Instance { get; private set; }

    public event System.EventHandler OnStateChanged;
    public event System.EventHandler OnLocalGamePaused;
    public event System.EventHandler OnLocalGameUnPaused;
    public event System.EventHandler OnMultiplayerGamePaused;
    public event System.EventHandler OnMultiplayerGameUnPaused;
    public event System.EventHandler OnLocalPlayerReadyChanged;

    [SerializeField] private Transform _playerPrefab;

    private NetworkVariable<State> _state = new NetworkVariable<State>(State.WaitingToStart);
    private bool _isLocalPlayerReady;

    private bool _autoTesteGamePausedState;
    private bool _isLocalGamePaused = false;
    private NetworkVariable<bool> _isGamePaused = new NetworkVariable<bool>(false);
    private Dictionary<ulong, bool> _playerReadyDictionary;
    private Dictionary<ulong, bool> _playerPausedDictionary;

    private NetworkVariable<float> _countdownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> _gamePlayingTimer = new NetworkVariable<float>(0f);
    private float _gamePlayingTimerMax = 120f;

    private void Awake()
    {
        Instance = this;

        _playerReadyDictionary = new Dictionary<ulong, bool>();
        _playerPausedDictionary = new Dictionary<ulong, bool>();
    }

    private void Start()
    {
        GameInput.Instance.OnPause += GameInput_OnPause;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    public override void OnNetworkSpawn()
    {
        _state.OnValueChanged += State_OnValueChanged;
        _isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(_playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        _autoTesteGamePausedState = true;
    }

    private void IsGamePaused_OnValueChanged(bool previousValue, bool newValue)
    {
        if (_isGamePaused.Value)
        {
            Time.timeScale = 0f;
            OnMultiplayerGamePaused?.Invoke(this, System.EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnMultiplayerGameUnPaused?.Invoke(this, System.EventArgs.Empty);
        }
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, System.EventArgs.Empty);
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if (_state.Value == State.WaitingToStart)
        {
            _isLocalPlayerReady = true;
            OnLocalPlayerReadyChanged?.Invoke(this, System.EventArgs.Empty);
            SetPlayerReadyServerRPC();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRPC(ServerRpcParams serverRpcParams = default)
    {
        _playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!_playerReadyDictionary.ContainsKey(clientId) || !_playerReadyDictionary[clientId])
            {
                // This player is NOT ready!
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            _state.Value = State.CountdownToStart;
        }
    }

    private void GameInput_OnPause(object sender, System.EventArgs e)
    {
        TogglePauseGame();
    }

    private void Update()
    {
        if (!IsServer) return;

        switch (_state.Value)
        {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                _countdownToStartTimer.Value -= Time.deltaTime;
                if (_countdownToStartTimer.Value < 0f)
                {
                    _state.Value = State.GamePlaying;
                    _gamePlayingTimer.Value = _gamePlayingTimerMax;
                }
                break;
            case State.GamePlaying:
                _gamePlayingTimer.Value -= Time.deltaTime;
                if (_gamePlayingTimer.Value < 0f)
                {
                    _state.Value = State.GameOver;
                }
                break;
            case State.GameOver:
                break;
        }

        // Debug.Log(_state);
    }

    private void LateUpdate()
    {
        if (_autoTesteGamePausedState)
        {
            _autoTesteGamePausedState = false;
            TestGamePausedState();
        }
    }

    public void TogglePauseGame()
    {
        _isLocalGamePaused = !_isLocalGamePaused;

        if (_isLocalGamePaused)
        {
            PauseGameServerRPC();
            OnLocalGamePaused?.Invoke(this, System.EventArgs.Empty);
        }
        else
        {
            UnPauseGameServerRPC();
            OnLocalGameUnPaused?.Invoke(this, System.EventArgs.Empty);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRPC(ServerRpcParams serverRpcParams = default)
    {
        _playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = true;
        TestGamePausedState();
    }

    [ServerRpc(RequireOwnership = false)]
    private void UnPauseGameServerRPC(ServerRpcParams serverRpcParams = default)
    {
        _playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = false;
        TestGamePausedState();
    }

    private void TestGamePausedState()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (_playerPausedDictionary.ContainsKey(clientId) && _playerPausedDictionary[clientId])
            {
                // This player is paused the game!
                _isGamePaused.Value = true;
                return;
            }
        }

        // All players are unpased!
        _isGamePaused.Value = false;
    }

    public bool IsGamePlaying()
    {
        return _state.Value == State.GamePlaying;
    }

    public bool IsCountdownToStartActive()
    {
        return _state.Value == State.CountdownToStart;
    }

    public bool IsGameOver()
    {
        return _state.Value == State.GameOver;
    }

    public bool IsWaitingToStart()
    {
        return _state.Value == State.WaitingToStart;
    }

    public bool IsLocalPlayerReady()
    {
        return _isLocalPlayerReady;
    }

    public float GetCountdownToStartTimer()
    {
        return _countdownToStartTimer.Value;
    }

    public float GetGamePlayingTimerNormalized()
    {
        return 1 - (_gamePlayingTimer.Value / _gamePlayingTimerMax);
    }
}
