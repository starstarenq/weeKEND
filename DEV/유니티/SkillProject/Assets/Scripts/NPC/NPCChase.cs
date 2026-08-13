using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NPCHP))]
public class NPCChase : MonoBehaviour
{
    public enum NPCState { Roaming, Fleeing }

    [Header("현재 상태")]
    public NPCState currentState = NPCState.Roaming;

    [Header("배회(Roam) 설정")]
    public float roamRadius = 10f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    [Header("도망(Flee) 설정")]
    public Transform playerTransform;
    public float fleeSpeed = 6f;          // 도망 속도 (기존 배회보다 빠르게 설정)
    public float fleeDistance = 8f;        // 플레이어로부터 도망칠 목표 거리
    public float safeDistance = 12f;       // 플레이어와 이 이상 떨어지면 다시 배회 상태로 변경

    private NavMeshAgent agent;
    private NPCHP npcHp;
    private Vector3 startPosition;
    private bool isWaiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        npcHp = GetComponent<NPCHP>();
        startPosition = transform.position;

        if (agent == null)
        {
            Debug.LogError($"{gameObject.name}: NavMeshAgent 컴포넌트가 없습니다!");
            enabled = false;
            return;
        }

        // 피격 시 도망치는 함수 연결
        if (npcHp != null)
        {
            npcHp.OnTakeDamageEvent.AddListener(StartFleeing);
        }

        FindPlayerTarget();
        MoveToRandomPosition();
    }

    void Update()
    {
        if (npcHp != null && npcHp.IsDead) return;

        if (currentState == NPCState.Roaming)
        {
            HandleRoaming();
        }
        else if (currentState == NPCState.Fleeing)
        {
            HandleFleeing();
        }
    }

    /// <summary>
    /// 공격을 받았을 때 호출되어 플레이어 반대 방향으로 도망 모드 시작
    /// </summary>
    public void StartFleeing()
    {
        if (playerTransform == null)
        {
            FindPlayerTarget();
        }

        if (playerTransform == null) return;

        currentState = NPCState.Fleeing;
        StopAllCoroutines();
        isWaiting = false;

        agent.speed = fleeSpeed;
        agent.stoppingDistance = 0f;

        Debug.Log($"{gameObject.name}: 공격을 받아 플레이어로부터 도망칩니다!");

        SetFleeDestination();
    }

    void HandleRoaming()
    {
        if (isWaiting || agent.pathPending) return;
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(WaitAndMove());
        }
    }

    void HandleFleeing()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(playerTransform.position.x, 0, playerTransform.position.z)
        );

        // 충분한 안전 거리에 도달하면 다시 평화로운 배회 상태로 복귀
        if (distanceToPlayer >= safeDistance)
        {
            Debug.Log($"{gameObject.name}: 안전 거리를 확보하여 다시 배회 상태로 전환합니다.");
            currentState = NPCState.Roaming;
            agent.speed = 3.5f; // 기본 이동 속도로 복원
            MoveToRandomPosition();
            return;
        }

        // 도망치는 중 지속적으로 플레이어 반대 방향을 업데이트
        if (!agent.pathPending && agent.remainingDistance <= 1f)
        {
            SetFleeDestination();
        }
    }

    /// <summary>
    /// 플레이어 반대 방향 위치를 계산하여 이동 지정
    /// </summary>
    void SetFleeDestination()
    {
        if (playerTransform == null) return;

        // 플레이어에서 NPC 방향 벡터 계산
        Vector3 runDirection = (transform.position - playerTransform.position).normalized;
        Vector3 targetFleePosition = transform.position + runDirection * fleeDistance;

        NavMeshHit hit;
        // NavMesh 상의 유효한 위치인지 탐색
        if (NavMesh.SamplePosition(targetFleePosition, out hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    IEnumerator WaitAndMove()
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

    void FindPlayerTarget()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(center, roamRadius);

        if (playerTransform != null)
        {
            // 도망 범위 시각화 (파란색)
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, fleeDistance);

            // 안전 거리 시각화 (cyan)
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, safeDistance);
        }
    }
}