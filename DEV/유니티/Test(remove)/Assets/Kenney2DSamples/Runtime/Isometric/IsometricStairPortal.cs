using UnityEngine;

namespace SysKill.Isometric
{
    /// <summary>
    /// A physical stair-ramp entrance. Walking into it transfers the player
    /// to the connected floor; there is no arbitrary floor-switch interaction.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public sealed class IsometricStairPortal : MonoBehaviour
    {
        [SerializeField] IsometricFloorManager floorManager;
        [SerializeField] int sourceFloor;
        [SerializeField] int destinationFloor;
        [SerializeField] Vector2 destinationPosition;

        void Reset()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (floorManager != null && floorManager.CurrentFloor == sourceFloor &&
                other.TryGetComponent(out IsometricPlayerController _))
                floorManager?.TraverseStairs(destinationFloor, destinationPosition);
        }

#if UNITY_EDITOR
        public void Configure(IsometricFloorManager manager, int fromFloor, int targetFloor, Vector2 targetPosition)
        {
            floorManager = manager;
            sourceFloor = fromFloor;
            destinationFloor = targetFloor;
            destinationPosition = targetPosition;
            GetComponent<Collider2D>().isTrigger = true;
        }
#endif
    }
}
