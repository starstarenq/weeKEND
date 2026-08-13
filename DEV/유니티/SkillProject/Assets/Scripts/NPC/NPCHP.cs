using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class NPCHP : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private float maxHp = 50f;
    private float currentHp;

    [Header("사망 페널티 설정")]
    [Tooltip("NPC 사망 시 감소시킬 감정 수치")]
    [SerializeField] private float emotionLoss = 15f;

    [Header("UI 연동 (선택 사항)")]
    [SerializeField] private Image hpBarFillImage;
    [SerializeField] private Slider hpBarSlider;

    [Header("피격 / 사망 이벤트")]
    public UnityEvent OnTakeDamageEvent;  // 피격 시 실행 (NPCChase 도망 연결)
    public UnityEvent OnDieEvent;         // 사망 시 연출 이벤트

    public bool IsDead { get; private set; } = false;

    private void Awake()
    {
        currentHp = maxHp;
        UpdateHpUI();
    }

    /// <summary>
    /// PlayerAttack 등 공격 스크립트에서 데미지를 가할 때 호출
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        if (IsDead || currentHp <= 0) return;

        currentHp -= damageAmount;
        currentHp = Mathf.Clamp(currentHp, 0f, maxHp);

        Debug.Log($"NPC {gameObject.name} 피격! 입은 데미지: {damageAmount}, 남은 체력: {currentHp}/{maxHp}");

        UpdateHpUI();

        // 피격 시 이벤트 실행 (NPCChase에서 도망 상태 전환)
        OnTakeDamageEvent?.Invoke();

        if (currentHp <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 사망 처리 및 감정 게이지 하락
    /// </summary>
    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        Debug.Log($"NPC {gameObject.name} 사망 - 감정 게이지 {emotionLoss} 하락!");

        // 인게임 UI 싱글톤을 찾아 감정 게이지 감소 (-emotionLoss)
        if (UI_InGameScene.Instance != null)
        {
            UI_InGameScene.Instance.AddEmotion(-emotionLoss);
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