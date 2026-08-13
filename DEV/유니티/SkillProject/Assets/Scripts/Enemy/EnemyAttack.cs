using UnityEngine;

[RequireComponent(typeof(EnemyHP))]
public class EnemyAttack : MonoBehaviour
{
    [Header("공격 설정")]
    [SerializeField] private float baseAttackDamage = 10f; // 기본 몬스터 공격 데미지
    public float attackDamage;                              // 실시간 보정된 데미지
    public float attackRate = 1.5f;                         // 공격 주기 (초)[cite: 2]
    public float attackRange = 3.5f;                        // 공격 가능 최대 거리[cite: 2]

    private float lastAttackTime = 0f;
    private EnemyHP enemyHp;

    private void Awake()
    {
        attackDamage = baseAttackDamage;
    }

    private void Start()
    {
        enemyHp = GetComponent<EnemyHP>();
    }

    private void OnEnable()
    {
        UI_EmotionGauge.OnEmotionChanged += ApplyEmotionStatModifier;

        UI_EmotionGauge gauge = FindAnyObjectByType<UI_EmotionGauge>();
        if (gauge != null)
        {
            ApplyEmotionStatModifier(gauge.CurrentEmotion);
        }
    }

    private void OnDisable()
    {
        UI_EmotionGauge.OnEmotionChanged -= ApplyEmotionStatModifier;
    }

    /// <summary>
    /// 감정 게이지 수치(0~100)에 맞춰 공격력 보정 적용
    /// </summary>
    private void ApplyEmotionStatModifier(float emotionValue)
    {
        // 1%당 0.1% 하락 -> 100% 시 10% 감소
        float statReductionRatio = (emotionValue * 0.001f);
        float modifier = Mathf.Clamp(1.0f - statReductionRatio, 0.1f, 1.0f);

        attackDamage = baseAttackDamage * modifier;
    }

    public void TryAttack(Transform target)
    {
        if ((enemyHp != null && enemyHp.IsDead) || target == null) return;

        if (Time.time >= lastAttackTime + attackRate)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= attackRange)
            {
                PerformAttack(target);
                lastAttackTime = Time.time;
            }
        }
    }

    private void PerformAttack(Transform target)
    {
        Debug.Log($"{gameObject.name}가 플레이어를 공격했습니다! (적용 데미지: {attackDamage})");

        PlayerHP playerHp = target.GetComponent<PlayerHP>();
        if (playerHp != null)
        {
            playerHp.TakeDamage(attackDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}