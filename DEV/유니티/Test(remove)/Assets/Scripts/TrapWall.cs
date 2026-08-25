using System.Collections;
using UnityEngine;

public class TrapWall : MonoBehaviour
{
    private bool isMoving = false;

    // 벽을 Y축으로 4만큼 이동시키는 메서드
    public void MoveWall()
    {
        // 이미 움직이고 있다면 중복 실행 방지
        if (!isMoving)
        {
            StartCoroutine(MoveRoutine());
        }
    }

    private IEnumerator MoveRoutine()
    {
        isMoving = true;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + new Vector3(0, 4f, 0); // Y축으로 +4
        float duration = 3f; // 3초 동안 이동
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            // Lerp를 사용하여 시간에 따라 부드럽게 이동
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            yield return null;
        }

        // 마지막 위치 정확히 맞추기
        transform.position = targetPosition;
    }
}
