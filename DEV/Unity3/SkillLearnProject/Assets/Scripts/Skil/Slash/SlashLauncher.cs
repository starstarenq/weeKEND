using UnityEngine;

public class SlashLauncher : MonoBehaviour
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

    [Header("검기 데이터 설정")]
    [SerializeField] private SlashData slashData;
    [SerializeField] private GameObject slashPrefab; // SlashProjectile 스크립트가 붙은 프리팹
    [SerializeField] private Transform firePoint;     // 발사 위치

    private float lastAttackTime;

    void Start()
    {
        // 발사 위치가 지정되지 않았다면 현재 오브젝트 위치로 설정
        if (firePoint == null) firePoint = this.transform;
    }

    void Update()
    {
        // 키 입력을 받아 발사 시도
        if (Input.GetKeyDown(KeyCode.A))
        {
            TryExecuteSlash();
        }
    }

    /// <summary>
    /// 쿨타임 체크 후 검기 발동
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
    /// 검기 생성 및 방향 계산
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
    /// 투사체 실시간 생성 및 데이터 주입, 속도 부여
    /// </summary>
    private void SpawnProjectile(Quaternion rotation)
    {
        GameObject projGO = Instantiate(slashPrefab, firePoint.position, rotation);
        SlashProjectile projScript = projGO.GetComponent<SlashProjectile>();

        if (projScript != null)
        {
            // 투사체에 공격력, 사거리, 속도 데이터 전달 후 초기화 함수 실행
            projScript.Initialize(slashData.damage, slashData.range, slashData.speed);
        }
    }
}
