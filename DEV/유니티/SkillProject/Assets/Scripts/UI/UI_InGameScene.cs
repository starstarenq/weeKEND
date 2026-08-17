using UnityEngine;

public class UI_InGameScene : SceneUI
{
    public static UI_InGameScene Instance { get; private set; }

    [SerializeField] private UI_EmotionGauge emotionGaugeSubUI;
    [SerializeField] private UI_Currency currencySubUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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

        // [수정된 부분] SetCurrency 대신 SetDeath, SetMemory로 초기화
        if (currencySubUI != null)
        {
            currencySubUI.Init();
            currencySubUI.SetDeath(0);
            currencySubUI.SetMemory(0);
        }

        UI_TraitBook traitBook = FindAnyObjectByType<UI_TraitBook>(FindObjectsInactive.Include);
        if (traitBook != null)
        {
            traitBook.Init();
            traitBook.gameObject.SetActive(false);
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

    // [추가된 부분] 외부에서 Death/Memory 재화를 조작하는 연동 메서드
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