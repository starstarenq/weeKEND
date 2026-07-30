using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Transform cameraTransform; // 메인 카메라의 Transform
    public float moveSpeed = 5f;       // 이동 속도
    public float rotationSpeed = 0.1f; // 회전 속도 (작을수록 빠름)

    [Header("점프 설정")]
    public float jumpForce = 7f;       // 점프 힘 (높이)
    public Transform groundCheck;      // 캐릭터 발밑에 배치할 빈 오브젝트 위치
    public float groundDistance = 0.2f; // 땅 감지 반경
    public LayerMask groundMask;       // 땅으로 인식할 레이어 지정

    private Rigidbody rb;
    private float turnCalVelocity;     // SmoothDamp 내부 계산용
    private bool isGrounded;           // 현재 땅에 닿아 있는지 여부

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 물리 충돌로 인해 캐릭터가 옆으로 쓰러지는 현상 방지
        rb.freezeRotation = true;
    }

    void Update()
    {
        // 1. [점프 핵심] 캐릭터 발 밑에 지정된 레이어(Ground)의 땅이 있는지 실시간 체크
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // 2. 키보드 입력 받기 (WASD)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // 카메라가 바라보는 Y축 기준 방향 계산
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;

            // 캐릭터가 부드럽게 이동 방향을 바라보도록 회전
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnCalVelocity, rotationSpeed);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // 카메라 정면 기준으로 캐릭터 이동 방향 계산
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // 물리 속도(Velocity)를 적용하여 정면으로 이동 (중력 값인 rb.velocity.y 유지)
            rb.linearVelocity = new Vector3(moveDirection.normalized.x * moveSpeed, rb.linearVelocity.y, moveDirection.normalized.z * moveSpeed);
        }
        else
        {
            // 키를 누르지 않을 때는 좌우 이동 속도를 0으로 만들어 미끄러짐 방지 (중력 값은 유지)
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        // 3. [점프 실행] 땅에 닿아 있는 상태에서 스페이스바를 누르면 위로 발사
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Rigidbody의 Y축 속도를 순간적으로 jumpForce만큼 올려줍니다.
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
        }
    }

    // 에디터 뷰에서 땅 감지 영역을 시각적으로 확인하기 위한 기능
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
