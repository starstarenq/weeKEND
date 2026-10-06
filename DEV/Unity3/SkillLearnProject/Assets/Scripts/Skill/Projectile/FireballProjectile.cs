using UnityEngine;

public class FireballProjectile : ProjectileBase
{
    // 추가적인 파이어볼 고유 변수가 필요하다면 여기에 선언 (예: 폭발 반경 등)

    protected override void OnExplode()
    {
        // 파이어볼 특유의 폭발 로그 및 이펙트 처리 생성부
        Debug.Log("💥 2D 파이어볼 투사체가 폭발하며 소멸했습니다.");

        // 예시: Instantiate(fireballExplosionPrefab, transform.position, Quaternion.identity);
    }
}
