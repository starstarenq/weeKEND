using UnityEngine;
using System.Collections.Generic;
// 데이터 (자주 변할 것들) 빼서 보관하자.

// 아이템, 스킬, 몬스터, 플레이어의 직업

// sphereInside

// 뱀파이어 서바이벌 몬스터 스폰 방식

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] List<Monster> monsterGroup = new List<Monster>();


    // 데이터가 있으니깐 데이터로 몬스터를 생성할게.

    private void Start()
    {
        // 내 몬스터 그룹에 있는 몬스터를 소환하는 코드를 작성하세요. 

        // wave -> wave당 몇 종류의 몇마리를 소환할까요? 

        Instantiate(monsterGroup[1]);
    }
}
