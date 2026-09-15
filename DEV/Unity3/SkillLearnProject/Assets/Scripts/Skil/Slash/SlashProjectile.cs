using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SlashProjectile : MonoBehaviour
{
    private float damage;
    private float range;
    private float speed;

    /// <summary>
    /// 발사대로부터 데이터를 넘겨받아 투사체를 초기화하고 발사합니다.
    /// </summary>
    public void Initialize(float damage, float range, float speed)
    {
        this.damage = damage;
        this.range = range;
        this.speed = speed;

        // Rigidbody를 사용해 앞으로 즉시 발사
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // 검기이므로 중력 비활성화
        rb.linearVelocity = transform.forward * speed;
        // ※ 유니티 구버전(2023 미만) 사용 시 linearVelocity 대신 velocity 사용

        // 사거리에 도달하면 자동으로 파괴되도록 수명 계산 (사거리 / 속도)
        float lifeTime = range / Mathf.Max(0.1f, speed);

        if (lifeTime <= 0 || float.IsInfinity(lifeTime)) lifeTime = 3f;

        Destroy(gameObject, lifeTime);
    }

    /// <summary>
    /// 충돌(트리거) 구역에 들어왔을 때 발동
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Enemy 컴포넌트를 가진 오브젝트인지 확인
        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy != null)
        {
            // 1. 적에게 데미지를 주는 로직 호출
            enemy.TakeDamage(damage);

            // 2. 검기 트리거 된 이후 발동하는 후속 효과
            OnSlashTriggeredEffect(other);

            // 3. 적중 후 투사체 삭제
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 검기 트리거 이후 발동하는 추가 효과 (예: 폭발 이펙트, 디버프 등)
    /// </summary>
    private void OnSlashTriggeredEffect(Collider hitTarget)
    {
        Debug.Log($"{hitTarget.name} 명중 완료! 이곳에 폭발 이펙트 생성이나 추가 상태이동 로직을 작성하세요.");
    }
}
