using TTest.UnityUtilities.Pooling;
using UnityEngine;

public class Monster : MonoBehaviour, IPoolable
{
    public PoolSpawner Owner;                   // 나를 꺼낸 풀(돌려보낼 곳)
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnPoolSpawned() { }   // 깨어날 때 타이머 리셋
    public void OnPoolDeSpawned() { }

    // Update is called once per frame
    void Update()
    {

    }
}