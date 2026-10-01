using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FireballMediatorSample
{
    public sealed class SkillHotbarUI : MonoBehaviour
    {
        [SerializeField] SkillManager manager;
        [SerializeField] PlayerSkillInput input;
        [SerializeField] Button castButton;
        [SerializeField] TextMeshProUGUI bindingText;
        [SerializeField] TextMeshProUGUI stateText;
        bool subscribed;

        public void Configure(SkillManager source, PlayerSkillInput playerInput, Button button,
            TextMeshProUGUI keyLabel, TextMeshProUGUI statusLabel)
        {
            Unsubscribe();
            manager = source; input = playerInput; castButton = button;
            bindingText = keyLabel; stateText = statusLabel;
            if (isActiveAndEnabled) Subscribe();
            RefreshBindings();
        }
        void OnEnable() => Subscribe();
        void Start() => RefreshBindings(); // All Awake methods, including the input clone, have completed.
        void OnDisable() => Unsubscribe();
        void Subscribe()
        {
            if (subscribed) return;
            InputSystem.onActionChange += ActionChanged;
            if (castButton != null) castButton.onClick.AddListener(Cast);
            subscribed = true;
            RefreshBindings();
        }
        void Unsubscribe()
        {
            if (!subscribed) return;
            InputSystem.onActionChange -= ActionChanged;
            if (castButton != null) castButton.onClick.RemoveListener(Cast);
            subscribed = false;
        }
        void Cast() { if (manager != null) manager.UseSkill(0); }
        void ActionChanged(object source, InputActionChange change)
        {
            if (change == InputActionChange.BoundControlsChanged) RefreshBindings();
        }
        public void RefreshBindings()
        {
            if (bindingText == null) return;
            var action = input != null ? input.FireballAction : null;
            var labels = new List<string>();
            if (action != null)
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    var binding = action.bindings[i];
                    if (binding.isPartOfComposite || string.IsNullOrEmpty(binding.effectivePath)) continue;
                    string label = action.GetBindingDisplayString(i);
                    if (!string.IsNullOrEmpty(label) && !labels.Contains(label)) labels.Add(label);
                }
            bindingText.text = labels.Count > 0 ? string.Join(" / ", labels) : "Unbound";
        }
        void Update()
        {
            if (manager == null) return;
            bool ready = manager.CanUse(0, out string reason);
            if (castButton != null) castButton.interactable = ready;
            if (stateText != null) stateText.text = ready ? "READY" : reason;
        }
    }
}
