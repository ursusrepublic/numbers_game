using System;
using System.Collections.Generic;
using Game.UI.Styling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay.Board
{
    [DisallowMultipleComponent]
    public sealed class BoardView : MonoBehaviour
    {
        private const int ExtraEmptyRows = 2;

        public event Action<int> TileClicked;

        private readonly List<BoardTileView> _tiles = new();
        private readonly HashSet<int> _hintedIndices = new();
        private readonly List<RectTransform> _developerLineRects = new();
        private readonly List<BoardMatchInfo> _developerPairs = new();
        private static Texture2D _notebookGridTexture;

        private RectTransform _viewportRect;
        private RectTransform _contentRect;
        private RectTransform _surfaceRect;
        private RectTransform _effectsRect;
        private GridLayoutGroup _gridLayoutGroup;
        private ScrollRect _scrollRect;
        private Canvas _rootCanvas;
        private RawImage _surfaceImage;
        private Image _surfaceRightBorder;
        private Image _surfaceBottomBorder;
        private TMP_FontAsset _boldFont;
        private int _columns;
        private int _cellCount;
        private int _visualRowCount;
        private float _lastViewportWidth = -1f;

        public static BoardView Create(Transform parent, int columns, TMP_FontAsset regularFont, TMP_FontAsset boldFont)
        {
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
            scrollViewRect.offsetMin = Vector2.zero;
            scrollViewRect.offsetMax = Vector2.zero;

            var scrollViewImage = scrollViewObject.GetComponent<Image>();
            scrollViewImage.color = GamePalette.BoardScrollBackground;

            var viewportObject = new GameObject(
                "Viewport",
                typeof(RectTransform),
                typeof(Image),
                typeof(Mask));

            viewportObject.transform.SetParent(scrollViewObject.transform, false);

            var viewportRect = (RectTransform)viewportObject.transform;
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(8f, 16f);
            viewportRect.offsetMax = new Vector2(-8f, -16f);

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

            var surfaceObject = new GameObject(
                "BoardSurface",
                typeof(RectTransform),
                typeof(RawImage),
                typeof(LayoutElement));

            surfaceObject.transform.SetParent(contentObject.transform, false);

            var surfaceRect = (RectTransform)surfaceObject.transform;
            surfaceRect.anchorMin = new Vector2(0f, 1f);
            surfaceRect.anchorMax = new Vector2(0f, 1f);
            surfaceRect.pivot = new Vector2(0f, 1f);
            surfaceRect.anchoredPosition = Vector2.zero;

            var surfaceImage = surfaceObject.GetComponent<RawImage>();
            surfaceImage.texture = GetNotebookGridTexture();
            surfaceImage.color = Color.white;
            surfaceImage.raycastTarget = false;

            var surfaceLayout = surfaceObject.GetComponent<LayoutElement>();
            surfaceLayout.ignoreLayout = true;

            var rightBorderObject = new GameObject(
                "RightBorder",
                typeof(RectTransform),
                typeof(Image));

            rightBorderObject.transform.SetParent(surfaceObject.transform, false);

            var rightBorderRect = (RectTransform)rightBorderObject.transform;
            rightBorderRect.anchorMin = new Vector2(1f, 0f);
            rightBorderRect.anchorMax = new Vector2(1f, 1f);
            rightBorderRect.pivot = new Vector2(1f, 0.5f);
            rightBorderRect.anchoredPosition = Vector2.zero;

            var rightBorderImage = rightBorderObject.GetComponent<Image>();
            rightBorderImage.color = GamePalette.GridLineColor;
            rightBorderImage.raycastTarget = false;

            var bottomBorderObject = new GameObject(
                "BottomBorder",
                typeof(RectTransform),
                typeof(Image));

            bottomBorderObject.transform.SetParent(surfaceObject.transform, false);

            var bottomBorderRect = (RectTransform)bottomBorderObject.transform;
            bottomBorderRect.anchorMin = new Vector2(0f, 0f);
            bottomBorderRect.anchorMax = new Vector2(1f, 0f);
            bottomBorderRect.pivot = new Vector2(0.5f, 0f);
            bottomBorderRect.anchoredPosition = Vector2.zero;

            var bottomBorderImage = bottomBorderObject.GetComponent<Image>();
            bottomBorderImage.color = GamePalette.GridLineColor;
            bottomBorderImage.raycastTarget = false;

            var effectsObject = new GameObject(
                "BoardEffects",
                typeof(RectTransform),
                typeof(LayoutElement));

            effectsObject.transform.SetParent(contentObject.transform, false);

            var effectsRect = (RectTransform)effectsObject.transform;
            effectsRect.anchorMin = Vector2.zero;
            effectsRect.anchorMax = Vector2.one;
            effectsRect.offsetMin = Vector2.zero;
            effectsRect.offsetMax = Vector2.zero;

            var effectsLayout = effectsObject.GetComponent<LayoutElement>();
            effectsLayout.ignoreLayout = true;

            var gridLayoutGroup = contentObject.GetComponent<GridLayoutGroup>();
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = Mathf.Max(1, columns);
            gridLayoutGroup.spacing = Vector2.zero;
            gridLayoutGroup.padding = new RectOffset(0, 0, 0, 0);
            gridLayoutGroup.childAlignment = TextAnchor.UpperCenter;

            var scrollRect = scrollViewObject.GetComponent<ScrollRect>();
            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 24f;

            var boardView = scrollViewObject.GetComponent<BoardView>();
            boardView.Setup(
                viewportRect,
                contentRect,
                surfaceRect,
                surfaceImage,
                rightBorderImage,
                bottomBorderImage,
                effectsRect,
                gridLayoutGroup,
                scrollRect,
                boldFont,
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

            _surfaceRect?.SetAsFirstSibling();

            if (_effectsRect != null)
            {
                _effectsRect.SetAsLastSibling();
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

        public void ShowDeveloperPairLines(IReadOnlyList<BoardMatchInfo> pairs)
        {
            _developerPairs.Clear();
            for (int index = 0; index < pairs.Count; index++)
            {
                _developerPairs.Add(pairs[index]);
            }

            RefreshDeveloperPairLines();
        }

        public void ClearDeveloperPairLines()
        {
            _developerPairs.Clear();

            for (int index = 0; index < _developerLineRects.Count; index++)
            {
                _developerLineRects[index].gameObject.SetActive(false);
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

        private void Setup(
            RectTransform viewportRect,
            RectTransform contentRect,
            RectTransform surfaceRect,
            RawImage surfaceImage,
            Image surfaceRightBorder,
            Image surfaceBottomBorder,
            RectTransform effectsRect,
            GridLayoutGroup gridLayoutGroup,
            ScrollRect scrollRect,
            TMP_FontAsset boldFont,
            int columns)
        {
            _viewportRect = viewportRect;
            _contentRect = contentRect;
            _surfaceRect = surfaceRect;
            _surfaceImage = surfaceImage;
            _surfaceRightBorder = surfaceRightBorder;
            _surfaceBottomBorder = surfaceBottomBorder;
            _effectsRect = effectsRect;
            _gridLayoutGroup = gridLayoutGroup;
            _scrollRect = scrollRect;
            _rootCanvas = GetComponentInParent<Canvas>();
            _columns = Mathf.Max(1, columns);
            _boldFont = boldFont != null ? boldFont : TMP_Settings.defaultFontAsset;
        }

        private void EnsureTileCount(int requiredCount)
        {
            while (_tiles.Count < requiredCount)
            {
                var tile = BoardTileView.Create(_contentRect, _boldFont, HandleTileClicked);
                _tiles.Add(tile);
            }

            _surfaceRect?.SetAsFirstSibling();
            _effectsRect?.SetAsLastSibling();
        }

        private void HandleTileClicked(int index)
        {
            TileClicked?.Invoke(index);
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
            if (_viewportRect == null || _contentRect == null || _gridLayoutGroup == null)
            {
                return;
            }

            float viewportWidth = _viewportRect.rect.width;
            if (viewportWidth <= 0f)
            {
                return;
            }

            float pixelSize = GetCanvasPixelSize();
            float totalSpacing = _gridLayoutGroup.spacing.x * Mathf.Max(0, _columns - 1);
            float usableWidth = Mathf.Max(0f, viewportWidth - totalSpacing);
            float cellSize = Mathf.Max(48f, SnapDownToCanvasPixel(usableWidth / _columns, pixelSize));

            _gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);

            int activeRows = _cellCount > 0 ? Mathf.CeilToInt((float)_cellCount / _columns) : 0;
            float rowStride = cellSize + _gridLayoutGroup.spacing.y;
            float viewportHeight = _viewportRect.rect.height;
            int minVisibleRows = Mathf.Max(1, Mathf.CeilToInt(viewportHeight / Mathf.Max(pixelSize, rowStride)));
            _visualRowCount = Mathf.Max(Mathf.Max(1, activeRows), minVisibleRows) + ExtraEmptyRows;

            float contentHeight = (_visualRowCount * cellSize) + (Mathf.Max(0, _visualRowCount - 1) * _gridLayoutGroup.spacing.y);
            _contentRect.sizeDelta = new Vector2(0f, Mathf.Max(viewportHeight, contentHeight));

            RefreshBoardSurface(cellSize, _visualRowCount);

            if (_developerPairs.Count > 0)
            {
                RefreshDeveloperPairLines();
            }
            else
            {
                ClearDeveloperPairLines();
            }
        }

        private void RefreshBoardSurface(float cellSize, int visualRowCount)
        {
            if (_contentRect == null || _surfaceRect == null || _surfaceImage == null || _columns <= 0 || visualRowCount <= 0)
            {
                return;
            }

            float gridWidth = (_columns * cellSize) + (Mathf.Max(0, _columns - 1) * _gridLayoutGroup.spacing.x);
            float gridHeight = (visualRowCount * cellSize) + (Mathf.Max(0, visualRowCount - 1) * _gridLayoutGroup.spacing.y);
            float startX = (_contentRect.rect.width - gridWidth) * 0.5f;

            ConfigureOverlayRect(_surfaceRect, startX, 0f, gridWidth, gridHeight);
            _surfaceImage.texture = GetNotebookGridTexture();
            _surfaceImage.uvRect = new Rect(0f, 0f, _columns, visualRowCount);

            _surfaceRect.SetAsFirstSibling();
            _effectsRect?.SetAsLastSibling();

            float thickness = GetCanvasPixelSize();
            if (_surfaceRightBorder != null)
            {
                RectTransform rightBorderRect = (RectTransform)_surfaceRightBorder.transform;
                rightBorderRect.sizeDelta = new Vector2(thickness, 0f);
            }

            if (_surfaceBottomBorder != null)
            {
                RectTransform bottomBorderRect = (RectTransform)_surfaceBottomBorder.transform;
                bottomBorderRect.sizeDelta = new Vector2(0f, thickness);
            }
        }

        private void ConfigureOverlayRect(RectTransform rect, float left, float top, float width, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(left, -top);
            rect.sizeDelta = new Vector2(width, height);
        }

        private float GetCanvasPixelSize()
        {
            float scaleFactor = _rootCanvas != null ? _rootCanvas.scaleFactor : 1f;
            return 1f / Mathf.Max(1f, scaleFactor);
        }

        private static float SnapDownToCanvasPixel(float value, float pixelSize)
        {
            if (pixelSize <= 0f)
            {
                return value;
            }

            return Mathf.Floor(value / pixelSize) * pixelSize;
        }

        private static Texture2D GetNotebookGridTexture()
        {
            if (_notebookGridTexture != null)
            {
                return _notebookGridTexture;
            }

            const int textureSize = 64;
            const int mainLineThickness = 2;
            const int softLineThickness = 1;

            _notebookGridTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            _notebookGridTexture.name = "NotebookGridTexture";
            _notebookGridTexture.wrapMode = TextureWrapMode.Repeat;
            _notebookGridTexture.filterMode = FilterMode.Bilinear;
            _notebookGridTexture.hideFlags = HideFlags.HideAndDontSave;

            Color background = GamePalette.BoardTileNormalBackground;
            Color mainLine = GamePalette.GridLineColor;
            Color softLine = Color.Lerp(background, mainLine, 0.35f);
            Color[] pixels = new Color[textureSize * textureSize];

            for (int index = 0; index < pixels.Length; index++)
            {
                pixels[index] = background;
            }

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    bool isMainVertical = x < mainLineThickness;
                    bool isSoftVertical = !isMainVertical && x < mainLineThickness + softLineThickness;
                    bool isMainHorizontal = y >= textureSize - mainLineThickness;
                    bool isSoftHorizontal = !isMainHorizontal && y >= textureSize - (mainLineThickness + softLineThickness);

                    Color color = background;
                    if (isMainVertical || isMainHorizontal)
                    {
                        color = mainLine;
                    }
                    else if (isSoftVertical || isSoftHorizontal)
                    {
                        color = softLine;
                    }

                    pixels[(y * textureSize) + x] = color;
                }
            }

            _notebookGridTexture.SetPixels(pixels);
            _notebookGridTexture.Apply(false, true);
            return _notebookGridTexture;
        }

        private void RefreshDeveloperPairLines()
        {
            if (_effectsRect == null)
            {
                return;
            }

            if (_developerPairs.Count == 0)
            {
                ClearDeveloperPairLines();
                return;
            }

            Canvas.ForceUpdateCanvases();
            _effectsRect.SetAsLastSibling();
            EnsureDeveloperLineCount(_developerPairs.Count);

            for (int index = 0; index < _developerPairs.Count; index++)
            {
                RectTransform lineRect = _developerLineRects[index];
                BoardMatchInfo pair = _developerPairs[index];

                if (!TryPositionDeveloperLine(lineRect, pair.FirstIndex, pair.SecondIndex))
                {
                    lineRect.gameObject.SetActive(false);
                }
            }

            for (int index = _developerPairs.Count; index < _developerLineRects.Count; index++)
            {
                _developerLineRects[index].gameObject.SetActive(false);
            }
        }

        private void EnsureDeveloperLineCount(int requiredCount)
        {
            while (_developerLineRects.Count < requiredCount)
            {
                var lineObject = new GameObject(
                    "DeveloperPairLine",
                    typeof(RectTransform),
                    typeof(Image),
                    typeof(LayoutElement));

                lineObject.transform.SetParent(_effectsRect, false);

                var layoutElement = lineObject.GetComponent<LayoutElement>();
                layoutElement.ignoreLayout = true;

                var lineImage = lineObject.GetComponent<Image>();
                lineImage.color = GamePalette.DeveloperPairLine;
                lineImage.raycastTarget = false;

                var lineRect = (RectTransform)lineObject.transform;
                lineRect.anchorMin = new Vector2(0.5f, 0.5f);
                lineRect.anchorMax = new Vector2(0.5f, 0.5f);
                lineRect.pivot = new Vector2(0.5f, 0.5f);

                _developerLineRects.Add(lineRect);
            }
        }

        private bool TryPositionDeveloperLine(RectTransform lineRect, int firstIndex, int secondIndex)
        {
            if (lineRect == null || !TryGetTileCenter(firstIndex, out Vector2 firstCenter) || !TryGetTileCenter(secondIndex, out Vector2 secondCenter))
            {
                return false;
            }

            Vector2 difference = secondCenter - firstCenter;
            float distance = difference.magnitude;
            if (distance <= 0.5f)
            {
                return false;
            }

            lineRect.gameObject.SetActive(true);
            lineRect.anchoredPosition = (firstCenter + secondCenter) * 0.5f;
            lineRect.sizeDelta = new Vector2(distance, 8f);
            lineRect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg);
            return true;
        }

        private bool TryGetTileCenter(int index, out Vector2 center)
        {
            center = Vector2.zero;

            if (index < 0 || index >= _cellCount || index >= _tiles.Count || _effectsRect == null)
            {
                return false;
            }

            var tileRect = (RectTransform)_tiles[index].transform;
            Vector3 worldCenter = tileRect.TransformPoint(tileRect.rect.center);
            center = _effectsRect.InverseTransformPoint(worldCenter);
            return true;
        }
    }
}
