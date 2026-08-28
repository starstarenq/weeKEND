using UnityEngine;

[CreateAssetMenu(fileName = "PersonaData", menuName = "Persona/Persona Data")]
public class PersonaData : ScriptableObject
{
    [Header("기본 정보")]
    public string personaName;
    public string arcana;
    public int baseLevel = 30;

    [Header("기본 능력치")]
    public int strength;    // 힘 (St)
    public int magic;       // 마력 (Ma)
    public int endurance;   // 내구 (En)
    public int agility;     // 속도 (Ag)
    public int luck;        // 운 (Lu)

    // 총합 계산 프로퍼티
    public int TotalStats => strength + magic + endurance + agility + luck;
}