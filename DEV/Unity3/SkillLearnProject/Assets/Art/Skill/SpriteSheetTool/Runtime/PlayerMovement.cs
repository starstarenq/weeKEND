using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    private Vector2 moveInput; // 입력받은 X, Y 축 값을 저장할 변수

    private PlayerInput playerInput;

    void Awake()
    {
        // 오브젝트에 붙어있는 Player Input 컴포넌트를 가져옵니다.
        playerInput = GetComponent<PlayerInput>();
    }

    void Start()
    {
        // 1. 시작 시 활성화할 Action Map 이름을 문자열로 지정합니다.
        // (주의: 인풋 액션 에셋에 작성한 Map 이름과 대소문자까지 똑같아야 합니다.)
        playerInput.SwitchCurrentActionMap("Player");

        // 2. 입력 시스템 전체가 작동하도록 켜줍니다.
        playerInput.ActivateInput();

        Debug.Log($"현재 액션 맵: {playerInput.currentActionMap.name}");
    }

    // 1. Input System의 이벤트에 의해 자동으로 호출되는 메서드
    // 컴포넌트의 Behavior가 Send Messages일 때, Action 이름이 'Move'라면 'OnMove'로 매칭됩니다.
    public void OnMove(InputAction.CallbackContext context)
    {
        // 컨텍스트로부터 Vector2 값을 읽어옵니다.
        moveInput = context.ReadValue<Vector2>();
    }

    // 2. 실제 이동 처리는 Update에서 수행합니다.
    private void Update()
    {
        // Vector2 입력값을 3D 공간의 X, Z 축 이동 방향으로 변환합니다.
        Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y).normalized;

        // 프레임 독립적인 이동 구현
        transform.Translate(moveDir * moveSpeed * Time.deltaTime, Space.World);
    }
}
