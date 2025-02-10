using PurrNet;
using PurrNet.Modules;
using PurrNet.StateMachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndState : StateNode
{
    [PurrScene] public string sceneToChange;
    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!asServer)
        {
            return;
        }

        ShowWinnerOnEveryone();
    }

    [ObserversRpc(runLocally:true)]
    public void ShowWinnerOnEveryone()
    {
        if (!InstanceHandler.TryGetInstance(out ScoreManager scoreManager))
        {
            Debug.LogError($"GameEndState failed to get scoremanager!", this);
            return;
        }

        var winner = scoreManager.GetWinner();
        if (winner == default)
        {
            Debug.LogError($"GameEndState failed to get winner!", this);
            return;
        }

        if (!InstanceHandler.TryGetInstance(out EndGameView endGameView))
        {
            Debug.LogError($"Failed to get end game view.", this);
        }

        if (!InstanceHandler.TryGetInstance(out GameViewManager gameViewManager))
        {
            Debug.LogError($"Failed to get game view manager.", this);
        }

        Debug.Log($"Game has now ended! {winner} is our champion!");
        endGameView.SetWinner(winner);
        gameViewManager.ShowView<EndGameView>();

        BackToMenu();
    }

    public void BackToMenu()
    {
        StartCoroutine(BackToMenuCoroutine());
    }

    private IEnumerator BackToMenuCoroutine()
    {
        yield return new WaitForSeconds(3f);
        PurrSceneSettings settings = new()
        {
            isPublic = true,
            mode = LoadSceneMode.Single
        };
        networkManager.sceneModule.LoadSceneAsync(sceneToChange, settings);

    }

}