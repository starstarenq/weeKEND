using UnityEngine;
using System;


[Serializable]
public class Monster : MonoBehaviour
{
    // 체력 - 정수, 소수점
    // 이름
    // 속도
    // 공격력

    // public  vs   [SerializeField] private

    [SerializeField] MonsterData monsterData;
    /// <summary>
    /// /////////////////// 데이터
    /// </summary>

    private void Start()
    {
        Debug.Log($"{monsterData.name}의 체력 : {monsterData.HP}");
        Debug.Log($"{monsterData.name}의 속도 : {monsterData.speed}");
        Debug.Log($"{monsterData.name}의 공격력 : {monsterData.damage}");
    }

    // 그래서 누가 나를 공격합니까? ->  1> 강결합 , 2>  Manager 누가 공격을 당하고 받는지 알려준다. 3> 이벤트

    /// <summary>
    /// 기능 : 한번 결정이 되었으면 특별한 일이 없으면 잘 변경되지 않을 것들
    /// </summary>
    /// <param name="defenderMonsterData"></param>
    /// <param name="attackerStat"></param>

    public void CombatBattle(MonsterData defenderStat, MonsterData attackerStat)
    {
        int finalDmg = defenderStat.HP - (int)attackerStat.damage;

        Debug.Log($"최종 데미지 {finalDmg}");
    }

}
