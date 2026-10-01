using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FireballMediatorSample.Editor
{
    public static class FireballHotbarInstaller
    {
        const string InstalledKey = "FireballMediatorSample.HotbarInstalled.v1";

        // Apply this requested migration to the currently open sample after script import.
        // Only dirty the scene; the user retains control of saving their current scene edits.
        [InitializeOnLoadMethod]
        static void ScheduleCurrentSceneInstall() => EditorApplication.delayCall += InstallOnce;

        static void InstallOnce()
        {
            if (SessionState.GetBool(InstalledKey, false) || EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += InstallOnce;
                return;
            }
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded) return;
            var manager = Find<SkillManager>(scene);
            var input = Find<PlayerSkillInput>(scene);
            if (manager == null || input == null) return;
            try
            {
                Install(scene, manager, input);
                SessionState.SetBool(InstalledKey, true);
                Debug.Log("Fireball UGUI hotbar installed in the current scene. Save the scene to keep it.");
            }
            catch (Exception e) { Debug.LogException(e); }
        }

        [MenuItem("Tools/Skill Manager/Add Hotbar To Current Scene")]
        public static void InstallCurrentScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play mode before adding the hotbar.");
            var scene = SceneManager.GetActiveScene();
            var manager = Find<SkillManager>(scene);
            var input = Find<PlayerSkillInput>(scene);
            if (manager == null || input == null)
                throw new InvalidOperationException("The active scene needs a SkillManager and PlayerSkillInput.");
            Install(scene, manager, input);
        }

        static T Find<T>(Scene scene) where T : Component => scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<T>(true)).FirstOrDefault();

        public static void Install(Scene scene, SkillManager manager, PlayerSkillInput input)
        {
            if (Find<SkillHotbarUI>(scene) != null) return;
            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (font == null) font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            if (font == null) throw new InvalidOperationException("Import TMP Essential Resources before creating the hotbar.");

            Undo.IncrementCurrentGroup();
            int group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Add Fireball Hotbar");
            var root = new GameObject("Fireball Hotbar Canvas", typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(root, scene);
            root.SetActive(false);
            Undo.RegisterCreatedObjectUndo(root, "Create hotbar");
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            var panel = Rect("Hotbar", root.transform, new Vector2(0.5f, 0), new Vector2(0, 100), new Vector2(320, 170));
            AddImage(panel, new Color(0.035f, 0.05f, 0.08f, 0.95f));
            var key = Label("Bound Keys", panel, new Vector2(0, 60), new Vector2(300, 30), font, 21);
            key.color = new Color(1, 0.83f, 0.42f);

            var slot = Rect("Fireball Slot", panel, Vector2.one * 0.5f, new Vector2(0, -1), new Vector2(82, 82));
            var face = AddImage(slot, new Color(0.8f, 0.22f, 0.045f));
            face.raycastTarget = true;
            var button = slot.gameObject.AddComponent<Button>();
            button.targetGraphic = face;
            // Space belongs to the skill action; avoid a second Submit cast from a selected UI button.
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            var title = Label("Skill Name", slot, Vector2.zero, new Vector2(80, 30), font, 17);
            title.text = "FIREBALL";

            var cover = Rect("Cooldown Fill", slot, Vector2.one * 0.5f, Vector2.zero, new Vector2(82, 82));
            var fill = AddImage(cover, new Color(0.015f, 0.02f, 0.04f, 0.85f));
            fill.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Radial360;
            fill.fillOrigin = (int)Image.Origin360.Top;
            fill.fillClockwise = true; fill.fillAmount = 0;
            var remaining = Label("Remaining Cooldown", slot, Vector2.zero, new Vector2(82, 40), font, 24);
            var state = Label("Skill State", panel, new Vector2(0, -64), new Vector2(300, 26), font, 16);
            state.text = "READY";

            root.AddComponent<CoolTimeUI>().Configure(manager, 0, fill, remaining);
            root.AddComponent<SkillHotbarUI>().Configure(manager, input, button, key, state);
            root.SetActive(true);

            var eventSystem = Find<EventSystem>(scene);
            if (eventSystem == null)
            {
                var events = new GameObject("Hotbar EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                SceneManager.MoveGameObjectToScene(events, scene);
                Undo.RegisterCreatedObjectUndo(events, "Create UI event system");
            }
            else if (eventSystem.GetComponent<InputSystemUIInputModule>() == null &&
                eventSystem.GetComponent<BaseInputModule>() == null)
                Undo.AddComponent<InputSystemUIInputModule>(eventSystem.gameObject);

            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(group);
        }

        static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position; rect.sizeDelta = size;
            return rect;
        }
        static Image AddImage(RectTransform rect, Color color)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color; image.raycastTarget = false;
            return image;
        }
        static TextMeshProUGUI Label(string name, Transform parent, Vector2 position, Vector2 size, TMP_FontAsset font, float fontSize)
        {
            var label = Rect(name, parent, Vector2.one * 0.5f, position, size).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font; label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white; label.raycastTarget = false;
            label.text = string.Empty;
            return label;
        }
    }
}
