using System;
using System.Collections;
using System.Collections.Generic;
using Game.Gameplay.Board;
using Game.UI.Ad;
using Game.UI.Layout;
using Game.UI.Styling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Game
{
    [DisallowMultipleComponent]
    public sealed class GameScreenView : MonoBehaviour
    {
        private const float TopAreaHeight = 56f;
        private const float MinTopAreaHeight = 40f;
        private const float MaxTopAreaHeight = 72f;
        private const float ScoreAreaHeight = 148f;
        private const float MinScoreAreaHeight = 112f;
        private const float MaxScoreAreaHeight = 176f;
        private const float BottomControlsHeight = 148f;
        private const float MinBottomControlsHeight = 112f;
        private const float MaxBottomControlsHeight = 168f;
        private const float ControlsToAdSpacing = 5f;
        private const float MaxControlsToAdSpacing = 10f;
        private const float DefaultAdHeight = 50f;
        private const int DefaultHintBadgeValue = 9;
        private const float BoardSidePadding = 16f;
        private const float MinBoardSidePadding = 12f;
        private const float MaxBoardSidePadding = 28f;
        private const float MaxBoardWidth = 980f;
        private const float BoardHeightRatio = 1.16f;
        private const float MinBoardHeight = 360f;
        private const float ControlsRowSpacing = 44f;
        private const float MinControlsRowSpacing = 24f;
        private const float MaxControlsRowSpacing = 56f;
        private const float ButtonSize = 118f;
        private const float MinButtonSize = 92f;
        private const float MaxButtonSize = 132f;
        private const float IconSize = 56f;
        private const float MinIconSize = 40f;
        private const float MaxIconSize = 62f;
        private const float BadgeSize = 38f;
        private const float MinBadgeSize = 30f;
        private const float MaxBadgeSize = 42f;
        private const float ScoreFontSize = 62f;
        private const float MinScoreFontSize = 44f;
        private const float MaxScoreFontSize = 72f;
        private const float TooltipFontSize = 24f;
        private const float MinTooltipFontSize = 18f;
        private const float MaxTooltipFontSize = 28f;
        private const float RestartPanelWidth = 700f;
        private const float RestartPanelHeight = 480f;

        private static Sprite _circleSprite;

        private RectTransform _safeAreaRect;
        private RectTransform _topAreaRect;
        private RectTransform _scoreAreaRect;
        private RectTransform _scoreValueRect;
        private RectTransform _tooltipRect;
        private RectTransform _boardAreaRect;
        private RectTransform _boardContainerRect;
        private RectTransform _bottomControlsRect;
        private RectTransform _controlsRowRect;
        private RectTransform _plusIconRect;
        private RectTransform _hintIconRect;
        private RectTransform _plusBadgeRect;
        private RectTransform _hintBadgeRect;
        private RectTransform _gameOverPanelRect;
        private BoardView _boardView;
        private AdSlotView _adSlotView;
        private GameObject _overlayRoot;
        private Button _backButton;
        private Button _plusButton;
        private Button _hintButton;
        private Button _restartButton;
        private Image _plusButtonBackgroundImage;
        private Image _hintButtonBackgroundImage;
        private Image _plusIconImage;
        private Image _hintIconImage;
        private TMP_Text _backButtonLabel;
        private TMP_Text _bestScoreLabel;
        private TMP_Text _scoreValueLabel;
        private TMP_Text _tooltipLabel;
        private TMP_Text _plusBadgeLabel;
        private TMP_Text _hintBadgeLabel;
        private HorizontalLayoutGroup _controlsLayout;
        private LayoutElement _plusButtonLayoutElement;
        private LayoutElement _hintButtonLayoutElement;
        private GameObject _gameOverOverlay;
        private TMP_Text _gameOverScoreLabel;
        private TMP_Text _gameOverStageLabel;
        private TMP_Text _gameOverTitleLabel;
        private TMP_Text _restartButtonLabel;
        private Coroutine _tooltipRoutine;
        private Vector2 _lastSafeAreaSize = new Vector2(-1f, -1f);
        private float _lastAdHeight = -1f;

        public event Action BackClicked;
        public event Action<int> TileClicked;
        public event Action PlusClicked;
        public event Action HintClicked;
        public event Action RestartClicked;

        public static GameScreenView Create(
            Transform parent,
            int columns,
            TMP_FontAsset regularFont,
            TMP_FontAsset boldFont,
            bool showSafeAreaDebugOverlay,
            Texture2D plusIconTexture,
            Texture2D hintIconTexture)
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

            if (showSafeAreaDebugOverlay)
            {
                SafeAreaDebugOverlayView.Create(
                    screenObject.transform,
                    "SafeAreaDebugOverlay",
                    GamePalette.SafeAreaDebugFill);
            }

            SafeAreaView safeAreaView = SafeAreaView.Create(screenObject.transform, "SafeAreaContent");
            RectTransform safeAreaRect = (RectTransform)safeAreaView.transform;

            var topAreaObject = new GameObject("TopArea", typeof(RectTransform));
            topAreaObject.transform.SetParent(safeAreaRect, false);

            var topAreaRect = (RectTransform)topAreaObject.transform;
            topAreaRect.anchorMin = new Vector2(0f, 1f);
            topAreaRect.anchorMax = new Vector2(1f, 1f);
            topAreaRect.pivot = new Vector2(0.5f, 1f);
            topAreaRect.sizeDelta = new Vector2(0f, TopAreaHeight);
            topAreaRect.anchoredPosition = Vector2.zero;

            (Button backButton, TMP_Text backButtonLabel) =
                CreateTopTextButton(topAreaObject.transform, "BackButton", "Back");
            ConfigureRect(
                (RectTransform)backButton.transform,
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f),
                new Vector2(140f, 44f),
                new Vector2(16f, 0f));

            TMP_Text bestScoreLabel = CreateTextElement(topAreaObject.transform, "BestScoreLabel");
            ConfigureRect(
                (RectTransform)bestScoreLabel.transform,
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(220f, 44f),
                new Vector2(-16f, 0f));

            var scoreAreaObject = new GameObject("ScoreArea", typeof(RectTransform));
            scoreAreaObject.transform.SetParent(safeAreaRect, false);

            var scoreAreaRect = (RectTransform)scoreAreaObject.transform;
            scoreAreaRect.anchorMin = new Vector2(0f, 1f);
            scoreAreaRect.anchorMax = new Vector2(1f, 1f);
            scoreAreaRect.pivot = new Vector2(0.5f, 1f);
            scoreAreaRect.sizeDelta = new Vector2(0f, ScoreAreaHeight);
            scoreAreaRect.anchoredPosition = new Vector2(0f, -TopAreaHeight);

            TMP_Text scoreValueLabel = CreateTextElement(scoreAreaObject.transform, "ScoreValue");
            ConfigureRect(
                (RectTransform)scoreValueLabel.transform,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(260f, 84f),
                new Vector2(0f, 10f));

            TMP_Text tooltipLabel = CreateTextElement(scoreAreaObject.transform, "TooltipLabel");
            ConfigureRect(
                (RectTransform)tooltipLabel.transform,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(760f, 48f),
                new Vector2(0f, 8f));

            var boardAreaObject = new GameObject("BoardArea", typeof(RectTransform));
            boardAreaObject.transform.SetParent(safeAreaRect, false);

            var boardAreaRect = (RectTransform)boardAreaObject.transform;
            boardAreaRect.anchorMin = Vector2.zero;
            boardAreaRect.anchorMax = Vector2.one;
            boardAreaRect.offsetMin = new Vector2(0f, DefaultAdHeight + ControlsToAdSpacing + BottomControlsHeight);
            boardAreaRect.offsetMax = new Vector2(0f, -(TopAreaHeight + ScoreAreaHeight));

            var boardContainerObject = new GameObject("BoardContainer", typeof(RectTransform));
            boardContainerObject.transform.SetParent(boardAreaObject.transform, false);

            var boardContainerRect = (RectTransform)boardContainerObject.transform;
            boardContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
            boardContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
            boardContainerRect.pivot = new Vector2(0.5f, 0.5f);
            boardContainerRect.sizeDelta = new Vector2(MaxBoardWidth, MaxBoardWidth * BoardHeightRatio);
            boardContainerRect.anchoredPosition = Vector2.zero;

            BoardView boardView = BoardView.Create(boardContainerObject.transform, columns, regularFont, boldFont);

            var bottomControlsObject = new GameObject("BottomControls", typeof(RectTransform));
            bottomControlsObject.transform.SetParent(safeAreaRect, false);

            var bottomControlsRect = (RectTransform)bottomControlsObject.transform;
            bottomControlsRect.anchorMin = new Vector2(0f, 0f);
            bottomControlsRect.anchorMax = new Vector2(1f, 0f);
            bottomControlsRect.pivot = new Vector2(0.5f, 0f);
            bottomControlsRect.sizeDelta = new Vector2(0f, BottomControlsHeight);
            bottomControlsRect.anchoredPosition = new Vector2(0f, DefaultAdHeight + ControlsToAdSpacing);

            var controlsRowObject = new GameObject(
                "ControlsRow",
                typeof(RectTransform),
                typeof(HorizontalLayoutGroup));

            controlsRowObject.transform.SetParent(bottomControlsObject.transform, false);

            var controlsRowRect = (RectTransform)controlsRowObject.transform;
            controlsRowRect.anchorMin = new Vector2(0.5f, 0.5f);
            controlsRowRect.anchorMax = new Vector2(0.5f, 0.5f);
            controlsRowRect.pivot = new Vector2(0.5f, 0.5f);
            controlsRowRect.sizeDelta = new Vector2(360f, 132f);
            controlsRowRect.anchoredPosition = new Vector2(0f, 6f);

            var controlsLayout = controlsRowObject.GetComponent<HorizontalLayoutGroup>();
            controlsLayout.childAlignment = TextAnchor.MiddleCenter;
            controlsLayout.spacing = ControlsRowSpacing;
            controlsLayout.padding = new RectOffset(0, 0, 0, 0);
            controlsLayout.childForceExpandHeight = false;
            controlsLayout.childForceExpandWidth = false;
            controlsLayout.childControlHeight = false;
            controlsLayout.childControlWidth = false;

            Sprite plusIconSprite = CreateTextureSprite(plusIconTexture);
            Sprite hintIconSprite = CreateTextureSprite(hintIconTexture);

            (Button plusButton, Image plusButtonBackgroundImage, Image plusIconImage, TMP_Text plusBadgeLabel, LayoutElement plusButtonLayoutElement, RectTransform plusIconRect, RectTransform plusBadgeRect) =
                CreateActionButton(controlsRowObject.transform, "PlusButton", plusIconSprite, "+", regularFont);

            (Button hintButton, Image hintButtonBackgroundImage, Image hintIconImage, TMP_Text hintBadgeLabel, LayoutElement hintButtonLayoutElement, RectTransform hintIconRect, RectTransform hintBadgeRect) =
                CreateActionButton(controlsRowObject.transform, "HintButton", hintIconSprite, "?", regularFont);

            AdSlotView adSlotView = AdSlotView.Create(safeAreaRect, regularFont);

            var overlayRootObject = new GameObject("OverlayRoot", typeof(RectTransform));
            overlayRootObject.transform.SetParent(screenObject.transform, false);

            var overlayRootRect = (RectTransform)overlayRootObject.transform;
            overlayRootRect.anchorMin = Vector2.zero;
            overlayRootRect.anchorMax = Vector2.one;
            overlayRootRect.offsetMin = Vector2.zero;
            overlayRootRect.offsetMax = Vector2.zero;

            var overlayObject = new GameObject(
                "GameOverOverlay",
                typeof(RectTransform),
                typeof(Image));

            overlayObject.transform.SetParent(overlayRootObject.transform, false);

            var overlayRect = (RectTransform)overlayObject.transform;
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
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

            (Button restartButton, TMP_Text restartButtonLabel) =
                CreateTextButton(panelObject.transform, "RestartButton", "Restart");

            ConfigureRect((RectTransform)restartButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(260f, 64f), new Vector2(0f, 54f));

            var screenView = screenObject.GetComponent<GameScreenView>();
            screenView.Setup(
                safeAreaRect,
                topAreaRect,
                scoreAreaRect,
                (RectTransform)scoreValueLabel.transform,
                (RectTransform)tooltipLabel.transform,
                boardAreaRect,
                boardContainerRect,
                bottomControlsRect,
                controlsRowRect,
                controlsLayout,
                boardView,
                adSlotView,
                overlayRootObject,
                backButton,
                backButtonLabel,
                bestScoreLabel,
                scoreValueLabel,
                tooltipLabel,
                plusButton,
                plusButtonBackgroundImage,
                plusIconImage,
                plusBadgeLabel,
                plusButtonLayoutElement,
                plusIconRect,
                plusBadgeRect,
                hintButton,
                hintButtonBackgroundImage,
                hintIconImage,
                hintBadgeLabel,
                hintButtonLayoutElement,
                hintIconRect,
                hintBadgeRect,
                overlayObject,
                panelRect,
                gameOverScoreLabel,
                gameOverStageLabel,
                restartButton,
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
            if (_scoreValueLabel != null)
            {
                _scoreValueLabel.text = totalScore.ToString();
            }
        }

        public void SetBestScore(int bestScore)
        {
            if (_bestScoreLabel != null)
            {
                _bestScoreLabel.text = $"Best {bestScore}";
            }
        }

        public void SetAdditions(int remainingAdditions)
        {
            if (_plusBadgeLabel != null)
            {
                _plusBadgeLabel.text = remainingAdditions.ToString();
            }
        }

        public void SetHintCount(int hintCount)
        {
            SetHintBadge(hintCount);
        }

        public void SetPlusButtonInteractable(bool interactable)
        {
            if (_plusButton == null || _plusButtonBackgroundImage == null || _plusIconImage == null)
            {
                return;
            }

            _plusButton.interactable = interactable;
            _plusButtonBackgroundImage.color = interactable
                ? GamePalette.ActionButtonSurface
                : GamePalette.ActionButtonSurfaceDisabled;
            _plusIconImage.color = interactable
                ? GamePalette.ActionButtonIcon
                : GamePalette.ActionButtonIconDisabled;
        }

        public void SetHintButtonLocked(bool isLocked)
        {
            if (_hintButtonBackgroundImage == null || _hintIconImage == null)
            {
                return;
            }

            _hintButtonBackgroundImage.color = isLocked
                ? GamePalette.ActionButtonSurfaceDisabled
                : GamePalette.ActionButtonSurface;
            _hintIconImage.color = isLocked
                ? GamePalette.ActionButtonIconDisabled
                : GamePalette.ActionButtonIcon;
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
            _overlayRoot?.transform.SetAsLastSibling();
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
            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(HandleBackClicked);
            }

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
            RectTransform safeAreaRect,
            RectTransform topAreaRect,
            RectTransform scoreAreaRect,
            RectTransform scoreValueRect,
            RectTransform tooltipRect,
            RectTransform boardAreaRect,
            RectTransform boardContainerRect,
            RectTransform bottomControlsRect,
            RectTransform controlsRowRect,
            HorizontalLayoutGroup controlsLayout,
            BoardView boardView,
            AdSlotView adSlotView,
            GameObject overlayRoot,
            Button backButton,
            TMP_Text backButtonLabel,
            TMP_Text bestScoreLabel,
            TMP_Text scoreValueLabel,
            TMP_Text tooltipLabel,
            Button plusButton,
            Image plusButtonBackgroundImage,
            Image plusIconImage,
            TMP_Text plusBadgeLabel,
            LayoutElement plusButtonLayoutElement,
            RectTransform plusIconRect,
            RectTransform plusBadgeRect,
            Button hintButton,
            Image hintButtonBackgroundImage,
            Image hintIconImage,
            TMP_Text hintBadgeLabel,
            LayoutElement hintButtonLayoutElement,
            RectTransform hintIconRect,
            RectTransform hintBadgeRect,
            GameObject gameOverOverlay,
            RectTransform gameOverPanelRect,
            TMP_Text gameOverScoreLabel,
            TMP_Text gameOverStageLabel,
            Button restartButton,
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
            _safeAreaRect = safeAreaRect;
            _topAreaRect = topAreaRect;
            _scoreAreaRect = scoreAreaRect;
            _scoreValueRect = scoreValueRect;
            _tooltipRect = tooltipRect;
            _boardContainerRect = boardContainerRect;
            _bottomControlsRect = bottomControlsRect;
            _controlsRowRect = controlsRowRect;
            _controlsLayout = controlsLayout;
            _boardView = boardView;
            _adSlotView = adSlotView;
            _overlayRoot = overlayRoot;
            _backButton = backButton;
            _backButtonLabel = backButtonLabel;
            _bestScoreLabel = bestScoreLabel;
            _scoreValueLabel = scoreValueLabel;
            _tooltipLabel = tooltipLabel;
            _plusButton = plusButton;
            _plusButtonBackgroundImage = plusButtonBackgroundImage;
            _plusIconImage = plusIconImage;
            _plusBadgeLabel = plusBadgeLabel;
            _plusButtonLayoutElement = plusButtonLayoutElement;
            _plusIconRect = plusIconRect;
            _plusBadgeRect = plusBadgeRect;
            _hintButton = hintButton;
            _hintButtonBackgroundImage = hintButtonBackgroundImage;
            _hintIconImage = hintIconImage;
            _hintBadgeLabel = hintBadgeLabel;
            _hintButtonLayoutElement = hintButtonLayoutElement;
            _hintIconRect = hintIconRect;
            _hintBadgeRect = hintBadgeRect;
            _gameOverOverlay = gameOverOverlay;
            _gameOverPanelRect = gameOverPanelRect;
            _gameOverScoreLabel = gameOverScoreLabel;
            _gameOverStageLabel = gameOverStageLabel;
            _gameOverTitleLabel = gameOverTitleLabel;
            _restartButton = restartButton;
            _restartButtonLabel = restartButtonLabel;

            ConfigureLabel(_backButtonLabel, effectiveRegularFont, 28, TextAnchor.MiddleLeft, GamePalette.BoardTileText);
            ConfigureLabel(_bestScoreLabel, effectiveRegularFont, 24, TextAnchor.MiddleRight, GamePalette.BoardTileText);
            ConfigureLabel(_scoreValueLabel, effectiveRegularFont, 62, TextAnchor.MiddleCenter, GamePalette.ScoreValueText);
            ConfigureLabel(_tooltipLabel, effectiveRegularFont, 24, TextAnchor.MiddleCenter, GamePalette.ActionButtonIcon);
            _tooltipLabel.textWrappingMode = TextWrappingModes.Normal;
            _tooltipLabel.enabled = false;
            _tooltipLabel.text = string.Empty;

            ConfigureLabel(_plusBadgeLabel, effectiveRegularFont, 22, TextAnchor.MiddleCenter, GamePalette.ActionButtonBadgeText);
            ConfigureLabel(_hintBadgeLabel, effectiveRegularFont, 22, TextAnchor.MiddleCenter, GamePalette.ActionButtonBadgeText);
            ConfigureLabel(gameOverTitleLabel, effectiveRegularFont, 48, TextAnchor.MiddleCenter, GamePalette.PrimaryText);
            ConfigureLabel(_gameOverScoreLabel, effectiveRegularFont, 34, TextAnchor.MiddleCenter, GamePalette.PrimaryText);
            ConfigureLabel(_gameOverStageLabel, effectiveRegularFont, 34, TextAnchor.MiddleCenter, GamePalette.SecondaryText);
            ConfigureLabel(_restartButtonLabel, effectiveRegularFont, 34, TextAnchor.MiddleCenter, GamePalette.PrimaryText);

            _backButton.onClick.AddListener(HandleBackClicked);
            _boardView.TileClicked += HandleBoardTileClicked;
            _adSlotView.HeightChanged += HandleAdSlotHeightChanged;
            _plusButton.onClick.AddListener(HandlePlusClicked);
            _hintButton.onClick.AddListener(HandleHintClicked);
            _restartButton.onClick.AddListener(HandleRestartClicked);

            SetScore(0);
            SetBestScore(0);
            SetAdditions(0);
            SetHintBadge(DefaultHintBadgeValue);
            SetPlusButtonInteractable(true);
            SetHintButtonLocked(false);
            HideGameOver();
            ApplyResponsiveLayout(force: true);
        }

        private void HandleBoardTileClicked(int index)
        {
            TileClicked?.Invoke(index);
        }

        private void HandleBackClicked()
        {
            BackClicked?.Invoke();
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
            ApplyResponsiveLayout(force: true);
        }

        private void OnEnable()
        {
            ApplyResponsiveLayout(force: true);
        }

        private void LateUpdate()
        {
            ApplyResponsiveLayout(force: false);
        }

        private void ApplyResponsiveLayout(bool force)
        {
            if (_safeAreaRect == null)
            {
                return;
            }

            Rect safeAreaRect = _safeAreaRect.rect;
            float safeWidth = safeAreaRect.width;
            float safeHeight = safeAreaRect.height;
            if (safeWidth <= 0f || safeHeight <= 0f)
            {
                return;
            }

            float adHeight = _adSlotView != null ? _adSlotView.CurrentHeight : DefaultAdHeight;
            Vector2 safeAreaSize = new Vector2(safeWidth, safeHeight);

            if (!force &&
                Vector2.Distance(_lastSafeAreaSize, safeAreaSize) < 0.5f &&
                Mathf.Abs(_lastAdHeight - adHeight) < 0.5f)
            {
                return;
            }

            _lastSafeAreaSize = safeAreaSize;
            _lastAdHeight = adHeight;

            float scale = MobileLayout.GetScale(safeWidth, safeHeight);
            float topAreaHeight = MobileLayout.ClampScaled(TopAreaHeight, MinTopAreaHeight, MaxTopAreaHeight, scale);
            float scoreAreaHeight = MobileLayout.ClampScaled(ScoreAreaHeight, MinScoreAreaHeight, MaxScoreAreaHeight, scale);
            float bottomControlsHeight = MobileLayout.ClampScaled(BottomControlsHeight, MinBottomControlsHeight, MaxBottomControlsHeight, scale);
            float controlsToAdSpacing = MobileLayout.ClampScaled(ControlsToAdSpacing, ControlsToAdSpacing, MaxControlsToAdSpacing, scale);
            float boardSidePadding = MobileLayout.ClampScaled(BoardSidePadding, MinBoardSidePadding, MaxBoardSidePadding, scale);
            float buttonSize = MobileLayout.ClampScaled(ButtonSize, MinButtonSize, MaxButtonSize, scale);
            float iconSize = MobileLayout.ClampScaled(IconSize, MinIconSize, MaxIconSize, scale);
            float badgeSize = MobileLayout.ClampScaled(BadgeSize, MinBadgeSize, MaxBadgeSize, scale);
            float controlsSpacing = MobileLayout.ClampScaled(ControlsRowSpacing, MinControlsRowSpacing, MaxControlsRowSpacing, scale);
            float scoreFontSize = MobileLayout.ClampScaled(ScoreFontSize, MinScoreFontSize, MaxScoreFontSize, scale);
            float tooltipFontSize = MobileLayout.ClampScaled(TooltipFontSize, MinTooltipFontSize, MaxTooltipFontSize, scale);

            if (_topAreaRect != null)
            {
                _topAreaRect.sizeDelta = new Vector2(0f, topAreaHeight);
            }

            if (_scoreAreaRect != null)
            {
                _scoreAreaRect.sizeDelta = new Vector2(0f, scoreAreaHeight);
                _scoreAreaRect.anchoredPosition = new Vector2(0f, -topAreaHeight);
            }

            if (_scoreValueRect != null)
            {
                _scoreValueRect.sizeDelta = new Vector2(Mathf.Min(safeWidth - (boardSidePadding * 2f), 420f), MobileLayout.ClampScaled(84f, 64f, 96f, scale));
                _scoreValueRect.anchoredPosition = new Vector2(0f, MobileLayout.ClampScaled(10f, 0f, 14f, scale));
            }

            if (_tooltipRect != null)
            {
                _tooltipRect.sizeDelta = new Vector2(Mathf.Max(240f, safeWidth - (boardSidePadding * 2f)), MobileLayout.ClampScaled(48f, 36f, 56f, scale));
                _tooltipRect.anchoredPosition = new Vector2(0f, MobileLayout.ClampScaled(8f, 6f, 12f, scale));
            }

            if (_scoreValueLabel != null)
            {
                _scoreValueLabel.fontSize = scoreFontSize;
            }

            if (_backButtonLabel != null)
            {
                _backButtonLabel.fontSize = MobileLayout.ClampScaled(28f, 22f, 30f, scale);
            }

            if (_bestScoreLabel != null)
            {
                _bestScoreLabel.fontSize = MobileLayout.ClampScaled(24f, 18f, 28f, scale);
            }

            if (_tooltipLabel != null)
            {
                _tooltipLabel.fontSize = tooltipFontSize;
            }

            if (_boardAreaRect != null)
            {
                _boardAreaRect.offsetMin = new Vector2(0f, adHeight + controlsToAdSpacing + bottomControlsHeight);
                _boardAreaRect.offsetMax = new Vector2(0f, -(topAreaHeight + scoreAreaHeight));
            }

            if (_bottomControlsRect != null)
            {
                _bottomControlsRect.sizeDelta = new Vector2(0f, bottomControlsHeight);
                _bottomControlsRect.anchoredPosition = new Vector2(0f, adHeight + controlsToAdSpacing);
            }

            if (_controlsRowRect != null)
            {
                _controlsRowRect.sizeDelta = new Vector2((buttonSize * 2f) + controlsSpacing, buttonSize + badgeSize);
                _controlsRowRect.anchoredPosition = new Vector2(0f, MobileLayout.ClampScaled(6f, 0f, 10f, scale));
            }

            if (_controlsLayout != null)
            {
                _controlsLayout.spacing = controlsSpacing;
            }

            ApplyActionButtonMetrics(_plusButtonLayoutElement, _plusIconRect, _plusBadgeRect, buttonSize, iconSize, badgeSize, scale);
            ApplyActionButtonMetrics(_hintButtonLayoutElement, _hintIconRect, _hintBadgeRect, buttonSize, iconSize, badgeSize, scale);

            if (_plusBadgeLabel != null)
            {
                _plusBadgeLabel.fontSize = MobileLayout.ClampScaled(22f, 18f, 24f, scale);
            }

            if (_hintBadgeLabel != null)
            {
                _hintBadgeLabel.fontSize = MobileLayout.ClampScaled(22f, 18f, 24f, scale);
            }

            if (_boardContainerRect != null && _boardAreaRect != null)
            {
                float availableBoardWidth = Mathf.Max(1f, safeWidth - (boardSidePadding * 2f));
                float boardWidth = Mathf.Min(MaxBoardWidth, availableBoardWidth);
                float availableBoardHeight = Mathf.Max(1f, _boardAreaRect.rect.height);
                float boardHeight = Mathf.Min(availableBoardHeight, Mathf.Max(MinBoardHeight, boardWidth * BoardHeightRatio));
                _boardContainerRect.sizeDelta = new Vector2(boardWidth, boardHeight);
            }

            if (_gameOverPanelRect != null)
            {
                float panelWidth = Mathf.Min(safeWidth - (boardSidePadding * 2f), RestartPanelWidth);
                float panelHeight = Mathf.Min(safeHeight - (boardSidePadding * 2f), RestartPanelHeight);
                _gameOverPanelRect.sizeDelta = new Vector2(Mathf.Max(320f, panelWidth), Mathf.Max(320f, panelHeight));
            }

            if (_gameOverTitleLabel != null)
            {
                _gameOverTitleLabel.fontSize = MobileLayout.ClampScaled(48f, 36f, 52f, scale);
            }

            if (_gameOverScoreLabel != null)
            {
                _gameOverScoreLabel.fontSize = MobileLayout.ClampScaled(34f, 26f, 38f, scale);
            }

            if (_gameOverStageLabel != null)
            {
                _gameOverStageLabel.fontSize = MobileLayout.ClampScaled(34f, 26f, 38f, scale);
            }

            if (_restartButtonLabel != null)
            {
                _restartButtonLabel.fontSize = MobileLayout.ClampScaled(34f, 26f, 38f, scale);
            }
        }

        private static void ApplyActionButtonMetrics(
            LayoutElement layoutElement,
            RectTransform iconRect,
            RectTransform badgeRect,
            float buttonSize,
            float iconSize,
            float badgeSize,
            float scale)
        {
            if (layoutElement != null)
            {
                layoutElement.preferredWidth = buttonSize;
                layoutElement.preferredHeight = buttonSize;
                layoutElement.minWidth = buttonSize;
                layoutElement.minHeight = buttonSize;
            }

            if (iconRect != null)
            {
                iconRect.sizeDelta = new Vector2(iconSize, iconSize);
                iconRect.anchoredPosition = new Vector2(0f, MobileLayout.ClampScaled(-2f, -4f, 0f, scale));
            }

            if (badgeRect != null)
            {
                badgeRect.sizeDelta = new Vector2(badgeSize, badgeSize);
                badgeRect.anchoredPosition = new Vector2(
                    MobileLayout.ClampScaled(-14f, -16f, -10f, scale),
                    MobileLayout.ClampScaled(-10f, -12f, -8f, scale));
            }
        }

        private void SetHintBadge(int count)
        {
            if (_hintBadgeLabel != null)
            {
                _hintBadgeLabel.text = count.ToString();
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

        private static (Button Button, Image BackgroundImage, Image IconImage, TMP_Text BadgeLabel, LayoutElement LayoutElement, RectTransform IconRect, RectTransform BadgeRect) CreateActionButton(
            Transform parent,
            string name,
            Sprite iconSprite,
            string fallbackText,
            TMP_FontAsset font)
        {
            var buttonObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));

            buttonObject.transform.SetParent(parent, false);

            var layoutElement = buttonObject.GetComponent<LayoutElement>();
            layoutElement.preferredWidth = 118f;
            layoutElement.preferredHeight = 118f;
            layoutElement.minWidth = 118f;
            layoutElement.minHeight = 118f;

            var buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.sprite = GetCircleSprite();
            buttonImage.color = GamePalette.ActionButtonSurface;
            buttonImage.type = Image.Type.Simple;

            var button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = buttonImage;

            var iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObject.transform.SetParent(buttonObject.transform, false);

            var iconRect = (RectTransform)iconObject.transform;
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(56f, 56f);
            iconRect.anchoredPosition = new Vector2(0f, -2f);

            var iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = iconSprite;
            iconImage.color = GamePalette.ActionButtonIcon;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            if (iconSprite == null)
            {
                TMP_Text fallbackLabel = CreateTextElement(buttonObject.transform, "FallbackLabel");
                RectTransform fallbackRect = (RectTransform)fallbackLabel.transform;
                fallbackRect.anchorMin = Vector2.zero;
                fallbackRect.anchorMax = Vector2.one;
                fallbackRect.offsetMin = Vector2.zero;
                fallbackRect.offsetMax = Vector2.zero;
                fallbackLabel.font = font != null ? font : TMP_Settings.defaultFontAsset;
                fallbackLabel.fontSize = 40f;
                fallbackLabel.enableAutoSizing = false;
                fallbackLabel.alignment = TextAlignmentOptions.Center;
                fallbackLabel.text = fallbackText;
                fallbackLabel.color = GamePalette.ActionButtonIcon;
                fallbackLabel.raycastTarget = false;
            }

            var badgeObject = new GameObject("Badge", typeof(RectTransform), typeof(Image));
            badgeObject.transform.SetParent(buttonObject.transform, false);

            var badgeRect = (RectTransform)badgeObject.transform;
            badgeRect.anchorMin = new Vector2(1f, 1f);
            badgeRect.anchorMax = new Vector2(1f, 1f);
            badgeRect.pivot = new Vector2(0.5f, 0.5f);
            badgeRect.sizeDelta = new Vector2(38f, 38f);
            badgeRect.anchoredPosition = new Vector2(-14f, -10f);

            var badgeImage = badgeObject.GetComponent<Image>();
            badgeImage.sprite = GetCircleSprite();
            badgeImage.color = GamePalette.ActionButtonBadgeBackground;
            badgeImage.raycastTarget = false;

            TMP_Text badgeLabel = CreateTextElement(badgeObject.transform, "Label");
            RectTransform badgeLabelRect = (RectTransform)badgeLabel.transform;
            badgeLabelRect.anchorMin = Vector2.zero;
            badgeLabelRect.anchorMax = Vector2.one;
            badgeLabelRect.offsetMin = Vector2.zero;
            badgeLabelRect.offsetMax = Vector2.zero;
            badgeLabel.font = font != null ? font : TMP_Settings.defaultFontAsset;
            badgeLabel.fontSize = 22f;
            badgeLabel.enableAutoSizing = false;
            badgeLabel.alignment = TextAlignmentOptions.Center;
            badgeLabel.color = GamePalette.ActionButtonBadgeText;
            badgeLabel.raycastTarget = false;

            return (button, buttonImage, iconImage, badgeLabel, layoutElement, iconRect, badgeRect);
        }

        private static (Button Button, TMP_Text Label) CreateTextButton(Transform parent, string name, string labelText)
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

            return (button, label);
        }

        private static (Button Button, TMP_Text Label) CreateTopTextButton(Transform parent, string name, string labelText)
        {
            var buttonObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button));

            buttonObject.transform.SetParent(parent, false);

            var buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0f, 0f, 0f, 0f);
            buttonImage.raycastTarget = true;

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

            return (button, label);
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
            label.textWrappingMode = TextWrappingModes.NoWrap;
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

        private static Sprite CreateTextureSprite(Texture2D texture)
        {
            if (texture == null)
            {
                return null;
            }

            var sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);

            sprite.name = $"{texture.name}_RuntimeSprite";
            return sprite;
        }

        private static Sprite GetCircleSprite()
        {
            if (_circleSprite != null)
            {
                return _circleSprite;
            }

            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "UiCircleSpriteTexture";
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            float radius = (size * 0.5f) - 1.5f;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.Clamp01(radius - distance);
                    pixels[(y * size) + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);

            _circleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
            _circleSprite.name = "UiCircleSprite";
            return _circleSprite;
        }
    }
}
