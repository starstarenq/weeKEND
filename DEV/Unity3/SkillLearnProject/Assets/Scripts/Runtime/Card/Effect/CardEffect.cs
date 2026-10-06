using UnityEngine;


public class GameContext
{
    // 공격하기 위한 대사장
    public ITargetable target; // GameObject 유니티의 모든 오브젝트의 기본이 됩니다. Find <- 비용이 비싸다.
    public IAttacker owner;  // 

   
}

[System.Serializable]
public abstract class CardEffect 
{

    public abstract void Apply(GameContext context);
}


