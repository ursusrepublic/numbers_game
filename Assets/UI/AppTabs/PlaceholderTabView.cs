using Game.UI.Layout;
using Game.UI.Styling;
using TMPro;
using UnityEngine;

namespace Game.UI.AppTabs
{
    [DisallowMultipleComponent]
    public sealed class PlaceholderTabView : MonoBehaviour
    {
        private const float TitleFontSize = 62f;
        private const float MinTitleFontSize = 36f;
        private const float MaxTitleFontSize = 68f;

        private RectTransform _rootRect;
        private TMP_Text _titleLabel;
        private Vector2 _lastRootSize = new Vector2(-1f, -1f);

        public static PlaceholderTabView Create(
            Transform parent,
            string title,
            TMP_FontAsset regularFont,
            TMP_FontAsset boldFont)
        {
            var rootObject = new GameObject(
                $"{title}TabView",
                typeof(RectTransform),
                typeof(PlaceholderTabView));

            rootObject.transform.SetParent(parent, false);

            var rootRect = (RectTransform)rootObject.transform;
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var titleObject = new GameObject("TitleLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObject.transform.SetParent(rootObject.transform, false);

            var titleRect = (RectTransform)titleObject.transform;
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(600f, 120f);
            titleRect.anchoredPosition = Vector2.zero;

            TMP_Text titleLabel = titleObject.GetComponent<TextMeshProUGUI>();
            titleLabel.text = title;
            titleLabel.alignment = TextAlignmentOptions.Center;
            titleLabel.enableAutoSizing = false;
            titleLabel.textWrappingMode = TextWrappingModes.NoWrap;
            titleLabel.overflowMode = TextOverflowModes.Overflow;

            var view = rootObject.GetComponent<PlaceholderTabView>();
            view.Setup(rootRect, titleLabel, regularFont, boldFont);
            return view;
        }

        private void OnEnable()
        {
            ApplyResponsiveLayout(force: true);
        }

        private void LateUpdate()
        {
            ApplyResponsiveLayout(force: false);
        }

        private void Setup(RectTransform rootRect, TMP_Text titleLabel, TMP_FontAsset regularFont, TMP_FontAsset boldFont)
        {
            TMP_FontAsset effectiveRegularFont = regularFont != null
                ? regularFont
                : boldFont != null
                    ? boldFont
                    : TMP_Settings.defaultFontAsset;
            TMP_FontAsset effectiveBoldFont = boldFont != null ? boldFont : effectiveRegularFont;

            _rootRect = rootRect;
            _titleLabel = titleLabel;

            _titleLabel.font = effectiveBoldFont;
            _titleLabel.fontSize = TitleFontSize;
            _titleLabel.color = GamePalette.ScoreValueText;

            ApplyResponsiveLayout(force: true);
        }

        private void ApplyResponsiveLayout(bool force)
        {
            if (_rootRect == null || _titleLabel == null)
            {
                return;
            }

            Rect rootRect = _rootRect.rect;
            Vector2 rootSize = new Vector2(rootRect.width, rootRect.height);
            if (rootSize.x <= 0f || rootSize.y <= 0f)
            {
                return;
            }

            if (!force && Vector2.Distance(_lastRootSize, rootSize) < 0.5f)
            {
                return;
            }

            _lastRootSize = rootSize;

            float scale = MobileLayout.GetScale(rootRect.width, rootRect.height);
            _titleLabel.fontSize = MobileLayout.ClampScaled(TitleFontSize, MinTitleFontSize, MaxTitleFontSize, scale);
        }
    }
}
