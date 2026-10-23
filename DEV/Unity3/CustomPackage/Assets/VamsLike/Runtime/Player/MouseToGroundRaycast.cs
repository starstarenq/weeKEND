using UnityEngine;
using UnityEngine.InputSystem;

namespace VamSurvivalLike.Player
{
    /// <summary>Converts a New Input System pointer position into a world point by raycasting the ground.</summary>
    public static class MouseToGroundRaycast
    {
        public static bool TryGetPoint(Camera camera, Vector2 screenPosition, LayerMask groundLayers, out Vector3 point)
        {
            point = default;
            if (camera == null)
                return false;

            var ray = camera.ScreenPointToRay(screenPosition);
            return Physics.Raycast(ray, out var hit, 500f, groundLayers, QueryTriggerInteraction.Ignore)
                && SetPoint(hit.point, out point);
        }

        public static Vector2 ReadPointerPosition(InputActionReference pointerPosition)
        {
            return pointerPosition != null && pointerPosition.action != null
                ? pointerPosition.action.ReadValue<Vector2>()
                : Mouse.current != null ? Mouse.current.position.ReadValue() : default;
        }

        private static bool SetPoint(Vector3 hitPoint, out Vector3 point)
        {
            point = hitPoint;
            return true;
        }
    }
}
