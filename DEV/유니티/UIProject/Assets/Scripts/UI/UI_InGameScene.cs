using UnityEngine;

public class UI_InGameScene : SceneUI
{
    [Header("Sub UI Elements")]
    [SerializeField] private UI_EmotionGauge emotionGaugeSubUI;
    // ⚠️ 새로 만든 체력바 SubUI를 에디터에서 연결할 칸 추가
    

    public override void Init()
    {
        // 인게임 메인 화면이 켜질 때 하위 고정 UI들을 일괄 초기화합니다.
        if (emotionGaugeSubUI != null)
        {
            emotionGaugeSubUI.Init();
            emotionGaugeSubUI.SetEmotionValue(50f); // 초기 감정 50%
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
