using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem; // 최신 Input System 패키지

public class MonsterRoamAndChase : MonoBehaviour
{
    public enum EnemyState { Roaming, Chasing }

    [Header("현재 상태")]
    public EnemyState currentState = EnemyState.Roaming;

    [Header("배회(Roam) 설정")]
    public float roamRadius = 10f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    [Header("추격(Chase) 및 멈춤 설정")]
    public Transform playerTransform;
    public float chaseSpeed = 5f;
    public float attackTargetDistance = 3f; // 이 원 범위 안으로 들어오면 멈춥니다.
    [Tooltip("원 범위를 벗어난 후, 플레이어가 이 거리(m)만큼 더 멀어지면 재추격을 시작합니다.")]
    public float chaseBufferDistance = 0.5f;

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private float normalSpeed;
    private bool isWaiting = false;
    private bool isArrivedAtCircle = false; // 원 경계에 도달했는지 체크하는 플래그

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        normalSpeed = agent.speed;

        if (agent == null)
        {
            Debug.LogError("오브젝트에 NavMeshAgent 컴포넌트가 없습니다!");
            enabled = false;
            return;
        }

        MoveToRandomPosition();
    }

    void Update()
    {
        // 1. 플레이어의 특정 키 입력 감지
        if (CheckPlayerInput())
        {
            StartChasing();
        }

        // 2. 현재 상태에 따른 행동 수행
        if (currentState == EnemyState.Roaming)
        {
            HandleRoaming();
        }
        else if (currentState == EnemyState.Chasing)
        {
            HandleChasing();
        }
    }

    bool CheckPlayerInput()
    {
        if (Keyboard.current == null || Mouse.current == null) return false;

        return Keyboard.current.qKey.wasPressedThisFrame ||
               Mouse.current.leftButton.wasPressedThisFrame ||
               Keyboard.current.digit1Key.wasPressedThisFrame ||
               Keyboard.current.digit2Key.wasPressedThisFrame ||
               Keyboard.current.digit3Key.wasPressedThisFrame ||
               Keyboard.current.digit4Key.wasPressedThisFrame ||
               Keyboard.current.digit5Key.wasPressedThisFrame;
    }

    void StartChasing()
    {
        if (playerTransform == null) return;

        currentState = EnemyState.Chasing;
        isArrivedAtCircle = false; // 추격 시작 시 플래그 초기화

        StopAllCoroutines();
        isWaiting = false;

        agent.speed = chaseSpeed;

        // 원의 경계선까지만 연산하기 위해 정지 거리는 원상태(0)로 유지하거나 매우 작게 잡습니다.
        agent.stoppingDistance = 0.1f;
    }

    void HandleRoaming()
    {
        if (isWaiting || agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(WaitAndMove());
        }
    }

    // [핵심 수정] 원에 도달하면 멈추고, 플레이어가 움직여 원이 이동하면 다시 추격하는 로직
    void HandleChasing()
    {
        if (playerTransform == null) return;

        // 플레이어와 에너미 사이의 평면(X, Z) 실제 거리를 계산합니다.
        float distanceToPlayer = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(playerTransform.position.x, 0, playerTransform.position.z)
        );

        if (!isArrivedAtCircle)
        {
            // [상태: 추격 중] 아직 플레이어의 원 범위(attackTargetDistance) 안으로 들어가지 않았다면
            if (distanceToPlayer > attackTargetDistance)
            {
                // 계속 플레이어 방향으로 전진합니다.
                agent.SetDestination(playerTransform.position);
            }
            else
            {
                // 원 범위 내부나 테두리에 도달하는 순간 즉시 자리에 브레이크를 밟아 멈춥니다.
                isArrivedAtCircle = true;
                agent.ResetPath(); // 내비메시 이동 명령을 완전히 취소하여 멈춤
            }
        }
        else
        {
            // [상태: 원에 도달하여 대기 중] 플레이어가 이동해서 설정한 원 범위를 완전히 벗어났는지 감지
            // 미세한 떨림 방지를 위해 버퍼 거리(chaseBufferDistance)를 더해 원을 확실히 벗어났을 때만 움직입니다.
            if (distanceToPlayer > attackTargetDistance + chaseBufferDistance)
            {
                // 원 밖으로 나갔으므로 다시 추격 상태로 전환하여 목적지를 갱신합니다.
                isArrivedAtCircle = false;
                agent.SetDestination(playerTransform.position);
            }
        }
    }

    System.Collections.IEnumerator WaitAndMove()
    {
        isWaiting = true;
        float randomWaitTime = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(randomWaitTime);

        MoveToRandomPosition();
        isWaiting = false;
    }

    void MoveToRandomPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
        randomDirection += startPosition;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(center, roamRadius);

        if (playerTransform != null)
        {
            // 에너미가 도달해서 멈추는 기준 원 (빨간색)
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, attackTargetDistance);

            // 플레이어가 이 원 바깥까지 나가야 에너미가 다시 반응합니다 (노란색 버퍼선)
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, attackTargetDistance + chaseBufferDistance);
        }
    }
}
