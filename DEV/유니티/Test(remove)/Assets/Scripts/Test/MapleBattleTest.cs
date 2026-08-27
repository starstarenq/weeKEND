using UnityEngine;

public class MapleBattleTest : MonoBehaviour
{
    [Header("전투 테스트 대상")]
    public MapleStoryComponent attacker;
    public MapleStoryComponent defender;

    [Header("공격 설정")]
    public int attackPower = 100;

    [ContextMenu("공격 실행 (인스펙터 우클릭)")]
    public void TestAttack()
    {
        if (attacker == null || defender == null)
        {
            Debug.LogError("테스트할 공격자와 방어자를 인스펙터에 할당해주세요.");
            return;
        }

        // 공격자가 방어자를 타격 (공격자의 타입과 이름을 넘겨줌)
        defender.TakeDamageFromType(attacker.MyType, attacker.CharacterName, attackPower);
    }
}
