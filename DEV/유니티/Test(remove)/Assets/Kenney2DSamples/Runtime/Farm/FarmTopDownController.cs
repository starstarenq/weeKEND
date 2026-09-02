using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace KenneySamples.Farm
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class FarmTopDownController : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] float speed = 4f;
        [SerializeField] SpriteRenderer characterRenderer;

        Rigidbody2D body;
        Vector2 input;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
        }

        void Update()
        {
            input = ReadInput();
            if (input.sqrMagnitude > 1f)
                input.Normalize();
            if (characterRenderer != null)
                characterRenderer.sortingOrder = 1000 - Mathf.RoundToInt(transform.position.y * 100f);
        }

        void FixedUpdate()
        {
            body.MovePosition(body.position + input * (speed * Time.fixedDeltaTime));
        }

        static Vector2 ReadInput()
        {
            Vector2 value = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) value.x -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) value.x += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) value.y -= 1f;
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) value.y += 1f;
            }
            if (Gamepad.current != null)
                value += Gamepad.current.leftStick.ReadValue();
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            if (value.sqrMagnitude < 0.001f)
                value = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
            return Vector2.ClampMagnitude(value, 1f);
        }

#if UNITY_EDITOR
        public void Configure(SpriteRenderer renderer)
        {
            characterRenderer = renderer;
        }
#endif
    }
}
