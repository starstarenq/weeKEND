using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] private TrapWall trapWall; // 연결할 벽 스크립트
    [SerializeField] private Vector3 boxSize = new Vector3(2f, 2f, 2f); // 감지할 사각형 영역 크기
    [SerializeField] private LayerMask playerLayer; // 플레이어의 레이어 지정

    private bool isTriggered = false;

    private void Update()
    {
        if (isTriggered || trapWall == null) return;

        // 함정 위치를 중심으로 사각형 박스 영역 안에 플레이어 레이어가 있는지 검사
        Collider[] hitColliders = Physics.OverlapBox(transform.position, boxSize / 2f, transform.rotation, playerLayer);

        if (hitColliders.Length > 0)
        {
            isTriggered = true; // 중복 실행 방지
            trapWall.MoveWall(); // 벽 이동 시작
        }
    }

    // 에디터의 씬(Scene) 뷰에서 감지 범위를 시각적으로 보여주는 코드
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}
