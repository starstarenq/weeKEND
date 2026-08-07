using UnityEngine;

[RequireComponent(typeof(EnemyHP))]
public class EnemyAttack : MonoBehaviour
{
    [Header("공격 설정")]
    public float attackDamage = 10f;       // 몬스터 공격 데미지
    public float attackRate = 1.5f;         // 공격 주기 (초)
    public float attackRange = 3.5f;        // 공격 가능 최대 거리

    private float lastAttackTime = 0f;
    private EnemyHP enemyHp;

    private void Start()
    {
        enemyHp = GetComponent<EnemyHP>();
    }

    /// <summary>
    /// EnemyChase에서 사거리 진입 시 호출
    /// </summary>
    public void TryAttack(Transform target)
    {
        // 몬스터가 이미 사망했거나 타겟이 없는 경우 공격 불가
        if ((enemyHp != null && enemyHp.IsDead) || target == null) return;

        // 쿨타임 검증
        if (Time.time >= lastAttackTime + attackRate)
        {
            // 거리 재검증
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
        Debug.Log($"{gameObject.name}가 플레이어를 공격했습니다! (데미지: {attackDamage})");

        // PlayerHP 연동 (플레이어의 3번 슬픔 스킬 무효화 배리어 연동)
        PlayerHP playerHp = target.GetComponent<PlayerHP>();
        if (playerHp != null)
        {
            playerHp.TakeDamage(attackDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 공격 사거리 시각화 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}