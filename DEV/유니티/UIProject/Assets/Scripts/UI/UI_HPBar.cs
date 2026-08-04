using UnityEngine;
using UnityEngine.UI;

public class UI_HPBar : SubUI
{
    enum Sliders
    {
        HpBarSlider
    }

    enum Texts
    {
        HpValueText
    }

    [Header("Player Settings")]
    [SerializeField] private string playerName = "주인공";

    private float _maxHp = 100f;
    private float _currentHp = 100f;
    private bool _initAlready = false;

    public override void Init()
    {
        if (_initAlready) return;

        Bind<Slider>(typeof(Sliders));
        Bind<Text>(typeof(Texts));

        _initAlready = true;
        RefreshHpUI();
    }

    public void UpdateHp(float currentHp, float maxHp)
    {
        _currentHp = currentHp;
        _maxHp = maxHp;
        RefreshHpUI();
    }

    private void RefreshHpUI()
    {
        Slider slider = Get<Slider>((int)Sliders.HpBarSlider);
        Text text = Get<Text>((int)Texts.HpValueText);

        if (slider != null)
        {
            slider.maxValue = _maxHp;
            slider.value = _currentHp;
        }

        // ⚠️ 강제로 데이터 브릿지 시점에 이름을 재입력하여 초기화 덮어쓰기를 원천 차단합니다.
        if (text != null)
        {
            text.text = playerName;
        }
    }
}
