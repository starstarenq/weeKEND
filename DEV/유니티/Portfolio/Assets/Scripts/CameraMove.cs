using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform cameraTransform; // 메인 카메라의 Transform을 드래그 등록
    public float moveSpeed = 5f;
    public float rotationSpeed = 0.1f; // 부드럽게 도는 시간 (값이 작을수록 빨리 회전)

    private float turnCalVelocity;     // [수정] SmoothDamp 함수가 내부 계산용으로 쓸 임시 변수

    void Update()
    {
        // 키보드 입력 받기 (WASD)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // 카메라가 바라보는 Y축 기준 방향 계산
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;

            // [수정] 3번째 자리에 rotationSpeed 대신 내부 계산용 turnCalVelocity를 넣어야 오류가 안 납니다.
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnCalVelocity, rotationSpeed);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // 카메라 정면 기준으로 캐릭터 이동
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime, Space.World);
        }
    }
}
