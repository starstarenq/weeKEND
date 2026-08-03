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

    /// <summary>
    /// 현재 감정 수치를 외부에서 확인할 때 사용하는 프로퍼티
    /// </summary>
    public float CurrentEmotion => _currentEmotion;

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
    /// 적을 처치하는 등 이벤트가 발생했을 때 호출하여 기존 감정 수치에 더해주는 함수
    /// </summary>
    public void AddEmotionValue(float amount)
    {
        SetEmotionValue(_currentEmotion + amount);
    }

    /// <summary>
    /// 감정 수치를 직접 변경할 때 호출하는 함수 (0 ~ 100 범위 제한)
    /// </summary>
    public void SetEmotionValue(float value)
    {
        _currentEmotion = Mathf.Clamp(value, 0f, 100f);

        // 초기화가 완료된 시점 이후부터 화면 UI를 업데이트
        if (_initAlready)
        {
            RefreshGauge();
        }
    }

    private void RefreshGauge()
    {
        // Get<T>를 통해 바인딩된 슬라이더와 텍스트를 꺼내와 조작합니다.
        Slider slider = Get<Slider>((int)Sliders.EmotionBarSlider);
        Text text = Get<Text>((int)Texts.EmotionValueText);

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