using System;
using System.Collections.Generic;
using Game.Gameplay.Board;
using Game.Gameplay.Score;
using Game.Gameplay.Stage;
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
        private BoardMatchRules _boardMatchRules;
        private BoardPairFinder _boardPairFinder;
        private ScoreService _scoreService;
        private StageState _stageState;
        private System.Random _runtimeRandom;
        private BoardMatchInfo _hintedPair;
        private int? _selectedCellIndex;
        private int _columns;
        private int _remainingAdditions;
        private int _additionsPerBoardClear;

        public void Initialize(
            int columns,
            int initialRows,
            int startingPairs,
            int randomSeed,
            int startingAdditions,
            int additionsPerBoardClear)
        {
            _columns = Mathf.Max(1, columns);
            int safeRows = Mathf.Max(1, initialRows);
            int maxStartingPairs = (_columns * safeRows) / 2;
            int safeStartingPairs = Mathf.Clamp(startingPairs, 0, maxStartingPairs);
            int actualSeed = randomSeed == 0 ? Environment.TickCount : randomSeed;

            _boardMatchRules = new BoardMatchRules();
            _boardPairFinder = new BoardPairFinder(_boardMatchRules);
            _runtimeRandom = new System.Random(actualSeed ^ 0x3F2A5C7);

            var generator = new BoardGenerator(actualSeed);
            List<BoardCell> generatedCells = generator.Generate(_columns, safeRows, safeStartingPairs);
            _boardState = new BoardState(generatedCells, _columns, _boardMatchRules);
            _scoreService = new ScoreService();
            _stageState = new StageState();
            _remainingAdditions = Mathf.Max(0, startingAdditions);
            _additionsPerBoardClear = Mathf.Max(0, additionsPerBoardClear);

            EnsureEventSystem();

            Transform canvasTransform = CreateCanvas();
            _boardView = BoardView.Create(canvasTransform, _columns);
            _boardView.TileClicked += OnTileClicked;
            _boardView.PlusClicked += OnPlusClicked;
            _boardView.HintClicked += OnHintClicked;
            _boardView.SetCells(_boardState.Cells);
            _boardView.ScrollToTop();
            _boardView.SetScore(_scoreService.TotalScore);
            _boardView.SetAdditions(_remainingAdditions);
            _boardView.SetPlusButtonInteractable(_remainingAdditions > 0);
            _boardView.SetHintButtonLocked(false);

            Debug.Log(
                $"GameplayController: Generated a scrollable board with {_boardState.Cells.Count} cells, " +
                $"{safeRows} rows, {_columns} columns, {safeStartingPairs} starting pairs and seed {actualSeed}.");
            Debug.Log(
                $"GameplayController: Stage {_stageState.Stage}, score {_scoreService.TotalScore}, " +
                $"future multiplier x{_stageState.Multiplier}, additions {_remainingAdditions}.");
            Debug.Log(
                "GameplayController: Click tiles to match them, tap '+' to duplicate active numbers, " +
                "and tap Hint to highlight a random valid pair.");
        }

        private void OnDestroy()
        {
            if (_boardView != null)
            {
                _boardView.TileClicked -= OnTileClicked;
                _boardView.PlusClicked -= OnPlusClicked;
                _boardView.HintClicked -= OnHintClicked;
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
                BoardMatchResolution resolution = _boardState.TryMatchPair(firstIndex, index);

                if (resolution.Success)
                {
                    HandleSuccessfulMatch(resolution, firstCell, secondCell);
                    return;
                }

                _boardState.SetSelected(firstIndex, false);
                _boardState.SetSelected(index, true);
                _selectedCellIndex = index;

                _boardView.RefreshCell(_boardState.Cells[firstIndex]);
                _boardView.RefreshCell(_boardState.Cells[index]);

                Debug.Log(
                    $"GameplayController: Invalid pair {DescribeCell(firstCell)} -> {DescribeCell(secondCell)}. " +
                    $"{resolution.FailureReason}");
                return;
            }

            _boardState.SetSelected(index, true);
            _boardView.RefreshCell(_boardState.Cells[index]);
            _selectedCellIndex = index;
            Debug.Log($"GameplayController: Selected {DescribeCell(_boardState.Cells[index])}.");
        }

        private void OnPlusClicked()
        {
            if (_boardState == null || _remainingAdditions <= 0)
            {
                return;
            }

            ClearCurrentSelection();

            int addedCount = _boardState.DuplicateUnmatchedNumbers();
            if (addedCount <= 0)
            {
                _boardView.ShowTooltip("Board is empty. '+' needs active numbers to duplicate.");
                Debug.Log("GameplayController: '+' pressed, but there were no active numbers to duplicate.");
                return;
            }

            _remainingAdditions--;
            UpdateAdditionsUi();
            _boardView.SetCells(_boardState.Cells);
            _boardView.ScrollToBottom();
            _boardView.ShowTooltip($"Added {addedCount} numbers.");

            Debug.Log(
                $"GameplayController: '+' duplicated {addedCount} active numbers. " +
                $"{_remainingAdditions} additions remaining.");
        }

        private void OnHintClicked()
        {
            if (_boardState == null || _boardView == null)
            {
                return;
            }

            ClearCurrentSelection();

            if (_hintedPair != null)
            {
                _boardView.ReplayHintFeedback();
                return;
            }

            List<BoardMatchInfo> pairs = _boardPairFinder.FindAll(_boardState.Cells, _columns);
            if (pairs.Count == 0)
            {
                _boardView.ShowTooltip("No more pairs. Please tap '+' button to add numbers.");
                Debug.Log("GameplayController: Hint requested, but the board has no valid pairs.");
                return;
            }

            _hintedPair = pairs[_runtimeRandom.Next(pairs.Count)];
            _boardView.ShowHint(_hintedPair);
            _boardView.SetHintButtonLocked(true);
            _boardView.ShowTooltip("Hint highlighted.");

            Debug.Log(
                $"GameplayController: Hint highlighted {DescribePair(_hintedPair.FirstIndex, _hintedPair.SecondIndex)}.");
        }

        private void HandleSuccessfulMatch(BoardMatchResolution resolution, BoardCell firstCell, BoardCell secondCell)
        {
            _selectedCellIndex = null;
            ClearHint();

            if (resolution.NewlyClearedRowCount > 0)
            {
                _boardView.SetCells(_boardState.Cells);
            }
            else
            {
                if (resolution.FirstIndex >= 0 && resolution.FirstIndex < _boardState.Cells.Count)
                {
                    _boardView.RefreshCell(_boardState.Cells[resolution.FirstIndex]);
                }

                if (resolution.SecondIndex >= 0 && resolution.SecondIndex < _boardState.Cells.Count)
                {
                    _boardView.RefreshCell(_boardState.Cells[resolution.SecondIndex]);
                }
            }

            ScoreResult scoreResult = _scoreService.ApplyMatch(resolution, _stageState.Multiplier);
            _boardView.SetScore(scoreResult.TotalScore);

            Debug.Log(
                $"GameplayController: Matched {DescribeCell(firstCell)} with {DescribeCell(secondCell)} " +
                $"as a {DescribeMatch(resolution.MatchInfo)}. +{scoreResult.AwardedScore} points " +
                $"(pair {scoreResult.PairScore}, row bonus {scoreResult.RowClearBonus}, " +
                $"board bonus {scoreResult.BoardClearBonus}) x{scoreResult.Multiplier}. " +
                $"Total score {scoreResult.TotalScore}.");

            if (resolution.BoardCleared)
            {
                _stageState.AdvanceAfterBoardClear();
                _remainingAdditions += _additionsPerBoardClear;
                UpdateAdditionsUi();
                _boardView.ShowTooltip($"Board cleared. Stage {_stageState.Stage}. +{_additionsPerBoardClear} additions.");

                Debug.Log(
                    $"GameplayController: Board cleared. Stage is now {_stageState.Stage}. " +
                    $"{_additionsPerBoardClear} additions awarded. Future matches use multiplier x{_stageState.Multiplier}.");
                return;
            }

            if (resolution.NewlyClearedRowCount > 0)
            {
                _boardView.ShowTooltip($"Removed {resolution.NewlyClearedRowCount} row(s).");
                Debug.Log(
                    $"GameplayController: Removed {resolution.NewlyClearedRowCount} matched row(s). " +
                    $"Rows below shifted up.");
            }
        }

        private void ClearCurrentSelection()
        {
            if (!_selectedCellIndex.HasValue || _boardState == null)
            {
                return;
            }

            int selectedIndex = _selectedCellIndex.Value;
            _boardState.SetSelected(selectedIndex, false);

            if (selectedIndex >= 0 && selectedIndex < _boardState.Cells.Count)
            {
                _boardView.RefreshCell(_boardState.Cells[selectedIndex]);
            }

            _selectedCellIndex = null;
        }

        private void ClearHint()
        {
            _hintedPair = null;

            if (_boardView == null)
            {
                return;
            }

            _boardView.ClearHint();
            _boardView.SetHintButtonLocked(false);
        }

        private void UpdateAdditionsUi()
        {
            if (_boardView == null)
            {
                return;
            }

            _boardView.SetAdditions(_remainingAdditions);
            _boardView.SetPlusButtonInteractable(_remainingAdditions > 0);
        }

        private string DescribeCell(BoardCell cell)
        {
            return $"cell {cell.Index} (row {cell.Row + 1}, column {cell.Column + 1}, number {cell.Number})";
        }

        private string DescribePair(int firstIndex, int secondIndex)
        {
            BoardCell firstCell = _boardState.Cells[firstIndex];
            BoardCell secondCell = _boardState.Cells[secondIndex];
            return $"{DescribeCell(firstCell)} and {DescribeCell(secondCell)}";
        }

        private string DescribeMatch(BoardMatchInfo matchInfo)
        {
            string position = matchInfo.PositionType switch
            {
                BoardPositionType.RowBoundary => "row-boundary",
                BoardPositionType.Horizontal => "horizontal",
                BoardPositionType.Vertical => "vertical",
                _ => "diagonal",
            };

            string value = matchInfo.ValueType == BoardValueType.SameValue
                ? "same-value"
                : "sum-to-10";

            string distance = matchInfo.IsAdjacent ? "adjacent" : "gap";
            return $"{distance} {position} {value} pair";
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
