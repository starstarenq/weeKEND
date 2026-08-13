using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_EmotionGauge : SubUI
{
    // [추가] 감정 게이지가 변경될 때 현재 감정 수치를 전달하는 이벤트
    public static event Action<float> OnEmotionChanged;

    enum Texts
    {
        EmotionValueText,
    }

    enum Sliders
    {
        EmotionBarSlider,
    }
    enum Images
    {
        SadnessIconImage,
        HappinessIconImage
    }

    private float _currentEmotion = 50f; // 초기값 50%
    private bool _initAlready = false;

    public float CurrentEmotion => _currentEmotion;

    public override void Init()
    {
        Bind<Text>(typeof(Texts));
        Bind<Slider>(typeof(Sliders));
        Bind<Image>(typeof(Images));

        _initAlready = true;

        RefreshGauge();
    }

    public void AddEmotionValue(float amount)
    {
        SetEmotionValue(_currentEmotion + amount);
    }

    public void SetEmotionValue(float value)
    {
        _currentEmotion = Mathf.Clamp(value, 0f, 100f);

        if (_initAlready)
        {
            RefreshGauge();
        }

        // [추가] 감정 게이지가 변경될 때마다 이벤트 호출
        OnEmotionChanged?.Invoke(_currentEmotion);
    }

    private void RefreshGauge()
    {
        Slider slider = Get<Slider>((int)Sliders.EmotionBarSlider);
        Text text = Get<Text>((int)Texts.EmotionValueText);

        Image sadnessIcon = Get<Image>((int)Images.SadnessIconImage);
        Image happinessIcon = Get<Image>((int)Images.HappinessIconImage);

        if (slider != null) slider.value = _currentEmotion;
        if (text != null) text.text = $"현재 감정 {Mathf.RoundToInt(_currentEmotion)}%";
    }
}