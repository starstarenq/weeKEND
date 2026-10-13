using System.Collections.Generic;
using GameLecture.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SysKill.DialogueSample
{
    public sealed class DialogueUGUIView : GameLecture.Framework.SceneUI
    {
        public DialogueSamplePlayer player;
        public TMP_Text speakerText, dialogueText, advanceLabel, statusText;
        public Image portraitImage;
        public Button advanceButton, previousButton, restartButton, saveButton, loadButton, closeButton;
        public Button jsonButton, configButton, branchJsonButton, branchConfigButton;
        public RectTransform choiceRoot;
        [Tooltip("Prefab asset or inactive scene template outside the choice Content.")]
        public DialogueChoiceButton choiceButtonPrefab;
        public ScrollRect choiceScroll;
        public ScrollRect dialogueScroll;
        private readonly List<DialogueChoiceButton> choices = new List<DialogueChoiceButton>();
        private readonly List<(Button button, UnityAction action)> listeners = new List<(Button, UnityAction)>();
        private DialogueSamplePlayer boundPlayer;
        private int revision = -1, choiceRevision = -1, displayedChoices = -1;
        private bool valid;
        private string lastIssue;

        public string ValidateReferences()
        {
            if (player == null || speakerText == null || dialogueText == null || advanceButton == null || advanceLabel == null)
                return "Dialogue UI: Player, Speaker Text, Dialogue Text, Advance Button / Label을 연결하세요.";
            if ((choiceRoot == null) != (choiceButtonPrefab == null) ||
                (choiceButtonPrefab != null && !choiceButtonPrefab.IsConfigured))
                return "Dialogue UI: Choice Root와 Button / Label이 연결된 선택지 템플릿이 필요합니다.";
            if (player.Ready && player.HasChoices && choiceRoot == null)
                return "Dialogue UI: 분기 대사를 표시할 Choice Root / Choice Button Prefab이 없습니다.";
            if (player.Portrait != null && portraitImage == null)
                return "Dialogue UI: 초상화를 표시할 Portrait Image를 연결하세요.";
            return null;
        }

        private void OnEnable()
        {
            revision = choiceRevision = displayedChoices = -1;
            string issue = ValidateReferences();
            if (issue != null) { Report(issue); SetButtons(false); return; }
            if (!player.AttachView(this))
            {
                Report("Dialogue UI: Player의 Use UGUI를 켜고 활성 View를 하나만 연결하세요.");
                SetButtons(false); return;
            }
            boundPlayer = player;
            boundPlayer.StateChanged += RefreshUI;
            boundPlayer.RevealChanged += RefreshReveal;
            Listen(advanceButton, boundPlayer.Advance);
            Listen(previousButton, boundPlayer.Previous);
            Listen(restartButton, boundPlayer.Restart);
            Listen(saveButton, boundPlayer.SaveDialogue);
            Listen(loadButton, boundPlayer.LoadDialogue);
            Listen(closeButton, boundPlayer.CloseDialogue);
            Listen(jsonButton, () => SwitchSource(false, false));
            Listen(configButton, () => SwitchSource(true, false));
            Listen(branchJsonButton, () => SwitchSource(false, true));
            Listen(branchConfigButton, () => SwitchSource(true, true));
            RefreshUI();
        }

        private void Listen(Button button, UnityAction action)
        {
            if (button == null) return;
            UnityAction guarded = () => { if (valid && boundPlayer != null && boundPlayer.isActiveAndEnabled) action(); };
            button.onClick.AddListener(guarded);
            listeners.Add((button, guarded));
        }

        private void SwitchSource(bool config, bool branch)
        {
            boundPlayer.SwitchSource(config ? DialogueSamplePlayer.DataSource.ScriptableObject : DialogueSamplePlayer.DataSource.Json, branch);
        }

        private void OnDisable()
        {
            foreach (var entry in listeners)
                if (entry.button != null) entry.button.onClick.RemoveListener(entry.action);
            listeners.Clear();
            foreach (var choice in choices) if (choice != null) { choice.Unbind(); choice.gameObject.SetActive(false); }
            if (boundPlayer != null)
            {
                boundPlayer.StateChanged -= RefreshUI;
                boundPlayer.RevealChanged -= RefreshReveal;
                boundPlayer.DetachView(this);
            }
            boundPlayer = null;
            valid = false;
        }

        private void Report(string issue)
        {
            valid = false;
            if (statusText != null) statusText.text = issue;
            if (lastIssue != issue) Debug.LogError(issue, this);
            lastIssue = issue;
        }

        public override void RefreshUI()
        {
            if (boundPlayer == null) return;
            string issue = ValidateReferences();
            valid = issue == null;
            SetButtons(valid);
            if (!valid)
            {
                foreach (var choice in choices) { choice.Unbind(); choice.gameObject.SetActive(false); }
                choiceRevision = -1;
                Report(issue); return;
            }
            lastIssue = null;
            speakerText.richText = false;
            speakerText.text = boundPlayer.Speaker;
            if (portraitImage != null)
            {
                portraitImage.sprite = boundPlayer.Portrait;
                portraitImage.preserveAspect = true;
                portraitImage.enabled = portraitImage.sprite != null;
            }
            if (statusText != null) { statusText.richText = false; statusText.text = boundPlayer.Status; }
            if (revision != boundPlayer.Revision)
            {
                revision = boundPlayer.Revision;
                dialogueText.richText = false;
                dialogueText.text = boundPlayer.FullText;
                dialogueText.maxVisibleCharacters = int.MaxValue;
                dialogueText.ForceMeshUpdate();
                if (dialogueScroll != null) dialogueScroll.verticalNormalizedPosition = 1;
            }
            advanceLabel.text = boundPlayer.AdvanceLabel;
            RefreshChoices();
            RefreshReveal();
        }

        private void SetButtons(bool configured)
        {
            var p = boundPlayer;
            bool ready = configured && p != null && p.Ready && p.isActiveAndEnabled;
            Set(advanceButton, ready && p.CanAdvance);
            Set(previousButton, ready && p.CanPrevious);
            Set(restartButton, ready);
            Set(saveButton, ready);
            Set(loadButton, ready);
            Set(closeButton, ready && !p.Closed);
            Set(jsonButton, configured && p != null && p.ResourcesReady);
            Set(configButton, configured && p != null && p.ResourcesReady);
            Set(branchJsonButton, configured && p != null && p.ResourcesReady);
            Set(branchConfigButton, configured && p != null && p.ResourcesReady);
        }

        private static void Set(Button button, bool value) { if (button != null) button.interactable = value; }

        private void RefreshChoices()
        {
            int count = boundPlayer.ChoiceCount;
            if (choiceRevision == revision && displayedChoices == count) return;
            choiceRevision = revision; displayedChoices = count;
            foreach (var choice in choices) { choice.Unbind(); choice.gameObject.SetActive(false); }
            for (int i = 0; i < count; i++)
            {
                if (i == choices.Count) choices.Add(Instantiate(choiceButtonPrefab, choiceRoot));
                var choice = choices[i];
                int index = i, generation = revision;
                choice.Bind(boundPlayer.ChoiceText(i), () =>
                {
                    if (!valid || boundPlayer == null || generation != boundPlayer.Revision) return;
                    boundPlayer.Choose(index, generation);
                });
                choice.gameObject.SetActive(true);
            }
            if (choiceScroll != null) choiceScroll.verticalNormalizedPosition = 1;
        }

        private void RefreshReveal()
        {
            if (!valid || boundPlayer == null) return;
            if (!boundPlayer.IsTyping) { dialogueText.maxVisibleCharacters = int.MaxValue; return; }
            var info = dialogueText.textInfo;
            int count = 0;
            while (count < info.characterCount &&
                   info.characterInfo[count].index + info.characterInfo[count].stringLength <= boundPlayer.VisibleEnd) count++;
            dialogueText.maxVisibleCharacters = count;
        }
    }
}
