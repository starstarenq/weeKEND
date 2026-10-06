using UnityEngine;
using UnityEngine.InputSystem;

namespace FireballMediatorSample
{
    public sealed class PlayerSkillInput : MonoBehaviour
    {
        [SerializeField] SkillManager manager;
        [SerializeField] InputActionAsset actions;
        InputActionAsset ownedActions;
        InputAction fire;
        InputAction ice;
        // Expose the actual runtime action, including binding overrides, without duplicating key names in UI.
        public InputAction FireballAction => fire ?? actions?.FindAction("Gameplay/Fireball");
        public InputAction IceballAction => ice ?? actions?.FindAction("Gameplay/Iceball");

        public void Configure(SkillManager mediator, InputActionAsset asset) { manager = mediator; actions = asset; }
        void Awake()
        {
            if (actions == null || manager == null) { enabled = false; return; }
            ownedActions = Instantiate(actions);
            fire = ownedActions.FindAction("Gameplay/Fireball", true);
        }
        void OnEnable()
        {
            if (fire == null) return;
            fire.performed += OnFireball;
            fire.Enable();
        }
        void OnDisable()
        {
            if (fire == null) return;
            fire.performed -= OnFireball;
            fire.Disable();
        }
        void OnDestroy() { if (ownedActions != null) Destroy(ownedActions); }
        void OnFireball(InputAction.CallbackContext context) => manager.UseSkill(0);
        void OnIceball(InputAction.CallbackContext context) => manager.UseSkill(1);
    }
}
