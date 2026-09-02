using UnityEngine;

namespace KenneySamples.SideView
{
    public sealed class ParallaxLayer2D : MonoBehaviour
    {
        [SerializeField] Camera targetCamera;
        [SerializeField, Range(0f, 1f)] float horizontalFactor = 0.25f;

        float initialCameraX;
        float initialLayerX;

        void Start()
        {
            if (targetCamera == null)
                targetCamera = Camera.main;
            initialCameraX = targetCamera != null ? targetCamera.transform.position.x : 0f;
            initialLayerX = transform.position.x;
        }

        void LateUpdate()
        {
            if (targetCamera == null)
                return;
            Vector3 position = transform.position;
            position.x = initialLayerX + (targetCamera.transform.position.x - initialCameraX) * horizontalFactor;
            transform.position = position;
        }

#if UNITY_EDITOR
        public void Configure(Camera camera, float factor)
        {
            targetCamera = camera;
            horizontalFactor = factor;
        }
#endif
    }
}
