using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class EquipmentSkillManager : MonoBehaviour
{
    [Header("스킬 쿨타임 (초 단위)")]
    public float skill1Cooldown = 30f;   // 1번 분노: 30초
    public float skill2Cooldown = 240f;  // 2번 탐욕: 4분 (240초)
    public float skill3Cooldown = 600f;  // 3번 슬픔: 10분 (600초)
    public float skill4Cooldown = 360f;  // 4번 사랑: 6분 (360초)
    public float skill5Cooldown = 1200f; // 5번 우정: 20분 (1200초)

    private float[] skillCooldownTimers = new float[5];

    [Header("스킬 상태 변수")]
    private int invulnerableCount = 0;   // 3번 슬픔: 남은 무효화 횟수
    private bool isLoveActive = false;   // 4번 사랑: 데미지 버프 여부
    private float loveDamageMultiplier = 1.0f;

    [Header("외부 참조 컴포넌트")]
    [SerializeField] private PlayerAttack playerAttack;

    private void Start()
    {
        if (playerAttack == null)
            playerAttack = GetComponent<PlayerAttack>();
    }

    private void Update()
    {
        // 쿨타임 타이머 차감
        for (int i = 0; i < skillCooldownTimers.Length; i++)
        {
            if (skillCooldownTimers[i] > 0f)
            {
                skillCooldownTimers[i] -= Time.deltaTime;
            }
        }

        HandleSkillInput();
    }

    private void HandleSkillInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) UseSkill(1);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) UseSkill(2);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame) UseSkill(3);
        else if (Keyboard.current.digit4Key.wasPressedThisFrame) UseSkill(4);
        else if (Keyboard.current.digit5Key.wasPressedThisFrame) UseSkill(5);
    }

    public void UseSkill(int skillIndex)
    {
        int arrayIndex = skillIndex - 1;

        if (skillCooldownTimers[arrayIndex] > 0f)
        {
            Debug.Log($"[Skill {skillIndex}] 쿨타임 중 (남은 시간: {skillCooldownTimers[arrayIndex]:F1}초)");
            return;
        }

        switch (skillIndex)
        {
            case 1: // 분노 (공격 스킬)
                ExecuteSkill1_Wrath();
                skillCooldownTimers[arrayIndex] = skill1Cooldown;
                break;

            case 2: // 탐욕 (1분간 재화 +100%)
                StartCoroutine(ExecuteSkill2_Greed());
                skillCooldownTimers[arrayIndex] = skill2Cooldown;
                break;

            case 3: // 슬픔 (3회 피격 무효)
                ExecuteSkill3_Sorrow();
                skillCooldownTimers[arrayIndex] = skill3Cooldown;
                break;

            case 4: // 사랑 (3분간 데미지 +30%)
                StartCoroutine(ExecuteSkill4_Love());
                skillCooldownTimers[arrayIndex] = skill4Cooldown;
                break;

            case 5: // 우정 (현재 소지 재화 2배)
                ExecuteSkill5_Friendship();
                skillCooldownTimers[arrayIndex] = skill5Cooldown;
                break;
        }
    }

    // --- 스킬 세부 구현 ---

    private void ExecuteSkill1_Wrath()
    {
        Debug.Log("🔥 [스킬 1 - 분노] 보조 공격 발동!");
        if (playerAttack != null)
        {
            // PlayerAttack의 보조 스킬 연동
            playerAttack.PerformSecondarySkill();
        }
    }

    private IEnumerator ExecuteSkill2_Greed()
    {
        Debug.Log("💰 [스킬 2 - 탐욕] 1분간 재화 획득량 +100% 버프!");
        yield return new WaitForSeconds(60f);
        Debug.Log("💰 [스킬 2 - 탐욕] 버프 종료");
    }

    private void ExecuteSkill3_Sorrow()
    {
        invulnerableCount = 3;
        Debug.Log("🛡️ [스킬 3 - 슬픔] 3회 데미지 무효화 배리어 생성!");
    }

    public bool CheckAndConsumeInvulnerability()
    {
        if (invulnerableCount > 0)
        {
            invulnerableCount--;
            Debug.Log($"🛡️ [스킬 3 - 슬픔] 데미지 무효화! (남은 횟수: {invulnerableCount})");
            return true;
        }
        return false;
    }

    private IEnumerator ExecuteSkill4_Love()
    {
        isLoveActive = true;
        loveDamageMultiplier = 1.3f;
        Debug.Log("💖 [스킬 4 - 사랑] 3분간 공격 데미지 +30% 버프 적용!");

        yield return new WaitForSeconds(180f);

        isLoveActive = false;
        loveDamageMultiplier = 1.0f;
        Debug.Log("💖 [스킬 4 - 사랑] 버프 종료");
    }

    public float GetCurrentDamageMultiplier()
    {
        return loveDamageMultiplier;
    }

    private void ExecuteSkill5_Friendship()
    {
        Debug.Log("🤝 [스킬 5 - 우정] 현재 재화 2배 즉시 획득!");
    }
}