using Newtonsoft.Json;
using PurrNet;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainGameView : View
{
    [SerializeField] private TMP_Text healthText, spectatingPlayerName;
    [SerializeField] private CanvasGroup spectatingPlayerNameParent;
    [SerializeField] private Slider healthBarSlider, easeHealthBarSlider;
    [SerializeField] private TextMeshProUGUI RoundText;
    private void Awake()
    {
        InstanceHandler.RegisterInstance(this);
    }

    private void OnDisable()
    {
        InstanceHandler.UnregisterInstance<MainGameView>(); 
    }

    public override void OnHide()
    {

    }

    public override void OnShow()
    {

    }

    public void UpdateRoundText(int currentRound)
    {
        RoundText.text = $"ROUND {currentRound}/{InstanceHandler.GetInstance<DataCarrier>().maxRound}";
    }

    public void UpdateHealthBarInstant(int currentHealth, int maxHealth)
    {      
        healthBarSlider.value = (float)currentHealth / maxHealth; 
    }

    public void UpdateHealthBarSmooth(int currentHealth, int maxHealth)
    {
        StartCoroutine(UpdateHealthBarSmooth_Coroutine(currentHealth, maxHealth));
    }

    private IEnumerator UpdateHealthBarSmooth_Coroutine(int currentHealth, int maxHealth)
    {
        float startValue = easeHealthBarSlider.value;
        float endValue = (float)currentHealth / maxHealth;
        float duration = 0.5f; // Smooth animation duration
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            easeHealthBarSlider.value = Mathf.Lerp(startValue, endValue, elapsed / duration);
            yield return null;
        }
        easeHealthBarSlider.value = endValue; // Ensure exact final value
    }

    public void UpdateHealth(int health, int maxHealth)
    {
        if(health< 0)
        {
            health = 0;
        }
        healthText.text = $"{health}/{maxHealth}";
        UpdateHealthBarInstant(health, maxHealth);

        int valueToWidthScale = 3;
        if(healthBarSlider.GetComponent<RectTransform>().rect.width != maxHealth * valueToWidthScale)
        {
            healthBarSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(maxHealth * valueToWidthScale, healthBarSlider.GetComponent<RectTransform>().sizeDelta.y);
            easeHealthBarSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(maxHealth * valueToWidthScale, healthBarSlider.GetComponent<RectTransform>().sizeDelta.y);
        }
    }

    public void UpdateSpectatingPlayerName(string name)
    {
        spectatingPlayerName.text = name;
    }

    public void toggleSpectatingPlayerName(bool toggle)
    {
        spectatingPlayerNameParent.alpha = toggle ? 1 : 0;
    }
}
