using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SysKill.SkillIndicators
{
    public sealed class SkillDamagePopup : MonoBehaviour
    {
        static Canvas canvas;
        Camera worldCamera;
        Vector3 worldPosition;
        RectTransform rect;
        CanvasGroup visibility;
        Sequence popupTween;
        float rise;

        public static void Show(int damage, Vector3 position, Camera camera = null)
        {
            if (camera == null) camera = Camera.main;
            if (camera == null) camera = FindAnyObjectByType<Camera>();
            if (camera == null) return;
            if (canvas == null)
            {
                var root = new GameObject("Skill Damage UI", typeof(Canvas), typeof(CanvasScaler));
                canvas = root.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                var scaler = root.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }
            var go = new GameObject("Damage", typeof(RectTransform), typeof(CanvasGroup), typeof(TextMeshProUGUI), typeof(SkillDamagePopup));
            go.transform.SetParent(canvas.transform, false);
            var popup = go.GetComponent<SkillDamagePopup>();
            popup.worldCamera = camera;
            popup.worldPosition = position;
            popup.rect = go.GetComponent<RectTransform>();
            popup.rect.sizeDelta = new Vector2(240, 90);
            popup.visibility = go.GetComponent<CanvasGroup>();
            popup.visibility.blocksRaycasts = false;
            popup.visibility.interactable = false;
            var label = go.GetComponent<TextMeshProUGUI>();
            label.text = damage > 0 ? damage.ToString() : "BLOCK";
            label.fontSize = damage > 0 ? 44 : 30;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = damage > 0 ? new Color(1, 0.35f, 0.16f) : new Color(0.65f, 0.8f, 1);
            label.raycastTarget = false;
            popup.rect.localScale = Vector3.one * 0.35f;
            popup.LateUpdate();
            popup.popupTween = DOTween.Sequence()
                .Append(popup.rect.DOScale(1.25f, 0.14f).SetEase(Ease.OutBack))
                .Append(popup.rect.DOScale(1, 0.12f).SetEase(Ease.OutQuad))
                .Insert(0, DOTween.To(() => popup.rise, value => popup.rise = value, 100, 1f).SetEase(Ease.OutCubic))
                .Insert(0.5f, DOTween.To(() => label.alpha, value => label.alpha = value, 0, 0.5f))
                .OnComplete(() => Destroy(go));
        }

        void LateUpdate()
        {
            if (worldCamera == null || canvas == null) { Destroy(gameObject); return; }
            Vector3 screen = worldCamera.WorldToScreenPoint(worldPosition);
            visibility.alpha = screen.z > 0 ? 1 : 0;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform, screen, null, out Vector2 local))
                rect.anchoredPosition = local + Vector2.up * rise;
        }
        void OnDestroy() => popupTween?.Kill();
    }
}
