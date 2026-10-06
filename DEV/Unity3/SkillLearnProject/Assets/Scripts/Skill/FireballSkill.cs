using UnityEngine;

public class FireballSkill : MonoBehaviour, ISkill
{
    public string SkillName => "파이어볼";
    public float CoolDown => 3.0f;

    public void Execute(GameObject caster)
    {
        Debug.Log($"{caster.name}이 화염구를 발사했습니다.");
    }
}
