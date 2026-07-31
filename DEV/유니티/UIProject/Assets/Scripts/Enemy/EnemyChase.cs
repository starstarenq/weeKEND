using UnityEngine;
using UnityEngine.AI;

public class EnemyChase : MonoBehaviour
{
    public enum EnemyState { Roaming, Chasing }

    [Header("현재 상태")]
    public EnemyState currentState = EnemyState.Roaming;

    [Header("체력 설정")]
    public float maxHp = 100f;
    private float currentHp;

    [Header("배회(Roam) 설정")]
    public float roamRadius = 10f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    [Header("추격(Chase) 및 멈춤 설정")]
    public Transform playerTransform;
    public float chaseSpeed = 5f;
    public float attackTargetDistance = 3f;

    [Tooltip("원 범위를 벗어난 후, 플레이어가 이 거리(m)만큼 더 멀어지면 재추격을 시작합니다.")]
    public float chaseBufferDistance = 0.5f;

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private float normalSpeed;
    private bool isWaiting = false;
    private bool isArrivedAtCircle = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
        normalSpeed = agent.speed;
        currentHp = maxHp;

        if (agent == null)
        {
            Debug.LogError("오브젝트에 NavMeshAgent 컴포넌트가 없습니다!");
            enabled = false;
            return;
        }

        // 시작할 때 플레이어를 자동으로 찾아 태그로 할당 (편의성)
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        MoveToRandomPosition();
    }

    void Update()
    {
        // [수정] 플레이어 키 입력을 실시간으로 체크하던 로직을 완전히 제거했습니다.

        if (currentState == EnemyState.Roaming)
        {
            HandleRoaming();
        }
        else if (currentState == EnemyState.Chasing)
        {
            HandleChasing();
        }
    }

    // [추가] 플레이어의 공격 스크립트에서 이 몬스터를 타격했을 때 호출할 함수
    public void TakeDamage(float damageAmount)
    {
        if (currentHp <= 0) return;

        currentHp -= damageAmount;
        Debug.Log($"{gameObject.name} 피격 당함! 남은 체력: {currentHp}");

        // 기획서 반영: 선제공격을 당하는 순간 배회를 멈추고 추격을 시작 (자유 전투 해제)
        if (currentState == EnemyState.Roaming)
        {
            StartChasing();
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    void StartChasing()
    {
        if (playerTransform == null) return;

        currentState = EnemyState.Chasing;
        isArrivedAtCircle = false;
        StopAllCoroutines();
        isWaiting = false;

        agent.speed = chaseSpeed;
        agent.stoppingDistance = 0.1f;

        Debug.Log($"{gameObject.name}: 선제공격을 받았습니다! 추격을 시작합니다.");
    }

    void HandleRoaming()
    {
        if (isWaiting || agent.pathPending) return;
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(WaitAndMove());
        }
    }

    void HandleChasing()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(playerTransform.position.x, 0, playerTransform.position.z)
        );

        if (!isArrivedAtCircle)
        {
            if (distanceToPlayer > attackTargetDistance)
            {
                agent.SetDestination(playerTransform.position);
            }
            else
            {
                isArrivedAtCircle = true;
                agent.ResetPath();
            }
        }
        else
        {
            if (distanceToPlayer > attackTargetDistance + chaseBufferDistance)
            {
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

    void Die()
    {
        Debug.Log($"{gameObject.name} 사망.");
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(center, roamRadius);

        if (playerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, attackTargetDistance);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, attackTargetDistance + chaseBufferDistance);
        }
    }
}
