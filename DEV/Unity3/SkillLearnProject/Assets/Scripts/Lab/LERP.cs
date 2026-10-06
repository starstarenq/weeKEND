using UnityEngine;


public class LERP : MonoBehaviour
{

    [Header("이동 경로 설정")]
    public Transform startTransform; // 시작 위치
    public Transform endTransform;   // 목표 위치

    [Header("시간 설정 (초)")]
    public float duration = 5.0f;    // 총 이동 시간 (5초)

    private float timeElapsed = 0.0f; // 누적된 경과 시간
    private bool isMoving = true;     // 이동 진행 여부

    void Start()
    {
        // 시작할 때 오브젝트를 시작 위치로 강제 설정
        if (startTransform != null)
        {
            transform.position = startTransform.position;
        }
    }

    void Update()
    {
        // 이동이 끝났다면 실행하지 않음
        if (!isMoving) return;

        // 1. 매 프레임의 시간을 누적
        timeElapsed += Time.deltaTime;

        // 2. 0~1 사이의 비율(t) 계산 (누적 시간 / 총 시간)
        float t = timeElapsed / duration;

        // 3. 안전장치: t가 1을 넘지 않도록 제한 (5초 도달 시 정확히 1이 됨)
        t = Mathf.Clamp01(t);

        // 4. Lerp를 이용한 위치 이동
        if (startTransform != null && endTransform != null)
        {
            transform.position = Vector3.Slerp(startTransform.position, endTransform.position, t);
        }

        // 5. 5초 완료 시 상태 변경 (더 이상 계산하지 않도록)
        if (t >= 1.0f)
        {
            isMoving = false;
            Debug.Log("5초 동안의 이동이 완료되었습니다.");
        }
    }

    // 테스트용: R 키를 누르면 언제든 이동을 리셋하고 다시 시작
    void ResetMovement()
    {
        timeElapsed = 0.0f;
        isMoving = true;
        if (startTransform != null) transform.position = startTransform.position;
    }
}
