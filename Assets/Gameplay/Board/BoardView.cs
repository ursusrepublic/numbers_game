using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Gameplay.Board
{
    [DisallowMultipleComponent]
    public sealed class BoardView : MonoBehaviour
    {
        public event Action<int> TileClicked;

        private readonly List<BoardTileView> _tiles = new();

        private RectTransform _viewportRect;
        private RectTransform _contentRect;
        private GridLayoutGroup _gridLayoutGroup;
        private ScrollRect _scrollRect;
        private Font _labelFont;
        private int _columns;
        private int _cellCount;
        private float _lastViewportWidth = -1f;

        public static BoardView Create(Transform parent, int columns)
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
            scrollViewRect.offsetMin = new Vector2(32f, 32f);
            scrollViewRect.offsetMax = new Vector2(-32f, -32f);

            var scrollViewImage = scrollViewObject.GetComponent<Image>();
            scrollViewImage.color = new Color(0.12f, 0.14f, 0.18f, 0.92f);

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

            var boardView = scrollViewObject.GetComponent<BoardView>();
            boardView.Setup(viewportRect, contentRect, gridLayoutGroup, scrollRect, columns);
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
            }

            for (int index = _cellCount; index < _tiles.Count; index++)
            {
                _tiles[index].gameObject.SetActive(false);
            }

            Canvas.ForceUpdateCanvases();
            UpdateLayout();
            _scrollRect.verticalNormalizedPosition = 1f;
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
            GridLayoutGroup gridLayoutGroup,
            ScrollRect scrollRect,
            int columns)
        {
            _viewportRect = viewportRect;
            _contentRect = contentRect;
            _gridLayoutGroup = gridLayoutGroup;
            _scrollRect = scrollRect;
            _columns = Mathf.Max(1, columns);
            _labelFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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

        private void UpdateLayout()
        {
            if (_cellCount == 0 || _viewportRect == null)
            {
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
    }
}
