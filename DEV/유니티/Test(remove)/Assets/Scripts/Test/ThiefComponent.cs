using UnityEngine;

public class ThiefComponent : MonoBehaviour, IDamageable
{
    [Header("초기 설정값")]
    [SerializeField] private string characterName = "도적";
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int attackPower = 15;
    [SerializeField][Range(0f, 1f)] private float lifeStealRatio = 0.2f;

    // 순수 C# 도적 객체 참조
    private Thief thiefLogic;
    public Thief Logic => thiefLogic;

    void Awake()
    {
        // 1. 순수 C# 객체 생성 및 데이터 주입
        thiefLogic = new Thief(characterName, maxHealth, attackPower, lifeStealRatio);

        // 2. 로직 내부의 체력 변경 이벤트 구독 (유니티 기능과 연결)
        thiefLogic.OnHealthChanged += HandleHealthChanged;
    }

    void OnDestroy()
    {
        // 메모리 누수 방지를 위한 이벤트 해제
        if (thiefLogic != null)
        {
            thiefLogic.OnHealthChanged -= HandleHealthChanged;
        }
    }

    // 다른 유니티 오브젝트(적 등)에서 이 도적을 공격할 때 호출됨
    public void TakeDamage(int damage)
    {
        thiefLogic.TakeDamage(damage);
    }

    // 대상을 공격하는 유니티 메서드
    public void CommandAttack(IDamageable target)
    {
        if (target == null) return;

        Debug.Log($"{thiefLogic.Name}이(가) 대상을 공격합니다!");
        thiefLogic.Attack(target);
    }

    // 이벤트 발생 시 실행될 유니티 전용 로직 (로그 출력 또는 추후 HP바 UI 갱신)
    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        Debug.Log($"[{thiefLogic.Name} 체력 변동] 현재: {currentHealth} / 최대: {maxHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log($"{thiefLogic.Name}이(가) 사망했습니다.");
            // 쓰러지는 애니메이션 재생 등의 유니티 코드 작성 가능
        }
    }
}
