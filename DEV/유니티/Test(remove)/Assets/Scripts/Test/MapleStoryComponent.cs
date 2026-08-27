using UnityEngine;

public class MapleStoryComponent : MonoBehaviour
{
    // [System.Serializable] 클래스를 인스펙터창에 그대로 노출시켜 수동 편집 가능하게 만듦
    [Header("캐릭터 스탯 설정")]
    [SerializeField] private MapleStats characterStats;

    // 순수 C# 계산 로직 인스턴스
    private MapleDamageCalculator damageCalculator;

    // 타 클래스에서 열람할 수 있도록 프로퍼티 제공
    public MapleType MyType => characterStats.mapleType;
    public string CharacterName => characterStats.characterName;

    void Awake()
    {
        // 게임 시작 시 순수 C# 로직 클래스에 데이터를 넘겨주며 초기화
        damageCalculator = new MapleDamageCalculator(characterStats);
    }

    // 외부에서 이 캐릭터를 공격할 때 호출하는 메서드
    public void TakeDamageFromType(MapleType attackerType, string attackerName, int rawDamage)
    {
        // 1. 순수 C# 로직에 연산을 위임하고 최종 데미지를 반환받음
        int finalDamage = damageCalculator.ProcessCalculateDamage(attackerType, rawDamage);

        // 2. 요구사항: Debug.Log에서 확인 할 수 있어야 함
        Debug.Log($"[피격 로그] <b>{characterStats.characterName}</b>({characterStats.mapleType})이(가) " +
                  $"<b>{attackerName}</b>({attackerType})에게 공격받았습니다!\n" +
                  $"기본 데미지: {rawDamage} -> <b>최종 적용 데미지: {finalDamage}</b> (남은 체력: {characterStats.currentHealth})");

        if (characterStats.currentHealth <= 0)
        {
            Debug.LogWarning($"[사망] {characterStats.characterName}이(가) 비석을 세웠습니다.");
        }
    }
}
