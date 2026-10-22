using System;
using TTest.UnityUtilities.Pooling;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField] PoolSpawner spawner;
    [SerializeField] Transform spawnPosition;
    [SerializeField] float spawnDelay = 1.0f;
    float checkTime = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
   

    // Update is called once per frame
    void Update()
    {
        checkTime += Time.deltaTime;
        if(checkTime >= spawnDelay)
        {
           
          Monster monster =  spawner.Spawn<Monster>();
            monster.Owner = spawner;
            checkTime = 0.0f;
        }
    }
}
