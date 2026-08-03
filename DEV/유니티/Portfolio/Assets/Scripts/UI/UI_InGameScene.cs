// UI_InGameScene.cs
using UnityEngine;

public class UI_InGameScene : SceneUI
{
    // [추가] 어디서든 쉽게 접근할 수 있는 싱글톤 인스턴스
    public static UI_InGameScene Instance { get; private set; }

    [SerializeField] private UI_EmotionGauge emotionGaugeSubUI;

    private void Awake()
    {
        // 싱글톤 초기화
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
            emotionGaugeSubUI.SetEmotionValue(38f); // 테스트용 초기값
        }
    }

    public void UpdateEmotion(float newValue)
    {
        if (emotionGaugeSubUI != null)
        {
            emotionGaugeSubUI.SetEmotionValue(newValue);
        }
    }

    /// <summary>
    /// [추가] 적 사망 등 이벤트 발생 시 호출할 감정 수치 증가 함수
    /// </summary>
    public void AddEmotion(float amount)
    {
        if (emotionGaugeSubUI != null)
        {
            emotionGaugeSubUI.AddEmotionValue(amount);
        }
    }
}