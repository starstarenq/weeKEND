using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class FireballLauncher : MonoBehaviour
{
    [Header("파이어볼 능력치")]
    [SerializeField] private float spellTime = 2.0f;     // 시전 시간
    [SerializeField] private float damage = 10f;          // 공격력
    [SerializeField] private float range = 15f;           // 사거리
    [SerializeField] private float speed = 10f;           // 속도
    [SerializeField] private int projectileCount = 1;     // 투사체 갯수
    [SerializeField] private float cooldown = 1f;         // 쿨타임
    [SerializeField] private float spreadAngle = 15f;     // 다중 발사 각도
    [SerializeField] private float postCastDelay = 0.5f;   // 후딜레이 시간

    [Header("설정 및 프리팹")]
    [SerializeField] private GameObject fireballPrefab;    // FireballProjectile 컴포넌트가 포함된 2D 프리팹
    [SerializeField] private Transform spawnPoint;         // 발사 위치

    private float currentCooldown = 0f;
    private Coroutine castCoroutine;

    public Action<float> CoolDownAction;

    public bool IsCasting { get; private set; } = false;

    public void HandleAction(int a)
    {
        Debug.Log($"숫자 : {a}");
    }

    public void HandleAction2(int a)
    {
        Debug.Log($"숫자2 : {a}");
    }


    private void Update()
    {
        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
        }

        // 스페이스바 입력 시 스킬 시전 시작
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TryCastFireball();
        }
    }

    public void TryCastFireball()
    {
        if (currentCooldown > 0 || IsCasting) return;

        if (fireballPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("2D 프리팹 또는 SpawnPoint가 누락되었습니다.");
            return;
        }

        castCoroutine = StartCoroutine(FireEnumerator());
    }

    public void CancelCasting()
    {
        if (IsCasting && castCoroutine != null)
        {
            StopCoroutine(castCoroutine);
            IsCasting = false;
            Debug.LogWarning("❌ [캔슬] 외부 요인으로 인해 2D 시전이 취소되었습니다.");
        }
    }

    private IEnumerator FireEnumerator()
    {
        IsCasting = true;

        // 시전 이펙트를 출력해라
        Debug.Log("🔥 [파이어볼 2D] 시전을 시작합니다.");
        yield return new WaitForSeconds(spellTime);

        // UI Fireball 스킬이 쿨타임 돌아가기 시작한다.

        CoolDownAction?.Invoke(cooldown);

        //coolDownbar.UseSkill(cooldown);
        // ????.Use();
        // ???,Use();


        Debug.Log("🚀 [발사 2D] 시전 완료! 파이어볼을 발사합니다.");
        SpawnProjectiles();

        currentCooldown = cooldown;

        yield return new WaitForSeconds(postCastDelay);
        IsCasting = false;
        Debug.Log("✅ [완료 2D] 후딜레이 종료.");

        // 시전 종료 이펙트 및 사운드가 출력해라
    }

    private void SpawnProjectiles()
    {
        int startIdx = -(projectileCount / 2);

        for (int i = 0; i < projectileCount; i++)
        {
            float angleOffset = (startIdx + i) * spreadAngle;
            if (projectileCount % 2 == 0) angleOffset += spreadAngle / 2f;

            // 💡 2D에서는 Z축을 기준으로 회전해야 평면상에서 부채꼴로 퍼집니다.
            Quaternion spawnRotation = spawnPoint.rotation * Quaternion.Euler(0, 0, angleOffset);
            GameObject projGo = Instantiate(fireballPrefab, spawnPoint.position, spawnRotation);

            if (projGo.TryGetComponent<FireballProjectile>(out var projectile))
            {
                projectile.Initialize(damage, range, speed, gameObject);
            }
            else
            {
                Debug.LogError("프리팹에 FireballProjectile 컴포넌트가 없습니다!");
                Destroy(projGo);
            }
        }
    }
}
