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
    [SerializeField] private float emotionReward = 10f; // 적 사망 시 수급할 감정 수치

    [Header("UI 연동 (선택 사항)")]
    [SerializeField] private Image hpBarFillImage;      // UI Image (Fill Amount)
    [SerializeField] private Slider hpBarSlider;        // UI Slider

    [Header("피격 / 사망 이벤트")]
    public UnityEvent OnTakeDamageEvent;                // 피격 시 실행될 이벤트
    public UnityEvent OnDieEvent;                       // 사망 시 추가 연출 이벤트

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
    /// 감정 게이지 수치(0~100)에 맞춰 체력 보정 적용 (1%당 0.1% 상승)
    /// </summary>
    private void ApplyEmotionStatModifier(float emotionValue)
    {
        if (IsDead) return;

        float modifier = 1.0f + (emotionValue * 0.001f);
        float hpRatio = maxHp > 0 ? currentHp / maxHp : 1f;

        maxHp = baseMaxHp * modifier;
        currentHp = maxHp * hpRatio;

        UpdateHpUI();
    }

    /// <summary>
    /// 데미지 적용 함수
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
            // 1. 감정 수치 증가
            UI_InGameScene.Instance.AddEmotion(emotionReward);

            // 2. 적 처치 시 데스 크리스탈(Death) 30 증가
            UI_InGameScene.Instance.AddDeath(30);

            // 3. 유니티 버전 호환성을 고려한 씬 전체 적 검사
#if UNITY_2023_1_OR_NEWER
            EnemyHP[] allEnemies = FindObjectsByType<EnemyHP>(FindObjectsSortMode.None);
#else
            EnemyHP[] allEnemies = FindObjectsOfType<EnemyHP>();
#endif

            // 살아있는 적(본인 제외, 아직 안 죽은 적) 개수 측정
            int aliveCount = 0;
            foreach (var enemy in allEnemies)
            {
                if (enemy != this && !enemy.IsDead)
                {
                    aliveCount++;
                }
            }

            // 모든 적이 처치되었으면 기억의 구슬(Memory) 1 증가
            if (aliveCount == 0)
            {
                UI_InGameScene.Instance.AddMemory(1);
                Debug.Log("스테이지의 모든 적 처치 완료! 기억의 구슬 +1");
            }
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