using UnityEngine;
using PurrNet.StateMachine;
using System.Collections;
using PurrNet;
using System.Collections.Generic;
public class RoundEndState : StateNode
{
    [SerializeField] private int amountOfRounds = 3;
    [SerializeField] private StateNode spawningState;

    public int _roundCount = 0;
    private WaitForSeconds _delay = new(3);

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!asServer)
        {
            return;
        }

        Debug.Log("Round has ended with no winner!");

        CheckForGameEnd();
    }


    private void CheckForGameEnd()
    {
        _roundCount++;

        if (_roundCount >= InstanceHandler.GetInstance<DataCarrier>().maxRound)
        {
            machine.Next();
            return;
        }

        StartCoroutine(DelayNextState());
    }

    private IEnumerator DelayNextState()
    {
        yield return _delay;
        
        machine.SetState(spawningState);
    }
}
