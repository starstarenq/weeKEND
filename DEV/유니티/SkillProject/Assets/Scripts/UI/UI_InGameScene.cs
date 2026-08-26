using UnityEngine;

public class UI_InGameScene : SceneUI
{
    public static UI_InGameScene Instance { get; private set; }

    [SerializeField] private UI_EmotionGauge emotionGaugeSubUI;
    [SerializeField] private UI_Currency currencySubUI;
    [SerializeField] private UI_HPBar hpBarSubUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void Init()
    {
        if (emotionGaugeSubUI != null)
        {
            emotionGaugeSubUI.Init();
            emotionGaugeSubUI.SetEmotionValue(50f);
        }

        if (currencySubUI != null)
        {
            currencySubUI.Init();
            currencySubUI.SetDeath(0);
            currencySubUI.SetMemory(0);
        }

        if (hpBarSubUI != null)
        {
            hpBarSubUI.Init();
        }

        UI_TraitBook traitBook = FindAnyObjectByType<UI_TraitBook>(FindObjectsInactive.Include);
        if (traitBook != null)
        {
            traitBook.Init();
            traitBook.gameObject.SetActive(false);
        }
    }

    public void ShowHPBar(string name, float currentHp, float maxHp)
    {
        if (hpBarSubUI != null)
        {
            hpBarSubUI.UpdateTargetHP(name, currentHp, maxHp);
        }
        else
        {
            Debug.LogWarning("UI_InGameScene: hpBarSubUI가 인스펙터에 연결되어 있지 않습니다.");
        }
    }

    public void UpdateEmotion(float newValue)
    {
        if (emotionGaugeSubUI != null)
        {
            emotionGaugeSubUI.SetEmotionValue(newValue);
        }
    }

    public void AddEmotion(float amount)
    {
        if (emotionGaugeSubUI != null)
        {
            emotionGaugeSubUI.AddEmotionValue(amount);
        }
    }

    public void AddDeath(int amount)
    {
        if (currencySubUI != null)
        {
            currencySubUI.AddDeath(amount);
        }
    }

    public void AddMemory(int amount)
    {
        if (currencySubUI != null)
        {
            currencySubUI.AddMemory(amount);
        }
    }
}