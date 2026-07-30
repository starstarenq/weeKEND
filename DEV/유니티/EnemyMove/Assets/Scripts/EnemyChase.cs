using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI; // UI 컴포넌트 사용을 위해 필수 추가

public class EnemyChase : MonoBehaviour
{
    public enum EnemyState { Roaming, Chasing, Stunned }

    [Header("현재 상태")]
    public EnemyState currentState = EnemyState.Roaming;
    private EnemyState savedStateBeforeStun;

    [Header("체력 및 UI 설정")]
    public float maxHp = 100f;
    private float currentHp;
    [SerializeField] private Slider hpSlider; // [추가] 머리 위 UI Slider 연결용 변수

    [Header("배회(Roam) 설정")]
    public float roamRadius = 10f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    [Header("추격(Chase) 및 멈춤 설정")]
    public Transform playerTransform;
    public float chaseSpeed = 5f;
    public float attackTargetDistance = 3f;
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

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        // [추가] 시작할 때 체력바 UI 초기화
        UpdateHPBar();

        MoveToRandomPosition();
    }

    void Update()
    {
        if (currentState == EnemyState.Stunned) return;

        if (currentState == EnemyState.Roaming)
        {
            HandleRoaming();
        }
        else if (currentState == EnemyState.Chasing)
        {
            HandleChasing();
        }
    }

    public void TakeDamage(float damageAmount, bool applyStun = false)
    {
        if (currentHp <= 0) return;

        currentHp -= damageAmount;
        Debug.Log($"{gameObject.name} 피격! 데미지: {damageAmount} | 남은 체력: {currentHp}");

        // [추가] 피격될 때마다 실시간으로 UI 게이지 갱신
        UpdateHPBar();

        if (applyStun && currentHp > 0)
        {
            ApplyStunEffect(0.5f);
        }
        else if (currentState == EnemyState.Roaming)
        {
            StartChasing();
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    // [추가] 체력 비율을 계산하여 슬라이더 값(0~1)에 반영하는 함수
    private void UpdateHPBar()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHp / maxHp;
        }
    }

    public void ApplyStunEffect(float duration)
    {
        if (currentState == EnemyState.Stunned)
        {
            StopCoroutine("StunRoutine");
        }
        else
        {
            savedStateBeforeStun = currentState;
        }

        currentState = EnemyState.Stunned;
        agent.isStopped = true;
        agent.ResetPath();

        StartCoroutine(StunRoutine(duration));
    }

    System.Collections.IEnumerator StunRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        currentState = EnemyState.Chasing;
        agent.isStopped = false;
        agent.speed = chaseSpeed;
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
        Destroy(gameObject);
    }
}
