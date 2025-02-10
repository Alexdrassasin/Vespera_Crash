using DG.Tweening;
using PurrNet;
using PurrNet.StateMachine;
using System.Collections;
using TMPro;
using UnityEngine;

public class WaitForPlayersState : StateNode
{
    [SerializeField] private int minPlayers = 2;
    [SerializeField] private TextMeshProUGUI waitingText;
    [SerializeField] private float dotInterval = 0.5f;
    public CanvasGroup _waitingPlayerCanvas;

    private string baseText = "Waiting for other player";
    private int dotCount = 0;

    private Coroutine animationCoroutine;

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<WaitForPlayersState>();
    }

    private void Start()
    {
        _waitingPlayerCanvas.alpha = 1f;
        _waitingPlayerCanvas.gameObject.SetActive(true);
    }

    public override void Enter(bool asServer)
    {
        base.Enter(asServer);

        if (!asServer)
        {
            return;
        }

        StartCoroutine(WaitForPlayers());
    }

    IEnumerator WaitForPlayers()
    {
        if (waitingText)
        {
            animationCoroutine = StartCoroutine(AnimateDots());
        }

        while (networkManager.players.Count < minPlayers)
        {
            yield return null;
        }

        StopAnimation();

        _waitingPlayerCanvas.DOFade(0, 0.5f).OnComplete(() =>
        {
            _waitingPlayerCanvas.gameObject.SetActive(false);
            machine.Next();
        });
    }



    IEnumerator AnimateDots()
    {
        while (true) // Loop indefinitely
        {
            string dots = "";
            for (int i = 0; i < dotCount; i++)
            {
                dots += ".";
            }

            waitingText.text = baseText + dots;

            dotCount++;
            if (dotCount > 3) // Reset dot count after 3 dots
            {
                dotCount = 0;
            }

            yield return new WaitForSeconds(dotInterval);
        }
    }

    // Optional: Stop the animation
    public void StopAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
            waitingText.text = baseText; // Reset to the base text
        }
    }
}
