using TTest.UnityUtilities.Pooling;
using UnityEngine;
using UnityEngine.InputSystem;

public class Spawn : MonoBehaviour
{
    // Instantiate - bullet , monster, particle(effect)
    // Destory

    [SerializeField] PoolSpawner spawner;
    [SerializeField] Transform _muzzle;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Bullet b = spawner.Spawn<Bullet>();
            b.transform.SetPositionAndRotation(_muzzle.position, _muzzle.rotation);
            b.Owner = spawner;
        }
    }
}