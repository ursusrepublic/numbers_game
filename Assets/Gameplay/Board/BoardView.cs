using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay.Board
{
    [DisallowMultipleComponent]
    public sealed class BoardView : MonoBehaviour
    {
        private static readonly Color HudPanelColor = new Color(0.16f, 0.18f, 0.24f, 0.96f);
        private static readonly Color ScrollPanelColor = new Color(0.12f, 0.14f, 0.18f, 0.92f);
        private static readonly Color ActiveButtonColor = new Color(0.28f, 0.56f, 0.92f, 1f);
        private static readonly Color LockedButtonColor = new Color(0.28f, 0.44f, 0.60f, 0.95f);
        private static readonly Color DisabledButtonColor = new Color(0.30f, 0.32f, 0.38f, 0.9f);
        private static readonly Color TooltipColor = new Color(1f, 0.93f, 0.70f, 1f);
        private static readonly Color TextColor = Color.white;

        public event Action<int> TileClicked;
        public event Action PlusClicked;
        public event Action HintClicked;
        public event Action RestartClicked;

        private readonly List<BoardTileView> _tiles = new();
        private readonly HashSet<int> _hintedIndices = new();

        private RectTransform _viewportRect;
        private RectTransform _contentRect;
        private GridLayoutGroup _gridLayoutGroup;
        private ScrollRect _scrollRect;
        private Button _plusButton;
        private Button _hintButton;
        private Image _plusButtonImage;
        private Image _hintButtonImage;
        private Text _scoreLabel;
        private Text _additionsLabel;
        private Text _tooltipLabel;
        private Text _plusButtonLabel;
        private Text _hintButtonLabel;
        private GameObject _gameOverOverlay;
        private Text _gameOverScoreLabel;
        private Text _gameOverStageLabel;
        private Button _restartButton;
        private Image _restartButtonImage;
        private Text _restartButtonLabel;
        private Font _labelFont;
        private Coroutine _tooltipRoutine;
        private int _columns;
        private int _cellCount;
        private float _lastViewportWidth = -1f;

        public static BoardView Create(Transform parent, int columns)
        {
            var hudPanelObject = new GameObject(
                "HudPanel",
                typeof(RectTransform),
                typeof(Image));

            hudPanelObject.transform.SetParent(parent, false);

            var hudPanelRect = (RectTransform)hudPanelObject.transform;
            hudPanelRect.anchorMin = new Vector2(0f, 1f);
            hudPanelRect.anchorMax = new Vector2(1f, 1f);
            hudPanelRect.pivot = new Vector2(0.5f, 1f);
            hudPanelRect.offsetMin = new Vector2(32f, -220f);
            hudPanelRect.offsetMax = new Vector2(-32f, -32f);

            var hudPanelImage = hudPanelObject.GetComponent<Image>();
            hudPanelImage.color = HudPanelColor;

            Text scoreLabel = CreateTextElement(hudPanelObject.transform, "ScoreLabel");
            ConfigureRect((RectTransform)scoreLabel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 52f), new Vector2(20f, -18f));

            Text additionsLabel = CreateTextElement(hudPanelObject.transform, "AdditionsLabel");
            ConfigureRect((RectTransform)additionsLabel.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(220f, 40f), new Vector2(20f, -78f));

            (Button hintButton, Image hintButtonImage, Text hintButtonLabel) = CreateButton(hudPanelObject.transform, "HintButton", "Hint");
            ConfigureRect((RectTransform)hintButton.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(136f, 52f), new Vector2(-152f, -18f));

            (Button plusButton, Image plusButtonImage, Text plusButtonLabel) = CreateButton(hudPanelObject.transform, "PlusButton", "+");
            ConfigureRect((RectTransform)plusButton.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(120f, 52f), new Vector2(-20f, -18f));

            Text tooltipLabel = CreateTextElement(hudPanelObject.transform, "TooltipLabel");
            ConfigureRect((RectTransform)tooltipLabel.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 0f));
            ((RectTransform)tooltipLabel.transform).offsetMin = new Vector2(20f, 18f);
            ((RectTransform)tooltipLabel.transform).offsetMax = new Vector2(-20f, 66f);

            var scrollViewObject = new GameObject(
                "BoardScrollView",
                typeof(RectTransform),
                typeof(Image),
                typeof(ScrollRect),
                typeof(BoardView));

            scrollViewObject.transform.SetParent(parent, false);

            var scrollViewRect = (RectTransform)scrollViewObject.transform;
            scrollViewRect.anchorMin = Vector2.zero;
            scrollViewRect.anchorMax = Vector2.one;
            scrollViewRect.offsetMin = new Vector2(32f, 32f);
            scrollViewRect.offsetMax = new Vector2(-32f, -236f);

            var scrollViewImage = scrollViewObject.GetComponent<Image>();
            scrollViewImage.color = ScrollPanelColor;

            var viewportObject = new GameObject(
                "Viewport",
                typeof(RectTransform),
                typeof(Image),
                typeof(Mask));

            viewportObject.transform.SetParent(scrollViewObject.transform, false);

            var viewportRect = (RectTransform)viewportObject.transform;
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(16f, 16f);
            viewportRect.offsetMax = new Vector2(-16f, -16f);

            var viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f);

            var viewportMask = viewportObject.GetComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            var contentObject = new GameObject(
                "BoardContent",
                typeof(RectTransform),
                typeof(GridLayoutGroup));

            contentObject.transform.SetParent(viewportObject.transform, false);

            var contentRect = (RectTransform)contentObject.transform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;

            var gridLayoutGroup = contentObject.GetComponent<GridLayoutGroup>();
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = Mathf.Max(1, columns);
            gridLayoutGroup.spacing = new Vector2(8f, 8f);
            gridLayoutGroup.padding = new RectOffset(8, 8, 8, 8);
            gridLayoutGroup.childAlignment = TextAnchor.UpperCenter;

            var scrollRect = scrollViewObject.GetComponent<ScrollRect>();
            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 24f;

            var overlayObject = new GameObject(
                "GameOverOverlay",
                typeof(RectTransform),
                typeof(Image));

            overlayObject.transform.SetParent(parent, false);

            var overlayRect = (RectTransform)overlayObject.transform;
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            var overlayImage = overlayObject.GetComponent<Image>();
            overlayImage.color = new Color(0.05f, 0.06f, 0.09f, 0.84f);

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
            panelImage.color = HudPanelColor;

            Text titleLabel = CreateTextElement(panelObject.transform, "TitleLabel");
            ConfigureRect((RectTransform)titleLabel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(520f, 72f), new Vector2(0f, -60f));
            titleLabel.text = "Game Over";

            Text gameOverScoreLabel = CreateTextElement(panelObject.transform, "ScoreLabel");
            ConfigureRect((RectTransform)gameOverScoreLabel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(520f, 56f), new Vector2(0f, -160f));

            Text gameOverStageLabel = CreateTextElement(panelObject.transform, "StageLabel");
            ConfigureRect((RectTransform)gameOverStageLabel.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(520f, 56f), new Vector2(0f, -230f));

            (Button restartButton, Image restartButtonImage, Text restartButtonLabel) = CreateButton(panelObject.transform, "RestartButton", "Restart");
            ConfigureRect((RectTransform)restartButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(260f, 64f), new Vector2(0f, 54f));

            var boardView = scrollViewObject.GetComponent<BoardView>();
            boardView.Setup(
                viewportRect,
                contentRect,
                gridLayoutGroup,
                scrollRect,
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
                columns);

            return boardView;
        }

        public void SetCells(IReadOnlyList<BoardCell> cells)
        {
            _cellCount = cells.Count;

            EnsureTileCount(_cellCount);

            for (int index = 0; index < _cellCount; index++)
            {
                _tiles[index].gameObject.SetActive(true);
                _tiles[index].SetCell(cells[index]);
                _tiles[index].SetHinted(_hintedIndices.Contains(index));
            }

            for (int index = _cellCount; index < _tiles.Count; index++)
            {
                _tiles[index].SetHinted(false);
                _tiles[index].gameObject.SetActive(false);
            }

            Canvas.ForceUpdateCanvases();
            UpdateLayout();
        }

        public void RefreshCell(BoardCell cell)
        {
            if (cell == null)
            {
                return;
            }

            if (cell.Index < 0 || cell.Index >= _tiles.Count)
            {
                return;
            }

            _tiles[cell.Index].SetCell(cell);
            _tiles[cell.Index].SetHinted(_hintedIndices.Contains(cell.Index));
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
            _plusButtonImage.color = interactable ? ActiveButtonColor : DisabledButtonColor;
            _plusButtonLabel.color = interactable ? TextColor : new Color(1f, 1f, 1f, 0.65f);
        }

        public void SetHintButtonLocked(bool isLocked)
        {
            if (_hintButtonImage == null || _hintButtonLabel == null)
            {
                return;
            }

            _hintButtonImage.color = isLocked ? LockedButtonColor : ActiveButtonColor;
            _hintButtonLabel.color = isLocked ? new Color(1f, 1f, 1f, 0.82f) : TextColor;
        }

        public void ShowHint(BoardMatchInfo matchInfo)
        {
            ClearHint();
            _hintedIndices.Clear();
            _hintedIndices.Add(matchInfo.FirstIndex);
            _hintedIndices.Add(matchInfo.SecondIndex);

            RefreshHintTiles();
            ReplayHintFeedback();
        }

        public void ClearHint()
        {
            if (_hintedIndices.Count == 0)
            {
                return;
            }

            var indicesToRefresh = new List<int>(_hintedIndices);
            _hintedIndices.Clear();

            for (int index = 0; index < indicesToRefresh.Count; index++)
            {
                int hintedIndex = indicesToRefresh[index];
                if (hintedIndex >= 0 && hintedIndex < _cellCount)
                {
                    _tiles[hintedIndex].SetHinted(false);
                }
            }
        }

        public void ReplayHintFeedback()
        {
            if (_hintedIndices.Count == 0)
            {
                return;
            }

            int firstVisibleIndex = int.MaxValue;
            foreach (int index in _hintedIndices)
            {
                if (index < 0 || index >= _cellCount)
                {
                    continue;
                }

                _tiles[index].ReplayHintFeedback();
                firstVisibleIndex = Mathf.Min(firstVisibleIndex, index);
            }

            if (firstVisibleIndex != int.MaxValue)
            {
                FocusOnIndex(firstVisibleIndex);
            }
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
            if (_scrollRect != null)
            {
                _scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        public void ScrollToBottom()
        {
            if (_scrollRect != null)
            {
                _scrollRect.verticalNormalizedPosition = 0f;
            }
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

        private void LateUpdate()
        {
            if (_viewportRect == null)
            {
                return;
            }

            float viewportWidth = _viewportRect.rect.width;
            if (viewportWidth <= 0f || Mathf.Abs(viewportWidth - _lastViewportWidth) < 0.5f)
            {
                return;
            }

            _lastViewportWidth = viewportWidth;
            UpdateLayout();
        }

        private void OnDestroy()
        {
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
            RectTransform viewportRect,
            RectTransform contentRect,
            GridLayoutGroup gridLayoutGroup,
            ScrollRect scrollRect,
            Text scoreLabel,
            Text additionsLabel,
            Text tooltipLabel,
            Button plusButton,
            Image plusButtonImage,
            Text plusButtonLabel,
            Button hintButton,
            Image hintButtonImage,
            Text hintButtonLabel,
            GameObject gameOverOverlay,
            Text gameOverScoreLabel,
            Text gameOverStageLabel,
            Button restartButton,
            Image restartButtonImage,
            Text restartButtonLabel,
            Text gameOverTitleLabel,
            int columns)
        {
            _viewportRect = viewportRect;
            _contentRect = contentRect;
            _gridLayoutGroup = gridLayoutGroup;
            _scrollRect = scrollRect;
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
            _columns = Mathf.Max(1, columns);
            _labelFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            ConfigureLabel(_scoreLabel, 42, TextAnchor.MiddleLeft, TextColor);
            ConfigureLabel(_additionsLabel, 30, TextAnchor.MiddleLeft, new Color(0.86f, 0.90f, 0.96f, 1f));
            ConfigureLabel(_tooltipLabel, 28, TextAnchor.MiddleCenter, TooltipColor);
            _tooltipLabel.enabled = false;
            _tooltipLabel.text = string.Empty;

            ConfigureLabel(_plusButtonLabel, 34, TextAnchor.MiddleCenter, TextColor);
            ConfigureLabel(_hintButtonLabel, 28, TextAnchor.MiddleCenter, TextColor);
            ConfigureLabel(gameOverTitleLabel, 48, TextAnchor.MiddleCenter, TextColor);
            ConfigureLabel(_gameOverScoreLabel, 34, TextAnchor.MiddleCenter, TextColor);
            ConfigureLabel(_gameOverStageLabel, 34, TextAnchor.MiddleCenter, new Color(0.86f, 0.90f, 0.96f, 1f));
            ConfigureLabel(_restartButtonLabel, 34, TextAnchor.MiddleCenter, TextColor);

            _plusButton.onClick.AddListener(HandlePlusClicked);
            _hintButton.onClick.AddListener(HandleHintClicked);
            _restartButton.onClick.AddListener(HandleRestartClicked);

            SetScore(0);
            SetAdditions(0);
            SetPlusButtonInteractable(true);
            SetHintButtonLocked(false);
            HideGameOver();
        }

        private void EnsureTileCount(int requiredCount)
        {
            while (_tiles.Count < requiredCount)
            {
                var tile = BoardTileView.Create(_contentRect, _labelFont, HandleTileClicked);
                _tiles.Add(tile);
            }
        }

        private void HandleTileClicked(int index)
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

        private void RefreshHintTiles()
        {
            foreach (int hintedIndex in _hintedIndices)
            {
                if (hintedIndex >= 0 && hintedIndex < _cellCount)
                {
                    _tiles[hintedIndex].SetHinted(true);
                }
            }
        }

        private void FocusOnIndex(int index)
        {
            if (_scrollRect == null || _viewportRect == null || _contentRect == null || _cellCount == 0)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();

            float contentHeight = _contentRect.rect.height;
            float viewportHeight = _viewportRect.rect.height;
            if (contentHeight <= viewportHeight)
            {
                return;
            }

            int row = index / _columns;
            float rowStride = _gridLayoutGroup.cellSize.y + _gridLayoutGroup.spacing.y;
            float targetOffset = _gridLayoutGroup.padding.top + (row * rowStride);
            float maxOffset = Mathf.Max(1f, contentHeight - viewportHeight);
            _scrollRect.verticalNormalizedPosition = 1f - Mathf.Clamp01(targetOffset / maxOffset);
        }

        private void UpdateLayout()
        {
            if (_viewportRect == null || _contentRect == null)
            {
                return;
            }

            if (_cellCount == 0)
            {
                _contentRect.sizeDelta = new Vector2(0f, Mathf.Max(0f, _viewportRect.rect.height));
                return;
            }

            float viewportWidth = _viewportRect.rect.width;
            if (viewportWidth <= 0f)
            {
                return;
            }

            float totalSpacing = _gridLayoutGroup.spacing.x * (_columns - 1);
            float usableWidth = viewportWidth
                - _gridLayoutGroup.padding.left
                - _gridLayoutGroup.padding.right
                - totalSpacing;
            float cellSize = Mathf.Max(48f, Mathf.Floor(usableWidth / _columns));

            _gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);

            int rows = Mathf.CeilToInt((float)_cellCount / _columns);
            float contentHeight = _gridLayoutGroup.padding.top
                + _gridLayoutGroup.padding.bottom
                + (rows * cellSize)
                + (Mathf.Max(0, rows - 1) * _gridLayoutGroup.spacing.y);

            _contentRect.sizeDelta = new Vector2(0f, Mathf.Max(_viewportRect.rect.height, contentHeight));
        }

        private IEnumerator HideTooltipAfterDelay(float duration)
        {
            yield return new WaitForSeconds(duration);

            _tooltipLabel.enabled = false;
            _tooltipLabel.text = string.Empty;
            _tooltipRoutine = null;
        }

        private static Text CreateTextElement(Transform parent, string name)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);
            return textObject.GetComponent<Text>();
        }

        private static (Button Button, Image Image, Text Label) CreateButton(Transform parent, string name, string labelText)
        {
            var buttonObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button));

            buttonObject.transform.SetParent(parent, false);

            var buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = ActiveButtonColor;

            var button = buttonObject.GetComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.targetGraphic = buttonImage;

            Text label = CreateTextElement(buttonObject.transform, "Label");
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

        private static void ConfigureLabel(Text label, int fontSize, TextAnchor alignment, Color color)
        {
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
        }
    }
}
