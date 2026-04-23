using System;
using Game.UI.Layout;
using Game.UI.Styling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.AppTabs
{
    [DisallowMultipleComponent]
    public sealed class MainTabView : MonoBehaviour
    {
        private const float PanelWidth = 760f;
        private const float MinPanelWidth = 320f;
        private const float ButtonWidth = 420f;
        private const float MinButtonWidth = 240f;
        private const float ButtonHeight = 96f;
        private const float MinButtonHeight = 72f;
        private const float MaxButtonHeight = 108f;
        private const float Spacing = 24f;
        private const float MinSpacing = 16f;
        private const float MaxSpacing = 28f;

        private RectTransform _contentRect;
        private RectTransform _panelRect;
        private VerticalLayoutGroup _panelLayout;
        private LayoutElement _continueLayout;
        private LayoutElement _newGameLayout;
        private Button _continueButton;
        private Button _newGameButton;
        private TMP_Text _titleLabel;
        private TMP_Text _bestScoreLabel;
        private TMP_Text _continueLabel;
        private TMP_Text _newGameLabel;
        private Vector2 _lastContentSize = new Vector2(-1f, -1f);

        public event Action ContinueClicked;
        public event Action NewGameClicked;

        public static MainTabView Create(
            Transform parent,
            TMP_FontAsset regularFont,
            TMP_FontAsset boldFont)
        {
            var rootObject = new GameObject(
                "MainTabView",
                typeof(RectTransform),
                typeof(MainTabView));

            rootObject.transform.SetParent(parent, false);

            var rootRect = (RectTransform)rootObject.transform;
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var panelObject = new GameObject(
                "CenterPanel",
                typeof(RectTransform),
                typeof(VerticalLayoutGroup));

            panelObject.transform.SetParent(rootObject.transform, false);

            var panelRect = (RectTransform)panelObject.transform;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(PanelWidth, 520f);

            var panelLayout = panelObject.GetComponent<VerticalLayoutGroup>();
            panelLayout.childAlignment = TextAnchor.MiddleCenter;
            panelLayout.spacing = Spacing;
            panelLayout.padding = new RectOffset(0, 0, 0, 0);
            panelLayout.childControlWidth = false;
            panelLayout.childControlHeight = false;
            panelLayout.childForceExpandWidth = false;
            panelLayout.childForceExpandHeight = false;

            TMP_Text titleLabel = CreateText(panelObject.transform, "TitleLabel", "Funny Numbers");
            TMP_Text bestScoreLabel = CreateText(panelObject.transform, "BestScoreLabel", "Best Score: 0");

            (Button continueButton, TMP_Text continueLabel, LayoutElement continueLayout) =
                CreateButton(panelObject.transform, "ContinueButton", "Continue Game");
            (Button newGameButton, TMP_Text newGameLabel, LayoutElement newGameLayout) =
                CreateButton(panelObject.transform, "NewGameButton", "New Game");

            var mainTabView = rootObject.GetComponent<MainTabView>();
            mainTabView.Setup(
                rootRect,
                panelRect,
                panelLayout,
                continueLayout,
                newGameLayout,
                continueButton,
                newGameButton,
                titleLabel,
                bestScoreLabel,
                continueLabel,
                newGameLabel,
                regularFont,
                boldFont);

            return mainTabView;
        }

        public void SetBestScore(int bestScore)
        {
            if (_bestScoreLabel != null)
            {
                _bestScoreLabel.text = $"Best Score: {bestScore}";
            }
        }

        public void SetContinueVisible(bool isVisible)
        {
            if (_continueButton != null)
            {
                _continueButton.gameObject.SetActive(isVisible);
            }
        }

        private void OnEnable()
        {
            ApplyResponsiveLayout(force: true);
        }

        private void LateUpdate()
        {
            ApplyResponsiveLayout(force: false);
        }

        private void OnDestroy()
        {
            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveListener(HandleContinueClicked);
            }

            if (_newGameButton != null)
            {
                _newGameButton.onClick.RemoveListener(HandleNewGameClicked);
            }
        }

        private void Setup(
            RectTransform contentRect,
            RectTransform panelRect,
            VerticalLayoutGroup panelLayout,
            LayoutElement continueLayout,
            LayoutElement newGameLayout,
            Button continueButton,
            Button newGameButton,
            TMP_Text titleLabel,
            TMP_Text bestScoreLabel,
            TMP_Text continueLabel,
            TMP_Text newGameLabel,
            TMP_FontAsset regularFont,
            TMP_FontAsset boldFont)
        {
            TMP_FontAsset effectiveRegularFont = regularFont != null
                ? regularFont
                : boldFont != null
                    ? boldFont
                    : TMP_Settings.defaultFontAsset;
            TMP_FontAsset effectiveBoldFont = boldFont != null ? boldFont : effectiveRegularFont;

            _contentRect = contentRect;
            _panelRect = panelRect;
            _panelLayout = panelLayout;
            _continueLayout = continueLayout;
            _newGameLayout = newGameLayout;
            _continueButton = continueButton;
            _newGameButton = newGameButton;
            _titleLabel = titleLabel;
            _bestScoreLabel = bestScoreLabel;
            _continueLabel = continueLabel;
            _newGameLabel = newGameLabel;

            ConfigureLabel(_titleLabel, effectiveBoldFont, 72f, GamePalette.ScoreValueText);
            ConfigureLabel(_bestScoreLabel, effectiveRegularFont, 34f, GamePalette.BoardTileText);
            ConfigureLabel(_continueLabel, effectiveRegularFont, 34f, GamePalette.PrimaryText);
            ConfigureLabel(_newGameLabel, effectiveRegularFont, 34f, GamePalette.PrimaryText);

            _continueButton.onClick.AddListener(HandleContinueClicked);
            _newGameButton.onClick.AddListener(HandleNewGameClicked);

            ApplyResponsiveLayout(force: true);
        }

        private void HandleContinueClicked()
        {
            ContinueClicked?.Invoke();
        }

        private void HandleNewGameClicked()
        {
            NewGameClicked?.Invoke();
        }

        private void ApplyResponsiveLayout(bool force)
        {
            if (_contentRect == null)
            {
                return;
            }

            Rect contentRect = _contentRect.rect;
            Vector2 contentSize = new Vector2(contentRect.width, contentRect.height);
            if (contentSize.x <= 0f || contentSize.y <= 0f)
            {
                return;
            }

            if (!force && Vector2.Distance(_lastContentSize, contentSize) < 0.5f)
            {
                return;
            }

            _lastContentSize = contentSize;

            float scale = MobileLayout.GetScale(contentRect.width, contentRect.height);
            float spacing = MobileLayout.ClampScaled(Spacing, MinSpacing, MaxSpacing, scale);
            float buttonWidth = Mathf.Min(MobileLayout.ClampScaled(ButtonWidth, MinButtonWidth, ButtonWidth, scale), contentRect.width - 64f);
            float buttonHeight = MobileLayout.ClampScaled(ButtonHeight, MinButtonHeight, MaxButtonHeight, scale);

            if (_panelRect != null)
            {
                _panelRect.sizeDelta = new Vector2(
                    Mathf.Min(PanelWidth, Mathf.Max(MinPanelWidth, contentRect.width - 64f)),
                    Mathf.Min(contentRect.height - 64f, 520f * scale));
            }

            if (_panelLayout != null)
            {
                _panelLayout.spacing = spacing;
            }

            if (_continueLayout != null)
            {
                _continueLayout.preferredWidth = buttonWidth;
                _continueLayout.preferredHeight = buttonHeight;
                _continueLayout.minWidth = buttonWidth;
                _continueLayout.minHeight = buttonHeight;
            }

            if (_newGameLayout != null)
            {
                _newGameLayout.preferredWidth = buttonWidth;
                _newGameLayout.preferredHeight = buttonHeight;
                _newGameLayout.minWidth = buttonWidth;
                _newGameLayout.minHeight = buttonHeight;
            }

            if (_titleLabel != null)
            {
                _titleLabel.fontSize = MobileLayout.ClampScaled(72f, 44f, 76f, scale);
            }

            if (_bestScoreLabel != null)
            {
                _bestScoreLabel.fontSize = MobileLayout.ClampScaled(34f, 24f, 36f, scale);
            }

            if (_continueLabel != null)
            {
                _continueLabel.fontSize = MobileLayout.ClampScaled(34f, 26f, 36f, scale);
            }

            if (_newGameLabel != null)
            {
                _newGameLabel.fontSize = MobileLayout.ClampScaled(34f, 26f, 36f, scale);
            }
        }

        private static TMP_Text CreateText(Transform parent, string name, string text)
        {
            var labelObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(TextMeshProUGUI),
                typeof(LayoutElement));

            labelObject.transform.SetParent(parent, false);

            var layout = labelObject.GetComponent<LayoutElement>();
            layout.preferredWidth = 760f;
            layout.preferredHeight = 72f;

            TMP_Text label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            return label;
        }

        private static (Button Button, TMP_Text Label, LayoutElement Layout) CreateButton(Transform parent, string name, string labelText)
        {
            var buttonObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));

            buttonObject.transform.SetParent(parent, false);

            var image = buttonObject.GetComponent<Image>();
            image.color = GamePalette.PrimaryButton;

            var button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = image;

            var layout = buttonObject.GetComponent<LayoutElement>();
            layout.preferredWidth = ButtonWidth;
            layout.preferredHeight = ButtonHeight;
            layout.minWidth = ButtonWidth;
            layout.minHeight = ButtonHeight;

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);

            var labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TMP_Text label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = labelText;
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            label.raycastTarget = false;

            return (button, label, layout);
        }

        private static void ConfigureLabel(TMP_Text label, TMP_FontAsset font, float fontSize, Color color)
        {
            label.font = font;
            label.fontSize = fontSize;
            label.color = color;
        }
    }
}
