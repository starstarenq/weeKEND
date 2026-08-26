using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    [Header("플레이어 체력 설정")]
    public float maxHp = 100f;
    private float currentHp;

    [Header("UI 연동")]
    [SerializeField] private UI_PlayerHPBar playerHPBar; // 인스펙터 직렬화 연결
    [SerializeField] private UI_GameOver gameOverUI;

    private EquipmentSkillManager equipmentSkillManager;

    private void Start()
    {
        currentHp = maxHp;
        equipmentSkillManager = GetComponent<EquipmentSkillManager>();

        // 수동 할당이 안 되어있을 경우 씬에서 자동 탐색
        if (playerHPBar == null)
        {
            playerHPBar = FindAnyObjectByType<UI_PlayerHPBar>();
        }

        // 초기 체력 UI 반영
        UpdateHPUI();
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

        UpdateHPUI();

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void UpdateHPUI()
    {
        if (playerHPBar != null)
        {
            playerHPBar.UpdateHP(currentHp, maxHp);
        }
    }

    private void Die()
    {
        Debug.Log("플레이어 사망!");

        if (gameOverUI == null)
        {
            gameOverUI = FindObjectOfType<UI_GameOver>(true);
        }

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