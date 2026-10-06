using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Fireball : MonoBehaviour
{
    [Header("파이어볼 능력치")]

    [SerializeField] private float spellTime = 2.0f;

    [SerializeField] private float damage = 10f;       // 공격력
    [SerializeField] private float range = 15f;        // 사거리
    [SerializeField] private float speed = 10f;        // 속도
    [SerializeField] private int projectileCount = 1;  // 투사체 갯수
    [SerializeField] private float cooldown = 1f;      // 쿨타임
    [SerializeField] private float spreadAngle = 15f;  // 다중 발사 시 회전 각도

    [Header("설정 및 프리팹")]
    [SerializeField] private GameObject fireballPrefab; // 투사체 프리팹 (Mesh와 Collider, Rigidbody가 포함된 상태)
    [SerializeField] private Transform spawnPoint;      // 발사 위치

    private float currentCooldown = 0f;


    private void Start()
    {
    }

    void Update()
    {
        // 쿨타임 타이머 관리
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }

        // 테스트용 입력 (Space 바를 누르면 발동)
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCoroutine(FireEnumerator());
        }
    }

    // [파이어볼이 실제로 발동했을 때 일어나는 일]
    public IEnumerator FireEnumerator()
    {
        #region 예외 처리
        if (currentCooldown > 0)
        {
            Debug.Log("쿨타임 중입니다.");
            yield break; // 💡 yield return null 대신 yield break를 사용해야 코루틴이 완전히 종료됩니다.
        }

        if (fireballPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("프리팹 또는 SpawnPoint가 지정되지 않았습니다.");
            yield break;
        }
        #endregion

        // ==========================================
        // 1. 스킬 시전 시작 단계 (Pre-Cast Phase)
        // ==========================================
        Debug.Log("🔥 [파이어볼] 시전을 시작합니다.");
        Debug.Log("🎵 [사운드] 파이어볼 주문 시전 사운드 재생 (쿠우우우-)");
        Debug.Log("💃 [애니메이션] 캐릭터가 주문을 외우는 애니메이션 재생");

        // 시전 중 방해 여부를 체크하기 위한 타이머 변수
        float elapsedCastTime = 0f;
        bool isCanceled = false;

        // spellTime(시전 시간) 동안 루프를 돌며 매 프레임마다 방해를 받았는지 체크합니다.
        while (elapsedCastTime < spellTime)
        {
            // 💡 외부 상태(예: IsStunned, IsKnockbacked 등)를 체크하는 조건문 예시입니다.
            // 프로젝트 환경에 맞는 '방해 상태 변수'로 대체하여 사용하세요.
            if (/* 예를 들어: isInterrupted || */ false)
            {
                isCanceled = true;
                break;
            }

            elapsedCastTime += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        // 시전 중에 방해를 받아 취소된 경우의 처리
        if (isCanceled)
        {
            Debug.LogWarning("❌ [캔슬] 시전 중 방해를 받아 파이어볼 스킬이 취소되었습니다!");
            Debug.Log("💃 [애니메이션] 시전 취소/피격 애니메이션으로 전환");
            yield break; // 스킬 발사 단계를 실행하지 않고 코루틴 종료
        }

        // ==========================================
        // 2. 스킬 발사 단계 (Cast Phase)
        // ==========================================
        Debug.Log("🚀 [발사] 시전 완료! 파이어볼을 발사합니다.");

        // 투사체 개수(projectileCount)에 따른 발사 로직
        int startIdx = -(projectileCount / 2);

        for (int i = 0; i < projectileCount; i++)
        {
            // 부채꼴 정렬 계산 (1개일 때는 정면, 여러 개일 때는 spreadAngle 간격으로 분산)
            float angleOffset = (startIdx + i) * spreadAngle;
            if (projectileCount % 2 == 0) angleOffset += spreadAngle / 2f; // 짝수 개수일 때 정렬 보정

            Quaternion spawnRotation = spawnPoint.rotation * Quaternion.Euler(0, angleOffset, 0);

            // 1. 프리팹 생성
            GameObject projectile = Instantiate(fireballPrefab, spawnPoint.position, spawnRotation);

            // 2. 생성된 프리팹에 마스터의 데이터와 로직을 주입하는 인스턴스 컴포넌트 추가
            FireballInstance instance = projectile.AddComponent<FireballInstance>();
            instance.Initialize(damage, range, speed, this);
        }

        // 쿨타임 적용
        currentCooldown = cooldown;

        // ==========================================
        // 3. 스킬 후반 딜레이 단계 (Post-Cast Phase)
        // ==========================================
        Debug.Log("💤 [후딜레이] 스킬 발사 후딜레이(경직) 상태 진입");
        Debug.Log("💃 [애니메이션] 스킬 발사 후 마무리 동작 애니메이션 재생");

        yield return new WaitForSeconds(0.5f); // 스킬 후딜레이 대기

        Debug.Log("✅ [완료] 후딜레이 종료. 이제 캐릭터가 자유롭게 이동하거나 다른 스킬을 쓸 수 있습니다.");
    }

    // [파이어볼이 트리거 된 이후에 발동하는 또다른 일]
    // 개별 투사체가 무언가와 부딪혔을 때 마스터 클래스에서 최종 처리할 로직
    public void OnProjectileTrigger(Collider other, GameObject projectile, float finalDamage)
    {
        if (other.CompareTag("Enemy"))
        {
            // 예시: Enemy 스크립트가 있다면 데미지 처리
            // other.GetComponent<Enemy>()?.TakeDamage(finalDamage);
            Debug.Log($"{other.name}에게 {finalDamage} 데미지를 입혔습니다.");

            // 적과 부딪히면 투사체 제거
            Destroy(projectile);
        }
        else if (other.CompareTag("Wall"))
        {
            // 벽에 부딪혀도 제거
            Destroy(projectile);
        }
    }
}

// ==========================================
// 마스터 클래스의 통제를 받는 실질적 투사체 움직임 컴포넌트
// ==========================================
public class FireballInstance : MonoBehaviour
{
    private float damage;
    private float range;
    private float speed;
    private Fireball master;
    private Vector3 startPosition;

    public void Initialize(float damage, float range, float speed, Fireball master)
    {
        this.damage = damage;
        this.range = range;
        this.speed = speed;
        this.master = master;
        this.startPosition = transform.position;

        // 트리거 감지를 위해 Collider가 반드시 Trigger 모드여야 함
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void Update()
    {
        // 전방 이동
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // 사거리 제한 초과 시 자체 제거
        if (Vector3.Distance(startPosition, transform.position) >= range)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (master != null)
        {
            // 충돌 시 마스터 클래스의 처리 함수로 토스
            master.OnProjectileTrigger(other, gameObject, damage);
        }
    }
}
