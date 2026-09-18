using UnityEngine;
using System;

[System.Serializable]


public class Monster : MonoBehaviour
{
    //체력 - 정수,소수점
    //이름
    //속도
    //공격력
[SerializeField] MonsterData monsterData;

    ///<summary>
    ////////////////////데이터
    /// </summary>
    private void Start()
    {
        Debug.Log($"{name}의 체력 : {monsterData.HP}");
        Debug.Log($"{name}의 속도 : {monsterData.speed}");
        Debug.Log($"{name}의 공격력 : {monsterData.damage}");
    }
    //그래서 누가 날 공격? -> 1> 강결합, 2> Manager 누가 공격 당하고 받는지 알려준다, 3> 이벤트

    ///<summary>
    /// 기능 : 한번 결정 되었으면 특별한 일이 없으면 잘 변경되지 않을것들
    /// </summary>

    public void CombarBattle(MonsterData defenderStat, MonsterData attackerStat)
    {
       int finalDmg = defenderStat.HP - (int)attackerStat.damage;
        Debug.Log($"최종 데미지 : {finalDmg}");
    }
}
