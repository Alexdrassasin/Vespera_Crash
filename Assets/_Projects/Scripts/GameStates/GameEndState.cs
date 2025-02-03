using PurrNet;
using PurrNet.StateMachine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameEndState : StateNode
{
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!asServer)
        {
            return;
        }

        if(!InstanceHandler.TryGetInstance(out ScoreManager scoreManager))
        {
            Debug.LogError($"GameEndState failed to get scoremanager!", this);
            return;
        }

        var winner = scoreManager.GetWinner();
        if(winner == default)
        {
            Debug.LogError($"GameEndState failed to get winner!", this);
            return;
        }

        if(!InstanceHandler.TryGetInstance(out EndGameView endGameView))
        {
            Debug.LogError($"Failed to get end game view.",this);
        }

        if (!InstanceHandler.TryGetInstance(out GameViewManager gameViewManager))
        {
            Debug.LogError($"Failed to get game view manager.", this);
        }

        endGameView.SetWinner(winner);
        gameViewManager.ShowView<EndGameView>();
        Debug.Log($"Game has now ended! {winner} is our champion!");
    }
}