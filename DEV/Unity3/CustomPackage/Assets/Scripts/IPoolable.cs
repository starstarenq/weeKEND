using UnityEngine;

public interface IPoolable
// Start is called once before the first execution of Update after the MonoBehaviour is created
{
    public void OnPoolSpawned();
   
    
    public void OnPoolDeSpawned();
    
}

