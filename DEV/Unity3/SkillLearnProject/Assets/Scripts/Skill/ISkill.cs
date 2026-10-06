using UnityEngine;

public interface ISkill
{
    string SkillName { get; }
    float CoolDown { get; }

    void Execute(GameObject caster);
}
