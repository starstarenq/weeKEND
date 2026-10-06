using UnityEngine;

public class IceBoltProjectile : ProjectileBase
{
    [Header("Ice Bolt Settings")]
    [SerializeField] private float chillDuration = 3f; // 얼음 화살 고유의 디버프 지속 시간 예시

    protected override void OnHitTarget(IDamageable2D target)
    {
        // 부모의 기본 데미지 주기 실행
        base.OnHitTarget(target);

        // 얼음 화살 고유 로직: 피격 대상에게 슬로우/빙결 컴포넌트가 있다면 추가 적용 가능
        if (target is MonoBehaviour targetMono)
        {
            // 예시: 피격 대상 오브젝트에서 빙결 메커니즘 컴포넌트를 찾아 지속시간 전달
            // if (targetMono.TryGetComponent<IChillageable>(out var chillable)) { chillable.ApplyChill(chillDuration); }
            Debug.Log($"?? {targetMono.name}에게 {chillDuration}초 동안 한기 효과를 부여합니다.");
        }
    }

    protected override void OnExplode()
    {
        // 아이스 볼트 특유의 파편/얼음 소멸 로그 및 이펙트 처리 생성부
        Debug.Log("?? 2D 아이스 볼트 투사체가 깨지며 소멸했습니다.");

        // 예시: Instantiate(iceShatterPrefab, transform.position, Quaternion.identity);
    }
}

