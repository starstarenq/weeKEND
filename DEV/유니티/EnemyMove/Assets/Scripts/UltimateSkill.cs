using UnityEngine;
using System.Collections;

public class UltimateSkill : MonoBehaviour
{
    [Header("입력 키")]
    [SerializeField] private KeyCode ultimateKey = KeyCode.Q;

    [Header("스킬 수치 세팅")]
    [SerializeField] private float skillDamage = 150f;     // 궁극기 데미지
    [SerializeField] private float activeDuration = 1.5f;   // 궁극기 연출/시전 시간 (시간 정지 유지 시간)

    [Header("변경용 사각형 범위 세팅 (정면)")]
    [SerializeField] private float boxWidth = 6.0f;         // 사각형의 가로 너비 (좌우 폭)
    [SerializeField] private float boxHeight = 3.0f;        // 사각형의 세로 높이 (Y축 폭)
    [SerializeField] private float boxLength = 10.0f;       // 정면 타격 거리 (앞으로 뻗어나가는 길이)
    [SerializeField] private float forwardOffset = 5.0f;    // 판정 박스 중심점 오프셋 (보통 앞방향 길이의 절반)

    [Header("횟수 및 제한")]
    public int currentCharges = 3;                         // 현재 쓸 수 있는 횟수

    [Header("시각 연출 (선택사항)")]
    [SerializeField] private ParticleSystem ultimateEffect; // 발동 시 터질 파티클 이펙트

    private bool isActivating = false;

    [HideInInspector] public bool isInvincible = false;

    void Update()
    {
        if (Input.GetKeyDown(ultimateKey) && currentCharges > 0 && !isActivating)
        {
            StartCoroutine(ActivateUltimateRoutine());
        }
    }

    private IEnumerator ActivateUltimateRoutine()
    {
        isActivating = true;
        currentCharges--;

        Debug.Log($"궁극기 발동! (남은 횟수: {currentCharges})");

        // [핵심 변경] 시간 정지(Time.timeScale = 0)가 되기 전, 정상 시간 상태에서 적을 먼저 타격합니다.
        // 이렇게 해야 유니티 물리 엔진(Physics)이 정상 작동하여 적의 콜라이더를 완벽하게 잡아냅니다.
        ApplyForwardBoxDamage();

        // 1. 타격 직후 전장 시간 정지 (몬스터 및 투사체 멈춤)
        Time.timeScale = 0f;

        // 2. 무적 상태 켜기
        isInvincible = true;

        // 3. 파티클 이펙트 재생 
        if (ultimateEffect != null)
        {
            ultimateEffect.Play();
        }

        // 4. 게임이 멈춘 상태이므로 실제 현실 시간(Realtime) 기준으로 시전 시간만큼 대기합니다.
        yield return new WaitForSecondsRealtime(activeDuration);

        // 5. 무적 상태 끄기
        isInvincible = false;

        // 6. 게임 시간 다시 정상화
        Time.timeScale = 1f;

        isActivating = false;
        Debug.Log("시간 정지 해제.");
    }

    private void ApplyForwardBoxDamage()
    {
        // 플레이어 정면 위치 계산
        Vector3 boxCenter = transform.position + (transform.forward * forwardOffset);
        Vector3 halfExtents = new Vector3(boxWidth / 2f, boxHeight / 2f, boxLength / 2f);

        // 사각형 영역 내 모든 콜라이더 검출
        Collider[] hitColliders = Physics.OverlapBox(boxCenter, halfExtents, transform.rotation);

        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                // [체크리스트] 주석을 해제하고 연동하셨던 몬스터 피격 코드가 정상 작동하는지 확인하세요.
                // EnemyHP enemy = hit.GetComponent<EnemyHP>();
               //  if (enemy != null) { enemy.TakeDamage(skillDamage); }

                Debug.Log($"🎯 [타격 성공] {hit.name}에게 궁극기 데미지 {skillDamage} 적용!");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        Vector3 localCenter = Vector3.forward * forwardOffset;
        Vector3 size = new Vector3(boxWidth, boxHeight, boxLength);

        Gizmos.DrawWireCube(localCenter, size);
        Gizmos.matrix = originalMatrix;
    }
}
