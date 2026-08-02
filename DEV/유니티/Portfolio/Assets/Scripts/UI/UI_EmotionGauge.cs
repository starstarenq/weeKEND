using UnityEngine;
using UnityEngine.UI;

public class UI_EmotionGauge : SubUI
{
    // 유니티 캔버스 내 하위 오브젝트 이름과 정확히 일치시켜야 바인딩됩니다.
    enum Texts
    {
        EmotionValueText, // "현재 감정 32%" 텍스트 오브젝트 명
    }

    enum Sliders
    {
        EmotionBarSlider, // 슬라이더 오브젝트 명
    }
    enum Images
    {
        SadnessIconImage,   // 왼쪽 불행 이모티콘
        HappinessIconImage  // 오른쪽 행복 이모티콘
    }

    private float _currentEmotion = 50f; // 초기값 50% (중립)
    private bool _initAlready = false;
    public override void Init()
    {
        // 1. 해당 타입의 UI 요소를 이름 기준으로 자동 매핑
        Bind<Text>(typeof(Texts));
        Bind<Slider>(typeof(Sliders));
        Bind<Image>(typeof(Images));

        _initAlready = true;

        // 2. 초기 UI 리프레시
        RefreshGauge();
    }

    /// <summary>
    /// 외부 게임 매니저나 이벤트에서 플레이어 상태 변화 시 호출하는 함수
    /// </summary>
    public void SetEmotionValue(float value)
    {
        _currentEmotion = Mathf.Clamp(value, 0f, 100f);
        RefreshGauge();
    }

    private void RefreshGauge()
    {
        // Get<T>를 통해 바인딩된 슬라이더와 텍스트를 꺼내와 조작합니다.
        Slider slider = Get<Slider>((int)Sliders.EmotionBarSlider);
        Text text = Get<Text>((int)Texts.EmotionValueText);

        // ⚠️ 딕셔너리에서 이미지 컴포넌트들을 안전하게 꺼내옵니다.
        Image sadnessIcon = Get<Image>((int)Images.SadnessIconImage);
        Image happinessIcon = Get<Image>((int)Images.HappinessIconImage);


        if (slider != null) slider.value = _currentEmotion;
        if (text != null) text.text = $"현재 감정 {Mathf.RoundToInt(_currentEmotion)}%";
        if (sadnessIcon != null && happinessIcon != null)
        {
            // 현재는 에디터에서 넣은 기본 이미지가 그대로 유지됩니다.
        }
    }
}
