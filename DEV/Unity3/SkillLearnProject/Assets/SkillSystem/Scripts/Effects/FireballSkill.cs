using UnityEngine;

namespace KDH_SkillSystem
{
    [CreateAssetMenu(fileName = "FireballSkill", menuName = "SkillSystem/Skills/Fireball")]
    public class FireballSkill : SkillBase, ISkillExecute
    {
        [Header("파이어볼 고유 설정 (2D)")]
        [SerializeField] private GameObject fireballPrefab; // 발사할 투사체 프리팹
        [SerializeField] private float projectileSpeed = 10f;

        public override void Initialize()
        {
            // 초기화 로직이 필요 없다면 비워둡니다.
            Debug.Log($"[FireballSkill] {SkillName} 데이터 초기화 완료.");
        }

        public void Execute(GameObject caster, Vector3 targetPosition)
        {
            if (fireballPrefab == null)
            {
                Debug.LogError($"[FireballSkill] {SkillName}의 투사체 프리팹이 할당되지 않았습니다.");
                return;
            }

            // 1. 발사 방향 계산 (2D 상면/정면 벡터 계산)
            Vector2 spawnPosition = caster.transform.position;
            Vector2 direction = ((Vector2)targetPosition - spawnPosition).normalized;

            // 2. 투사체 생성 및 방향 정렬 (Z축 회전)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            GameObject projectile = Instantiate(fireballPrefab, spawnPosition, Quaternion.AngleAxis(angle, Vector3.forward));

            // 3. 2D 물리(Rigidbody2D)를 통한 발사 처리
            if (projectile.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.linearVelocity = direction * projectileSpeed;
            }
            else
            {
                // Rigidbody2D가 없을 경우를 대비한 뼈대 코드
                Debug.LogWarning("[FireballSkill] 생성된 투사체에 Rigidbody2D가 없습니다.");
            }

            Debug.Log($"[FireballSkill] {caster.name}이(가) {targetPosition} 방향으로 파이어볼을 발사했습니다!");
        }
    }
}
