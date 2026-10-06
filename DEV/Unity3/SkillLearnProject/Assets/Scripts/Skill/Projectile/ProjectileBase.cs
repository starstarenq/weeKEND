using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public abstract class ProjectileBase : MonoBehaviour
{
    protected float damage;
    protected float range;
    protected float speed;
    protected Vector2 startPosition;
    protected GameObject owner;

    protected Rigidbody2D rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // 2D 투사체가 중력의 영향을 받아 가라앉지 않도록 설정
        rb.gravityScale = 0f;
    }

    // 공통 초기화 메서드 (기존 Initialize와 동일)
    public virtual void Initialize(float damage, float range, float speed, GameObject owner)
    {
        this.damage = damage;
        this.range = range;
        this.speed = speed;
        this.owner = owner;
        this.startPosition = transform.position;

        // 충돌 감지를 위해 2D 콜라이더를 트리거로 설정
        if (TryGetComponent<Collider2D>(out var col))
        {
            col.isTrigger = true;
        }

        // Rigidbody2D의 속도(Velocity)를 직접 제어하여 이동
        rb.linearVelocity = transform.right * speed;
    }

    protected virtual void Update()
    {
        // 2D 사거리 초과 체크
        if (Vector2.Distance(startPosition, transform.position) >= range)
        {
            Explode();
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // 발사한 주인이 자기 자신이나 자식 오브젝트와 충돌하는 것 방지
        if (owner != null && (other.gameObject == owner || other.transform.IsChildOf(owner.transform)))
        {
            return;
        }

        // 1. 피격 대상이 IDamageable2D 컴포넌트를 가지고 있는지 확인
        if (other.TryGetComponent<IDamageable2D>(out var target))
        {
            OnHitTarget(target);
            Explode();
            return;
        }

        // 2. 2D 벽이나 장애물 태그/레이어 충돌 체크
        if (other.CompareTag("Wall") || other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Explode();
        }
    }

    // 타겟 적중 시 기본 동작 (자식 클래스에서 오버라이드하여 추가 효과 구현 가능)
    protected virtual void OnHitTarget(IDamageable2D target)
    {
        target.TakeDamage(damage, owner);
    }

    // 투사체 소멸 및 이펙트 처리 공통 로직
    protected void Explode()
    {
        OnExplode();
        Destroy(gameObject);
    }

    // 자식 클래스에서 각자의 연출을 담당할 추상/가상 메서드
    protected abstract void OnExplode();
}

