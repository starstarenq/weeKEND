using UnityEngine;
using System.Collections.Generic;
//데이터(자주 변할것들) 빼서 보관
//아이템, 스킬, 몬스터, 플레이어 직업

public class Wave
{
    [SerializeField] List<Monster> monsterGroup = new List<Monster>();
}
public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] List<Monster> monsterGroup = new List<Monster>(); // 10..
    //데이터 있으니 데이터로 몬스터 생성
    private void Start()
    {
        //내 몬스터 그룹에 있는 몬스터를 소환하는 코드 작성
        // wave -> wave당 몇 종류의 몇마리 소환?
        
        Instantiate(monsterGroup[1]);
    }
}
