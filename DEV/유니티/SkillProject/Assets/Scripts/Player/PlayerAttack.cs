using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("기본 공격 (부채꼴 설정)")]
    public float attackRange = 3.0f;
    [Range(0, 360)]
    public float attackAngle = 90f;
    public float attackDamage = 20f;
    public LayerMask enemyLayer;

    private EquipmentSkillManager equipmentSkillManager;

    [Header("보조 스킬 (우클릭 설정)")]
    public float secondaryAttackRange = 5.0f;
    [Range(0, 360)]
    public float secondaryAttackAngle = 120f;
    public float secondaryAttackDamage = 35f;
    public float secondaryCooldown = 3.0f;

    private float lastSecondarySkillTime = -999f;

    private void Start()
    {
        equipmentSkillManager = GetComponent<EquipmentSkillManager>();
    }

    void Update()
    {
        // 1. 일반 공격 (Q, 마우스 좌클릭)
        if (CheckAttackInput())
        {
            PerformAttack();
        }

        // 2. 보조 스킬 (마우스 우클릭)
        if (CheckSecondarySkillInput())
        {
            PerformSecondarySkill();
        }
    }

    /// <summary>
    /// 일반 공격 입력 감지 (숫자키 1~5 제거하여 스킬 키 충돌 방지)
    /// </summary>
    bool CheckAttackInput()
    {
        if (Keyboard.current == null || Mouse.current == null) return false;

        return Keyboard.current.qKey.wasPressedThisFrame ||
               Mouse.current.leftButton.wasPressedThisFrame;
    }

    bool CheckSecondarySkillInput()
    {
        if (Mouse.current == null) return false;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (Time.time >= lastSecondarySkillTime + secondaryCooldown)
            {
                return true;
            }
        }
        return false;
    }

    public void PerformAttack()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);

        float currentMultiplier = 1.0f;
        if (equipmentSkillManager != null)
        {
            currentMultiplier = equipmentSkillManager.GetCurrentDamageMultiplier();
        }

        float finalDamage = attackDamage * currentMultiplier;

        foreach (Collider enemyCollider in targets)
        {
            Vector3 directionToTarget = (enemyCollider.transform.position - transform.position);
            directionToTarget.y = 0;

            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget.normalized);

            if (angleToTarget <= attackAngle / 2f)
            {
                EnemyHP monsterHP = enemyCollider.GetComponent<EnemyHP>();
                if (monsterHP != null)
                {
                    monsterHP.TakeDamage(finalDamage);
                }
            }
        }
    }

    public void PerformSecondarySkill()
    {
        lastSecondarySkillTime = Time.time;
        Debug.Log("🔥 보조 스킬 발동!");

        Collider[] targets = Physics.OverlapSphere(transform.position, secondaryAttackRange, enemyLayer);

        float currentMultiplier = 1.0f;
        if (equipmentSkillManager != null)
        {
            currentMultiplier = equipmentSkillManager.GetCurrentDamageMultiplier();
        }

        float finalDamage = secondaryAttackDamage * currentMultiplier;

        foreach (Collider enemyCollider in targets)
        {
            Vector3 directionToTarget = (enemyCollider.transform.position - transform.position);
            directionToTarget.y = 0;

            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget.normalized);

            if (angleToTarget <= secondaryAttackAngle / 2f)
            {
                EnemyHP monsterHP = enemyCollider.GetComponent<EnemyHP>();
                if (monsterHP != null)
                {
                    monsterHP.TakeDamage(finalDamage);
                }
            }
        }
    }
}