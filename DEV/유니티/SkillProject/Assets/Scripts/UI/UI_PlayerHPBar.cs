using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_PlayerHPBar : SubUI
{
    enum Sliders
    {
        PlayerHPSlider
    }

    enum Texts
    {
        PlayerHPText
    }

    [Header("UI 컴포넌트 직접할당 (권장)")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("게이지 줄어드는 속도")]
    [SerializeField] private float fillSpeed = 5.0f; // 숫자가 클수록 빠르게 변경됨

    private float targetRatio = 1.0f;
    private float displayHp = 100f;
    private float targetMaxHp = 100f;

    private bool isInit = false;

    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        if (isInit) return;

        Bind<Slider>(typeof(Sliders));
        Bind<TextMeshProUGUI>(typeof(Texts));

        if (hpSlider == null) hpSlider = Get<Slider>((int)Sliders.PlayerHPSlider);
        if (hpText == null) hpText = Get<TextMeshProUGUI>((int)Texts.PlayerHPText);

        if (hpSlider == null) hpSlider = GetComponentInChildren<Slider>(true);
        if (hpText == null) hpText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (hpSlider != null)
        {
            hpSlider.minValue = 0f;
            hpSlider.maxValue = 1f;
        }

        isInit = true;
    }

    private void Update()
    {
        if (hpSlider == null) return;

        // 슬라이더 바를 부드럽게 목표 수치로 이동
        hpSlider.value = Mathf.Lerp(hpSlider.value, targetRatio, Time.deltaTime * fillSpeed);

        // 텍스트 수치도 부드럽게 감소
        displayHp = Mathf.Lerp(displayHp, targetRatio * targetMaxHp, Time.deltaTime * fillSpeed);

        if (hpText != null)
        {
            hpText.text = $"{Mathf.CeilToInt(displayHp)} / {Mathf.CeilToInt(targetMaxHp)}";
        }
    }

    /// <summary>
    /// 외부(PlayerHP)에서 피격 시 목표 체력값 전달
    /// </summary>
    public void UpdateHP(float currentHp, float maxHp)
    {
        Init();

        targetMaxHp = maxHp;
        float clampedHp = Mathf.Clamp(currentHp, 0f, maxHp);
        targetRatio = maxHp > 0 ? clampedHp / maxHp : 0f;
    }
}