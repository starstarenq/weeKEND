using UnityEngine;

public class CardEffectExecutor : MonoBehaviour
{
    public CardData sample;

    private void Start()
    {
        Excute(sample);
    }

    // <??을> CardData가 정의해 실행하다.

    public void Excute(CardData data)
    {
        // 1. if 조건문 

        //if(data.CardTitle == "타격")
        // {
        //     int dmg = data.CardDescription.value;
        //
        //     // Target에 데미지를 줍니다.
        //
        //     dmg *= 2;
        //
        //     Debug.Log($"{dmg} 공격을 가했습니다.");
        // }
        //else if(data.CardTitle == "수비")
        // {
        //
        //     // 사용자가 방어력을 얻습니다.
        // }
        //else if(data.CardTitle == "힐")
        // {
        //     // 사용자가 체력을 회복합니다.
        // }

        // 1-2 switch 조건문

        //switch (data.SkillType)
        //{
        //    case SkillType.ATTACK:
        //        // 공격 형태의 작동 방식을 정의
        //        break;
        //    case SkillType.GUARD:
        //        // 수비 형태의 작동 방식을 정의
        //        break;
        //    case SkillType.BUFF:
        //        // 버프 형태의 작동 방식을 정의 ...
        //        break;
        //    case SkillType.DEBUFF:
        //        break;
        //    case SkillType.HEAL:
        //        break;
        //}

        // 2 상속

        GameContext context = new GameContext();

        SampleAttackMonster sampleAttackMonster = new SampleAttackMonster();

        context.target = sampleAttackMonster as ITargetable; // 너가 만약에 타겟이 가능한 녀석이면 그 형식으로 바꾸어라

        //context.target = ?;
        //context.owner = ?;

        // Open Closed 원칙

        data.cardEffect.Apply(context);

        // 3 인터페이스

        // 4 command 패턴
    }
}