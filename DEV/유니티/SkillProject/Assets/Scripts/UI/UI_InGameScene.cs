using UnityEngine;

public class UI_InGameScene : SceneUI
{
    public static UI_InGameScene Instance { get; private set; }

    [SerializeField] private UI_EmotionGauge emotionGaugeSubUI;
    [SerializeField] private UI_Currency currencySubUI;
    [SerializeField] private UI_EnemyHPBar enemyHPBarSubUI; // 🎯 상단 적 체력바 추가

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

        if (currencySubUI != null)
        {
            currencySubUI.Init();
            currencySubUI.SetDeath(0);
            currencySubUI.SetMemory(0);
        }

        if (enemyHPBarSubUI != null)
        {
            enemyHPBarSubUI.Init();
        }

        UI_TraitBook traitBook = FindAnyObjectByType<UI_TraitBook>(FindObjectsInactive.Include);
        if (traitBook != null)
        {
            traitBook.Init();
            traitBook.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 공격받은 적의 체력바를 상단 UI로 출력
    /// </summary>
    public void ShowEnemyHP(EnemyHP enemy)
    {
        if (enemyHPBarSubUI != null)
        {
            enemyHPBarSubUI.TargetEnemy(enemy);
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