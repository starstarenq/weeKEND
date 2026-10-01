using UnityEngine;

namespace FireballMediatorSample
{
    public sealed class FireballProjectile : MonoBehaviour
    {
        float speed, damage;
        readonly RaycastHit2D[] hits = new RaycastHit2D[1];
        public void Launch(float velocity, float lifetime, float power)
        {
            speed = velocity; damage = power;
            Destroy(gameObject, lifetime);
        }
        void Update()
        {
            float distance = speed * Time.deltaTime;
            var filter = new ContactFilter2D();
            filter.SetLayerMask(Physics2D.DefaultRaycastLayers);
            filter.useTriggers = false;
            if (Physics2D.CircleCast(transform.position, 0.22f, transform.right,
                filter, hits, distance) > 0)
            {
                var target = hits[0].collider.GetComponent<FireballTarget>();
                if (target != null) target.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
            transform.position += transform.right * distance;
        }
    }
}
