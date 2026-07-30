using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("부채꼴 공격 범위 설정")]
    public float attackRange = 3.0f;
    [Range(0, 360)] public float attackAngle = 90f;
    public LayerMask enemyLayer;

    [Header("콤보 설정 (타수별 데미지)")]
    private int comboStep = 0;           // 현재 몇 타째인지 (0: 대기, 1: 1타, 2: 2타, 3: 3타)
    private float lastAttackTime;        // 마지막으로 공격한 시간 체크

    public float attackDelay = 0.15f;    // [조건] 각 공격간 최소 딜레이 (0.15초)
    public float comboResetTime = 1.0f;  // 이 시간 내에 다음 클릭을 안 하면 1타로 초기화

    void Update()
    {
        // 마지막 공격 후 오랜 시간이 지나면 콤보 단계를 자동으로 초기화
        if (comboStep > 0 && Time.time - lastAttackTime > comboResetTime)
        {
            ResetCombo();
        }

        // 오직 마우스 좌클릭으로만 기본 콤보 공격이 나가도록 세팅 (기획서 마우스 좌클릭 반영)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryComboAttack();
        }
    }

    void TryComboAttack()
    {
        // [조건] 공격간 최소 딜레이(0.15초)가 지나지 않았다면 클릭 입력 무시
        if (Time.time - lastAttackTime < attackDelay) return;

        // 콤보 단계 한 칸 전진 (3타 다음엔 다시 1타로 돌아옴)
        comboStep++;
        if (comboStep > 3) comboStep = 1;

        lastAttackTime = Time.time;
        ExecuteAttack(comboStep);
    }

    void ExecuteAttack(int currentStep)
    {
        float damage = 0f;
        bool shouldStun = false;

        // [조건] 타수별 데이터 처리
        switch (currentStep)
        {
            case 1:
                damage = 10f;
                Debug.Log("★ 콤보 1타 발동! (데미지: 10)");
                break;
            case 2:
                damage = 15f;
                Debug.Log("★★ 콤보 2타 발동! (데미지: 15)");
                break;
            case 3:
                damage = 30f;
                shouldStun = true; // [조건] 3타에는 스턴 여부 활성화
                Debug.Log("★★★ 콤보 3타 막타 발동!! (데미지: 30 + 0.5초 스턴)");
                break;
        }

        // 부채꼴 범위 검색 연산
        Collider[] targetsInMinusRadius = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        foreach (Collider enemyCollider in targetsInMinusRadius)
        {
            Vector3 directionToTarget = (enemyCollider.transform.position - transform.position);
            directionToTarget.y = 0;

            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget.normalized);

            if (angleToTarget <= attackAngle / 2f)
            {
                EnemyChase monster = enemyCollider.GetComponent<EnemyChase>();
                if (monster != null)
                {
                    // 몬스터에게 데미지와 스턴 여부 동시 전달
                    monster.TakeDamage(damage, shouldStun);
                }
            }
        }
    }

    void ResetCombo()
    {
        comboStep = 0;
        Debug.Log("콤보 타이밍을 놓쳐 연속기가 초기화되었습니다.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Vector3 leftBoundary = Quaternion.Euler(0, -attackAngle / 2f, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, attackAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * attackRange);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * attackRange);
    }
}
