using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Scriptable Objects/Enemy Data", order = 1)]
public class EnemyData : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("적의 이름 또는 타입")]
    public string enemyName = "Basic Enemy";

    [Header("체력 설정 (EnemyHP 연동)")]
    [SerializeField] private float baseMaxHp = 100f;
    public float BaseMaxHp => baseMaxHp;

    [Header("사망 보상 설정")]
    [SerializeField] private float emotionReward = 10f;
    public float EmotionReward => emotionReward;

    [Header("공격 설정 (EnemyAttack 연동)")]
    [SerializeField] private float baseAttackDamage = 10f;
    public float BaseAttackDamage => baseAttackDamage;
    public float attackRate = 1.5f;         // 공격 주기 (초)
    public float attackRange = 3.5f;        // 공격 가능 최대 거리

    [Header("배회 및 추격 설정 (EnemyChase 연동)")]
    public float roamRadius = 10f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;
    public float chaseSpeed = 5f;
    public float attackTargetDistance = 3f;
    public float chaseBufferDistance = 0.5f;
}