using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace KenneySamples.SideView
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class SideViewPlayerController : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] float moveSpeed = 5f;
        [SerializeField, Min(0.1f)] float jumpSpeed = 7.5f;

        Rigidbody2D body;
        Collider2D ownCollider;
        float horizontal;
        bool jumpRequested;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            ownCollider = GetComponent<Collider2D>();
            body.freezeRotation = true;
        }

        void Update()
        {
            horizontal = ReadHorizontal();
            jumpRequested |= ReadJumpPressed();
        }

        void FixedUpdate()
        {
            Vector2 velocity = body.linearVelocity;
            velocity.x = horizontal * moveSpeed;
            if (jumpRequested && IsGrounded())
                velocity.y = jumpSpeed;
            body.linearVelocity = velocity;
            jumpRequested = false;
        }

        bool IsGrounded()
        {
            Bounds bounds = ownCollider.bounds;
            Vector2 center = new Vector2(bounds.center.x, bounds.min.y - 0.04f);
            Collider2D[] hits = Physics2D.OverlapBoxAll(center, new Vector2(bounds.size.x * 0.75f, 0.08f), 0f);
            foreach (Collider2D hit in hits)
                if (hit != null && hit != ownCollider && !hit.isTrigger)
                    return true;
            return false;
        }

        static float ReadHorizontal()
        {
            float value = 0f;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) value -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) value += 1f;
            }
            if (Gamepad.current != null && Mathf.Abs(value) < 0.01f)
                value = Gamepad.current.leftStick.x.ReadValue();
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Mathf.Abs(value) < 0.01f)
                value = Input.GetAxisRaw("Horizontal");
#endif
            return Mathf.Clamp(value, -1f, 1f);
        }

        static bool ReadJumpPressed()
        {
            bool pressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
                pressed |= Keyboard.current.spaceKey.wasPressedThisFrame;
            if (Gamepad.current != null)
                pressed |= Gamepad.current.buttonSouth.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            pressed |= Input.GetKeyDown(KeyCode.Space);
#endif
            return pressed;
        }
    }
}
