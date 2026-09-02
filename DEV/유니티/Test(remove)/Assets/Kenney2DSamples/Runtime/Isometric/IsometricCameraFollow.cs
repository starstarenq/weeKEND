using UnityEngine;

namespace SysKill.Isometric
{
    public sealed class IsometricCameraFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(0f, 0.7f, -10f);
        [SerializeField, Min(0f)] float smoothTime = 0.15f;

        Vector3 velocity;

        void LateUpdate()
        {
            if (target == null)
                return;

            Vector3 destination = target.position + offset;
            destination.z = offset.z;
            transform.position = smoothTime <= 0f
                ? destination
                : Vector3.SmoothDamp(transform.position, destination, ref velocity, smoothTime);
        }

#if UNITY_EDITOR
        public void Configure(Transform followTarget, Vector3 followOffset)
        {
            target = followTarget;
            offset = followOffset;
        }
#endif
    }
}
