using UnityEngine;

public interface ISkill
{
    string Name { get; }
    float Cooldown { get; }

    // 스킬 실행 로직 (스킬을 쓰는 주체의 GameObject를 넘겨줍니다)
    void Execute(GameObject caster);
}
