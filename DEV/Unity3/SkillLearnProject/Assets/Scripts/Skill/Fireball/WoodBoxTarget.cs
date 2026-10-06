using UnityEngine;

public class WoodBoxTarget : MonoBehaviour, IDamageable2D
{
    public int environHitCount = 1;

    public void TakeDamage(float amount, GameObject attacker)
    {
        environHitCount--;

        if (environHitCount <=0)
        {
            DestoryItem();
        }
    }

    public void DestoryItem()
    {
        Destroy(gameObject);
    }

}
