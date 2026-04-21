using System;
using System.Collections;
using System.Collections.Generic;
using Game.Gameplay.Board;
using Game.UI.Ad;
using Game.UI.Styling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Game
{
    [DisallowMultipleComponent]
    public sealed class GameScreenView : MonoBehaviour
    {
        private const float BoardTopInset = 236f;
        private const float ControlsToAdSpacing = 5f;
        private const float DefaultAdHeight = 50f;

        private RectTransform _boardAreaRect;
        private RectTransform _gameOverOverlayRect;
        private BoardView _boardView;
        private AdSlotView _adSlotView;
        private Button _plusButton;
        private Button _hintButton;
        private Button _restartButton;
        private Image _plusButtonImage;
        private Image _hintButtonImage;
        private Image _restartButtonImage;
        private TMP_Text _scoreLabel;
        private TMP_Text _additionsLabel;
        private TMP_Text _tooltipLabel;
        private TMP_Text _plusButtonLabel;
        private TMP_Text _hintButtonLabel;
        private GameObject _gameOverOverlay;
        private TMP_Text _gameOverScoreLabel;
        private TMP_Text _gameOverStageLabel;
        private TMP_Text _restartButtonLabel;
        private Coroutine _tooltipRoutine;

        public event Action<int> TileClicked;
        public event Action PlusClicked;
        public event Action HintClicked;
        public event Action RestartClicked;

        public static GameScreenView Create(Transform parent, int columns, TMP_FontAsset regularFont, TMP_FontAsset boldFont)
        {
            var screenObject = new GameObject(
                "GameScreenView",
                typeof(RectTransform),
                typeof(GameScreenView));

            screenObject.transform.SetParent(parent, false);

            var screenRect = (RectTransform)screenObject.transform;
            screenRect.anchorMin = Vector2.zero;
            screenRect.anchorMax = Vector2.one;
            screenRect.offsetMin = Vector2.zero;
            screenRect.offsetMax = Vector2.zero;

            var backgroundObject = new GameObject(
                "GameBackground",
                typeof(RectTransform),
                typeof(Image));

            backgroundObject.transform.SetParent(screenObject.transform, false);

            var backgroundRect = (RectTransform)backgroundObject.transform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            var backgroundImage = backgroundObject.GetComponent<Image>();
            backgroundImage.color = GamePalette.GameBackground;

            var hudPanelObject = new GameObject(
                "HudPanel",
                typeof(RectTransform),
                typeof(Image));

            hudPanelObject.transform.SetParent(screenObject.transform, false);

            var hudPanelRect = (RectTransform)hudPanelObject.transform;
            hudPanelRect.anchorMin = new Vector2(0f, 1f);
            hudPanelRect.anchorMax = new Vector2(1f, 1f);
            hudPanelRect.pivot = new Vector2(0.5f, 1f);
            hudPanelRect.offsetMin = new Vector2(0f, -220f);
            hudPanelRect.offsetMax = new Vector2(0f, -32f);

            var hudPanelImage = hudPanelObject.GetComponent<Image>();
            hudPanelImage.color = GamePalette.HudPanelBackground;

            TMP_Text scoreLabel = CreateTextElement(hudPanelObject.transform, "ScoreLabel");
            ConfigureRect((RectTransform)scoreLabel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 52f), new Vector2(20f, -18f));

            TMP_Text additionsLabel = CreateTextElement(hudPanelObject.transform, "AdditionsLabel");
            ConfigureRect((RectTransform)additionsLabel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 40f), new Vector2(20f, -78f));

            (Button hintButton, Image hintButtonImage, TMP_Text hintButtonLabel) = CreateButton(hudPanelObject.transform, "HintButton", "Hint");
            ConfigureRect((RectTransform)hintButton.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(136f, 52f), new Vector2(-152f, -18f));

            (Button plusButton, Image plusButtonImage, TMP_Text plusButtonLabel) = CreateButton(hudPanelObject.transform, "PlusButton", "+");
            ConfigureRect((RectTransform)plusButton.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(120f, 52f), new Vector2(-20f, -18f));

            TMP_Text tooltipLabel = CreateTextElement(hudPanelObject.transform, "TooltipLabel");
            ConfigureRect((RectTransform)tooltipLabel.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero);
            ((RectTransform)tooltipLabel.transform).offsetMin = new Vector2(20f, 18f);
            ((RectTransform)tooltipLabel.transform).offsetMax = new Vector2(-20f, 66f);

            var boardAreaObject = new GameObject("BoardArea", typeof(RectTransform));
            boardAreaObject.transform.SetParent(screenObject.transform, false);

            var boardAreaRect = (RectTransform)boardAreaObject.transform;
            boardAreaRect.anchorMin = Vector2.zero;
            boardAreaRect.anchorMax = Vector2.one;
            boardAreaRect.offsetMin = new Vector2(0f, DefaultAdHeight + ControlsToAdSpacing);
            boardAreaRect.offsetMax = new Vector2(0f, -BoardTopInset);

            BoardView boardView = BoardView.Create(boardAreaObject.transform, columns, regularFont, boldFont);

            AdSlotView adSlotView = AdSlotView.Create(screenObject.transform, regularFont);

            var overlayObject = new GameObject(
                "GameOverOverlay",
                typeof(RectTransform),
                typeof(Image));

            overlayObject.transform.SetParent(screenObject.transform, false);

            var overlayRect = (RectTransform)overlayObject.transform;
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = new Vector2(0f, DefaultAdHeight);
            overlayRect.offsetMax = Vector2.zero;

            var overlayImage = overlayObject.GetComponent<Image>();
            overlayImage.color = GamePalette.GameOverOverlay;

            var panelObject = new GameObject(
                "GameOverPanel",
                typeof(RectTransform),
                typeof(Image));

            panelObject.transform.SetParent(overlayObject.transform, false);

            var panelRect = (RectTransform)panelObject.transform;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(700f, 480f);
            panelRect.anchoredPosition = Vector2.zero;

            var panelImage = panelObject.GetComponent<Image>();
            panelImage.color = GamePalette.HudPanelBackground;

            TMP_Text titleLabel = CreateTextElement(panelObject.transform, "TitleLabel");
            ConfigureRect((RectTransform)titleLabel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(520f, 72f), new Vector2(0f, -60f));
            titleLabel.text = "Game Over";

            TMP_Text gameOverScoreLabel = CreateTextElement(panelObject.transform, "ScoreLabel");
            ConfigureRect((RectTransform)gameOverScoreLabel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(520f, 56f), new Vector2(0f, -160f));

            TMP_Text gameOverStageLabel = CreateTextElement(panelObject.transform, "StageLabel");
            ConfigureRect((RectTransform)gameOverStageLabel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(520f, 56f), new Vector2(0f, -230f));

            (Button restartButton, Image restartButtonImage, TMP_Text restartButtonLabel) = CreateButton(panelObject.transform, "RestartButton", "Restart");
            ConfigureRect((RectTransform)restartButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(260f, 64f), new Vector2(0f, 54f));

            var screenView = screenObject.GetComponent<GameScreenView>();
            screenView.Setup(
                boardAreaRect,
                overlayRect,
                boardView,
                adSlotView,
                scoreLabel,
                additionsLabel,
                tooltipLabel,
                plusButton,
                plusButtonImage,
                plusButtonLabel,
                hintButton,
                hintButtonImage,
                hintButtonLabel,
                overlayObject,
                gameOverScoreLabel,
                gameOverStageLabel,
                restartButton,
                restartButtonImage,
                restartButtonLabel,
                titleLabel,
                regularFont,
                boldFont);

            return screenView;
        }

        public void SetCells(IReadOnlyList<BoardCell> cells)
        {
            _boardView?.SetCells(cells);
        }

        public void RefreshCell(BoardCell cell)
        {
            _boardView?.RefreshCell(cell);
        }

        public void SetScore(int totalScore)
        {
            if (_scoreLabel != null)
            {
                _scoreLabel.text = $"Score: {totalScore}";
            }
        }

        public void SetAdditions(int remainingAdditions)
        {
            if (_additionsLabel != null)
            {
                _additionsLabel.text = $"Additions: {remainingAdditions}";
            }
        }

        public void SetPlusButtonInteractable(bool interactable)
        {
            if (_plusButton == null || _plusButtonImage == null || _plusButtonLabel == null)
            {
                return;
            }

            _plusButton.interactable = interactable;
            _plusButtonImage.color = interactable ? GamePalette.PrimaryButton : GamePalette.DisabledButton;
            _plusButtonLabel.color = interactable ? GamePalette.PrimaryText : GamePalette.DisabledText;
        }

        public void SetHintButtonLocked(bool isLocked)
        {
            if (_hintButtonImage == null || _hintButtonLabel == null)
            {
                return;
            }

            _hintButtonImage.color = isLocked ? GamePalette.LockedButton : GamePalette.PrimaryButton;
            _hintButtonLabel.color = isLocked ? GamePalette.LockedText : GamePalette.PrimaryText;
        }

        public void ShowHint(BoardMatchInfo matchInfo)
        {
            _boardView?.ShowHint(matchInfo);
        }

        public void ClearHint()
        {
            _boardView?.ClearHint();
        }

        public void ReplayHintFeedback()
        {
            _boardView?.ReplayHintFeedback();
        }

        public void ShowTooltip(string message, float duration = 2.5f)
        {
            if (_tooltipLabel == null)
            {
                return;
            }

            if (_tooltipRoutine != null)
            {
                StopCoroutine(_tooltipRoutine);
            }

            _tooltipLabel.text = message;
            _tooltipLabel.enabled = true;
            _tooltipRoutine = StartCoroutine(HideTooltipAfterDelay(duration));
        }

        public void ScrollToTop()
        {
            _boardView?.ScrollToTop();
        }

        public void ScrollToBottom()
        {
            _boardView?.ScrollToBottom();
        }

        public void ShowDeveloperPairLines(IReadOnlyList<BoardMatchInfo> pairs)
        {
            _boardView?.ShowDeveloperPairLines(pairs);
        }

        public void ClearDeveloperPairLines()
        {
            _boardView?.ClearDeveloperPairLines();
        }

        public void ShowGameOver(int finalScore, int finalStage)
        {
            if (_gameOverOverlay == null || _gameOverScoreLabel == null || _gameOverStageLabel == null)
            {
                return;
            }

            if (_tooltipRoutine != null)
            {
                StopCoroutine(_tooltipRoutine);
                _tooltipRoutine = null;
            }

            if (_tooltipLabel != null)
            {
                _tooltipLabel.enabled = false;
                _tooltipLabel.text = string.Empty;
            }

            _gameOverScoreLabel.text = $"Final Score: {finalScore}";
            _gameOverStageLabel.text = $"Final Stage: {finalStage}";
            _gameOverOverlay.SetActive(true);
            _gameOverOverlay.transform.SetAsLastSibling();
        }

        public void HideGameOver()
        {
            if (_gameOverOverlay != null)
            {
                _gameOverOverlay.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (_boardView != null)
            {
                _boardView.TileClicked -= HandleBoardTileClicked;
            }

            if (_adSlotView != null)
            {
                _adSlotView.HeightChanged -= HandleAdSlotHeightChanged;
            }

            if (_plusButton != null)
            {
                _plusButton.onClick.RemoveListener(HandlePlusClicked);
            }

            if (_hintButton != null)
            {
                _hintButton.onClick.RemoveListener(HandleHintClicked);
            }

            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveListener(HandleRestartClicked);
            }
        }

        private void Setup(
            RectTransform boardAreaRect,
            RectTransform gameOverOverlayRect,
            BoardView boardView,
            AdSlotView adSlotView,
            TMP_Text scoreLabel,
            TMP_Text additionsLabel,
            TMP_Text tooltipLabel,
            Button plusButton,
            Image plusButtonImage,
            TMP_Text plusButtonLabel,
            Button hintButton,
            Image hintButtonImage,
            TMP_Text hintButtonLabel,
            GameObject gameOverOverlay,
            TMP_Text gameOverScoreLabel,
            TMP_Text gameOverStageLabel,
            Button restartButton,
            Image restartButtonImage,
            TMP_Text restartButtonLabel,
            TMP_Text gameOverTitleLabel,
            TMP_FontAsset regularFont,
            TMP_FontAsset boldFont)
        {
            TMP_FontAsset effectiveRegularFont = regularFont != null
                ? regularFont
                : boldFont != null
                    ? boldFont
                    : TMP_Settings.defaultFontAsset;

            _boardAreaRect = boardAreaRect;
            _gameOverOverlayRect = gameOverOverlayRect;
            _boardView = boardView;
            _adSlotView = adSlotView;
            _scoreLabel = scoreLabel;
            _additionsLabel = additionsLabel;
            _tooltipLabel = tooltipLabel;
            _plusButton = plusButton;
            _plusButtonImage = plusButtonImage;
            _plusButtonLabel = plusButtonLabel;
            _hintButton = hintButton;
            _hintButtonImage = hintButtonImage;
            _hintButtonLabel = hintButtonLabel;
            _gameOverOverlay = gameOverOverlay;
            _gameOverScoreLabel = gameOverScoreLabel;
            _gameOverStageLabel = gameOverStageLabel;
            _restartButton = restartButton;
            _restartButtonImage = restartButtonImage;
            _restartButtonLabel = restartButtonLabel;

            ConfigureLabel(_scoreLabel, effectiveRegularFont, 42, TextAnchor.MiddleLeft, GamePalette.PrimaryText);
            ConfigureLabel(_additionsLabel, effectiveRegularFont, 30, TextAnchor.MiddleLeft, GamePalette.SecondaryText);
            ConfigureLabel(_tooltipLabel, effectiveRegularFont, 28, TextAnchor.MiddleCenter, GamePalette.TooltipText);
            _tooltipLabel.enabled = false;
            _tooltipLabel.text = string.Empty;

            ConfigureLabel(_plusButtonLabel, effectiveRegularFont, 34, TextAnchor.MiddleCenter, GamePalette.PrimaryText);
            ConfigureLabel(_hintButtonLabel, effectiveRegularFont, 28, TextAnchor.MiddleCenter, GamePalette.PrimaryText);
            ConfigureLabel(gameOverTitleLabel, effectiveRegularFont, 48, TextAnchor.MiddleCenter, GamePalette.PrimaryText);
            ConfigureLabel(_gameOverScoreLabel, effectiveRegularFont, 34, TextAnchor.MiddleCenter, GamePalette.PrimaryText);
            ConfigureLabel(_gameOverStageLabel, effectiveRegularFont, 34, TextAnchor.MiddleCenter, GamePalette.SecondaryText);
            ConfigureLabel(_restartButtonLabel, effectiveRegularFont, 34, TextAnchor.MiddleCenter, GamePalette.PrimaryText);

            _boardView.TileClicked += HandleBoardTileClicked;
            _adSlotView.HeightChanged += HandleAdSlotHeightChanged;
            _plusButton.onClick.AddListener(HandlePlusClicked);
            _hintButton.onClick.AddListener(HandleHintClicked);
            _restartButton.onClick.AddListener(HandleRestartClicked);

            SetScore(0);
            SetAdditions(0);
            SetPlusButtonInteractable(true);
            SetHintButtonLocked(false);
            HideGameOver();
            ApplyAdInsets(_adSlotView != null ? _adSlotView.CurrentHeight : DefaultAdHeight);
        }

        private void HandleBoardTileClicked(int index)
        {
            TileClicked?.Invoke(index);
        }

        private void HandlePlusClicked()
        {
            PlusClicked?.Invoke();
        }

        private void HandleHintClicked()
        {
            HintClicked?.Invoke();
        }

        private void HandleRestartClicked()
        {
            RestartClicked?.Invoke();
        }

        private void HandleAdSlotHeightChanged(float adHeight)
        {
            ApplyAdInsets(adHeight);
        }

        private void ApplyAdInsets(float adHeight)
        {
            if (_boardAreaRect != null)
            {
                _boardAreaRect.offsetMin = new Vector2(0f, adHeight + ControlsToAdSpacing);
                _boardAreaRect.offsetMax = new Vector2(0f, -BoardTopInset);
            }

            if (_gameOverOverlayRect != null)
            {
                _gameOverOverlayRect.offsetMin = new Vector2(0f, adHeight);
                _gameOverOverlayRect.offsetMax = Vector2.zero;
            }
        }

        private IEnumerator HideTooltipAfterDelay(float duration)
        {
            yield return new WaitForSeconds(duration);

            _tooltipLabel.enabled = false;
            _tooltipLabel.text = string.Empty;
            _tooltipRoutine = null;
        }

        private static TextMeshProUGUI CreateTextElement(Transform parent, string name)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            return textObject.GetComponent<TextMeshProUGUI>();
        }

        private static (Button Button, Image Image, TMP_Text Label) CreateButton(Transform parent, string name, string labelText)
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

            TMP_Text label = CreateTextElement(buttonObject.transform, "Label");
            RectTransform labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            label.text = labelText;
            label.raycastTarget = false;

            return (button, buttonImage, label);
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

        private static void ConfigureLabel(TMP_Text label, TMP_FontAsset font, int fontSize, TextAnchor alignment, Color color)
        {
            label.font = font;
            label.fontSize = fontSize;
            label.alignment = ConvertAlignment(alignment);
            label.color = color;
            label.enableAutoSizing = false;
            label.enableWordWrapping = true;
            label.overflowMode = TextOverflowModes.Overflow;
        }

        private static TextAlignmentOptions ConvertAlignment(TextAnchor alignment)
        {
            return alignment switch
            {
                TextAnchor.UpperLeft => TextAlignmentOptions.TopLeft,
                TextAnchor.UpperCenter => TextAlignmentOptions.Top,
                TextAnchor.UpperRight => TextAlignmentOptions.TopRight,
                TextAnchor.MiddleLeft => TextAlignmentOptions.Left,
                TextAnchor.MiddleCenter => TextAlignmentOptions.Center,
                TextAnchor.MiddleRight => TextAlignmentOptions.Right,
                TextAnchor.LowerLeft => TextAlignmentOptions.BottomLeft,
                TextAnchor.LowerCenter => TextAlignmentOptions.Bottom,
                TextAnchor.LowerRight => TextAlignmentOptions.BottomRight,
                _ => TextAlignmentOptions.Center,
            };
        }
    }
}
