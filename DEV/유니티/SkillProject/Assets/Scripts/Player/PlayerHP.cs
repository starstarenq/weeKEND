using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    [Header("플레이어 체력 설정")]
    public float maxHp = 100f;
    private float currentHp;

    private EquipmentSkillManager equipmentSkillManager;

    private void Start()
    {
        currentHp = maxHp;
        equipmentSkillManager = GetComponent<EquipmentSkillManager>();
    }

    /// <summary>
    /// 플레이어 피격 시 호출
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        // [3번 슬픔 스킬 연동] 무효화 스택(3회)이 남아있는지 체크
        if (equipmentSkillManager != null && equipmentSkillManager.CheckAndConsumeInvulnerability())
        {
            Debug.Log("[슬픔 스킬] 몬스터 공격 무효화!");
            return; // 피격 무시
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
    }
}