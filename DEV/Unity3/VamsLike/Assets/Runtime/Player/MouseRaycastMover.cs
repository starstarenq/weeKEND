using UnityEngine;
using UnityEngine.InputSystem;

namespace VamSurvivalLike.Player
{
    /// <summary>Click-and-hold movement toward the pointer's raycast point using the New Input System.</summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class MouseRaycastMover : MonoBehaviour
    {
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private InputActionReference pointerPosition;
        [SerializeField] private InputActionReference movePressed;
        [SerializeField] private LayerMask groundLayers;
        [SerializeField, Min(0f)] private float movementSpeed = 6f;
        [SerializeField, Min(0f)] private float stoppingDistance = 0.1f;
        [SerializeField, Min(0f)] private float rotationSpeed = 900f;

        private CharacterController controller;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (gameplayCamera == null)
                gameplayCamera = Camera.main;
        }

        private void OnEnable()
        {
            pointerPosition?.action?.Enable();
            movePressed?.action?.Enable();
        }

        private void OnDisable()
        {
            pointerPosition?.action?.Disable();
            movePressed?.action?.Disable();
        }

        private void Update()
        {
            if (!IsMoveHeld() || !MouseToGroundRaycast.TryGetPoint(gameplayCamera,
                    MouseToGroundRaycast.ReadPointerPosition(pointerPosition), groundLayers, out var destination))
                return;

            var direction = destination - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= stoppingDistance * stoppingDistance)
                return;

            direction.Normalize();
            controller.Move(direction * (movementSpeed * Time.deltaTime));
            var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        private bool IsMoveHeld()
        {
            return movePressed != null && movePressed.action != null
                ? movePressed.action.IsPressed()
                : Mouse.current != null && Mouse.current.leftButton.isPressed;
        }
    }
}
