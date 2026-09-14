using UnityEngine;

// 충돌 및 이동 처리를 위해 Rigidbody 컴포넌트가 필수적으로 필요합니다.
[RequireComponent(typeof(Rigidbody))]
public class Slash : MonoBehaviour
{
    [System.Serializable]
    public struct SlashData
    {
        public float damage;          // 공격력
        public float speed;           // 투사체 속도
        public float range;           // 사거리 (유지 시간으로 계산)
        public int projectileCount;   // 투사체 개수
        public float cooldown;        // 쿨타임
    }

    [Header("--- [설정 구분] ---")]
    [Tooltip("True: 플레이어가 부착하는 발사대 | False: 날아가는 검기 프리팹 자체")]
    [SerializeField] private bool isLauncher = true;

    [Header("검기 데이터 설정 (발사대 전용)")]
    [SerializeField] private SlashData slashData;
    [SerializeField] private GameObject slashPrefab; // 이 스크립트가 붙은 프리팹 자신을 넣으셔도 됩니다.
    [SerializeField] private Transform firePoint;     // 발사 위치

    // 내부 전용 데이터 (생성된 투사체가 사용할 값)
    private float currentDamage;
    private float currentRange;
    private float lastAttackTime;

    void Start()
    {
        if (isLauncher)
        {
            // [발사대 로직] 초기 설정
            if (firePoint == null) firePoint = this.transform;
        }
        else
        {
            // [투사체 로직] 사거리에 도달하면 자동으로 파괴되도록 설정 (사거리 / 속도 = 유지시간)
            float lifeTime = currentRange / Mathf.Max(0.1f, GetComponent<Rigidbody>().linearVelocity.magnitude);
            // ※ 유니티 구버전(2023 미만)을 쓰신다면 linearVelocity 대신 velocity를 사용하세요.

            // 만약 물리 속도가 없다면 대안으로 데이터 기반 시간 파괴
            if (lifeTime <= 0 || float.IsInfinity(lifeTime)) lifeTime = 3f;

            Destroy(gameObject, lifeTime);
        }
    }

    void Update()
    {
        // 발사대일 때만 키 입력을 받습니다.
        if (isLauncher)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                TryExecuteSlash();
            }
        }
    }

    /// <summary>
    /// [발사대] 쿨타임 체크 후 검기 발동
    /// </summary>
    public void TryExecuteSlash()
    {
        if (Time.time >= lastAttackTime + slashData.cooldown)
        {
            ExecuteSlash();
            lastAttackTime = Time.time;
        }
    }

    /// <summary>
    /// [발사대] 검기 생성 및 데이터 전달
    /// </summary>
    private void ExecuteSlash()
    {
        if (slashPrefab == null) return;

        if (slashData.projectileCount <= 1)
        {
            SpawnProjectile(firePoint.rotation);
        }
        else
        {
            float angleStep = 15f;
            float startAngle = -((slashData.projectileCount - 1) * angleStep) / 2f;

            for (int i = 0; i < slashData.projectileCount; i++)
            {
                float targetAngle = startAngle + (angleStep * i);
                Quaternion rotation = firePoint.rotation * Quaternion.Euler(0, targetAngle, 0);
                SpawnProjectile(rotation);
            }
        }
    }

    /// <summary>
    /// [발사대] 투사체 실시간 생성 및 물리 속도 부여
    /// </summary>
    private void SpawnProjectile(Quaternion rotation)
    {
        GameObject projGO = Instantiate(slashPrefab, firePoint.position, rotation);
        Slash projScript = projGO.GetComponent<Slash>();

        if (projScript != null)
        {
            // 생성된 투사체의 모드를 변경하고 데이터를 주입합니다.
            projScript.isLauncher = false;
            projScript.currentDamage = slashData.damage;
            projScript.currentRange = slashData.range;

            // Rigidbody를 사용해 앞으로 즉시 발사합니다.
            Rigidbody rb = projGO.GetComponent<Rigidbody>();
            rb.useGravity = false; // 검기이므로 중력은 끕니다.
            rb.linearVelocity = projGO.transform.forward * slashData.speed;
            // ※ 유니티 구버전(2023 미만)을 쓰신다면 linearVelocity 대신 velocity를 사용하세요.
        }
    }

    /// <summary>
    /// [투사체] 충돌(트리거) 구역에 들어왔을 때 발동하는 일
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 발사대 컴포넌트는 충돌 처리를 하지 않습니다.
        if (isLauncher) return;

        if (other.CompareTag("Enemy"))
        {
            // 1. 적에게 데미지를 주는 로직
            Debug.Log($"{other.name}에게 {currentDamage} 피해를 입혔습니다.");

            // 2. 검기 트리거 된 이후 발동하는 또다른 일 (후속 효과)
            OnSlashTriggeredEffect(other);

            // 3. 적중 후 투사체 삭제
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// [투사체] 검기 트리거 이후 발동하는 추가 효과 (예: 폭발 이펙트, 디버프 등)
    /// </summary>
    private void OnSlashTriggeredEffect(Collider hitTarget)
    {
        Debug.Log($"{hitTarget.name} 명중 완료! 이곳에 폭발 이펙트 생성이나 추가 상태이동 로직을 작성하세요.");
        // 예: Instantiate(explosionPrefab, transform.position, Quaternion.identity);
    }
}
