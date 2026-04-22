using System;
using System.Collections.Generic;
using Game.Core;
using Game.Gameplay.Board;
using Game.Gameplay.Dev;
using Game.Gameplay.Score;
using Game.Gameplay.Stage;
using Game.UI.Game;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Gameplay.Core
{
    [DisallowMultipleComponent]
    public sealed class GameplayController : MonoBehaviour
    {
        private BoardState _boardState;
        private GameScreenView _gameScreenView;
        private DevPanelView _devPanelView;
        private BoardMatchRules _boardMatchRules;
        private BoardPairFinder _boardPairFinder;
        private ScoreService _scoreService;
        private StageState _stageState;
        private System.Random _boardSeedRandom;
        private System.Random _runtimeRandom;
        private BoardMatchInfo _hintedPair;
        private AppMode _appMode;
        private int? _selectedCellIndex;
        private int _currentBoardSeed;
        private int _columns;
        private int _initialRows;
        private int _startingPairs;
        private int _startingAdditions;
        private int _remainingAdditions;
        private GameSessionState _sessionState;
        private TMP_FontAsset _regularFont;
        private TMP_FontAsset _boldFont;
        private bool _showSafeAreaDebugOverlay;
        private Texture2D _plusIconTexture;
        private Texture2D _hintIconTexture;

        public void Initialize(
            AppMode appMode,
            int columns,
            int initialRows,
            int startingPairs,
            int randomSeed,
            int startingAdditions,
            TMP_FontAsset regularFont,
            TMP_FontAsset boldFont,
            bool showSafeAreaDebugOverlay,
            Texture2D plusIconTexture,
            Texture2D hintIconTexture)
        {
            _appMode = appMode;
            _columns = Mathf.Max(1, columns);
            _initialRows = Mathf.Max(1, initialRows);
            int maxStartingPairs = (_columns * _initialRows) / 2;
            _startingPairs = Mathf.Clamp(startingPairs, 0, maxStartingPairs);
            int actualSeed = randomSeed == 0 ? Environment.TickCount : randomSeed;

            _boardMatchRules = new BoardMatchRules();
            _boardPairFinder = new BoardPairFinder(_boardMatchRules);
            _boardSeedRandom = new System.Random(actualSeed ^ 0x51F2D4A);
            _runtimeRandom = new System.Random(actualSeed ^ 0x3F2A5C7);
            _boardState = CreateBoardState(actualSeed);
            _scoreService = new ScoreService();
            _stageState = new StageState();
            _startingAdditions = Mathf.Max(0, startingAdditions);
            _remainingAdditions = _startingAdditions;
            _regularFont = regularFont != null ? regularFont : boldFont != null ? boldFont : TMP_Settings.defaultFontAsset;
            _boldFont = boldFont != null ? boldFont : _regularFont;
            _showSafeAreaDebugOverlay = showSafeAreaDebugOverlay;
            _plusIconTexture = plusIconTexture;
            _hintIconTexture = hintIconTexture;

            EnsureEventSystem();

            Transform canvasTransform = CreateCanvas();
            _gameScreenView = GameScreenView.Create(
                canvasTransform,
                _columns,
                _regularFont,
                _boldFont,
                _appMode == AppMode.Developer && _showSafeAreaDebugOverlay,
                _plusIconTexture,
                _hintIconTexture);
            _gameScreenView.TileClicked += OnTileClicked;
            _gameScreenView.PlusClicked += OnPlusClicked;
            _gameScreenView.HintClicked += OnHintClicked;
            _gameScreenView.RestartClicked += OnRestartClicked;
            _gameScreenView.SetCells(_boardState.Cells);
            _gameScreenView.ScrollToTop();
            _gameScreenView.SetScore(_scoreService.TotalScore);
            UpdateAdditionsUi();
            _gameScreenView.SetHintButtonLocked(false);
            _gameScreenView.HideGameOver();

            if (_appMode == AppMode.Developer)
            {
                _devPanelView = DevPanelView.Create(canvasTransform, _regularFont);
                _devPanelView.ShowPairsClicked += OnShowDeveloperPairsClicked;
                _devPanelView.SolveOnePairClicked += OnSolveOnePairClicked;
            }

            Debug.Log(
                $"GameplayController: Generated a scrollable board with {_boardState.Cells.Count} cells, " +
                $"{_initialRows} rows, {_columns} columns, {_startingPairs} starting pairs and seed {actualSeed}. " +
                $"Mode {_appMode}.");
            Debug.Log(
                $"GameplayController: Stage {_stageState.Stage}, score {_scoreService.TotalScore}, " +
                $"future multiplier x{_stageState.Multiplier}, additions {_remainingAdditions}.");
            Debug.Log(
                "GameplayController: Click tiles to match them, tap '+' to duplicate active numbers, " +
                "and tap Hint to highlight a random valid pair.");

            EvaluateSessionState(showNoMovesTooltip: true);
            UpdateDeveloperInfo();
        }

        private void OnDestroy()
        {
            if (_gameScreenView != null)
            {
                _gameScreenView.TileClicked -= OnTileClicked;
                _gameScreenView.PlusClicked -= OnPlusClicked;
                _gameScreenView.HintClicked -= OnHintClicked;
                _gameScreenView.RestartClicked -= OnRestartClicked;
            }

            if (_devPanelView != null)
            {
                _devPanelView.ShowPairsClicked -= OnShowDeveloperPairsClicked;
                _devPanelView.SolveOnePairClicked -= OnSolveOnePairClicked;
            }
        }

        private void OnTileClicked(int index)
        {
            if (_sessionState == GameSessionState.GameOver || _boardState == null || !_boardState.IsSelectable(index))
            {
                return;
            }

            if (_selectedCellIndex == index)
            {
                _boardState.SetSelected(index, false);
                _gameScreenView.RefreshCell(_boardState.Cells[index]);
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

                _gameScreenView.RefreshCell(_boardState.Cells[firstIndex]);
                _gameScreenView.RefreshCell(_boardState.Cells[index]);

                Debug.Log(
                    $"GameplayController: Invalid pair {DescribeCell(firstCell)} -> {DescribeCell(secondCell)}. " +
                    $"{resolution.FailureReason}");
                return;
            }

            _boardState.SetSelected(index, true);
            _gameScreenView.RefreshCell(_boardState.Cells[index]);
            _selectedCellIndex = index;
            Debug.Log($"GameplayController: Selected {DescribeCell(_boardState.Cells[index])}.");
        }

        private void OnPlusClicked()
        {
            if (_sessionState == GameSessionState.GameOver || _boardState == null || !CanUsePlus())
            {
                return;
            }

            ClearCurrentSelection();
            ClearDeveloperPairLines();

            int addedCount = _boardState.DuplicateUnmatchedNumbers();
            if (addedCount <= 0)
            {
                _gameScreenView.ShowTooltip("Board is empty. '+' needs active numbers to duplicate.");
                Debug.Log("GameplayController: '+' pressed, but there were no active numbers to duplicate.");
                return;
            }

            _remainingAdditions--;
            UpdateAdditionsUi();
            _gameScreenView.SetCells(_boardState.Cells);
            _gameScreenView.ScrollToBottom();
            _gameScreenView.ShowTooltip($"Added {addedCount} numbers.");

            Debug.Log(
                $"GameplayController: '+' duplicated {addedCount} active numbers. " +
                $"{_remainingAdditions} additions remaining.");

            EvaluateSessionState(showNoMovesTooltip: true);
            UpdateDeveloperInfo();
        }

        private void OnHintClicked()
        {
            if (_sessionState == GameSessionState.GameOver || _boardState == null || _gameScreenView == null)
            {
                return;
            }

            ClearCurrentSelection();

            if (_hintedPair != null)
            {
                _gameScreenView.ReplayHintFeedback();
                return;
            }

            List<BoardMatchInfo> pairs = _boardPairFinder.FindAll(_boardState.Cells, _columns);
            if (pairs.Count == 0)
            {
                _gameScreenView.ShowTooltip("No more pairs. Please tap '+' button to add numbers.");
                Debug.Log("GameplayController: Hint requested, but the board has no valid pairs.");
                return;
            }

            _hintedPair = pairs[_runtimeRandom.Next(pairs.Count)];
            _gameScreenView.ShowHint(_hintedPair);
            _gameScreenView.SetHintButtonLocked(true);
            _gameScreenView.ShowTooltip("Hint highlighted.");

            Debug.Log(
                $"GameplayController: Hint highlighted {DescribePair(_hintedPair.FirstIndex, _hintedPair.SecondIndex)}.");
        }

        private void HandleSuccessfulMatch(BoardMatchResolution resolution, BoardCell firstCell, BoardCell secondCell)
        {
            _selectedCellIndex = null;
            ClearHint();
            ClearDeveloperPairLines();

            if (resolution.NewlyClearedRowCount > 0)
            {
                _gameScreenView.SetCells(_boardState.Cells);
            }
            else
            {
                if (resolution.FirstIndex >= 0 && resolution.FirstIndex < _boardState.Cells.Count)
                {
                    _gameScreenView.RefreshCell(_boardState.Cells[resolution.FirstIndex]);
                }

                if (resolution.SecondIndex >= 0 && resolution.SecondIndex < _boardState.Cells.Count)
                {
                    _gameScreenView.RefreshCell(_boardState.Cells[resolution.SecondIndex]);
                }
            }

            ScoreResult scoreResult = _scoreService.ApplyMatch(resolution, _stageState.Multiplier);
            _gameScreenView.SetScore(scoreResult.TotalScore);

            Debug.Log(
                $"GameplayController: Matched {DescribeCell(firstCell)} with {DescribeCell(secondCell)} " +
                $"as a {DescribeMatch(resolution.MatchInfo)}. +{scoreResult.AwardedScore} points " +
                $"(pair {scoreResult.PairScore}, row bonus {scoreResult.RowClearBonus}, " +
                $"board bonus {scoreResult.BoardClearBonus}) x{scoreResult.Multiplier}. " +
                $"Total score {scoreResult.TotalScore}.");

            if (resolution.BoardCleared)
            {
                _stageState.AdvanceAfterBoardClear();
                _remainingAdditions = _startingAdditions;
                UpdateAdditionsUi();
                StartNextStageBoard();
                _gameScreenView.ShowTooltip($"Board cleared. Stage {_stageState.Stage}. Additions reset to {_startingAdditions}.");

                Debug.Log(
                    $"GameplayController: Board cleared. Stage is now {_stageState.Stage}. " +
                    $"Additions reset to {_startingAdditions}. Future matches use multiplier x{_stageState.Multiplier}.");
            }
            else if (resolution.NewlyClearedRowCount > 0)
            {
                _gameScreenView.ShowTooltip($"Removed {resolution.NewlyClearedRowCount} row(s).");
                Debug.Log(
                    $"GameplayController: Removed {resolution.NewlyClearedRowCount} matched row(s). " +
                    $"Rows below shifted up.");
            }

            EvaluateSessionState(showNoMovesTooltip: !resolution.BoardCleared);
            UpdateDeveloperInfo();
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
                _gameScreenView.RefreshCell(_boardState.Cells[selectedIndex]);
            }

            _selectedCellIndex = null;
        }

        private void ClearHint()
        {
            _hintedPair = null;

            if (_gameScreenView == null)
            {
                return;
            }

            _gameScreenView.ClearHint();
            _gameScreenView.SetHintButtonLocked(false);
        }

        private void UpdateAdditionsUi()
        {
            if (_gameScreenView == null)
            {
                return;
            }

            _gameScreenView.SetAdditions(_remainingAdditions);
            _gameScreenView.SetPlusButtonInteractable(CanUsePlus());
        }

        private BoardState CreateBoardState(int seed)
        {
            _currentBoardSeed = seed;
            var generator = new BoardGenerator(seed);
            List<BoardCell> generatedCells = generator.Generate(_columns, _initialRows, _startingPairs);
            return new BoardState(generatedCells, _columns, _boardMatchRules);
        }

        private void StartNextStageBoard()
        {
            int nextBoardSeed = _boardSeedRandom.Next();
            _boardState = CreateBoardState(nextBoardSeed);
            UpdateAdditionsUi();
            _gameScreenView.SetCells(_boardState.Cells);
            _gameScreenView.ScrollToTop();

            Debug.Log(
                $"GameplayController: Generated stage {_stageState.Stage} board with {_boardState.Cells.Count} cells, " +
                $"{_initialRows} rows, {_columns} columns, {_startingPairs} starting pairs and seed {nextBoardSeed}.");
        }

        private void OnShowDeveloperPairsClicked()
        {
            if (_boardState == null || _gameScreenView == null)
            {
                return;
            }

            List<BoardMatchInfo> pairs = GetValidPairs();
            if (pairs.Count == 0)
            {
                _gameScreenView.ClearDeveloperPairLines();
                _gameScreenView.ShowTooltip("Developer: no valid pairs on the current board.");
                UpdateDeveloperInfo(0);
                return;
            }

            _gameScreenView.ShowDeveloperPairLines(pairs);
            _gameScreenView.ShowTooltip($"Developer: showing {pairs.Count} valid pair(s).");
            UpdateDeveloperInfo(pairs.Count);
        }

        private void OnSolveOnePairClicked()
        {
            if (_sessionState == GameSessionState.GameOver || _boardState == null || _gameScreenView == null)
            {
                return;
            }

            List<BoardMatchInfo> pairs = GetValidPairs();
            if (pairs.Count == 0)
            {
                _gameScreenView.ShowTooltip("Developer: no valid pairs to solve.");
                UpdateDeveloperInfo(0);
                return;
            }

            BoardMatchInfo pairToSolve = pairs[_runtimeRandom.Next(pairs.Count)];
            ClearCurrentSelection();
            ClearHint();
            ClearDeveloperPairLines();

            BoardCell firstCell = _boardState.Cells[pairToSolve.FirstIndex];
            BoardCell secondCell = _boardState.Cells[pairToSolve.SecondIndex];
            BoardMatchResolution resolution = _boardState.TryMatchPair(pairToSolve.FirstIndex, pairToSolve.SecondIndex);
            if (!resolution.Success)
            {
                _gameScreenView.ShowTooltip("Developer: failed to solve the selected pair.");
                UpdateDeveloperInfo();
                return;
            }

            bool showSolvedTooltip = !resolution.BoardCleared && resolution.NewlyClearedRowCount == 0;
            Debug.Log(
                $"GameplayController: Developer solved {DescribeCell(firstCell)} with {DescribeCell(secondCell)}.");
            HandleSuccessfulMatch(resolution, firstCell, secondCell);

            if (showSolvedTooltip)
            {
                _gameScreenView.ShowTooltip("Developer: solved one valid pair.");
            }
        }

        private void EvaluateSessionState(bool showNoMovesTooltip)
        {
            if (_boardState == null || _gameScreenView == null)
            {
                return;
            }

            GameSessionState nextState;
            if (HasValidPairs())
            {
                nextState = GameSessionState.Playing;
            }
            else if (CanUsePlus())
            {
                nextState = GameSessionState.NoMovesAvailable;
            }
            else
            {
                nextState = GameSessionState.GameOver;
            }

            if (nextState == _sessionState)
            {
                return;
            }

            _sessionState = nextState;

            if (_sessionState == GameSessionState.Playing)
            {
                _gameScreenView.HideGameOver();
                return;
            }

            if (_sessionState == GameSessionState.NoMovesAvailable)
            {
                if (showNoMovesTooltip)
                {
                    _gameScreenView.ShowTooltip("No more pairs. Please tap '+' button to add numbers.");
                }

                Debug.Log(
                    $"GameplayController: No moves available. '+' can still continue the run. " +
                    $"{_remainingAdditions} additions remaining.");
                return;
            }

            ClearCurrentSelection();
            ClearHint();
            _gameScreenView.ShowGameOver(_scoreService.TotalScore, _stageState.Stage);

            Debug.Log(
                $"GameplayController: Game Over. Final score {_scoreService.TotalScore}, " +
                $"final stage {_stageState.Stage}.");
        }

        private List<BoardMatchInfo> GetValidPairs()
        {
            return _boardPairFinder.FindAll(_boardState.Cells, _columns);
        }

        private bool HasValidPairs()
        {
            List<BoardMatchInfo> pairs = GetValidPairs();
            return pairs.Count > 0;
        }

        private bool CanUsePlus()
        {
            if (_remainingAdditions <= 0 || _boardState == null)
            {
                return false;
            }

            IReadOnlyList<BoardCell> cells = _boardState.Cells;
            for (int index = 0; index < cells.Count; index++)
            {
                if (!cells[index].IsMatched)
                {
                    return true;
                }
            }

            return false;
        }

        private void ClearDeveloperPairLines()
        {
            _gameScreenView?.ClearDeveloperPairLines();
        }

        private void UpdateDeveloperInfo(int? validPairCount = null)
        {
            if (_appMode != AppMode.Developer || _devPanelView == null || _boardState == null)
            {
                return;
            }

            int pairsCount = validPairCount ?? GetValidPairs().Count;
            _devPanelView.SetInfo(
                $"Mode: {_appMode}\n" +
                $"Seed: {_currentBoardSeed}\n" +
                $"Score: {_scoreService.TotalScore}\n" +
                $"Stage: {_stageState.Stage}\n" +
                $"Additions: {_remainingAdditions}\n" +
                $"Valid Pairs: {pairsCount}");
        }

        private void OnRestartClicked()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null)
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
