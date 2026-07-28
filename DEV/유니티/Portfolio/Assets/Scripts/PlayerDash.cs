using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerDash : MonoBehaviour
{
    private CharacterController controller;

    [Header("회피/돌진(Dash) 설정")]
    public float dashSpeed = 15f;       // 돌진 속도 (원하는 속도감에 맞게 인스펙터에서 수정)
    public float dashDuration = 0.2f;    // 돌진 지속 시간 (초 단위)
    public float dashCooldown = 1.0f;    // 회피 재사용 대기시간

    private bool isDashing = false;      // 현재 돌진 중인지 여부
    private bool isCooldown = false;     // 쿨타임 여부
    public bool isInvincible = false;    // 무적 상태 플래그 (타 스크립트에서 참조 가능)

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // 대시 중이 아니고 쿨타임이 아닐 때, Shift 키 입력 감지
        if (!isDashing && !isCooldown && Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(DashCoroutine());
        }
    }

    // 자연스러운 대시 및 무적 판정 코루틴
    IEnumerator DashCoroutine()
    {
        isDashing = true;
        isInvincible = true; // 무적 판정 시작
        Debug.Log("★ 회피 시작! (무적 상태 활성화)");

        // 현재 눌려 있는 WASD 입력 방향만 체크하여 대시 방향 결정 (이동은 시키지 않음)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        Vector3 dashDirection = new Vector3(moveX, 0f, moveZ).normalized;

        // 만약 정지 상태(입력 없음)라면 캐릭터가 현재 바라보고 있는 정면 방향으로 대시
        if (dashDirection.magnitude < 0.1f)
        {
            dashDirection = transform.forward;
        }

        // 대시할 방향으로 캐릭터의 회전 고정
        float targetAngle = Mathf.Atan2(dashDirection.x, dashDirection.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

        float startTime = Time.time;

        // 지정된 시간(dashDuration) 동안 매 프레임 부드럽게 미끄러지듯 이동
        while (Time.time < startTime + dashDuration)
        {
            // CharacterController를 통해 벽을 뚫지 않고 미끄러지듯 이동 구현
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            yield return null;
        }

        isDashing = false;
        isInvincible = false; // 무적 판정 종료
        Debug.Log("☆ 회피 종료! (무적 상태 해제)");

        // 회피 재사용 대기시간(쿨타임) 작동
        StartCoroutine(DashCooldownCoroutine());
    }

    // 회피 재사용 대기시간 처리 코루틴
    IEnumerator DashCooldownCoroutine()
    {
        isCooldown = true;
        yield return new WaitForSeconds(dashCooldown);
        isCooldown = false;
        Debug.Log("회피 재사용 가능(쿨타임 완료)");
    }
}
