using UnityEngine;


// 공격을 할 수 있는 클래스에게 붙여서 사용하라.
// 인터페이스를 부착하면 어떤 효과를 얻습니까?
// 인터페이스 안에 정의되어 있는 모든 기능을 반드시 구현해야 한다. 강제하는 문법


public interface IAttacker 
{

    public int HP { get; }
    public int ATK { get; }

    public void DoAttack(ITargetable targetable);
}

public interface ITargetable
{
    public int HP { get; set; }
    public int DEF { get; set; }

    public void TakeDamage();
}


public class SampleAttackMonster : MonoBehaviour, IAttacker, ITargetable
{
    CardData cardData;

    public int HP { get => cardData.CardDescription.value; }
    public int ATK { get => cardData.CardDescription.value; }
    public int DEF { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    int ITargetable.HP { get => HP; set => throw new System.NotImplementedException(); }

    public void DoAttack(ITargetable targetable)
    {
        // 피격자에게 나의 공격을 가한다.

        targetable.HP -= ATK;
    }

    public void TakeDamage()
    {
        throw new System.NotImplementedException();
    }
}

