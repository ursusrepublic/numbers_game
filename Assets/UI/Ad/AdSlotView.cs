using System;
using Game.UI.Styling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Ad
{
    [DisallowMultipleComponent]
    public sealed class AdSlotView : MonoBehaviour
    {
        private const float MinimumBannerHeight = 50f;
        private const float BannerAspectWidth = 32f;
        private const float BannerAspectHeight = 5f;

        private RectTransform _rootRect;
        private RectTransform _placeholderRect;
        private float _currentWidth = -1f;
        private float _currentHeight = -1f;

        public event Action<float> HeightChanged;

        public float CurrentHeight => _currentHeight > 0f ? _currentHeight : MinimumBannerHeight;

        public static AdSlotView Create(Transform parent, TMP_FontAsset regularFont)
        {
            var adSlotObject = new GameObject(
                "AdSlotRoot",
                typeof(RectTransform),
                typeof(Image),
                typeof(AdSlotView));

            adSlotObject.transform.SetParent(parent, false);

            var rootRect = (RectTransform)adSlotObject.transform;
            rootRect.anchorMin = new Vector2(0f, 0f);
            rootRect.anchorMax = new Vector2(1f, 0f);
            rootRect.pivot = new Vector2(0.5f, 0f);
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            rootRect.sizeDelta = new Vector2(0f, MinimumBannerHeight);

            var rootImage = adSlotObject.GetComponent<Image>();
            rootImage.color = GamePalette.AdSlotBackground;
            rootImage.raycastTarget = false;

            var topBorderObject = new GameObject(
                "TopBorder",
                typeof(RectTransform),
                typeof(Image));

            topBorderObject.transform.SetParent(adSlotObject.transform, false);

            var topBorderRect = (RectTransform)topBorderObject.transform;
            topBorderRect.anchorMin = new Vector2(0f, 1f);
            topBorderRect.anchorMax = new Vector2(1f, 1f);
            topBorderRect.pivot = new Vector2(0.5f, 1f);
            topBorderRect.offsetMin = new Vector2(0f, -2f);
            topBorderRect.offsetMax = Vector2.zero;

            var topBorderImage = topBorderObject.GetComponent<Image>();
            topBorderImage.color = GamePalette.AdSlotBorder;
            topBorderImage.raycastTarget = false;

            var placeholderObject = new GameObject(
                "AdSlotPlaceholder",
                typeof(RectTransform),
                typeof(Image),
                typeof(Outline));

            placeholderObject.transform.SetParent(adSlotObject.transform, false);

            var placeholderRect = (RectTransform)placeholderObject.transform;
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = new Vector2(16f, 8f);
            placeholderRect.offsetMax = new Vector2(-16f, -8f);

            var placeholderImage = placeholderObject.GetComponent<Image>();
            placeholderImage.color = new Color(1f, 1f, 1f, 0.72f);
            placeholderImage.raycastTarget = false;

            var placeholderOutline = placeholderObject.GetComponent<Outline>();
            placeholderOutline.effectColor = GamePalette.AdSlotBorder;
            placeholderOutline.effectDistance = new Vector2(1f, -1f);
            placeholderOutline.useGraphicAlpha = false;

            var labelObject = new GameObject(
                "Label",
                typeof(RectTransform),
                typeof(TextMeshProUGUI));

            labelObject.transform.SetParent(placeholderObject.transform, false);

            var labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = regularFont != null ? regularFont : TMP_Settings.defaultFontAsset;
            label.fontSize = 34f;
            label.enableAutoSizing = false;
            label.enableWordWrapping = false;
            label.alignment = TextAlignmentOptions.Center;
            label.color = GamePalette.BoardTileText;
            label.text = "Ad Banner Placeholder";
            label.raycastTarget = false;

            var adSlotView = adSlotObject.GetComponent<AdSlotView>();
            adSlotView.Setup(rootRect, placeholderRect);
            return adSlotView;
        }

        private void LateUpdate()
        {
            RefreshLayout();
        }

        private void OnEnable()
        {
            RefreshLayout();
        }

        private void Setup(RectTransform rootRect, RectTransform placeholderRect)
        {
            _rootRect = rootRect;
            _placeholderRect = placeholderRect;
            RefreshLayout();
        }

        private void RefreshLayout()
        {
            if (_rootRect == null)
            {
                return;
            }

            float width = _rootRect.rect.width;
            if (width <= 0f)
            {
                return;
            }

            float targetHeight = Mathf.Max(MinimumBannerHeight, width * (BannerAspectHeight / BannerAspectWidth));
            if (Mathf.Abs(width - _currentWidth) < 0.5f && Mathf.Abs(targetHeight - _currentHeight) < 0.5f)
            {
                return;
            }

            _currentWidth = width;
            _currentHeight = targetHeight;
            _rootRect.sizeDelta = new Vector2(0f, targetHeight);

            if (_placeholderRect != null)
            {
                _placeholderRect.offsetMin = new Vector2(16f, 8f);
                _placeholderRect.offsetMax = new Vector2(-16f, -8f);
            }

            HeightChanged?.Invoke(targetHeight);
        }
    }
}
