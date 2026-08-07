using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyHP))]
[RequireComponent(typeof(EnemyAttack))] // EnemyAttack 컴포넌트 필수 포함
public class EnemyChase : MonoBehaviour
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
    public float attackTargetDistance = 3f;

    [Tooltip("원 범위를 벗어난 후, 플레이어가 이 거리(m)만큼 더 멀어지면 재추격을 시작합니다.")]
    public float chaseBufferDistance = 0.5f;

    private NavMeshAgent agent;
    private EnemyHP enemyHp;
    private EnemyAttack enemyAttack; // [추가] EnemyAttack 참조
    private Vector3 startPosition;
    private bool isWaiting = false;
    private bool isArrivedAtCircle = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyHp = GetComponent<EnemyHP>();
        enemyAttack = GetComponent<EnemyAttack>(); // [추가] 참조 할당
        startPosition = transform.position;

        if (agent == null)
        {
            Debug.LogError($"{gameObject.name}: NavMeshAgent 컴포넌트가 없습니다!");
            enabled = false;
            return;
        }

        if (enemyHp != null)
        {
            enemyHp.OnTakeDamageEvent.AddListener(StartChasing);
        }

        FindPlayerTarget();
        MoveToRandomPosition();
    }

    void Update()
    {
        if (enemyHp != null && enemyHp.IsDead) return;

        if (currentState == EnemyState.Roaming)
        {
            HandleRoaming();
        }
        else if (currentState == EnemyState.Chasing)
        {
            HandleChasing();
        }
    }

    public void StartChasing()
    {
        if (currentState == EnemyState.Chasing) return;

        if (playerTransform == null)
        {
            FindPlayerTarget();
        }

        if (playerTransform == null) return;

        currentState = EnemyState.Chasing;
        isArrivedAtCircle = false;
        StopAllCoroutines();
        isWaiting = false;

        agent.speed = chaseSpeed;
        agent.stoppingDistance = 0.1f;

        Debug.Log($"{gameObject.name}: 타격을 받아 추격을 시작합니다.");
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
            // [추가] 사거리 내 진입 시 플레이어 공격 시도
            if (enemyAttack != null)
            {
                enemyAttack.TryAttack(playerTransform);
            }

            if (distanceToPlayer > attackTargetDistance + chaseBufferDistance)
            {
                isArrivedAtCircle = false;
                agent.SetDestination(playerTransform.position);
            }
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
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, attackTargetDistance);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, attackTargetDistance + chaseBufferDistance);
        }
    }
}