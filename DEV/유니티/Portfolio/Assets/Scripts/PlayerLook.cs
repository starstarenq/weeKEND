using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Transform cameraTransform;   // 메인 카메라의 Transform 연결
    public float mouseSensitivity = 2f; // 마우스 감도
    public float yMinLimit = -30f;      // 위를 볼 때 제한 각도
    public float yMaxLimit = 60f;       // 아래를 볼 때 제한 각도

    public float distance = 5f;         // 캐릭터와 카메라 사이의 거리 (3인칭 카메라 거리)
    public float heightOffset = 1.5f;   // 캐릭터 중심에서 머리 높이만큼 올릴 오프셋

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        // 게임 화면 클릭 시 마우스 커서 숨기기
        Cursor.lockState = CursorLockMode.Locked;

        if (cameraTransform != null)
        {
            Vector3 angles = cameraTransform.eulerAngles;
            rotationX = angles.y;
            rotationY = angles.x;
        }
    }

    void LateUpdate() // 카메라는 캐릭터 이동이 끝난 후 처리하기 위해 LateUpdate를 씁니다.
    {
        if (cameraTransform == null) return;

        // 1. 마우스 입력 받기
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(mouseX) < 0.01f) mouseX = 0f;
        if (Mathf.Abs(mouseY) < 0.01f) mouseY = 0f;

        // 2. 입력값 누적 계산
        rotationX += mouseX * mouseSensitivity;
        rotationY -= mouseY * mouseSensitivity;

        // 3. 위아래 회전 제한 (뒤집힘 방지)
        rotationY = Mathf.Clamp(rotationY, yMinLimit, yMaxLimit);

        // 4. 좌우 회전값으로 캐릭터 몸통 회전 시키기
        transform.rotation = Quaternion.Euler(0f, rotationX, 0f);

        // 5. [핵심] 카메라의 회전(Rotation) 계산
        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0f);

        // 6. [핵심] 캐릭터 머리 위치 기준(오프셋 적용)으로 카메라를 뒤로 밀어 위치(Position) 정하기
        Vector3 targetPosition = transform.position + Vector3.up * heightOffset;
        Vector3 cameraPosition = targetPosition - (rotation * Vector3.forward * distance);

        // 7. 카메라에 계산된 회전과 위치를 최종 대입
        cameraTransform.rotation = rotation;
        cameraTransform.position = cameraPosition;
    }
}
