using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SysKill.DialogueSample
{
    public sealed class DialogueChoiceButton : MonoBehaviour
    {
        public Button button;
        public TMP_Text label;
        private UnityAction listener;
        public bool IsConfigured => button != null && label != null;

        public void Bind(string text, Action selected)
        {
            Unbind();
            label.richText = false;
            label.text = text;
            listener = () => selected();
            button.onClick.AddListener(listener);
            button.interactable = true;
        }

        public void Unbind()
        {
            if (button != null)
            {
                if (listener != null) button.onClick.RemoveListener(listener);
                button.interactable = false;
            }
            listener = null;
        }

        private void OnDestroy() => Unbind();
    }
}
