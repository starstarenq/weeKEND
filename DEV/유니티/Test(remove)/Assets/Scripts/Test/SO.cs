using Unity.Burst.CompilerServices;
using UnityEngine;

public enum SkillType
{
    Active,
    Passive
}

[CreateAssetMenu(fileName = "SkillData", menuName = "Maplestory/SkillData", order = 1)]
public class SO : ScriptableObject
{
    public string SkillName;
    public SkillType skillType;
    public float SkillDamage;
}
