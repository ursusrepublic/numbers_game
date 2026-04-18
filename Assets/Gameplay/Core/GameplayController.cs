using System;
using Game.Gameplay.Board;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Game.Gameplay.Core
{
    [DisallowMultipleComponent]
    public sealed class GameplayController : MonoBehaviour
    {
        private BoardState _boardState;
        private BoardView _boardView;
        private int? _selectedCellIndex;
        private int _columns;

        public void Initialize(int columns, int rows, int startingPairs, int randomSeed)
        {
            _columns = Mathf.Max(1, columns);
            int safeRows = Mathf.Max(1, rows);
            int maxStartingPairs = (_columns * safeRows) / 2;
            int safeStartingPairs = Mathf.Clamp(startingPairs, 0, maxStartingPairs);
            int actualSeed = randomSeed == 0 ? Environment.TickCount : randomSeed;

            var generator = new BoardGenerator(actualSeed);
            var generatedCells = generator.Generate(_columns, safeRows, safeStartingPairs);
            _boardState = new BoardState(generatedCells, _columns, new BoardMatchRules());

            EnsureEventSystem();

            var canvasTransform = CreateCanvas();
            _boardView = BoardView.Create(canvasTransform, _columns);
            _boardView.TileClicked += OnTileClicked;
            _boardView.SetCells(_boardState.Cells);

            Debug.Log(
                $"GameplayController: Generated a scrollable board with {_boardState.Cells.Count} cells, " +
                $"{safeRows} rows, {_columns} columns, {safeStartingPairs} starting pairs and seed {actualSeed}.");
            Debug.Log(
                "GameplayController: Click one tile to select it. Click another tile with the same value or a " +
                "total of 10 if there is a clear flat, vertical, or diagonal path through cleared cells.");
        }

        private void OnDestroy()
        {
            if (_boardView != null)
            {
                _boardView.TileClicked -= OnTileClicked;
            }
        }

        private void OnTileClicked(int index)
        {
            if (_boardState == null || !_boardState.IsSelectable(index))
            {
                return;
            }

            if (_selectedCellIndex == index)
            {
                _boardState.SetSelected(index, false);
                _boardView.RefreshCell(_boardState.Cells[index]);
                _selectedCellIndex = null;
                Debug.Log($"GameplayController: Deselected {DescribeCell(_boardState.Cells[index])}.");
                return;
            }

            if (_selectedCellIndex.HasValue)
            {
                int firstIndex = _selectedCellIndex.Value;
                BoardCell firstCell = _boardState.Cells[firstIndex];
                BoardCell secondCell = _boardState.Cells[index];

                if (_boardState.TryMatchPair(firstIndex, index, out string failureReason))
                {
                    _selectedCellIndex = null;
                    _boardView.RefreshCell(_boardState.Cells[firstIndex]);
                    _boardView.RefreshCell(_boardState.Cells[index]);

                    Debug.Log(
                        $"GameplayController: Matched {DescribeCell(firstCell)} with {DescribeCell(secondCell)}.");
                    return;
                }

                _boardState.SetSelected(firstIndex, false);
                _boardState.SetSelected(index, true);
                _selectedCellIndex = index;

                _boardView.RefreshCell(_boardState.Cells[firstIndex]);
                _boardView.RefreshCell(_boardState.Cells[index]);

                Debug.Log(
                    $"GameplayController: Invalid pair {DescribeCell(firstCell)} -> {DescribeCell(secondCell)}. " +
                    $"{failureReason}");
                return;
            }

            _boardState.SetSelected(index, true);
            _boardView.RefreshCell(_boardState.Cells[index]);
            _selectedCellIndex = index;
            Debug.Log($"GameplayController: Selected {DescribeCell(_boardState.Cells[index])}.");
        }

        private string DescribeCell(BoardCell cell)
        {
            return $"cell {cell.Index} (row {cell.Row + 1}, column {cell.Column + 1}, number {cell.Number})";
        }

        private Transform CreateCanvas()
        {
            var canvasObject = new GameObject(
                "GameplayCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvasObject.transform;
        }

        private void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemObject = new GameObject(
                "GameplayEventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule));

            eventSystemObject.transform.SetParent(transform, false);

            var inputModule = eventSystemObject.GetComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }
    }
}
