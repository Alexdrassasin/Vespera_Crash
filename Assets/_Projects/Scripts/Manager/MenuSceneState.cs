using DG.Tweening;
using PurrNet;
using PurrNet.Modules;
using PurrNet.StateMachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSceneState : StateNode
{

    [PurrScene] public string sceneToChange;


    [SerializeField] private int minPlayers = 2;
    [SerializeField] SyncVar<int> readyPlayersCount = new(0); // Number of players who have pressed ready
    [SerializeField] public SyncDictionary<PlayerID, bool> playerStatusDict = new();
    [SerializeField] private Dictionary<PlayerID, PlayerStatusController> playerTagDict = new();
    private int totalPlayers = 0; // The total number of players who have joined

    [SerializeField] private PlayerStatusController readyUIPrefab; // Assign your UI prefab in the Inspector
    [SerializeField] private Transform layoutParent;
    [SerializeField] private GameObject GreenBg;
    [SerializeField] private TextMeshProUGUI ReadyText;

    private bool isReady;
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        Initialization();

        if (!asServer)
        {
            return;
        }

        foreach (var player in networkManager.players)
        {
            SetStatus(player, false);
        }
    }

    public void Initialization() 
    {
        Debug.Log("Initialization enter");
        totalPlayers = networkManager.players.Count;
        readyPlayersCount.value = 0;

        GreenBg.SetActive(false);
        ReadyText.text = "READY";

        foreach (var player in networkManager.players)
        {
            var playerStatus = Instantiate(readyUIPrefab, layoutParent);
            playerStatus.UpdateStatus(player.ToString(), false);
            playerTagDict[player] = playerStatus;
        }

        if(InstanceHandler.GetInstance<WaitForPlayersState>()._waitingPlayerCanvas.alpha != 0)
        {
            InstanceHandler.GetInstance<WaitForPlayersState>()._waitingPlayerCanvas.DOFade(0, 0.5f).OnComplete(() =>
            {
                InstanceHandler.GetInstance<WaitForPlayersState>()._waitingPlayerCanvas.gameObject.SetActive(false);
            });
        }

    }
    public void TogglePlayerReady()
    {
        isReady = !isReady;
        if (isReady)
        {
            Ready(networkManager.localPlayer);
            GreenBg.SetActive(true);
            ReadyText.text = "CANCEL";
        }
        else
        {
            UnReady();
            GreenBg.SetActive(false);
            ReadyText.text = "READY";
        }
    }

    [ServerRpc(requireOwnership: false)]
    public void Ready(PlayerID playerID, RPCInfo info = default)
    {
        readyPlayersCount.value++;
        SetStatus(info.sender, true);
        UpdateAllPlayerTags();
        InstanceHandler.GetInstance<DataCarrier>().CheckForDictionaryEntry(playerID);
        CheckCanStartGame();
    }

    [ServerRpc(requireOwnership: false)]
    public void UnReady(RPCInfo info = default)
    {
        if(readyPlayersCount.value > 0)
        readyPlayersCount.value--;
        SetStatus(info.sender, false);
        UpdateAllPlayerTags();
    }

    public void SetStatus(PlayerID playerID, bool status)
    {
        playerStatusDict[playerID] = status;
    }

    [ObserversRpc(runLocally:true)]
    public void UpdateAllPlayerTags()
    {
        foreach (var player in networkManager.players)
        {
            var controller = playerTagDict[player];
            controller.UpdateStatus(player.ToString(), playerStatusDict[player]);
        }
    }

    public void CheckCanStartGame()
    {
        StartCoroutine(CheckCanStartGame_Coroutine());
    }
    public IEnumerator CheckCanStartGame_Coroutine()
    {
        if(readyPlayersCount < networkManager.playerCount)
        {
            yield break;
        }

        yield return new WaitForSeconds(0.3f);

        PurrSceneSettings settings = new()
        {
            isPublic = true,
            mode = LoadSceneMode.Single
        };
        networkManager.sceneModule.LoadSceneAsync(sceneToChange, settings);
    }
}
    
   

