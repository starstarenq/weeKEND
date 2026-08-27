using System;
using System.Collections.Generic;
using UnityEngine;

// 1. 인스펙터에서 수동 편집이 가능하도록 직렬화 속성 부여
[Serializable]
public class MapleStats
{
    public string characterName = "메이플 캐릭터";
    public MapleType mapleType;
    public int currentHealth = 1000;

    [Tooltip("기본 방어력 (데미지를 차감합니다)")]
    public int defense = 10;

    [Tooltip("특정 타입에게 받는 데미지 배율 설정 (예: Magician에게 1.5배 피해 등)")]
    public List<TypeWeakness> weaknesses = new List<TypeWeakness>();
}

// 인스펙터에서 리스트 형태로 편하게 작성하기 위한 상성 데이터 구조
[Serializable]
public class TypeWeakness
{
    public MapleType attackerType; // 공격자 타입
    [Range(0f, 3f)] public float damageMultiplier = 1.0f; // 데미지 배율
}

// 2. 순수 데미지 계산을 담당하는 로직 클래스
public class MapleDamageCalculator
{
    private MapleStats stats;

    // 생성자를 통해 데이터 주입
    public MapleDamageCalculator(MapleStats stats)
    {
        this.stats = stats;
    }

    // 공격자 타입과 기본 데미지를 받아 최종 피해량을 계산하고 체력을 깎는 메서드
    public int ProcessCalculateDamage(MapleType attackerType, int rawDamage)
    {
        // 상성 배율 찾기 (리스트에 없으면 기본 1.0배)
        float multiplier = 1.0f;
        foreach (var weakness in stats.weaknesses)
        {
            if (weakness.attackerType == attackerType)
            {
                multiplier = weakness.damageMultiplier;
                break;
            }
        }

        // 최종 데미지 공식 = (기본 데미지 * 상성 배율) - 방어력
        int finalDamage = Mathf.RoundToInt(rawDamage * multiplier) - stats.defense;

        // 데미지가 음수가 되는 것을 방지 (최소 1의 피해는 입음)
        finalDamage = Mathf.Max(finalDamage, 1);

        // 체력 감소 처리
        stats.currentHealth -= finalDamage;
        stats.currentHealth = Mathf.Max(stats.currentHealth, 0);

        return finalDamage;
    }
}
