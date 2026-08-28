using UnityEngine;

public class PersonaController : MonoBehaviour
{
    [Header("ReadMe 데이터베이스 참조")]
    [SerializeField] private PersonaReadmeSO readmeSO;

    [Header("현재 장착 중인 페르소나")]
    [SerializeField] private PersonaData currentPersona;

    private void Start()
    {
        // ReadMe에 등록된 데이터 요약 출력
        if (readmeSO != null)
        {
            Debug.Log($"[ReadMe 내용]\n{readmeSO.readMeDescription}");
        }

        // 현재 페르소나 능력치 출력
        if (currentPersona != null)
        {
            PrintCurrentPersonaInfo();
        }
    }

    public void PrintCurrentPersonaInfo()
    {
        Debug.Log($"[현재 페르소나] {currentPersona.personaName} ({currentPersona.arcana})");
        Debug.Log($"Stats - St: {currentPersona.strength} | Ma: {currentPersona.magic} | En: {currentPersona.endurance} | Ag: {currentPersona.agility} | Lu: {currentPersona.luck}");
        Debug.Log($"Total Stats: {currentPersona.TotalStats}");
    }
}