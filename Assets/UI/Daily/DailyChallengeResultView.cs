using System;
using System.Globalization;
using Game.App.Daily;
using Game.UI.Layout;
using Game.UI.Styling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Daily
{
    [DisallowMultipleComponent]
    public sealed class DailyChallengeResultView : MonoBehaviour
    {
        private const float PanelWidth = 760f;
        private const float MinPanelWidth = 320f;
        private const float PanelHeight = 720f;
        private const float MinPanelHeight = 420f;

        private RectTransform _safeAreaRect;
        private RectTransform _panelRect;
        private RectTransform _progressTrackRect;
        private RectTransform _progressFillRect;
        private TMP_Text _titleLabel;
        private TMP_Text _dateLabel;
        private TMP_Text _progressValueLabel;
        private TMP_Text _messageLabel;
        private TMP_Text _continueButtonLabel;
        private Button _continueButton;
        private Vector2 _lastSafeAreaSize = new Vector2(-1f, -1f);

        public event Action ContinueClicked;

        public static DailyChallengeResultView Create(
            Transform parent,
            TMP_FontAsset regularFont,
            TMP_FontAsset boldFont,
            bool showSafeAreaDebugOverlay)
        {
            var rootObject = new GameObject(
                "DailyChallengeResultView",
                typeof(RectTransform),
                typeof(DailyChallengeResultView));

            rootObject.transform.SetParent(parent, false);

            var rootRect = (RectTransform)rootObject.transform;
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var backgroundObject = new GameObject("BackgroundFullBleed", typeof(RectTransform), typeof(Image));
            backgroundObject.transform.SetParent(rootObject.transform, false);

            var backgroundRect = (RectTransform)backgroundObject.transform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            var backgroundImage = backgroundObject.GetComponent<Image>();
            backgroundImage.color = GamePalette.GameBackground;

            if (showSafeAreaDebugOverlay)
            {
                SafeAreaDebugOverlayView.Create(
                    rootObject.transform,
                    "SafeAreaDebugOverlay",
                    GamePalette.SafeAreaDebugFill);
            }

            SafeAreaView safeAreaView = SafeAreaView.Create(rootObject.transform, "SafeAreaContent");
            RectTransform safeAreaRect = (RectTransform)safeAreaView.transform;

            var panelObject = new GameObject("CenterPanel", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(safeAreaRect, false);

            var panelRect = (RectTransform)panelObject.transform;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(PanelWidth, PanelHeight);
            panelRect.anchoredPosition = Vector2.zero;

            var panelImage = panelObject.GetComponent<Image>();
            panelImage.color = GamePalette.BoardTileNormalBackground;

            TMP_Text titleLabel = CreateText(panelObject.transform, "TitleLabel", "Daily Challenge");
            ConfigureRect((RectTransform)titleLabel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(560f, 64f), new Vector2(0f, -72f));

            TMP_Text dateLabel = CreateText(panelObject.transform, "DateLabel", string.Empty);
            ConfigureRect((RectTransform)dateLabel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(560f, 44f), new Vector2(0f, -138f));

            var progressTrackObject = new GameObject("ProgressTrack", typeof(RectTransform), typeof(Image));
            progressTrackObject.transform.SetParent(panelObject.transform, false);
            var progressTrackRect = (RectTransform)progressTrackObject.transform;
            ConfigureRect(progressTrackRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(520f, 28f), new Vector2(0f, 34f));

            var progressTrackImage = progressTrackObject.GetComponent<Image>();
            progressTrackImage.color = GamePalette.DailyProgressBarTrack;

            var progressFillObject = new GameObject("ProgressFill", typeof(RectTransform), typeof(Image));
            progressFillObject.transform.SetParent(progressTrackObject.transform, false);
            var progressFillRect = (RectTransform)progressFillObject.transform;
            progressFillRect.anchorMin = new Vector2(0f, 0f);
            progressFillRect.anchorMax = new Vector2(0f, 1f);
            progressFillRect.offsetMin = Vector2.zero;
            progressFillRect.offsetMax = Vector2.zero;

            var progressFillImage = progressFillObject.GetComponent<Image>();
            progressFillImage.color = GamePalette.DailyProgressBarFill;

            TMP_Text progressValueLabel = CreateText(panelObject.transform, "ProgressValueLabel", string.Empty);
            ConfigureRect((RectTransform)progressValueLabel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560f, 56f), new Vector2(0f, -24f));

            TMP_Text messageLabel = CreateText(panelObject.transform, "MessageLabel", string.Empty);
            ConfigureRect((RectTransform)messageLabel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(600f, 140f), new Vector2(0f, -132f));

            (Button continueButton, TMP_Text continueButtonLabel) =
                CreateButton(panelObject.transform, "ContinueButton", "Continue");
            ConfigureRect((RectTransform)continueButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(360f, 88f), new Vector2(0f, 44f));

            var view = rootObject.GetComponent<DailyChallengeResultView>();
            view.Setup(
                safeAreaRect,
                panelRect,
                progressTrackRect,
                progressFillRect,
                titleLabel,
                dateLabel,
                progressValueLabel,
                messageLabel,
                continueButton,
                continueButtonLabel,
                regularFont,
                boldFont);

            return view;
        }

        public void SetResult(DailyChallengeDateKey date, int progressScore, int goalScore, bool isCompleted)
        {
            int safeGoal = Mathf.Max(1, goalScore);
            int safeProgress = Mathf.Max(0, progressScore);
            float progress01 = Mathf.Clamp01((float)safeProgress / safeGoal);

            if (_dateLabel != null)
            {
                _dateLabel.text = date.ToDateTime().ToString("d MMMM yyyy", CultureInfo.CurrentCulture);
            }

            if (_progressValueLabel != null)
            {
                int clampedProgress = Mathf.Min(safeGoal, safeProgress);
                _progressValueLabel.text = $"{clampedProgress} / {safeGoal}";
            }

            if (_messageLabel != null)
            {
                _messageLabel.text = isCompleted
                    ? $"You have completed the daily challenge for {date.ToDateTime():d MMMM yyyy}!"
                    : "Keep going to finish this daily challenge.";
            }

            if (_progressFillRect != null)
            {
                _progressFillRect.anchorMax = new Vector2(progress01, 1f);
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
        }

        private void Setup(
            RectTransform safeAreaRect,
            RectTransform panelRect,
            RectTransform progressTrackRect,
            RectTransform progressFillRect,
            TMP_Text titleLabel,
            TMP_Text dateLabel,
            TMP_Text progressValueLabel,
            TMP_Text messageLabel,
            Button continueButton,
            TMP_Text continueButtonLabel,
            TMP_FontAsset regularFont,
            TMP_FontAsset boldFont)
        {
            TMP_FontAsset effectiveRegularFont = regularFont != null
                ? regularFont
                : boldFont != null
                    ? boldFont
                    : TMP_Settings.defaultFontAsset;
            TMP_FontAsset effectiveBoldFont = boldFont != null ? boldFont : effectiveRegularFont;

            _safeAreaRect = safeAreaRect;
            _panelRect = panelRect;
            _progressTrackRect = progressTrackRect;
            _progressFillRect = progressFillRect;
            _titleLabel = titleLabel;
            _dateLabel = dateLabel;
            _progressValueLabel = progressValueLabel;
            _messageLabel = messageLabel;
            _continueButton = continueButton;
            _continueButtonLabel = continueButtonLabel;

            ConfigureLabel(_titleLabel, effectiveBoldFont, 52f, GamePalette.ScoreValueText, TextAlignmentOptions.Center);
            ConfigureLabel(_dateLabel, effectiveRegularFont, 28f, GamePalette.DailyHeaderText, TextAlignmentOptions.Center);
            ConfigureLabel(_progressValueLabel, effectiveBoldFont, 36f, GamePalette.ScoreValueText, TextAlignmentOptions.Center);
            ConfigureLabel(_messageLabel, effectiveRegularFont, 28f, GamePalette.DailyHeaderText, TextAlignmentOptions.Center);
            _messageLabel.textWrappingMode = TextWrappingModes.Normal;
            ConfigureLabel(_continueButtonLabel, effectiveBoldFont, 32f, GamePalette.PrimaryText, TextAlignmentOptions.Center);

            _continueButton.onClick.AddListener(HandleContinueClicked);
            ApplyResponsiveLayout(force: true);
        }

        private void HandleContinueClicked()
        {
            ContinueClicked?.Invoke();
        }

        private void ApplyResponsiveLayout(bool force)
        {
            if (_safeAreaRect == null)
            {
                return;
            }

            Rect safeRect = _safeAreaRect.rect;
            Vector2 safeSize = new Vector2(safeRect.width, safeRect.height);
            if (safeSize.x <= 0f || safeSize.y <= 0f)
            {
                return;
            }

            if (!force && Vector2.Distance(_lastSafeAreaSize, safeSize) < 0.5f)
            {
                return;
            }

            _lastSafeAreaSize = safeSize;

            float scale = MobileLayout.GetScale(safeRect.width, safeRect.height);
            if (_panelRect != null)
            {
                _panelRect.sizeDelta = new Vector2(
                    Mathf.Max(MinPanelWidth, Mathf.Min(safeRect.width - 48f, PanelWidth)),
                    Mathf.Max(MinPanelHeight, Mathf.Min(safeRect.height - 48f, PanelHeight)));
            }

            if (_progressTrackRect != null)
            {
                _progressTrackRect.sizeDelta = new Vector2(Mathf.Max(240f, (_panelRect != null ? _panelRect.rect.width : PanelWidth) - 160f), MobileLayout.ClampScaled(28f, 22f, 32f, scale));
            }

            if (_titleLabel != null)
            {
                _titleLabel.fontSize = MobileLayout.ClampScaled(52f, 38f, 56f, scale);
            }

            if (_dateLabel != null)
            {
                _dateLabel.fontSize = MobileLayout.ClampScaled(28f, 20f, 32f, scale);
            }

            if (_progressValueLabel != null)
            {
                _progressValueLabel.fontSize = MobileLayout.ClampScaled(36f, 24f, 40f, scale);
            }

            if (_messageLabel != null)
            {
                _messageLabel.fontSize = MobileLayout.ClampScaled(28f, 20f, 32f, scale);
            }

            if (_continueButtonLabel != null)
            {
                _continueButtonLabel.fontSize = MobileLayout.ClampScaled(32f, 24f, 36f, scale);
            }
        }

        private static (Button Button, TMP_Text Label) CreateButton(Transform parent, string name, string labelText)
        {
            var buttonObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button));

            buttonObject.transform.SetParent(parent, false);

            var buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = GamePalette.PrimaryButton;

            var button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = buttonImage;

            TMP_Text label = CreateText(buttonObject.transform, "Label", labelText);
            RectTransform labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            return (button, label);
        }

        private static TMP_Text CreateText(Transform parent, string name, string text)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            TMP_Text label = textObject.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.raycastTarget = false;
            return label;
        }

        private static void ConfigureRect(
            RectTransform rectTransform,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 sizeDelta,
            Vector2 anchoredPosition)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.sizeDelta = sizeDelta;
            rectTransform.anchoredPosition = anchoredPosition;
        }

        private static void ConfigureLabel(TMP_Text label, TMP_FontAsset font, float fontSize, Color color, TextAlignmentOptions alignment)
        {
            label.font = font;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;
            label.enableAutoSizing = false;
        }
    }
}
