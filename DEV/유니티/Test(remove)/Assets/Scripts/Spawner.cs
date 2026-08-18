using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject[] enemyPrefabs; // Enemy1, Enemy2, Enemy3을 인스펙터에서 할당

    [Header("Spawn Settings")]
    public float spawnDistance = 10f; // 플레이어로부터의 거리
    public float spawnInterval = 2f;  // 스폰 주기 (초)

    private Transform playerTransform;

    void Start()
    {
        SpawnPlayer();
        // 지정된 시간(spawnInterval)마다 SpawnEnemy 함수를 반복 실행
        InvokeRepeating(nameof(SpawnEnemy), spawnInterval, spawnInterval);
    }

    void SpawnPlayer()
    {
        if (playerPrefab != null)
        {
            // 플레이어를 월드 정중앙(0, 0, 0)에 생성
            GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player Prefab이 할당되지 않았습니다!");
        }
    }

    void SpawnEnemy()
    {
        if (playerTransform == null || enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        // 1. 2D 원형 랜덤 방향 벡터 생성 (X, Z 평면 기준)
        Vector2 randomCircle = Random.insideUnitCircle.normalized;

        // 2. 플레이어 위치 기준으로 거리(spawnDistance)만큼 떨어진 3D 좌표 계산
        Vector3 spawnPosition = playerTransform.position + new Vector3(randomCircle.x, 0f, randomCircle.y) * spawnDistance;

        // 3. 배열에 등록된 적 프리팹 중 하나를 랜덤 선택
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject selectedEnemy = enemyPrefabs[randomIndex];

        // 4. 생성 및 플레이어를 바라보도록 회전 처리
        if (selectedEnemy != null)
        {
            Vector3 lookDirection = playerTransform.position - spawnPosition;
            Quaternion spawnRotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0f, lookDirection.z));

            Instantiate(selectedEnemy, spawnPosition, spawnRotation);
        }
    }
}
