using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class EnemyHP : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private float baseMaxHp = 100f;
    private float maxHp;
    private float currentHp;

    public float MaxHP => maxHp;
    public float CurrentHP => currentHp;

    [Header("사망 보상 설정")]
    [SerializeField] private float emotionReward = 10f;

    [Header("UI 연동 (선택 사항)")]
    [SerializeField] private Image hpBarFillImage;
    [SerializeField] private Slider hpBarSlider;

    [Header("피격 / 사망 이벤트")]
    public UnityEvent OnTakeDamageEvent;
    public UnityEvent OnDieEvent;

    public bool IsDead { get; private set; } = false;

    private void Awake()
    {
        maxHp = baseMaxHp;
        currentHp = maxHp;
        UpdateHpUI();
    }

    private void OnEnable()
    {
        UI_EmotionGauge.OnEmotionChanged += ApplyEmotionStatModifier;

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

    private void ApplyEmotionStatModifier(float emotionValue)
    {
        if (IsDead) return;

        float modifier = 1.0f + (emotionValue * 0.001f);
        float hpRatio = maxHp > 0 ? currentHp / maxHp : 1f;

        maxHp = baseMaxHp * modifier;
        currentHp = maxHp * hpRatio;

        UpdateHpUI();
    }

    public void TakeDamage(float damageAmount)
    {
        if (IsDead || currentHp <= 0) return;

        currentHp -= damageAmount;
        currentHp = Mathf.Clamp(currentHp, 0f, maxHp);

        Debug.Log($"{gameObject.name} 피격! 입은 데미지: {damageAmount}, 남은 체력: {currentHp}/{maxHp}");

        UpdateHpUI();

        // 🎯 공통 상단 체력바(UI_HPBar)에 현재 오브젝트 이름과 체력 전달
        if (UI_InGameScene.Instance != null)
        {
            UI_InGameScene.Instance.ShowHPBar(gameObject.name, currentHp, maxHp);
        }

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
            UI_InGameScene.Instance.AddEmotion(emotionReward);
            UI_InGameScene.Instance.AddDeath(30);

#if UNITY_2023_1_OR_NEWER
            EnemyHP[] allEnemies = FindObjectsByType<EnemyHP>(FindObjectsSortMode.None);
#else
            EnemyHP[] allEnemies = FindObjectsOfType<EnemyHP>();
#endif

            int aliveCount = 0;
            foreach (var enemy in allEnemies)
            {
                if (enemy != this && !enemy.IsDead)
                {
                    aliveCount++;
                }
            }

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