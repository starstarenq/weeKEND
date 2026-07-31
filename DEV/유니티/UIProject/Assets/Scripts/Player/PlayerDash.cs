using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerDash : MonoBehaviour
{
    private CharacterController controller;

    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float gravity = -20f;        // 점프 후 더 빠르게 내려오도록 중력 기본값 강화
    public float jumpHeight = 2f;

    private Vector3 moveDirection;
    private Vector3 velocity;

    [Header("Ground Check Settings")]
    public Transform groundCheck;       // 캐릭터 발밑에 배치할 빈 오브젝트 (가장 중요)
    public float groundDistance = 0.3f; // 바닥 감지 반경
    public LayerMask groundMask;        // 바닥으로 인식할 레이어
    private bool isGrounded;            // 실시간 바닥 체크 결과

    [Header("Dash Settings")]
    public float dashSpeed = 25f;
    public float dashDuration = 1f;
    public float dashCooldown = 2f;

    private bool isDashing = false;
    private bool canDash = true;

    [HideInInspector]
    public bool isInvincible = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (isDashing) return;

        // 1. 물리 기반 물리 구체(CheckSphere)로 정확한 바닥 감지
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 바닥에 안정적으로 붙어있도록 유도
        }

        // 2. 키보드 이동 입력
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        moveDirection = transform.right * x + transform.forward * z;
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        // 3. 점프 입력 처리 (개선된 바닥 체크 변수 사용)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 4. 대쉬 입력 처리
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(DashCoroutine());
            return;
        }

        // 5. 중력 적용
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private IEnumerator DashCoroutine()
    {
        canDash = false;
        isDashing = true;
        isInvincible = true;

        Vector3 dashDir = moveDirection.magnitude > 0.1f ? moveDirection.normalized : transform.forward;
        velocity.y = 0;

        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            controller.Move(dashDir * dashSpeed * Time.deltaTime);
            yield return null;
        }

        isDashing = false;
        isInvincible = false;
        velocity.y = -2f;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // 인스펙터창 밖에서도 바닥 감지 범위를 시각적으로 볼 수 있게 해주는 기능
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
