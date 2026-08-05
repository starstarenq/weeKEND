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
        // 인게임 메인 화면이 켜질 때 하위 고정 UI들을 초기화합니다.
        if (emotionGaugeSubUI != null)
        {
            emotionGaugeSubUI.Init();
            emotionGaugeSubUI.SetEmotionValue(50f);
        }

        // ⚠️ 핵심 수정: 도감의 Init()만 미리 실행해 두고, 
        // 오브젝트의 활성화 상태(꺼진 상태)는 그대로 유지되도록 처리합니다.
        UI_TraitBook traitBook = FindAnyObjectByType<UI_TraitBook>(FindObjectsInactive.Include);
        if (traitBook != null)
        {
            traitBook.Init();
            // 확실하게 시작할 때는 꺼지도록 강제 고정합니다.
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