using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class EnemyHP : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private float baseMaxHp = 100f; // 기본 최대 체력
    private float maxHp;
    private float currentHp;

    [Header("사망 보상 설정")]
    [SerializeField] private float emotionReward = 10f; // 적 사망 시 수급할 감정 수치[cite: 1]

    [Header("UI 연동 (선택 사항)")]
    [SerializeField] private Image hpBarFillImage;      // UI Image (Fill Amount)[cite: 1]
    [SerializeField] private Slider hpBarSlider;        // UI Slider[cite: 1]

    [Header("피격 / 사망 이벤트")]
    public UnityEvent OnTakeDamageEvent;                // 피격 시 실행될 이벤트[cite: 1]
    public UnityEvent OnDieEvent;                       // 사망 시 추가 연출 이벤트[cite: 1]

    public bool IsDead { get; private set; } = false;

    private void Awake()
    {
        maxHp = baseMaxHp;
        currentHp = maxHp;
        UpdateHpUI();
    }

    private void OnEnable()
    {
        // 감정 게이지 변경 이벤트 구독
        UI_EmotionGauge.OnEmotionChanged += ApplyEmotionStatModifier;

        // 현재 감정 수치가 존재한다면 초기 적용
        if (UI_InGameScene.Instance != null)
        {
            UI_EmotionGauge gauge = FindAnyObjectByType<UI_EmotionGauge>();
            if (gauge != null)
            {
                ApplyEmotionStatModifier(gauge.CurrentEmotion);
            }
        }
    }

    private void OnDisable()
    {
        UI_EmotionGauge.OnEmotionChanged -= ApplyEmotionStatModifier;
    }

    /// <summary>
    /// 감정 게이지 수치(0~100)에 맞춰 체력 보정 적용 (1%당 0.1% 스탯 하락 = 100% 시 10% 하락)
    /// </summary>
    private void ApplyEmotionStatModifier(float emotionValue)
    {
        if (IsDead) return;

        // 1%당 0.1% 하락 -> 감정 100%일 때 10%(0.1) 감소 (multiplier = 0.9)
        float statReductionRatio = (emotionValue * 0.001f);
        float modifier = Mathf.Clamp(1.0f - statReductionRatio, 0.1f, 1.0f);

        // 이전 최대 체력 대비 현재 체력 비율 유지
        float hpRatio = maxHp > 0 ? currentHp / maxHp : 1f;

        maxHp = baseMaxHp * modifier;
        currentHp = maxHp * hpRatio;

        UpdateHpUI();
    }

    /// <summary>
    /// PlayerAttack 등 공격 스크립트에서 데미지를 가할 때 호출[cite: 1]
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        if (IsDead || currentHp <= 0) return;

        currentHp -= damageAmount;
        currentHp = Mathf.Clamp(currentHp, 0f, maxHp);

        Debug.Log($"{gameObject.name} 피격! 입은 데미지: {damageAmount}, 남은 체력: {currentHp}/{maxHp}");

        UpdateHpUI();

        OnTakeDamageEvent?.Invoke();

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        Debug.Log($"{gameObject.name} 사망.");

        if (UI_InGameScene.Instance != null)
        {
            UI_InGameScene.Instance.AddEmotion(emotionReward); // 감정 수치 증가
        }

        OnDieEvent?.Invoke();
        Destroy(gameObject);
    }

    private void UpdateHpUI()
    {
        float ratio = maxHp > 0 ? currentHp / maxHp : 0f;

        if (hpBarFillImage != null) hpBarFillImage.fillAmount = ratio;
        if (hpBarSlider != null) hpBarSlider.value = ratio;
    }
}