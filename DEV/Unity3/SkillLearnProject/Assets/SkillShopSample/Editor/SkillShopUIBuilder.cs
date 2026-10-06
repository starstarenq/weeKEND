using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KDH_SkillSystem.ShopSample.EditorTools
{
    /// <summary>
    /// 상점/인벤토리 UGUI 계층을 Unity API(AddComponent, LayoutGroup)로만 생성한다.
    /// 씬 YAML 이나 GUID 를 직접 다루지 않으며, 직렬화는 Unity 가 처리한다.
    /// </summary>
    public static class SkillShopUIBuilder
    {
        static readonly Color PanelColor = new Color(0.07f, 0.09f, 0.13f, 0.97f);
        static readonly Color RowColor = new Color(0.12f, 0.15f, 0.21f, 1f);
        static readonly Color HeaderColor = new Color(1f, 0.83f, 0.42f);
        static readonly Color ButtonColor = new Color(0.22f, 0.55f, 0.32f, 1f);

        /// <summary>Canvas, 상점 패널, 인벤토리 패널, EventSystem 을 씬에 생성하고 뷰를 연결한다.</summary>
        public static void Build(Scene scene, SkillShop shop, SkillInventory inventory, TMP_FontAsset font)
        {
            if (font == null) throw new InvalidOperationException("TMP 폰트를 찾지 못했습니다. TMP Essential Resources 를 임포트하세요.");
            Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            Sprite bgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");

            // ── Canvas ──
            var root = new GameObject("Skill Shop Canvas", typeof(RectTransform));
            SceneManager.MoveGameObjectToScene(root, scene);
            root.SetActive(false); // 연결이 끝난 뒤 활성화해 OnEnable 구독이 올바른 참조로 이뤄지게 한다.
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();

            var background = Stretch("Background", root.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            AddImage(background, new Color(0.03f, 0.04f, 0.06f, 1f));

            var title = Label("Title", root.transform, font, 34, TextAlignmentOptions.Center);
            Anchor(title.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -70), new Vector2(0, -16));
            title.text = "SKILL SHOP";
            title.color = HeaderColor;
            title.fontStyle = FontStyles.Bold;

            // ── 상점 패널 (왼쪽) ──
            var shopPanel = Stretch("Shop Panel", root.transform, new Vector2(0, 0), new Vector2(0.6f, 1),
                new Vector2(24, 24), new Vector2(-12, -84));
            AddImage(shopPanel, PanelColor, bgSprite);

            var shopHeader = Label("Shop Header", shopPanel, font, 24, TextAlignmentOptions.MidlineLeft);
            Anchor(shopHeader.rectTransform, new Vector2(0, 1), new Vector2(0.5f, 1), new Vector2(20, -52), new Vector2(0, -12));
            shopHeader.text = "상점";
            shopHeader.color = HeaderColor;

            var goldLabel = Label("Gold Label", shopPanel, font, 22, TextAlignmentOptions.MidlineRight);
            Anchor(goldLabel.rectTransform, new Vector2(0.5f, 1), new Vector2(1, 1), new Vector2(0, -52), new Vector2(-20, -12));

            // 스크롤 목록 (Viewport + Content: VerticalLayoutGroup + ContentSizeFitter)
            var scroll = Stretch("Shop Scroll View", shopPanel, Vector2.zero, Vector2.one, new Vector2(16, 64), new Vector2(-16, -60));
            var viewport = Stretch("Viewport", scroll, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            viewport.gameObject.AddComponent<RectMask2D>();
            AddImage(viewport, new Color(0, 0, 0, 0.001f)).raycastTarget = true; // 스크롤 드래그 영역
            var content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.offsetMin = content.offsetMax = Vector2.zero;
            var list = content.gameObject.AddComponent<VerticalLayoutGroup>();
            list.spacing = 10;
            list.padding = new RectOffset(4, 4, 4, 4);
            list.childControlWidth = list.childControlHeight = true;
            list.childForceExpandWidth = true; list.childForceExpandHeight = false;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scrollRect = scroll.gameObject.AddComponent<ScrollRect>();
            scrollRect.viewport = viewport; scrollRect.content = content;
            scrollRect.horizontal = false; scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 30;

            var itemTemplate = BuildShopItemTemplate(content, font, uiSprite);

            var messageBox = Stretch("Message Box", shopPanel, new Vector2(0, 0), new Vector2(1, 0), new Vector2(16, 14), new Vector2(-16, 54));
            AddImage(messageBox, new Color(0, 0, 0, 0.35f), bgSprite);
            var message = Label("Message", messageBox, font, 18, TextAlignmentOptions.Center);
            Anchor(message.rectTransform, Vector2.zero, Vector2.one, new Vector2(8, 0), new Vector2(-8, 0));

            root.AddComponent<SkillShopView>().Configure(shop, itemTemplate, content, goldLabel, message);

            // ── 인벤토리 패널 (오른쪽) ──
            var invPanel = Stretch("Inventory Panel", root.transform, new Vector2(0.6f, 0), new Vector2(1, 1),
                new Vector2(12, 24), new Vector2(-24, -84));
            AddImage(invPanel, PanelColor, bgSprite);

            var invHeader = Label("Inventory Header", invPanel, font, 24, TextAlignmentOptions.MidlineLeft);
            Anchor(invHeader.rectTransform, new Vector2(0, 1), new Vector2(0.6f, 1), new Vector2(20, -52), new Vector2(0, -12));
            invHeader.text = "인벤토리 (샘플)";
            invHeader.color = HeaderColor;

            var countLabel = Label("Count Label", invPanel, font, 20, TextAlignmentOptions.MidlineRight);
            Anchor(countLabel.rectTransform, new Vector2(0.6f, 1), new Vector2(1, 1), new Vector2(0, -52), new Vector2(-20, -12));

            var grid = Stretch("Slot Grid", invPanel, Vector2.zero, Vector2.one, new Vector2(16, 16), new Vector2(-16, -64));
            var gridLayout = grid.gameObject.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = new Vector2(104, 124);
            gridLayout.spacing = new Vector2(10, 10);
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 4;
            gridLayout.childAlignment = TextAnchor.UpperCenter;

            var slotTemplate = BuildSlotTemplate(grid, font, bgSprite);
            root.AddComponent<SkillInventoryView>().Configure(inventory, slotTemplate, grid, countLabel);

            root.SetActive(true);
            EnsureEventSystem(scene);
        }

        /// <summary>상품 한 줄 템플릿: [아이콘][이름/설명][가격][구매 버튼] (HorizontalLayoutGroup)</summary>
        static SkillShopItemView BuildShopItemTemplate(Transform parent, TMP_FontAsset font, Sprite uiSprite)
        {
            var row = new GameObject("Shop Item Template", typeof(RectTransform)).GetComponent<RectTransform>();
            row.SetParent(parent, false);
            AddImage(row, RowColor, uiSprite);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 96;
            var h = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.padding = new RectOffset(12, 12, 10, 10);
            h.spacing = 14;
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = h.childControlHeight = true;
            h.childForceExpandWidth = h.childForceExpandHeight = false;

            var iconFrame = Child("Icon Frame", row);
            AddImage(iconFrame, new Color(0.05f, 0.06f, 0.09f, 1f), uiSprite);
            Fixed(iconFrame, 72, 72);
            var icon = AddImage(Stretch("Icon", iconFrame, Vector2.zero, Vector2.one, new Vector2(6, 6), new Vector2(-6, -6)), Color.white);
            icon.preserveAspect = true;

            var textColumn = Child("Text", row);
            var textLayout = textColumn.gameObject.AddComponent<LayoutElement>();
            textLayout.flexibleWidth = 1; textLayout.minWidth = 120;
            var v = textColumn.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = 4;
            v.childAlignment = TextAnchor.MiddleLeft;
            v.childControlWidth = v.childControlHeight = true;
            v.childForceExpandWidth = true; v.childForceExpandHeight = false;
            var nameLabel = Label("Name", textColumn, font, 22, TextAlignmentOptions.MidlineLeft);
            var description = Label("Description", textColumn, font, 16, TextAlignmentOptions.TopLeft);
            description.color = new Color(0.75f, 0.8f, 0.88f);
            description.overflowMode = TextOverflowModes.Ellipsis;

            var price = Label("Price", row, font, 22, TextAlignmentOptions.MidlineRight);
            Fixed(price.rectTransform, 110, 40);

            var buttonRect = Child("Buy Button", row);
            Fixed(buttonRect, 104, 48);
            var buttonImage = AddImage(buttonRect, ButtonColor, uiSprite);
            buttonImage.raycastTarget = true;
            var button = buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            var colors = button.colors;
            colors.disabledColor = new Color(0.45f, 0.45f, 0.5f, 0.6f);
            button.colors = colors;
            var buyLabel = Label("Label", buttonRect, font, 20, TextAlignmentOptions.Center);
            Anchor(buyLabel.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            buyLabel.text = "구매";

            var view = row.gameObject.AddComponent<SkillShopItemView>();
            view.Configure(icon, nameLabel, description, price, button, buyLabel);
            row.gameObject.SetActive(false); // 비활성 템플릿은 레이아웃 계산에서 제외된다.
            return view;
        }

        /// <summary>인벤토리 한 칸 템플릿: [프레임 > 아이콘, 이름]</summary>
        static SkillInventorySlotView BuildSlotTemplate(Transform parent, TMP_FontAsset font, Sprite bgSprite)
        {
            var slot = new GameObject("Slot Template", typeof(RectTransform)).GetComponent<RectTransform>();
            slot.SetParent(parent, false);
            var frame = AddImage(slot, RowColor, bgSprite);
            var icon = AddImage(Stretch("Icon", slot, new Vector2(0, 0.3f), Vector2.one, new Vector2(14, 0), new Vector2(-14, -10)), Color.white);
            icon.preserveAspect = true;
            var label = Label("Name", slot, font, 16, TextAlignmentOptions.Center);
            Anchor(label.rectTransform, Vector2.zero, new Vector2(1, 0.3f), new Vector2(4, 4), new Vector2(-4, 0));
            label.overflowMode = TextOverflowModes.Ellipsis;

            var view = slot.gameObject.AddComponent<SkillInventorySlotView>();
            view.Configure(frame, icon, label);
            slot.gameObject.SetActive(false);
            return view;
        }

        /// <summary>New Input System 용 EventSystem 이 없으면 생성한다.</summary>
        static void EnsureEventSystem(Scene scene)
        {
            foreach (var go in scene.GetRootGameObjects())
                if (go.GetComponentInChildren<EventSystem>(true) != null) return;
            var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            SceneManager.MoveGameObjectToScene(events, scene);
        }

        // ── 공통 헬퍼 ──
        static RectTransform Child(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        static RectTransform Stretch(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rect = Child(name, parent);
            Anchor(rect, anchorMin, anchorMax, offsetMin, offsetMax);
            return rect;
        }

        static void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
        }

        static void Fixed(RectTransform rect, float width, float height)
        {
            var element = rect.gameObject.GetComponent<LayoutElement>();
            if (element == null) element = rect.gameObject.AddComponent<LayoutElement>();
            element.minWidth = element.preferredWidth = width;
            element.minHeight = element.preferredHeight = height;
        }

        static Image AddImage(RectTransform rect, Color color, Sprite sprite = null)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            if (sprite != null) { image.sprite = sprite; image.type = Image.Type.Sliced; }
            return image;
        }

        static TextMeshProUGUI Label(string name, Transform parent, TMP_FontAsset font, float size, TextAlignmentOptions alignment)
        {
            var label = Child(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font; label.fontSize = size;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.text = string.Empty;
            return label;
        }
    }
}
