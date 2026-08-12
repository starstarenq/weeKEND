using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Transform cameraTransform;   // 메인 카메라 Transform
    public float moveSpeed = 6f;          // 이동 속도
    public float rotationSpeed = 0.1f;    // 회전 속도

    private Rigidbody rb;
    private float turnCalVelocity;
    private Vector3 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = true;
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        moveInput = new Vector3(horizontal, 0f, vertical).normalized;
    }

    void FixedUpdate()
    {
        if (moveInput.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(moveInput.x, moveInput.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnCalVelocity, rotationSpeed);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // X, Z 속도만 제어하고 Y 속도(점프/중력)는 변경하지 않음
            rb.linearVelocity = new Vector3(moveDirection.x * moveSpeed, rb.linearVelocity.y, moveDirection.z * moveSpeed);
        }
        else
        {
            // 입력이 없어도 Y 속도는 유지
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }
}