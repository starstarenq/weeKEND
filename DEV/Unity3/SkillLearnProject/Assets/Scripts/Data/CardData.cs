using System;
using System.Collections.Generic;
using UnityEngine;


// enum을 포함해서 카드데이터를 표현을 해보세요.

[System.Serializable]
public struct CardDescription
{
    public string Description;
    public int value; // 카드의 수치로 모든걸 표현할 수 있다.

    public CardDescription(string description, int value)
    {
        Description = description;
        this.value = value;
    }
}

// 타격 (공격) 데미지를 줍니다.   +  
// 수비 (방어) 쉴드가 생성됩니다. +
// 취약 (받는 피해 증가)
// 약화 (상대의 공격력 약화)
// 도트 (지속시간 동안 데미지를 강하다)
// 지속가능한 효과 카드군

public enum AttackType
{
    MELEE, RANGED, AREA, NONE
}

public enum PowerType
{
    PHYSICAL, FIRE, ICE, POISION
}

public enum SkillType
{
    ATTACK, GUARD, BUFF, DEBUFF, HEAL
}

// Effect : 전투를 실행할 때 어떤 효과가 발생해야 하나요?

[CreateAssetMenu(fileName = "CardData", menuName = "Scriptable Objects/CardData")]
public class CardData : ScriptableObject
{
    public int CostText;
    public string CardTitle;
    public Sprite CardImage;
    public CardDescription CardDescription;

    public SkillType SkillType;

    [SerializeReference]
    public CardEffect cardEffect;



    public CardData Clone()
    {
        var clone = Instantiate(this);
        clone.CardDescription = new CardDescription(this.CardDescription.Description, this.CardDescription.value);

        return clone;
    }
}
