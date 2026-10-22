using TTest.UnityUtilities.Pooling;
using Unity.VisualScripting;
using UnityEngine;



public class Bullet : MonoBehaviour, IPoolable
{
    [SerializeField] private float speed = 20f, life = 2f;
    private float _age;
    public PoolSpawner Owner;                   // 나를 꺼낸 풀(돌려보낼 곳)

    public void OnPoolSpawned() => _age = 0f;   // 깨어날 때 타이머 리셋
    public void OnPoolDeSpawned() { }

    private void Update()
    {
        transform.Translate(Vector3.forward*(speed * Time.deltaTime));
        _age += Time.deltaTime;
        if (_age >= life) Owner.Despawn(gameObject);  // 수명 끝 → 반납(Destroy 아님!)
    }
}
