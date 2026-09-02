using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace SysKill.Isometric
{
    /// <summary>
    /// Keeps every floor and every floor collider alive in the same scene.
    /// The player ignores colliders belonging to other elevations; stair ramps
    /// are the only components allowed to change that collision context.
    /// </summary>
    public sealed class IsometricFloorManager : MonoBehaviour
    {
        [SerializeField] GameObject[] floorRoots;
        [SerializeField] Vector2[] floorWorldOffsets;
        [SerializeField] Rigidbody2D playerBody;
        [SerializeField] IsometricPlayerController playerController;
        [SerializeField] int currentFloor;
        [SerializeField, Min(0.1f)] float stairTravelDuration = 0.65f;
        [SerializeField, Min(0f)] float transitionCooldown = 0.3f;
        [SerializeField, Min(100)] int floorSortingStride = 4000;
        [SerializeField, Range(0f, 1f)] float upperFloorAlpha = 0.45f;

        bool transitioning;
        float nextTransitionTime;
        Collider2D[] playerColliders;

        public int CurrentFloor => currentFloor;
        public bool CanTransition => !transitioning && Time.time >= nextTransitionTime;

        void Awake()
        {
            ApplyFloorState();
        }

        public bool TraverseStairs(int destinationFloor, Vector2 destination)
        {
            if (!CanTransition || floorRoots == null || destinationFloor < 0 || destinationFloor >= floorRoots.Length)
                return false;

            StartCoroutine(TraverseRoutine(destinationFloor, destination));
            return true;
        }

        IEnumerator TraverseRoutine(int destinationFloor, Vector2 destination)
        {
            transitioning = true;
            Vector2 start = playerBody != null ? playerBody.position : destination;
            if (playerController != null)
                playerController.enabled = false;
            if (playerBody != null)
                playerBody.linearVelocity = Vector2.zero;

            float elapsed = 0f;
            while (elapsed < stairTravelDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / stairTravelDuration));
                if (playerBody != null)
                    playerBody.position = Vector2.Lerp(start, destination, t);
                yield return null;
            }

            currentFloor = destinationFloor;
            if (playerBody != null)
            {
                playerBody.position = destination;
                playerBody.transform.position = destination;
            }
            ApplyFloorState();
            Physics2D.SyncTransforms();

            if (playerController != null)
                playerController.enabled = true;
            nextTransitionTime = Time.time + transitionCooldown;
            transitioning = false;
        }

        void ApplyFloorState()
        {
            if (floorRoots == null)
                return;

            if (playerColliders == null && playerBody != null)
                playerColliders = playerBody.GetComponentsInChildren<Collider2D>(true);

            // Every floor and Collider remains enabled. Collision pairs are
            // filtered per player, preserving simultaneous floor geometry for
            // other actors and editor inspection.
            for (int i = 0; i < floorRoots.Length; i++)
            {
                if (floorRoots[i] == null)
                    continue;
                floorRoots[i].SetActive(true);
                bool activeCollisionFloor = i == currentFloor;
                foreach (Collider2D floorCollider in floorRoots[i].GetComponentsInChildren<Collider2D>(true))
                {
                    floorCollider.enabled = true;
                    if (playerColliders == null)
                        continue;
                    foreach (Collider2D playerCollider in playerColliders)
                        if (playerCollider != null && floorCollider != playerCollider)
                            Physics2D.IgnoreCollision(playerCollider, floorCollider, !activeCollisionFloor);
                }

                // Floors above the player remain spatially visible, but are
                // translucent so they do not hide the current-floor gameplay.
                float alpha = i > currentFloor ? upperFloorAlpha : 1f;
                foreach (Tilemap tilemap in floorRoots[i].GetComponentsInChildren<Tilemap>(true))
                    tilemap.color = new Color(1f, 1f, 1f, alpha);
                foreach (SpriteRenderer spriteRenderer in floorRoots[i].GetComponentsInChildren<SpriteRenderer>(true))
                    spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
            }

            if (playerController != null && floorWorldOffsets != null && currentFloor < floorWorldOffsets.Length)
                playerController.SetFloorContext(floorWorldOffsets[currentFloor], currentFloor * floorSortingStride);
        }

        void OnGUI()
        {
            const float width = 430f;
            GUI.Box(new Rect(16f, 16f, width, 62f), string.Empty);
            GUI.Label(new Rect(30f, 24f, width - 24f, 24f), $"Floor {currentFloor + 1} / {floorRoots?.Length ?? 0}");
            GUI.Label(new Rect(30f, 47f, width - 24f, 24f),
                transitioning ? "Moving on stairs..." : "Move: WASD / Arrows / Gamepad (stairs change height automatically)");
        }

#if UNITY_EDITOR
        public void Configure(GameObject[] roots, Vector2[] offsets, Rigidbody2D player, int sortingStride, int startFloor = 0)
        {
            floorRoots = roots;
            floorWorldOffsets = offsets;
            playerBody = player;
            playerController = player != null ? player.GetComponent<IsometricPlayerController>() : null;
            playerColliders = player != null ? player.GetComponentsInChildren<Collider2D>(true) : null;
            floorSortingStride = sortingStride;
            currentFloor = Mathf.Clamp(startFloor, 0, Mathf.Max(0, roots.Length - 1));
            ApplyFloorState();
        }
#endif
    }
}
