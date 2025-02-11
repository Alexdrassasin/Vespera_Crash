using DG.Tweening;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using PurrNet;
using PurrNet.StateMachine;
using System.Collections.Generic;
using UnityEngine;

public class RoundRunningState :  StateNode<List<PlayerHealth>>
{
    private List<PlayerID> _players = new();
    [SerializeField] private RoundEndState _endState;

    [ObserversRpc]
    private void InitializeUI()
    {
        Debug.Log("UI Initialization Done");
        if(_endState._roundCount < InstanceHandler.GetInstance<DataCarrier>().maxRound)
        {
            InstanceHandler.GetInstance<MainGameView>().UpdateRoundText(_endState._roundCount + 1);
        }

        InstanceHandler.GetInstance<MainGameView>().toggleSpectatingPlayerName(false);
        if (InstanceHandler.GetInstance<WaitForPlayersState>()._waitingPlayerCanvas.alpha != 0)
        {
            InstanceHandler.GetInstance<WaitForPlayersState>()._waitingPlayerCanvas.DOFade(0, 0.5f).OnComplete(() =>
            {
                InstanceHandler.GetInstance<WaitForPlayersState>()._waitingPlayerCanvas.gameObject.SetActive(false);
            });
        }
    }

    public override void Enter(List<PlayerHealth> data, bool asServer)
    {
        base.Enter(data, asServer);

        InitializeUI();


        if (!asServer)
        {
            InstanceHandler.GetInstance<TerrainManager>().InitializeTerrain();
            return;
        }

        _players.Clear();
        foreach (var player in data)
        {
            if(player.owner.HasValue)
            {
                _players.Add(player.owner.Value);
            }
            player.OnDeath_Server += OnPlayerDeath;
        }
    }

    private void OnPlayerDeath(PlayerID deadPlayer)
    {
        _players.Remove(deadPlayer);

        if(_players.Count <= 1)
        {
            machine.Next();
        }
    }
}
