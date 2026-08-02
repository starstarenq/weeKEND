using UnityEngine;

public class UI_InGameScene : SceneUI
{
    // [SerializeField]를 붙여야 유니티 인스펙터 창에 드래그할 수 있는 빈 칸이 나타납니다.
    [SerializeField] private UI_EmotionGauge emotionGaugeSubUI;

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
}
