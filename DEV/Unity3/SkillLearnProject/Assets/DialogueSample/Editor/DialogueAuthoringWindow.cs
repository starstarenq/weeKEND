using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace SysKill.DialogueSample.Editor
{
    public sealed class DialogueAuthoringWindow : EditorWindow
    {
        [SerializeField] private DialogueAuthoringDocument document;
        [SerializeField] private int selected;
        [SerializeField] private UnityEngine.Object importAsset;
        [SerializeField] private UnityEngine.Object exportedAsset;
        [SerializeField] private DialogueSamplePlayer player;
        private Vector2 listScroll, detailScroll;
        private string message;
        private MessageType messageType;
        private string DraftKey => "SysKill.DialogueAuthoring.Draft.v1." + Application.dataPath;

        [MenuItem("GameLecture/Dialogue/Dialogue Creator")]
        public static void Open()
        {
            var window = GetWindow<DialogueAuthoringWindow>("Dialogue Creator");
            window.minSize = new Vector2(850, 620);
            window.Show();
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += UndoRedo;
            if (document != null) return;
            try
            {
                if (EditorPrefs.HasKey(DraftKey))
                    document = JsonConvert.DeserializeObject<DialogueAuthoringDocument>(EditorPrefs.GetString(DraftKey));
            }
            catch (Exception exception) { SetMessage("이전 초안 복원 실패: " + exception.Message, MessageType.Warning); }
            if (document == null || document.entries == null)
            {
                document = new DialogueAuthoringDocument(); document.Add();
            }
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedo;
            PersistDraft();
        }

        private void UndoRedo() { PersistDraft(); Repaint(); }
        private void PersistDraft()
        {
            if (document != null) EditorPrefs.SetString(DraftKey, JsonConvert.SerializeObject(document));
        }
        private void Changed() { PersistDraft(); Repaint(); }
        private void SetMessage(string value, MessageType type = MessageType.Info) { message = value; messageType = type; }
        private void Run(Action action)
        {
            try { action(); }
            catch (Exception exception) { SetMessage(exception.Message, MessageType.Error); }
        }

        private void OnGUI()
        {
            if (document == null) return;
            Undo.RecordObject(this, "Edit dialogue draft");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("대화 생성 · JSON / ScriptableObject", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("ID: 10000000~10009999. 첫 항목에서 시작합니다. 순서를 바꾸면 순차 진행 경로도 바뀝니다.", MessageType.Info);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("새 대화", GUILayout.Width(90)))
                    Run(() =>
                    {
                        if (!EditorUtility.DisplayDialog("새 대화", "현재 초안을 새 대화로 바꿀까요? 필요하면 먼저 내보내세요.", "새로 만들기", "취소")) return;
                        document = new DialogueAuthoringDocument(); document.Add(); selected = 0; exportedAsset = null; Changed();
                    });
                importAsset = EditorGUILayout.ObjectField(importAsset, typeof(UnityEngine.Object), false);
                if (GUILayout.Button("JSON / SO 불러오기", GUILayout.Width(150)))
                    Run(() =>
                    {
                        var imported = DialogueAuthoringAssets.Import(importAsset);
                        if (!EditorUtility.DisplayDialog("대화 불러오기", "현재 초안을 선택한 데이터로 바꿀까요? 원본은 수정하지 않습니다.", "불러오기", "취소")) return;
                        document = imported; selected = 0; exportedAsset = null;
                        SetMessage("불러왔습니다. 편집 후 원하는 형식으로 내보내세요."); Changed();
                    });
            }
            document.resourceName = EditorGUILayout.TextField("출력 리소스 이름", document.resourceName);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("분기 검증")) Run(() => { document.Validate(); SetMessage("ID · 화자 · 대사 · 분기 검증 통과. Sprite는 내보낼 때 검증합니다."); });
                if (GUILayout.Button("JSON 내보내기")) Export(true);
                if (GUILayout.Button("SO 내보내기")) Export(false);
            }
            if (!string.IsNullOrEmpty(message)) EditorGUILayout.HelpBox(message, messageType);
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawList();
                using (new EditorGUILayout.VerticalScope())
                {
                    detailScroll = EditorGUILayout.BeginScrollView(detailScroll);
                    if (document.entries.Count > 0)
                    {
                        selected = Mathf.Clamp(selected, 0, document.entries.Count - 1);
                        DrawEntry(document.entries[selected]);
                    }
                    else EditorGUILayout.HelpBox("대사 추가 버튼으로 시작하세요.", MessageType.Info);
                    EditorGUILayout.EndScrollView();
                }
            }
            DrawApply();
            EditorGUILayout.LabelField("초안은 이 컴퓨터의 프로젝트별 EditorPrefs에 자동 보관됩니다. 공유할 데이터는 내보내세요.", EditorStyles.miniLabel);
            if (EditorGUI.EndChangeCheck()) Changed();
        }

        private void DrawList()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(245)))
            {
                if (GUILayout.Button("+ 대사 추가")) Run(() => { document.Add(); selected = document.entries.Count - 1; Changed(); });
                listScroll = EditorGUILayout.BeginScrollView(listScroll);
                for (int i = 0; i < document.entries.Count; i++)
                {
                    var node = document.entries[i].node;
                    var style = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, fontStyle = i == selected ? FontStyle.Bold : FontStyle.Normal };
                    if (GUILayout.Button($"{i + 1}. {node.id} · {node.speaker}", style, GUILayout.Height(30))) selected = i;
                }
                EditorGUILayout.EndScrollView();
                using (new EditorGUI.DisabledScope(document.entries.Count == 0))
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("복제")) Run(() => { document.Duplicate(selected); selected++; Changed(); });
                    if (GUILayout.Button("삭제")) Run(() => { document.Remove(selected); selected = Mathf.Max(0, selected - 1); Changed(); });
                    using (new EditorGUI.DisabledScope(selected <= 0))
                        if (GUILayout.Button("↑")) { document.Move(selected, -1); selected--; Changed(); }
                    using (new EditorGUI.DisabledScope(selected >= document.entries.Count - 1))
                        if (GUILayout.Button("↓")) { document.Move(selected, 1); selected++; Changed(); }
                }
            }
        }

        private void DrawEntry(DialogueAuthoringEntry entry)
        {
            var node = entry.node;
            int id = EditorGUILayout.DelayedIntField("대사 ID", node.id);
            if (id != node.id) Run(() => { document.RenameId(selected, id); SetMessage("ID와 연결된 분기 참조를 갱신했습니다."); Changed(); });
            node.speaker = EditorGUILayout.TextField("화자", node.speaker);
            var sprite = DialogueAuthoringAssets.FindSprite(entry.spriteGlobalId);
            var replacement = (Sprite)EditorGUILayout.ObjectField("Sprite", sprite, typeof(Sprite), false);
            if (replacement != sprite)
            {
                entry.spriteGlobalId = DialogueAuthoringAssets.SpriteId(replacement);
                entry.spriteKey = replacement == null ? "" : replacement.name;
            }
            string key = EditorGUILayout.DelayedTextField("JSON Sprite 키", entry.spriteKey ?? "");
            if (key != (entry.spriteKey ?? "")) { entry.spriteKey = key; entry.spriteGlobalId = null; }
            if (GUILayout.Button("초상화 없음", GUILayout.Width(100))) { entry.spriteKey = ""; entry.spriteGlobalId = null; Changed(); }
            if (sprite == null && !string.IsNullOrWhiteSpace(entry.spriteKey))
                EditorGUILayout.HelpBox("키로 연결된 Sprite는 내보낼 때 찾습니다. JSON에는 기본 Resources 로딩 경로의 고유 이름이 필요합니다.", MessageType.Info);
            EditorGUILayout.LabelField("대사");
            node.text = EditorGUILayout.TextArea(node.text ?? "", EditorStyles.textArea, GUILayout.MinHeight(100));
            int mode = node.end ? 3 : node.choices != null && node.choices.Length > 0 ? 2 : node.nextId != 0 ? 1 : 0;
            int newMode = EditorGUILayout.Popup("진행 방식", mode, new[] { "다음 순서", "특정 ID로 이동", "선택지 분기", "대화 종료" });
            if (newMode != mode)
            {
                node.nextId = 0; node.end = newMode == 3; node.choices = null;
                int target = document.entries[(selected + 1) % document.entries.Count].node.id;
                if (newMode == 1) node.nextId = target;
                if (newMode == 2) node.choices = new[] { new DialogueChoice { text = "선택지", targetId = target } };
                mode = newMode;
            }
            if (mode == 1) node.nextId = TargetPopup("이동할 ID", node.nextId);
            if (mode == 2)
            {
                int remove = -1;
                for (int i = 0; i < node.choices.Length; i++)
                {
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        node.choices[i].text = EditorGUILayout.TextField("선택지 " + (i + 1), node.choices[i].text);
                        node.choices[i].targetId = TargetPopup("Target ID", node.choices[i].targetId);
                        if (GUILayout.Button("선택지 삭제", GUILayout.Width(100))) remove = i;
                    }
                }
                if (remove >= 0)
                {
                    var choices = new List<DialogueChoice>(node.choices); choices.RemoveAt(remove); node.choices = choices.ToArray(); Changed();
                }
                if (GUILayout.Button("+ 선택지 추가"))
                {
                    var choices = new List<DialogueChoice>(node.choices);
                    choices.Add(new DialogueChoice { text = "선택지", targetId = document.entries[(selected + 1) % document.entries.Count].node.id });
                    node.choices = choices.ToArray(); Changed();
                }
            }
        }

        private int TargetPopup(string label, int value)
        {
            var names = new List<string>(); var ids = new List<int>();
            foreach (var entry in document.entries)
            {
                ids.Add(entry.node.id); names.Add(entry.node.id + " · " + entry.node.speaker);
            }
            if (!ids.Contains(value)) { ids.Insert(0, value); names.Insert(0, value + " (없는 대상)"); }
            return EditorGUILayout.IntPopup(label, value, names.ToArray(), ids.ToArray());
        }

        private void Export(bool json)
        {
            Run(() =>
            {
                var result = DialogueAuthoringAssets.Export(document, json);
                if (result == null) return;
                exportedAsset = result;
                SetMessage("내보내기 완료: " + AssetDatabase.GetAssetPath(result));
                EditorGUIUtility.PingObject(result); Changed();
            });
        }

        private void DrawApply()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("내보낸 데이터를 Scene에 적용", EditorStyles.boldLabel);
                using (new EditorGUI.DisabledScope(true)) EditorGUILayout.ObjectField("내보낸 자산", exportedAsset, typeof(UnityEngine.Object), false);
                player = (DialogueSamplePlayer)EditorGUILayout.ObjectField("대상 Player", player, typeof(DialogueSamplePlayer), true);
                using (new EditorGUI.DisabledScope(exportedAsset == null || player == null || EditorApplication.isPlayingOrWillChangePlaymode))
                    if (GUILayout.Button("대상 Player에 적용")) Run(() =>
                    {
                        DialogueAuthoringAssets.ApplyToPlayer(exportedAsset, player);
                        SetMessage("Player에 적용했습니다. Scene을 저장한 뒤 Play하세요. 적용 대상은 마지막 내보낸 자산입니다.");
                    });
            }
        }
    }
}
