using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class EnemyHP : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private float maxHp = 100f;
    private float currentHp;

    [Header("사망 보상 설정")]
    [SerializeField] private float emotionReward = 10f; // 적 사망 시 수급할 감정 수치

    [Header("UI 연동 (선택 사항)")]
    [SerializeField] private Image hpBarFillImage;      // UI Image (Fill Amount)
    [SerializeField] private Slider hpBarSlider;        // UI Slider

    [Header("피격 / 사망 이벤트")]
    public UnityEvent OnTakeDamageEvent;                // 피격 시 실행될 이벤트 (EnemyChase의 추격 전환 연동 가능)
    public UnityEvent OnDieEvent;                       // 사망 시 추가 연출 이벤트

    public bool IsDead { get; private set; } = false;

    private void Awake()
    {
        currentHp = maxHp;
        UpdateHpUI();
    }

    /// <summary>
    /// PlayerAttack 등 공격 스크립트에서 데미지를 가할 때 호출[cite: 3, 5]
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        if (IsDead || currentHp <= 0) return;

        currentHp -= damageAmount;
        currentHp = Mathf.Clamp(currentHp, 0f, maxHp);

        Debug.Log($"{gameObject.name} 피격! 입은 데미지: {damageAmount}, 남은 체력: {currentHp}/{maxHp}");

        UpdateHpUI();

        // 피격 시 등록된 이벤트 실행 (예: EnemyChase 추격 전환)
        OnTakeDamageEvent?.Invoke();

        if (currentHp <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 사망 처리 및 감정 게이지 보상 지급[cite: 5]
    /// </summary>
    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        Debug.Log($"{gameObject.name} 사망.");

        // 인게임 UI 싱글톤을 찾아 감정 게이지 증가[cite: 5]
        if (UI_InGameScene.Instance != null)
        {
            UI_InGameScene.Instance.AddEmotion(emotionReward);
        }

        OnDieEvent?.Invoke();
        Destroy(gameObject);
    }

    private void UpdateHpUI()
    {
        float ratio = currentHp / maxHp;

        if (hpBarFillImage != null) hpBarFillImage.fillAmount = ratio;
        if (hpBarSlider != null) hpBarSlider.value = ratio;
    }
}