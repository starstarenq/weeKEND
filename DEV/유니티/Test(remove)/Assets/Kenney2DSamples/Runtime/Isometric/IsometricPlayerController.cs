using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SysKill.Isometric
{
    /// <summary>
    /// Screen-relative eight-direction movement and sprite animation for the isometric demo.
    /// The GameObject position represents the character's feet; keep the renderer on a child.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class IsometricPlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0.1f)] float moveSpeed = 3.5f;

        [Header("Animation")]
        [SerializeField] SpriteRenderer characterRenderer;
        [SerializeField] Sprite[] idleSprites = new Sprite[8];
        [SerializeField] Sprite[] runSprites = new Sprite[80];
        [SerializeField, Min(1f)] float runFramesPerSecond = 12f;

        [Header("Isometric map bounds")]
        [SerializeField] bool constrainToMap = true;
        [SerializeField] Vector2 mapMinimum = new Vector2(-3.35f, -3.35f);
        [SerializeField] Vector2 mapMaximum = new Vector2(3.35f, 3.35f);
        [SerializeField] Vector2 cellDiamondSize = new Vector2(2.56f, 1.28f);
        [SerializeField] Vector2 floorWorldOffset;
        [SerializeField] int sortingOrderBase;

        Rigidbody2D body;
        Vector2 input;
        float animationTime;
        int facing = 3;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            UpdateSprite(false);
        }

        void Update()
        {
            input = ReadInput();
            if (input.sqrMagnitude > 1f)
                input.Normalize();

            if (input.sqrMagnitude > 0.001f)
            {
                facing = DirectionToIndex(input);
                animationTime += Time.deltaTime;
            }
            else
            {
                animationTime = 0f;
            }

            UpdateSprite(input.sqrMagnitude > 0.001f);
            if (characterRenderer != null)
                characterRenderer.sortingOrder = sortingOrderBase - Mathf.RoundToInt(transform.position.y * 100f);
        }

        void FixedUpdate()
        {
            Vector2 target = body.position + input * (moveSpeed * Time.fixedDeltaTime);
            body.MovePosition(constrainToMap ? ClampToMap(target) : target);
        }

        Vector2 ClampToMap(Vector2 world)
        {
            world -= floorWorldOffset;
            float halfWidth = Mathf.Max(0.01f, cellDiamondSize.x * 0.5f);
            float halfHeight = Mathf.Max(0.01f, cellDiamondSize.y * 0.5f);
            float cellX = 0.5f * ((world.x / halfWidth) + (world.y / halfHeight));
            float cellY = 0.5f * ((world.y / halfHeight) - (world.x / halfWidth));
            cellX = Mathf.Clamp(cellX, mapMinimum.x, mapMaximum.x);
            cellY = Mathf.Clamp(cellY, mapMinimum.y, mapMaximum.y);
            return new Vector2((cellX - cellY) * halfWidth, (cellX + cellY) * halfHeight) + floorWorldOffset;
        }

        public void SetFloorContext(Vector2 worldOffset, int floorSortingOrderBase)
        {
            floorWorldOffset = worldOffset;
            sortingOrderBase = floorSortingOrderBase;
        }

        void UpdateSprite(bool running)
        {
            if (characterRenderer == null)
                return;

            if (running && runSprites != null && runSprites.Length >= 80)
            {
                int frame = Mathf.FloorToInt(animationTime * runFramesPerSecond) % 10;
                Sprite sprite = runSprites[(facing * 10) + frame];
                if (sprite != null)
                    characterRenderer.sprite = sprite;
            }
            else if (idleSprites != null && idleSprites.Length >= 8 && idleSprites[facing] != null)
            {
                characterRenderer.sprite = idleSprites[facing];
            }
        }

        static int DirectionToIndex(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (angle < 0f)
                angle += 360f;

            if (angle < 22.5f || angle >= 337.5f) return 1; // right
            if (angle < 67.5f) return 0;                    // up-right
            if (angle < 112.5f) return 7;                   // up
            if (angle < 157.5f) return 6;                   // up-left
            if (angle < 202.5f) return 5;                   // left
            if (angle < 247.5f) return 4;                   // down-left
            if (angle < 292.5f) return 3;                   // down
            return 2;                                      // down-right
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
        public void Configure(SpriteRenderer renderer, Sprite[] idle, Sprite[] run, Vector2 min, Vector2 max, Vector2 diamondSize)
        {
            characterRenderer = renderer;
            idleSprites = idle;
            runSprites = run;
            mapMinimum = min;
            mapMaximum = max;
            cellDiamondSize = diamondSize;
        }
#endif
    }
}
