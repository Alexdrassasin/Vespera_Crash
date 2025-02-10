using PurrLobby;
using PurrNet;
using PurrNet.Logging;
using PurrNet.Steam;
using PurrNet.Transports;
using Steamworks;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionStarter : MonoBehaviour
{
    [SerializeField] private CanvasGroup BlackOverlay;
    [SerializeField] private bool useSteamTransport;
    private SteamTransport _steamTransport;
    private PurrTransport _purrTransport;
    private UDPTransport _udpTransport;
    private NetworkManager _networkManager;
    private LobbyDataHolder _lobbyDataHolder;

    private bool _isFromLobby;

    private void Awake()
    {
        // Ensure components are correctly assigned
        _steamTransport = GetComponent<SteamTransport>();
        _purrTransport = GetComponent<PurrTransport>();
        _udpTransport = GetComponent<UDPTransport>();
        _networkManager = GetComponent<NetworkManager>();

        if (_steamTransport == null)
            PurrLogger.LogError($"Failed to get {nameof(SteamTransport)} component.", this);

        if (_purrTransport == null)
            PurrLogger.LogError($"Failed to get {nameof(PurrTransport)} component.", this);

        if (_udpTransport == null)
            PurrLogger.LogError($"Failed to get {nameof(UDPTransport)} component.", this);

        if (_networkManager == null)
            PurrLogger.LogError($"Failed to get {nameof(NetworkManager)} component.", this);

        _lobbyDataHolder = FindFirstObjectByType<LobbyDataHolder>();
        if (_lobbyDataHolder != null)
            _isFromLobby = true;
    }

    private void Start()
    {
        if (BlackOverlay == null || _networkManager == null)
        {
            PurrLogger.LogError($"Failed to start connection. {nameof(BlackOverlay)} or {nameof(NetworkManager)} is null!", this);
            return;
        }

        if (_isFromLobby)
        {
            StartFromLobby();
        }
        else
        {
            StartNormal();
        }

        if (BlackOverlay != null)
        {
            BlackOverlay.alpha = 1;
            StartCoroutine(FadeOut(BlackOverlay, 0.5f));
        }
    }

    private void StartNormal()
    {
#if UNITY_EDITOR
        if (_networkManager != null && _udpTransport != null)
        {
            _networkManager.transport = _udpTransport;

            if (!ParrelSync.ClonesManager.IsClone())
            {
                _networkManager.StartServer();
                Debug.Log("startServer");
            }

            _networkManager.StartClient();
            Debug.Log("startClient");
        }
        else
        {
            PurrLogger.LogError($"Failed to start connection. {nameof(NetworkManager)} or {nameof(UDPTransport)} is null!", this);
        }
#endif
    }

    private void StartFromLobby()
    {
        if (_networkManager == null || _lobbyDataHolder == null || !_lobbyDataHolder.CurrentLobby.IsValid)
        {
            PurrLogger.LogError($"Failed to start connection. {nameof(NetworkManager)}, {nameof(LobbyDataHolder)}, or Lobby is invalid!", this);
            return;
        }

        if (useSteamTransport)
        {
            if (_steamTransport != null)
            {
                _networkManager.transport = _steamTransport;
                if (ulong.TryParse(_lobbyDataHolder.CurrentLobby.lobbyId, out ulong ulongId))
                {
                    var lobbyOwner = SteamMatchmaking.GetLobbyOwner(new CSteamID(ulongId));
                    if (lobbyOwner.IsValid())
                    {
                        _steamTransport.address = lobbyOwner.ToString();
                    }
                    else
                    {
                        Debug.LogError($"Failed to get lobby owner from parsed lobby ID", this);
                        return;
                    }
                }
                else
                {
                    Debug.LogError($"Failed to parse lobbyId into ulong!", this);
                    return;
                }
            }
            else
            {
                PurrLogger.LogError($"Failed to start connection. {nameof(SteamTransport)} is null!", this);
                return;
            }
        }
        else
        {
            if (_purrTransport != null)
            {
                _networkManager.transport = _purrTransport;
                _purrTransport.roomName = _lobbyDataHolder.CurrentLobby.lobbyId;
            }
            else
            {
                PurrLogger.LogError($"Failed to start connection. {nameof(PurrTransport)} is null!", this);
                return;
            }
        }

        if (_lobbyDataHolder.CurrentLobby.IsOwner)
            _networkManager.StartServer();

        StartCoroutine(StartClient());
    }

    private IEnumerator StartClient()
    {
        yield return new WaitForSeconds(1f);
        if (_networkManager != null)
        {
            _networkManager.StartClient();
        }
        else
        {
            PurrLogger.LogError($"Failed to start client. {nameof(NetworkManager)} is null!", this);
        }
    }

    private IEnumerator FadeOut(CanvasGroup fadeCanvas, float fadeDuration)
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            yield return null;
        }
        fadeCanvas.alpha = 0; // Ensure fully black
    }
}

