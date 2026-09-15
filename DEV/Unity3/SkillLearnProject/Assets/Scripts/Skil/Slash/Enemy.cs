using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("적 능력치")]
    [SerializeField] private float health = 100f;

    /// <summary>
    /// 투사체로부터 호출되어 피해를 입는 함수
    /// </summary>
    /// <param name="amount">받는 피해량</param>
    public void TakeDamage(float amount)
    {
        health -= amount;
        Debug.Log($"{gameObject.name}이(가) {amount}의 피해를 입었습니다. (남은 체력: {health})");

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} 사망!");
        Destroy(gameObject);
    }
}
