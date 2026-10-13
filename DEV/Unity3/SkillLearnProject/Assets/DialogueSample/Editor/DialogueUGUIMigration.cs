using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SysKill.DialogueSample.Editor
{
    public sealed class DialogueUGUIMigration : EditorWindow
    {
        private TMP_FontAsset font;
        private DialogueSamplePlayer target;
        private string result;

        [MenuItem("GameLecture/Dialogue/Migrate Current Scene to UGUI")]
        public static void Open()
        {
            var window = GetWindow<DialogueUGUIMigration>("Dialogue → UGUI");
            window.font = TMP_Settings.defaultFontAsset;
            var players = Players(SceneManager.GetActiveScene());
            window.target = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<DialogueSamplePlayer>() : null;
            if (window.target == null && players.Length == 1) window.target = players[0];
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("현재 활성 Scene의 기존 Player와 데이터 키를 유지하고 UGUI를 생성합니다. " +
                "Player가 여럿이면 하나씩 선택하세요. 이미 연결된 View는 재사용합니다. 변경은 Undo 가능하며 Scene은 자동 저장하지 않습니다.", MessageType.Info);
            target = (DialogueSamplePlayer)EditorGUILayout.ObjectField("Scene Player", target, typeof(DialogueSamplePlayer), true);
            font = (TMP_FontAsset)EditorGUILayout.ObjectField("TMP Font (한글 지원)", font, typeof(TMP_FontAsset), false);
            EditorGUILayout.HelpBox("한글을 지원하는 TMP Font Asset 또는 fallback을 지정하세요. 생성 후 긴 대사와 작은 화면에서 Game View를 확인하세요.", MessageType.Info);
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode || target == null || font == null))
                if (GUILayout.Button("현재 Scene의 Player를 UGUI로 이전"))
                {
                    try
                    {
                        if (target.gameObject.scene != SceneManager.GetActiveScene())
                            throw new InvalidOperationException("활성 Scene에 속한 Player를 선택하세요.");
                        var view = Migrate(target, font);
                        Selection.activeGameObject = view.gameObject;
                        result = "이전 완료. Hierarchy와 Inspector를 확인한 뒤 Scene을 저장하세요.";
                    }
                    catch (Exception exception) { result = exception.Message; Debug.LogException(exception); }
                }
            if (!string.IsNullOrEmpty(result)) EditorGUILayout.HelpBox(result, MessageType.Info);
        }

        private static DialogueSamplePlayer[] Players(Scene scene) => scene.IsValid() && scene.isLoaded
            ? scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<DialogueSamplePlayer>(true)).ToArray()
            : Array.Empty<DialogueSamplePlayer>();

        public static DialogueUGUIView Migrate(DialogueSamplePlayer player, TMP_FontAsset font)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || player == null || EditorUtility.IsPersistent(player))
                throw new InvalidOperationException("편집 모드의 Scene Player가 필요합니다.");
            var scene = player.gameObject.scene;
            if (!scene.IsValid() || !scene.isLoaded || EditorSceneManager.IsPreviewScene(scene))
                throw new InvalidOperationException("Prefab Stage가 아닌 열린 Scene을 사용하세요.");
            if (font == null || !EditorUtility.IsPersistent(font))
                throw new InvalidOperationException("프로젝트에 저장된 TMP Font Asset을 지정하세요.");
            var views = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<DialogueUGUIView>(true))
                .Where(view => view.player == player).ToArray();
            if (views.Length > 1) throw new InvalidOperationException("Player에 View가 여러 개 연결되어 있습니다. 하나만 남겨주세요.");
            if (views.Length == 1 && views[0].ValidateReferences() != null)
                throw new InvalidOperationException(views[0].ValidateReferences());
            var systems = UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .Where(system => system.gameObject.scene.IsValid() && !EditorSceneManager.IsPreviewScene(system.gameObject.scene)).ToArray();
            var active = systems.Where(system => system.isActiveAndEnabled).ToArray();
            if (active.Length > 1) throw new InvalidOperationException("열린 Scene에 활성 EventSystem이 여러 개 있습니다. 중복을 먼저 정리하세요.");
            var eventSystem = active.FirstOrDefault();
            if (eventSystem != null && eventSystem.gameObject.scene != scene)
                throw new InvalidOperationException("다른 Scene의 EventSystem이 활성 상태입니다. 대상 Scene을 단독으로 열고 이전하세요.");
            if (eventSystem == null && systems.Any(system => system.gameObject.scene == scene))
                throw new InvalidOperationException("현재 Scene의 비활성 EventSystem을 먼저 활성화하세요.");

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Migrate dialogue to UGUI");
            try
            {
                EnsureEventSystem(scene, eventSystem);
                var view = views.FirstOrDefault() ?? CreateView(player, font);
                Undo.RecordObject(player, "Use UGUI");
                player.useUGUI = true;
                PrefabUtility.RecordPrefabInstancePropertyModifications(player);
                EditorUtility.SetDirty(player);
                EditorSceneManager.MarkSceneDirty(scene);
                Undo.CollapseUndoOperations(group);
                // Glyph availability depends on the selected font/fallback; do not mutate font assets here.
                if (!font.HasCharacters("안내자대화이전다음저장불러오기종료", out uint[] _, true, false))
                    Debug.LogWarning("선택한 TMP 폰트에 한글 UI 글리프가 부족합니다. 한글 폰트/fallback을 연결하세요.", view);
                return view;
            }
            catch { Undo.RevertAllDownToGroup(group); throw; }
        }

        private static void EnsureEventSystem(Scene scene, EventSystem system)
        {
            if (system == null)
            {
                var host = new GameObject("EventSystem");
                SceneManager.MoveGameObjectToScene(host, scene);
                Undo.RegisterCreatedObjectUndo(host, "Create EventSystem");
                system = Undo.AddComponent<EventSystem>(host);
            }
            foreach (var legacy in system.GetComponents<BaseInputModule>())
            {
                if (legacy is InputSystemUIInputModule) continue;
                Undo.RecordObject(legacy, "Disable legacy input module");
                legacy.enabled = false;
                PrefabUtility.RecordPrefabInstancePropertyModifications(legacy);
            }
            var module = system.GetComponent<InputSystemUIInputModule>();
            if (module == null) module = Undo.AddComponent<InputSystemUIInputModule>(system.gameObject);
            Undo.RecordObject(module, "Configure UI input");
            module.enabled = true;
            if (module.actionsAsset == null || !EditorUtility.IsPersistent(module.actionsAsset)) AssignPersistentDefaultActions(module);
            if (module.point == null || module.leftClick == null || module.move == null || module.submit == null)
                throw new InvalidOperationException("EventSystem의 UI Actions에 Point, Left Click, Move, Submit을 연결하세요.");
            PrefabUtility.RecordPrefabInstancePropertyModifications(module);
        }

        private static void AssignPersistentDefaultActions(InputSystemUIInputModule module)
        {
            // AssignDefaultActions creates transient objects for runtime use. Scene references
            // must point to the package's imported asset/subassets so they survive reopening.
            const string path = "Packages/com.unity.inputsystem/InputSystem/Plugins/PlayerInput/DefaultInputActions.inputactions";
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            if (asset == null) throw new InvalidOperationException("Input System 기본 UI Actions 자산을 찾을 수 없습니다.");
            var references = AssetDatabase.LoadAllAssetsAtPath(path).OfType<InputActionReference>().ToArray();
            InputActionReference Action(string name)
            {
                var action = asset.FindAction("UI/" + name, true);
                return references.FirstOrDefault(reference => reference.action != null && reference.action.id == action.id)
                    ?? throw new InvalidOperationException("기본 UI Action 참조를 찾을 수 없습니다: " + name);
            }
            module.actionsAsset = asset;
            module.point = Action("Point");
            module.leftClick = Action("Click");
            module.rightClick = Action("RightClick");
            module.middleClick = Action("MiddleClick");
            module.scrollWheel = Action("ScrollWheel");
            module.move = Action("Navigate");
            module.submit = Action("Submit");
            module.cancel = Action("Cancel");
            module.trackedDevicePosition = Action("TrackedDevicePosition");
            module.trackedDeviceOrientation = Action("TrackedDeviceOrientation");
        }

        private static DialogueUGUIView CreateView(DialogueSamplePlayer player, TMP_FontAsset font)
        {
            var canvasObject = new GameObject(player.name + " UGUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            SceneManager.MoveGameObjectToScene(canvasObject, player.gameObject.scene);
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create dialogue canvas");
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            var panel = Rect("DialoguePanel", canvasObject.transform, 16, 16, 928, 688);
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(.5f, .5f);
            panel.anchoredPosition = Vector2.zero;
            var background = panel.gameObject.AddComponent<Image>();
            background.color = new Color(.04f, .06f, .1f, .97f);
            background.raycastTarget = false;
            var view = panel.gameObject.AddComponent<DialogueUGUIView>();
            view.player = player;
            Label("Title", panel, font, "Dialogue", 24, 14, 880, 40);
            view.jsonButton = Button("JSON", panel, font, 24, 65, 180);
            view.configButton = Button("ScriptableObject", panel, font, 219, 65, 180);
            view.branchJsonButton = Button("Branch JSON", panel, font, 414, 65, 180);
            view.branchConfigButton = Button("Branch SO", panel, font, 609, 65, 180);
            var portrait = Rect("PortraitImage", panel, 24, 144, 200, 240).gameObject.AddComponent<Image>();
            portrait.preserveAspect = true; portrait.raycastTarget = false;
            view.portraitImage = portrait;
            view.speakerText = Label("SpeakerText", panel, font, "화자", 254, 139, 630, 45);
            // Both body and choices scroll independently, allowing arbitrarily long text.
            var bodyScroll = Scroll("DialogueScroll", panel, 254, 199, 630, 165, out var bodyContent);
            view.dialogueScroll = bodyScroll;
            view.dialogueText = Label("DialogueText", bodyContent, font, "대사", 0, 0, 630, 0);
            StretchWidth(view.dialogueText.rectTransform);
            var bodyFitter = bodyContent.gameObject.AddComponent<ContentSizeFitter>();
            bodyFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var bodyLayout = bodyContent.gameObject.AddComponent<VerticalLayoutGroup>();
            bodyLayout.childControlHeight = bodyLayout.childControlWidth = true;
            bodyLayout.childForceExpandHeight = false;
            bodyScroll.scrollSensitivity = 30;
            view.choiceScroll = Scroll("ChoiceScrollView", panel, 254, 369, 630, 160, out var content);
            view.choiceRoot = content;
            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6; layout.padding = new RectOffset(4, 4, 4, 4);
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            // Inactive scene template gives Undo atomicity and avoids overwriting any prefab asset.
            var templateButton = Button("ChoiceTemplate", panel, font, 0, 0, 610);
            var templateLayout = templateButton.gameObject.AddComponent<VerticalLayoutGroup>();
            templateLayout.padding = new RectOffset(12, 12, 10, 10);
            templateLayout.childControlWidth = templateLayout.childControlHeight = true;
            templateLayout.childForceExpandHeight = false;
            var minimum = templateButton.gameObject.AddComponent<LayoutElement>();
            minimum.minHeight = 58;
            view.choiceButtonPrefab = templateButton.gameObject.AddComponent<DialogueChoiceButton>();
            view.choiceButtonPrefab.button = templateButton;
            view.choiceButtonPrefab.label = templateButton.GetComponentInChildren<TMP_Text>();
            templateButton.gameObject.SetActive(false);
            view.previousButton = Button("이전", panel, font, 254, 539, 180);
            view.restartButton = Button("재시작", panel, font, 449, 539, 180);
            view.advanceButton = Button("다음", panel, font, 644, 539, 180);
            view.advanceLabel = view.advanceButton.GetComponentInChildren<TMP_Text>();
            view.saveButton = Button("저장", panel, font, 254, 589, 180);
            view.loadButton = Button("불러오기", panel, font, 449, 589, 180);
            view.closeButton = Button("닫기", panel, font, 644, 589, 180);
            view.statusText = Label("StatusText", panel, font, "", 24, 634, 880, 48);
            view.statusText.fontSize = 16;
            return view;
        }

        private static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(width, height);
            return rect;
        }

        private static void StretchWidth(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(1, 1);
            rect.sizeDelta = new Vector2(0, rect.sizeDelta.y);
        }

        private static TMP_Text Label(string name, Transform parent, TMP_FontAsset font, string text, float x, float y, float width, float height)
        {
            var label = Rect(name, parent, x, y, width, height).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font; label.fontSize = 22; label.text = text;
            label.richText = false; label.raycastTarget = false;
            label.color = Color.white;
            return label;
        }

        private static Button Button(string text, Transform parent, TMP_FontAsset font, float x, float y, float width)
        {
            var rect = Rect(text, parent, x, y, width, 40);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(.16f, .22f, .32f);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var label = Label("Label", rect, font, text, 0, 0, width, 40);
            label.fontSize = 18; label.alignment = TextAlignmentOptions.Center;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.offsetMin = new Vector2(6, 2);
            label.rectTransform.offsetMax = new Vector2(-6, -2);
            return button;
        }

        private static ScrollRect Scroll(string name, Transform parent, float x, float y, float width, float height, out RectTransform content)
        {
            var rect = Rect(name, parent, x, y, width, height);
            var scroll = rect.gameObject.AddComponent<ScrollRect>();
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0, 0, 0, .05f);
            rect.gameObject.AddComponent<RectMask2D>();
            content = Rect("Content", rect, 0, 0, width, 0);
            StretchWidth(content);
            scroll.viewport = rect; scroll.content = content;
            scroll.horizontal = false; scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30;
            return scroll;
        }
    }
}
