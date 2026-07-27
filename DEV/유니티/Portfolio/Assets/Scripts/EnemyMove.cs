using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMove : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("이동 설정")]
    public float wanderRadius = 10f;  // 랜덤 목표지점을 찾을 반경
    public float minWaitTime = 1f;    // 목적지 도착 후 최소 대기 시간
    public float maxWaitTime = 3f;    // 목적지 도착 후 최대 대기 시간

    private float waitTimer;
    private bool isWaiting;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // 시작할 때 적이 내비메시 영역 밖에 있다면 가장 가까운 바닥으로 강제 순간이동(Warp)
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 10.0f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }

        SetNewRandomDestination();
    }

    void Update()
    {
        // 적이 이동 중이고, 목적지에 거의 도착했는지 확인
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!isWaiting)
            {
                // 대기 상태 시작 (랜덤 대기 시간 설정)
                isWaiting = true;
                waitTimer = Random.Range(minWaitTime, maxWaitTime);
            }
        }

        // 대기 중일 때 타이머 작동
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                SetNewRandomDestination(); // 대기가 끝나면 새로운 목적지 설정
            }
        }
    }

    // NavMesh 위의 유효한 랜덤 좌표를 찾는 함수
    void SetNewRandomDestination()
    {
        // 1. 적 주변으로 너무 멀지 않게 무작위 방향 벡터 생성
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit navHit;

        // 2. [핵심] 영역을 NavMesh.AllAreas 대신 Walkable 레이어(기본값 1)로 고정합니다.
        // 또한 최대 탐색 거리를 명확히 주어 수학적 에러(Infinity) 발생을 차단합니다.
        if (NavMesh.SamplePosition(randomDirection, out navHit, wanderRadius, 1))
        {
            // 3. 찾은 좌표가 비정상적인 값(Infinity, NaN 등)이 아닐 때만 목적지로 설정
            if (!float.IsInfinity(navHit.position.x) && !float.IsNaN(navHit.position.x))
            {
                agent.SetDestination(navHit.position);
            }
        }
    }

    // 버전에 따른 NavMesh 호환성을 위한 샘플링 함수
    bool NavMeshSamplePosition(Vector3 sourcePosition, out NavMeshHit hit, float maxDistance, int areaMask)
    {
#if UNITY_2022_1_OR_NEWER
        return NavMesh.SamplePosition(sourcePosition, out hit, maxDistance, areaMask);
#else
        return NavMesh.SamplePosition(sourcePosition, out hit, maxDistance, areaMask);
#endif
    }
}
