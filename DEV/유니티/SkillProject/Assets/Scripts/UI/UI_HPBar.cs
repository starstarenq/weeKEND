using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_HPBar : SubUI
{
    enum Texts
    {
        TargetNameText
    }

    enum Sliders
    {
        HPSlider
    }

    [Header("UI 컴포넌트 직접할당 (선택 사항)")]
    [SerializeField] private TextMeshProUGUI targetNameText;
    [SerializeField] private Slider hpSlider;

    [Header("타이머 설정")]
    [SerializeField] private float hideDelay = 4.0f; // 피격 후 UI 유지 시간
    private float lastHitTime;

    private float targetMaxHp;
    private float targetCurrentHp;
    private bool hasTarget = false;

    public override void Init()
    {
        // UIBase 자동 바인딩 시도
        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Slider>(typeof(Sliders));

        // 직접 할당 안 되어있을 경우 자동 바인딩 검색 결과 적용
        if (targetNameText == null) targetNameText = Get<TextMeshProUGUI>((int)Texts.TargetNameText);
        if (hpSlider == null) hpSlider = Get<Slider>((int)Sliders.HPSlider);

        // 예외 대비 안전 탐색
        if (targetNameText == null) targetNameText = GetComponentInChildren<TextMeshProUGUI>(true);
        if (hpSlider == null) hpSlider = GetComponentInChildren<Slider>(true);

        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!hasTarget)
        {
            gameObject.SetActive(false);
            return;
        }

        // 일정 시간 피격이 없으면 UI 비활성화
        if (Time.time - lastHitTime > hideDelay)
        {
            hasTarget = false;
            gameObject.SetActive(false);
            return;
        }

        // 슬라이더 값 지속 반영 (0 ~ 1)
        if (hpSlider != null && targetMaxHp > 0)
        {
            hpSlider.value = targetCurrentHp / targetMaxHp;
        }

        // 체력이 0 이하일 경우 닫기
        if (targetCurrentHp <= 0)
        {
            hasTarget = false;
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 공격받은 대상의 이름 및 체력 수치 갱신
    /// </summary>
    public void UpdateTargetHP(string name, float currentHp, float maxHp)
    {
        Init();

        targetCurrentHp = Mathf.Clamp(currentHp, 0f, maxHp);
        targetMaxHp = maxHp;
        lastHitTime = Time.time;
        hasTarget = true;

        // 이름 텍스트 변경
        if (targetNameText != null)
        {
            targetNameText.text = name;
        }

        // 슬라이더 체력 수치 변경
        if (hpSlider != null && targetMaxHp > 0)
        {
            hpSlider.minValue = 0f;
            hpSlider.maxValue = 1f;
            hpSlider.value = targetCurrentHp / targetMaxHp;
        }

        gameObject.SetActive(true);
    }
}