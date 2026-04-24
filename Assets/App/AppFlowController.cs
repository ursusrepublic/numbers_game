using Game.App.Daily;
using Game.App.Save;
using Game.Core;
using Game.Gameplay.Core;
using Game.UI.AppTabs;
using Game.UI.Daily;
using Game.UI.Layout;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Game.App
{
    [DisallowMultipleComponent]
    public sealed class AppFlowController : MonoBehaviour
    {
        private AppMode _appMode;
        private int _boardColumns;
        private int _initialRows;
        private int _startingPairs;
        private int _randomSeed;
        private int _startingAdditions;
        private TMP_FontAsset _regularFont;
        private TMP_FontAsset _boldFont;
        private bool _showSafeAreaDebugOverlay;
        private Texture2D _plusIconTexture;
        private Texture2D _hintIconTexture;
        private Texture2D _mainTabIconTexture;
        private Texture2D _dailyTabIconTexture;
        private Texture2D _journeyTabIconTexture;
        private Texture2D _meTabIconTexture;

        private LocalSaveService _saveService;
        private AppSaveData _appSaveData;
        private DailyChallengesService _dailyChallengesService;
        private AppTabsView _appTabsView;
        private Transform _appTabsCanvasTransform;
        private GameplayController _gameplayController;
        private Transform _dailyResultCanvasTransform;
        private DailyChallengeResultView _dailyChallengeResultView;
        private bool _hasUnfinishedRun;
        private bool _isDailyGameplayRun;
        private DailyChallengeDateKey _currentDailyDate;

        public void Initialize(
            AppMode appMode,
            int boardColumns,
            int initialRows,
            int startingPairs,
            int randomSeed,
            int startingAdditions,
            TMP_FontAsset regularFont,
            TMP_FontAsset boldFont,
            bool showSafeAreaDebugOverlay,
            Texture2D plusIconTexture,
            Texture2D hintIconTexture,
            Texture2D mainTabIconTexture,
            Texture2D dailyTabIconTexture,
            Texture2D journeyTabIconTexture,
            Texture2D meTabIconTexture)
        {
            _appMode = appMode;
            _boardColumns = boardColumns;
            _initialRows = initialRows;
            _startingPairs = startingPairs;
            _randomSeed = randomSeed;
            _startingAdditions = startingAdditions;
            _regularFont = regularFont;
            _boldFont = boldFont;
            _showSafeAreaDebugOverlay = showSafeAreaDebugOverlay;
            _plusIconTexture = plusIconTexture;
            _hintIconTexture = hintIconTexture;
            _mainTabIconTexture = mainTabIconTexture;
            _dailyTabIconTexture = dailyTabIconTexture;
            _journeyTabIconTexture = journeyTabIconTexture;
            _meTabIconTexture = meTabIconTexture;

            ApplyPortraitOrientation();
            EnsureEventSystem();

            _saveService = new LocalSaveService();
            _appSaveData = _saveService.Load() ?? new AppSaveData();
            _appSaveData.DailyChallenges ??= new DailyChallengesSaveData();
            _dailyChallengesService = new DailyChallengesService(_appSaveData.DailyChallenges);
            _appSaveData.DailyChallenges = _dailyChallengesService.SaveData;

            if (!HasValidActiveRun())
            {
                _appSaveData.ActiveRun = null;
            }

            SanitizeDailyActiveRuns();
            _saveService.Save(_appSaveData);

            ShowAppTabs(AppTabId.Main);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveCurrentRunIfNeeded();
            }
        }

        private void OnApplicationQuit()
        {
            SaveCurrentRunIfNeeded();
        }

        private void ShowAppTabs(AppTabId selectedTab)
        {
            DestroyGameplay();
            DestroyDailyResult();
            _isDailyGameplayRun = false;

            if (_appTabsView == null)
            {
                _appTabsCanvasTransform = CreateCanvas("AppTabsCanvas");
                _appTabsView = AppTabsView.Create(
                    _appTabsCanvasTransform,
                    _regularFont,
                    _boldFont,
                    _appMode == AppMode.Developer && _showSafeAreaDebugOverlay,
                    _mainTabIconTexture,
                    _dailyTabIconTexture,
                    _journeyTabIconTexture,
                    _meTabIconTexture);
                _appTabsView.ContinueClicked += HandleContinueClicked;
                _appTabsView.NewGameClicked += HandleNewGameClicked;
                _appTabsView.DailyPreviousMonthClicked += HandleDailyPreviousMonthClicked;
                _appTabsView.DailyNextMonthClicked += HandleDailyNextMonthClicked;
                _appTabsView.DailyDateSelected += HandleDailyDateSelected;
                _appTabsView.DailyPlayClicked += HandleDailyPlayClicked;
            }

            RefreshAppTabsState();
            _appTabsView.SelectTab(selectedTab);
        }

        private void ShowDailyResult(DailyChallengeDateKey date, int progressScore, int goalScore, bool isCompleted)
        {
            DestroyAppTabs();
            DestroyGameplay();
            DestroyDailyResult();
            _isDailyGameplayRun = false;

            _dailyResultCanvasTransform = CreateCanvas("DailyResultCanvas");
            _dailyChallengeResultView = DailyChallengeResultView.Create(
                _dailyResultCanvasTransform,
                _regularFont,
                _boldFont,
                _appMode == AppMode.Developer && _showSafeAreaDebugOverlay);
            _dailyChallengeResultView.ContinueClicked += HandleDailyResultContinueClicked;
            _dailyChallengeResultView.SetResult(date, progressScore, goalScore, isCompleted);
        }

        private void StartNewGame()
        {
            _appSaveData.ActiveRun = null;
            CreateGameplayController();
            _gameplayController.InitializeNewRun(
                _appMode,
                _boardColumns,
                _initialRows,
                _startingPairs,
                _randomSeed,
                _startingAdditions,
                _regularFont,
                _boldFont,
                _appMode == AppMode.Developer && _showSafeAreaDebugOverlay,
                _plusIconTexture,
                _hintIconTexture,
                _appSaveData.BestScore);

            _isDailyGameplayRun = false;
            _hasUnfinishedRun = true;
            _appSaveData.ActiveRun = _gameplayController.CreateRunSaveData();
            _saveService.Save(_appSaveData);
        }

        private void ContinueGame()
        {
            if (!HasValidActiveRun())
            {
                _appSaveData.ActiveRun = null;
                _saveService.Save(_appSaveData);
                ShowAppTabs(AppTabId.Main);
                return;
            }

            CreateGameplayController();
            _gameplayController.InitializeFromSave(
                _appMode,
                _appSaveData.ActiveRun,
                _regularFont,
                _boldFont,
                _appMode == AppMode.Developer && _showSafeAreaDebugOverlay,
                _plusIconTexture,
                _hintIconTexture,
                _appSaveData.BestScore);

            _isDailyGameplayRun = false;
            _hasUnfinishedRun = true;
        }

        private void StartNewDailyRun(DailyChallengeDateKey date)
        {
            CreateGameplayController();

            var dailyConfig = new DailyChallengeSessionConfig
            {
                Date = date,
                GoalScore = _dailyChallengesService.GetGoalScore(date),
                AccumulatedScoreBeforeRun = _dailyChallengesService.GetAccumulatedScore(date),
            };

            _gameplayController.InitializeNewRun(
                _appMode,
                _boardColumns,
                _initialRows,
                _startingPairs,
                _randomSeed,
                _startingAdditions,
                _regularFont,
                _boldFont,
                _appMode == AppMode.Developer && _showSafeAreaDebugOverlay,
                _plusIconTexture,
                _hintIconTexture,
                _appSaveData.BestScore,
                dailyConfig);

            _currentDailyDate = date;
            _isDailyGameplayRun = true;
            _hasUnfinishedRun = true;
            _dailyChallengesService.SetActiveRun(date, _gameplayController.CreateRunSaveData());
            _saveService.Save(_appSaveData);
        }

        private void ContinueDailyRun(DailyChallengeDateKey date)
        {
            RunSaveData dailyRun = _dailyChallengesService.GetActiveRun(date);
            if (!HasValidRun(dailyRun))
            {
                _dailyChallengesService.ClearActiveRun(date);
                _saveService.Save(_appSaveData);
                StartNewDailyRun(date);
                return;
            }

            CreateGameplayController();

            var dailyConfig = new DailyChallengeSessionConfig
            {
                Date = date,
                GoalScore = _dailyChallengesService.GetGoalScore(date),
                AccumulatedScoreBeforeRun = _dailyChallengesService.GetAccumulatedScore(date),
            };

            _gameplayController.InitializeFromSave(
                _appMode,
                dailyRun,
                _regularFont,
                _boldFont,
                _appMode == AppMode.Developer && _showSafeAreaDebugOverlay,
                _plusIconTexture,
                _hintIconTexture,
                _appSaveData.BestScore,
                dailyConfig);

            _currentDailyDate = date;
            _isDailyGameplayRun = true;
            _hasUnfinishedRun = true;
        }

        private void CreateGameplayController()
        {
            DestroyAppTabs();
            DestroyDailyResult();
            DestroyGameplay();

            var gameplayRoot = new GameObject("GameplayRoot");
            gameplayRoot.transform.SetParent(transform, false);

            _gameplayController = gameplayRoot.AddComponent<GameplayController>();
            _gameplayController.BackToLobbyRequested += HandleBackToLobbyRequested;
            _gameplayController.RunStateChanged += HandleRunStateChanged;
            _gameplayController.RunCompleted += HandleRunCompleted;
            _gameplayController.RestartRequested += HandleRestartRequested;
            _gameplayController.RewardedHintsRequested += HandleRewardedHintsRequested;
        }

        private void DestroyGameplay()
        {
            if (_gameplayController == null)
            {
                return;
            }

            _gameplayController.BackToLobbyRequested -= HandleBackToLobbyRequested;
            _gameplayController.RunStateChanged -= HandleRunStateChanged;
            _gameplayController.RunCompleted -= HandleRunCompleted;
            _gameplayController.RestartRequested -= HandleRestartRequested;
            _gameplayController.RewardedHintsRequested -= HandleRewardedHintsRequested;

            _gameplayController.gameObject.SetActive(false);
            Destroy(_gameplayController.gameObject);
            _gameplayController = null;
        }

        private void DestroyAppTabs()
        {
            if (_appTabsView != null)
            {
                _appTabsView.ContinueClicked -= HandleContinueClicked;
                _appTabsView.NewGameClicked -= HandleNewGameClicked;
                _appTabsView.DailyPreviousMonthClicked -= HandleDailyPreviousMonthClicked;
                _appTabsView.DailyNextMonthClicked -= HandleDailyNextMonthClicked;
                _appTabsView.DailyDateSelected -= HandleDailyDateSelected;
                _appTabsView.DailyPlayClicked -= HandleDailyPlayClicked;
            }

            if (_appTabsCanvasTransform != null)
            {
                _appTabsCanvasTransform.gameObject.SetActive(false);
                Destroy(_appTabsCanvasTransform.gameObject);
                _appTabsCanvasTransform = null;
            }

            _appTabsView = null;
        }

        private void DestroyDailyResult()
        {
            if (_dailyChallengeResultView != null)
            {
                _dailyChallengeResultView.ContinueClicked -= HandleDailyResultContinueClicked;
            }

            if (_dailyResultCanvasTransform != null)
            {
                _dailyResultCanvasTransform.gameObject.SetActive(false);
                Destroy(_dailyResultCanvasTransform.gameObject);
                _dailyResultCanvasTransform = null;
            }

            _dailyChallengeResultView = null;
        }

        private void RefreshAppTabsState()
        {
            if (_appTabsView == null)
            {
                return;
            }

            _appTabsView.SetBestScore(_appSaveData.BestScore);
            _appTabsView.SetContinueVisible(HasValidActiveRun());
            _appTabsView.SetDailyMonthState(_dailyChallengesService.BuildMonthState(System.DateTime.Today));
        }

        private void HandleContinueClicked()
        {
            ContinueGame();
        }

        private void HandleNewGameClicked()
        {
            StartNewGame();
        }

        private void HandleDailyPreviousMonthClicked()
        {
            System.DateTime viewedMonth = _dailyChallengesService.GetViewedMonth();
            _dailyChallengesService.SetViewedMonth(viewedMonth.AddMonths(-1), System.DateTime.Today);
            _saveService.Save(_appSaveData);
            RefreshAppTabsState();
            _appTabsView?.SelectTab(AppTabId.Daily);
        }

        private void HandleDailyNextMonthClicked()
        {
            System.DateTime viewedMonth = _dailyChallengesService.GetViewedMonth();
            _dailyChallengesService.SetViewedMonth(viewedMonth.AddMonths(1), System.DateTime.Today);
            _saveService.Save(_appSaveData);
            RefreshAppTabsState();
            _appTabsView?.SelectTab(AppTabId.Daily);
        }

        private void HandleDailyDateSelected(DailyChallengeDateKey date)
        {
            if (!_dailyChallengesService.IsSelectable(date, System.DateTime.Today))
            {
                return;
            }

            _dailyChallengesService.SetSelectedDate(date);
            _saveService.Save(_appSaveData);
            RefreshAppTabsState();
            _appTabsView?.SelectTab(AppTabId.Daily);
        }

        private void HandleDailyPlayClicked(DailyChallengeDateKey date)
        {
            if (!_dailyChallengesService.IsSelectable(date, System.DateTime.Today))
            {
                return;
            }

            if (_dailyChallengesService.IsCompleted(date))
            {
                return;
            }

            _dailyChallengesService.SetSelectedDate(date);
            _saveService.Save(_appSaveData);

            if (_dailyChallengesService.HasActiveRun(date))
            {
                ContinueDailyRun(date);
                return;
            }

            StartNewDailyRun(date);
        }

        private void HandleDailyResultContinueClicked()
        {
            ShowAppTabs(AppTabId.Daily);
        }

        private void HandleBackToLobbyRequested()
        {
            AppTabId targetTab = _isDailyGameplayRun ? AppTabId.Daily : AppTabId.Main;
            SaveCurrentRunIfNeeded();
            ShowAppTabs(targetTab);
        }

        private void HandleRunStateChanged()
        {
            if (_gameplayController == null)
            {
                return;
            }

            if (_isDailyGameplayRun)
            {
                _dailyChallengesService.SetActiveRun(_currentDailyDate, _gameplayController.CreateRunSaveData());
            }
            else
            {
                _appSaveData.ActiveRun = _gameplayController.CreateRunSaveData();
            }

            _saveService.Save(_appSaveData);
        }

        private void HandleRunCompleted(int finalScore)
        {
            _hasUnfinishedRun = false;

            if (finalScore > _appSaveData.BestScore)
            {
                _appSaveData.BestScore = finalScore;
            }

            if (_isDailyGameplayRun)
            {
                DailyChallengeDateKey completedDate = _currentDailyDate;
                _dailyChallengesService.ApplyCompletedRun(completedDate, finalScore);
                int goalScore = _dailyChallengesService.GetGoalScore(completedDate);
                int progressScore = _dailyChallengesService.GetDisplayedProgress(completedDate);
                bool isCompleted = _dailyChallengesService.IsCompleted(completedDate);
                _saveService.Save(_appSaveData);

                ShowDailyResult(completedDate, progressScore, goalScore, isCompleted);
                return;
            }

            _appSaveData.ActiveRun = null;
            _saveService.Save(_appSaveData);
            _gameplayController?.SetBestScore(_appSaveData.BestScore);
        }

        private void HandleRestartRequested()
        {
            StartNewGame();
        }

        private void HandleRewardedHintsRequested()
        {
            // Future rewarded-ad integration point: show the ad here, then call AddRewardedAdHints().
            Debug.Log(
                "AppFlowController: Rewarded hints requested. " +
                "Show a rewarded ad here, then call GameplayController.AddRewardedAdHints().");
        }

        private void SaveCurrentRunIfNeeded()
        {
            if (_gameplayController != null && _hasUnfinishedRun)
            {
                RunSaveData runSave = _gameplayController.CreateRunSaveData();
                if (_isDailyGameplayRun)
                {
                    _dailyChallengesService.SetActiveRun(_currentDailyDate, runSave);
                }
                else
                {
                    _appSaveData.ActiveRun = runSave;
                }
            }

            _saveService?.Save(_appSaveData);
        }

        private bool HasValidActiveRun()
        {
            return HasValidRun(_appSaveData?.ActiveRun);
        }

        private static bool HasValidRun(RunSaveData run)
        {
            return run != null &&
                   run.Columns > 0 &&
                   run.Cells != null &&
                   run.Cells.Length > 0;
        }

        private void SanitizeDailyActiveRuns()
        {
            DailyChallengeDaySaveData[] days = _appSaveData?.DailyChallenges?.Days;
            if (days == null)
            {
                return;
            }

            for (int index = 0; index < days.Length; index++)
            {
                DailyChallengeDaySaveData day = days[index];
                if (day == null)
                {
                    continue;
                }

                if (day.IsCompleted)
                {
                    day.ActiveRun = null;
                    continue;
                }

                if (day.ActiveRun != null && !HasValidRun(day.ActiveRun))
                {
                    day.ActiveRun = null;
                }
            }
        }

        private Transform CreateCanvas(string name)
        {
            var canvasObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            MobileLayout.ConfigureCanvasScaler(scaler);

            return canvasObject.transform;
        }

        private void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemObject = new GameObject(
                "AppEventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule));

            eventSystemObject.transform.SetParent(transform, false);

            var inputModule = eventSystemObject.GetComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }

        private static void ApplyPortraitOrientation()
        {
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.Portrait;
        }
    }
}
