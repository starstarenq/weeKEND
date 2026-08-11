using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    [Header("플레이어 체력 설정")]
    public float maxHp = 100f;
    private float currentHp;

    [Header("UI 연동")]
    [SerializeField] private UI_GameOver gameOverUI; // 인스펙터에서 UI_GameOver 패널 할당

    private EquipmentSkillManager equipmentSkillManager;

    private void Start()
    {
        currentHp = maxHp;
        equipmentSkillManager = GetComponent<EquipmentSkillManager>();
    }

    public void TakeDamage(float damageAmount)
    {
        if (equipmentSkillManager != null && equipmentSkillManager.CheckAndConsumeInvulnerability())
        {
            Debug.Log("[슬픔 스킬] 몬스터 공격 무효화!");
            return;
        }

        currentHp -= damageAmount;
        Debug.Log($"플레이어 피격! 남은 체력: {currentHp}/{maxHp}");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("플레이어 사망!");

        // 1. 인스펙터에 수동 연동되지 않았을 경우 씬 전체 탐색 (비활성화된 것도 포함)
        if (gameOverUI == null)
        {
            gameOverUI = FindObjectOfType<UI_GameOver>(true);
        }

        // 2. PopUI 계열 표준 함수인 ShowPopup() 호출
        if (gameOverUI != null)
        {
            gameOverUI.ShowPopup();
        }
        else
        {
            Debug.LogError("UI_GameOver를 찾을 수 없습니다! Canvas 하위에 UI_GameOver가 존재하는지 확인하세요.");
        }
    }
}