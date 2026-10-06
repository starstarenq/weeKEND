using UnityEngine;

[System.Serializable]
public class HealEffect : CardEffect
{
    public override void Apply(GameContext context)
    {
        // 자힐  owner -> owner.hp 회복합니다.
        // 파티원 힐 owner -> target.hp 회복합니다.
    }
}

