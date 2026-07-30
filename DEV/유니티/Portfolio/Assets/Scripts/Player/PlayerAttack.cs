using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("부채꼴 공격 설정")]
    public float attackRange = 3.0f;     // 부채꼴의 반지름 (최대 사거리)
    [Range(0, 360)]
    public float attackAngle = 90f;      // 부채꼴의 총 각도 (예: 90도면 정면 기준 좌우 45도씩)
    public float attackDamage = 20f;     // 공격 데미지
    public LayerMask enemyLayer;         // 몬스터들이 속한 레이어 (Enemy)

    void Update()
    {
        if (CheckAttackInput())
        {
            PerformAttack();
        }
    }

    bool CheckAttackInput()
    {
        if (Keyboard.current == null || Mouse.current == null) return false;

        return Keyboard.current.qKey.wasPressedThisFrame ||
               Mouse.current.leftButton.wasPressedThisFrame ||
               Keyboard.current.digit1Key.wasPressedThisFrame ||
               Keyboard.current.digit2Key.wasPressedThisFrame ||
               Keyboard.current.digit3Key.wasPressedThisFrame ||
               Keyboard.current.digit4Key.wasPressedThisFrame ||
               Keyboard.current.digit5Key.wasPressedThisFrame;
    }

    void PerformAttack()
    {
        Debug.Log("부채꼴 범위 공격 개시!");

        // 1. 먼저 플레이어 주변 반지름(attackRange) 내의 모든 콜라이더를 1차 검색 (원형 1차 필터링)
        Collider[] targetsInMinusRadius = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        foreach (Collider enemyCollider in targetsInMinusRadius)
        {
            // 플레이어에서 적을 향하는 방향 벡터 계산 (Y축 높이 차이로 인한 왜곡 방지 위해 y는 0으로)
            Vector3 directionToTarget = (enemyCollider.transform.position - transform.position);
            directionToTarget.y = 0;

            // 2. 플레이어 정면 바라보는 방향과 적을 향한 방향 사이의 각도 계산
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget.normalized);

            // 3. 계산된 각도가 설정한 부채꼴 각도의 절반(좌/우 범위) 이내인지 확인
            if (angleToTarget <= attackAngle / 2f)
            {
                EnemyChase monster = enemyCollider.GetComponent<EnemyChase>();
                if (monster != null)
                {
                    // 부채꼴 범위 안에 있는 적만 피격 및 추격 활성화
                    monster.TakeDamage(attackDamage);
                }
            }
        }
    }

    // [중요] 에디터 씬 뷰에서 부채꼴 범위를 눈으로 확인할 수 있게 시각화합니다.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        // 원의 테두리 선 그리기
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 부채꼴의 좌측 끝과 우측 끝 경계선 시각화
        Vector3 leftBoundary = Quaternion.Euler(0, -attackAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, attackAngle / 2f, 0) * transform.forward;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * attackRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * attackRange);
    }
}
