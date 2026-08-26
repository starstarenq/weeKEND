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

    [Header("UI 컴포넌트 직접할당 (권장)")]
    [SerializeField] private TextMeshProUGUI targetNameText;
    [SerializeField] private Slider hpSlider;

    [Header("타이머 설정")]
    [SerializeField] private float hideDelay = 4.0f;
    private float lastHitTime;

    [Header("게이지 줄어드는 속도")]
    [SerializeField] private float fillSpeed = 5.0f; // 숫자가 클수록 빠르게 줄어듦

    private float targetMaxHp;
    private float targetCurrentHp;
    private float targetRatio = 0f;
    private bool hasTarget = false;

    private bool isInit = false;

    private void Awake()
    {
        Init();
    }

    public override void Init()
    {
        if (isInit) return;

        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Slider>(typeof(Sliders));

        if (targetNameText == null) targetNameText = Get<TextMeshProUGUI>((int)Texts.TargetNameText);
        if (hpSlider == null) hpSlider = Get<Slider>((int)Sliders.HPSlider);

        if (targetNameText == null) targetNameText = GetComponentInChildren<TextMeshProUGUI>(true);
        if (hpSlider == null) hpSlider = GetComponentInChildren<Slider>(true);

        if (hpSlider != null)
        {
            hpSlider.minValue = 0f;
            hpSlider.maxValue = 1f;
        }

        isInit = true;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!hasTarget)
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);
            return;
        }

        // 🎯 슬라이더 값을 목표 비율(targetRatio)로 부드럽게 감소시킴
        if (hpSlider != null)
        {
            hpSlider.value = Mathf.Lerp(hpSlider.value, targetRatio, Time.deltaTime * fillSpeed);
        }

        // 일정 시간이 지나면 UI 비활성화
        if (Time.time - lastHitTime > hideDelay)
        {
            hasTarget = false;
            gameObject.SetActive(false);
            return;
        }

        // 체력이 0 이하이고 게이지가 거의 다 줄어들었을 때 닫기
        if (targetCurrentHp <= 0 && hpSlider != null && hpSlider.value <= 0.01f)
        {
            hasTarget = false;
            gameObject.SetActive(false);
        }
    }

    public void UpdateTargetHP(string name, float currentHp, float maxHp)
    {
        // 1. 초기화 보장
        Init();

        // 2. 오브젝트를 먼저 활성화
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);

            // 처음 켜질 때는 이전 값에서 Lerp되지 않고 즉시 해당 체력 비율에서 시작하도록 설정
            if (hpSlider != null && maxHp > 0)
            {
                hpSlider.value = currentHp / maxHp;
            }
        }

        // 3. 목표 값 할당
        targetCurrentHp = Mathf.Clamp(currentHp, 0f, maxHp);
        targetMaxHp = maxHp;
        targetRatio = targetMaxHp > 0 ? targetCurrentHp / targetMaxHp : 0f;

        lastHitTime = Time.time;
        hasTarget = true;

        if (targetNameText != null)
        {
            targetNameText.text = name;
        }
    }
}