using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    [Header("점프 설정")]
    public float jumpForce = 10f;         // 점프 힘 (기존보다 상향)
    public float jumpCooldown = 2.0f;     // 점프 쿨타임 (2초)

    [Header("바닥 감지 설정")]
    public Transform groundCheck;         // 발밑 빈 오브젝트
    public float groundDistance = 0.2f;    // 땅 감지 반경
    public LayerMask groundMask;          // 땅 레이어

    private Rigidbody rb;
    private bool isGrounded;
    private float nextJumpTime = 0f;
    private float jumpIgnoreTimer = 0f;   // 점프 도약 직후 바닥 감지 유예 타이머

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (groundCheck == null) return;

        // 점프 직후 0.15초 동안은 바닥 체크를 false로 강제하여 착지 오작동 방지
        if (Time.time < jumpIgnoreTimer)
        {
            isGrounded = false;
        }
        else
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask, QueryTriggerInteraction.Ignore);
        }

        bool jumpPressed = Input.GetButtonDown("Jump");
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpPressed = true;
        }

        if (jumpPressed && isGrounded && Time.time >= nextJumpTime)
        {
            nextJumpTime = Time.time + jumpCooldown;
            jumpIgnoreTimer = Time.time + 0.15f; // 점프 순간 0.15초간 바닥 감지 중단

            // Y 속도를 깔끔하게 0으로 리셋 후 순간 힘(Impulse) 적용
            Vector3 vel = rb.linearVelocity;
            vel.y = 0f;
            rb.linearVelocity = vel;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}