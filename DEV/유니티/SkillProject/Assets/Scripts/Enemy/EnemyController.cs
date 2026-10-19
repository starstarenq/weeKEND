using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    public enum EnemyState { Roaming, Chasing }

    [Header("적 데이터 설정 (ScriptableObject)")]
    [SerializeField] private EnemyData enemyData;

    [Header("현재 상태 및 타겟")]
    public EnemyState currentState = EnemyState.Roaming;
    public Transform playerTransform;

    [Header("UI 연동 (선택 사항)")]
    [SerializeField] private Image hpBarFillImage;
    [SerializeField] private Slider hpBarSlider;

    [Header("피격 / 사망 이벤트")]
    public UnityEvent OnTakeDamageEvent;
    public UnityEvent OnDieEvent;

    // --- 내부 관리 변수 ---
    private float maxHp;
    private float currentHp;
    private float emotionReward = 10f;
    public bool IsDead { get; private set; } = false;
    public float MaxHP => maxHp;
    public float CurrentHP => currentHp;

    private float baseAttackDamage = 10f;
    public float attackDamage { get; private set; }
    public float attackRate { get; private set; } = 1.5f;
    public float attackRange { get; private set; } = 3.5f;
    private float lastAttackTime = 0f;

    private float roamRadius = 10f;
    private float minWaitTime = 2f;
    private float maxWaitTime = 5f;
    private float chaseSpeed = 5f;
    private float attackTargetDistance = 3f;
    private float chaseBufferDistance = 0.5f;

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private bool isWaiting = false;
    private bool isArrivedAtCircle = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;

        InitData();
    }

    private void Start()
    {
        if (agent == null)
        {
            Debug.LogError($"{gameObject.name}: NavMeshAgent 컴포넌트가 없습니다!");
            enabled = false;
            return;
        }

        // 공중 뜸 방지 바닥 착지 보정
        agent.enabled = false;

        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position + Vector3.up * 5f, Vector3.down, out hitInfo, 20f))
        {
            transform.position = hitInfo.point;
        }

        NavMeshHit navHit;
        if (NavMesh.SamplePosition(transform.position, out navHit, 5.0f, NavMesh.AllAreas))
        {
            transform.position = navHit.position;
            agent.Warp(navHit.position);
        }

        agent.enabled = true;
        startPosition = transform.position;
        agent.speed = chaseSpeed;

        OnTakeDamageEvent.AddListener(StartChasing);

        FindPlayerTarget();
        MoveToRandomPosition();
    }

    private void OnEnable()
    {
        UI_EmotionGauge.OnEmotionChanged += ApplyEmotionStatModifier;

        UI_EmotionGauge gauge = FindAnyObjectByType<UI_EmotionGauge>();
        if (gauge != null)
        {
            ApplyEmotionStatModifier(gauge.CurrentEmotion);
        }
    }

    private void OnDisable()
    {
        UI_EmotionGauge.OnEmotionChanged -= ApplyEmotionStatModifier;
    }

    private void Update()
    {
        if (IsDead) return;

        if (currentState == EnemyState.Roaming)
        {
            HandleRoaming();
        }
        else if (currentState == EnemyState.Chasing)
        {
            HandleChasing();
        }
    }

    private void InitData()
    {
        if (enemyData != null)
        {
            maxHp = enemyData.BaseMaxHp;
            emotionReward = enemyData.EmotionReward;

            baseAttackDamage = enemyData.BaseAttackDamage;
            attackRate = enemyData.attackRate;
            attackRange = enemyData.attackRange;

            roamRadius = enemyData.roamRadius;
            minWaitTime = enemyData.minWaitTime;
            maxWaitTime = enemyData.maxWaitTime;
            chaseSpeed = enemyData.chaseSpeed;
            attackTargetDistance = enemyData.attackTargetDistance;
            chaseBufferDistance = enemyData.chaseBufferDistance;
        }
        else
        {
            maxHp = 100f;
        }

        currentHp = maxHp;
        attackDamage = baseAttackDamage;
        UpdateHpUI();
    }

    private void ApplyEmotionStatModifier(float emotionValue)
    {
        if (IsDead || enemyData == null) return;

        float modifier = 1.0f + (emotionValue * 0.001f);

        float hpRatio = maxHp > 0 ? currentHp / maxHp : 1f;
        maxHp = enemyData.BaseMaxHp * modifier;
        currentHp = maxHp * hpRatio;
        UpdateHpUI();

        attackDamage = baseAttackDamage * modifier;
    }

    public void TakeDamage(float damageAmount)
    {
        if (IsDead || currentHp <= 0) return;

        currentHp -= damageAmount;
        currentHp = Mathf.Clamp(currentHp, 0f, maxHp);

        UpdateHpUI();

        if (UI_InGameScene.Instance != null)
        {
            UI_InGameScene.Instance.ShowHPBar(gameObject.name, currentHp, maxHp);
        }

        OnTakeDamageEvent?.Invoke();

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        if (UI_InGameScene.Instance != null)
        {
            UI_InGameScene.Instance.AddEmotion(emotionReward);
            UI_InGameScene.Instance.AddDeath(30);

#if UNITY_2023_1_OR_NEWER
            EnemyController[] allEnemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
#else
            EnemyController[] allEnemies = FindObjectsOfType<EnemyController>();
#endif

            int aliveCount = 0;
            foreach (var enemy in allEnemies)
            {
                if (enemy != this && !enemy.IsDead)
                {
                    aliveCount++;
                }
            }

            if (aliveCount == 0)
            {
                UI_InGameScene.Instance.AddMemory(1);
            }
        }

        OnDieEvent?.Invoke();
        Destroy(gameObject);
    }

    private void UpdateHpUI()
    {
        float ratio = maxHp > 0 ? currentHp / maxHp : 0f;

        if (hpBarFillImage != null) hpBarFillImage.fillAmount = ratio;
        if (hpBarSlider != null) hpBarSlider.value = ratio;
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
    }

    private void HandleRoaming()
    {
        if (isWaiting || agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(WaitAndMove());
        }
    }

    private void HandleChasing()
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
            TryAttack(playerTransform);

            if (distanceToPlayer > attackTargetDistance + chaseBufferDistance)
            {
                isArrivedAtCircle = false;
                agent.SetDestination(playerTransform.position);
            }
        }
    }

    private IEnumerator WaitAndMove()
    {
        isWaiting = true;
        float randomWaitTime = Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(randomWaitTime);
        MoveToRandomPosition();
        isWaiting = false;
    }

    private void MoveToRandomPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
        randomDirection += startPosition;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void FindPlayerTarget()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }
    }

    public void TryAttack(Transform target)
    {
        if (IsDead || target == null) return;

        if (Time.time >= lastAttackTime + attackRate)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= attackRange)
            {
                PerformAttack(target);
                lastAttackTime = Time.time;
            }
        }
    }

    private void PerformAttack(Transform target)
    {
        PlayerHP playerHp = target.GetComponent<PlayerHP>();
        if (playerHp != null)
        {
            playerHp.TakeDamage(attackDamage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 1. 배회 반경 기즈모 (초록색, 적 기준)
        Gizmos.color = Color.green;
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        float currentRoam = enemyData != null ? enemyData.roamRadius : roamRadius;
        Gizmos.DrawWireSphere(center, currentRoam);

        // 2. 공격 범위 기즈모 수정 (빨간색, 적 기준)[cite: 4]
        float currentAttackRange = enemyData != null ? enemyData.attackRange : attackRange;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, currentAttackRange); // [수정됨] playerTransform 기준에서 적 자신 기준으로 변경

        // 3. 추격 및 공격 정지 거리 기즈모 (노란색/빨간색, 플레이어 기준 유지)
        if (playerTransform != null)
        {
            float currentAttackDist = enemyData != null ? enemyData.attackTargetDistance : attackTargetDistance;
            float currentBuffer = enemyData != null ? enemyData.chaseBufferDistance : chaseBufferDistance;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, currentAttackDist);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, currentAttackDist + currentBuffer);
        }
    }
}