using UnityEngine;

namespace KenneySamples.Common
{
    public sealed class SampleCameraFollow2D : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] bool followX = true;
        [SerializeField] bool followY = true;
        [SerializeField] Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField, Min(0f)] float smoothTime = 0.15f;

        Vector3 velocity;

        void LateUpdate()
        {
            if (target == null)
                return;
            Vector3 destination = transform.position;
            if (followX) destination.x = target.position.x + offset.x;
            if (followY) destination.y = target.position.y + offset.y;
            destination.z = offset.z;
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, smoothTime);
        }

#if UNITY_EDITOR
        public void Configure(Transform followTarget, bool x, bool y, Vector3 followOffset)
        {
            target = followTarget;
            followX = x;
            followY = y;
            offset = followOffset;
        }
#endif
    }
}
