using PurrNet;
using System.Collections;
using TMPro;
using UnityEngine;

public class EndGameView : View
{
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private TMP_Text winnerText;

    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    private void OnDestroy()
    {
        InstanceHandler.UnregisterInstance<EndGameView>();
    }

    public void SetWinner(PlayerID winner)
    {
        winnerText.text = $"{winner} has won the match!";
        //StartCoroutine(FadeScreen(true));
    }

    private IEnumerator FadeScreen(bool fadeIn)
    {
        float t = 0;
        while (t < fadeDuration)
        {
            canvasGroup.alpha = fadeIn? t/fadeDuration: (1 - t) /fadeDuration;

            t+= Time.deltaTime;
            yield return null;
        }
    }
    public override void OnHide()
    {

    }

    public override void OnShow()
    {

    }

}
