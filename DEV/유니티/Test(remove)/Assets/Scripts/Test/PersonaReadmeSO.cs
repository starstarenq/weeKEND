using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Persona_ReadMe", menuName = "Persona/ReadMe SO")]
public class PersonaReadmeSO : ScriptableObject
{
    [TextArea(5, 12)]
    public string readMeDescription =
        "=== 페르소나 5 로얄 레벨 30 동등 데이터베이스 ===\n\n" +
        "• 아라하바키 : 내구/마력 중심 (방어/탱킹)\n" +
        "• 네비로스   : 높은 마력 (주원 마법 특화)\n" +
        "• 람다       : 높은 힘/속도 (물리/상태이상)\n" +
        "• 키운       : 마력/운 중심 (염동/크리티컬)";

    [Header("등록된 페르소나 자산 목록")]
    public List<PersonaData> personaList = new List<PersonaData>();
}