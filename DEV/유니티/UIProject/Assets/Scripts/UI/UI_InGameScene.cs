using UnityEngine;

public class UI_InGameScene : SceneUI
{
    [Header("Sub UI Elements")]
    [SerializeField] private UI_EmotionGauge emotionGaugeSubUI;
    // ⚠️ 새로 만든 체력바 SubUI를 에디터에서 연결할 칸 추가


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
        UI_TraitBook traitBook = FindObjectOfType<UI_TraitBook>(true);
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
    /// 외부 매니저 클래스들이 캐릭터 체력을 깎을 때 이 브릿지 함수를 거쳐갑니다.
    /// </summary>

}
