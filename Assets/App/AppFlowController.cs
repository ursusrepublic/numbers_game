using Game.App.Save;
using Game.Core;
using Game.Gameplay.Core;
using Game.UI.AppTabs;
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
        private AppTabsView _appTabsView;
        private Transform _appTabsCanvasTransform;
        private GameplayController _gameplayController;
        private bool _hasUnfinishedRun;

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

            if (!HasValidActiveRun())
            {
                _appSaveData.ActiveRun = null;
                _saveService.Save(_appSaveData);
            }

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
            }

            _appTabsView.SetBestScore(_appSaveData.BestScore);
            _appTabsView.SetContinueVisible(HasValidActiveRun());
            _appTabsView.SelectTab(selectedTab);
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

            _hasUnfinishedRun = true;
        }

        private void CreateGameplayController()
        {
            DestroyAppTabs();
            DestroyGameplay();

            var gameplayRoot = new GameObject("GameplayRoot");
            gameplayRoot.transform.SetParent(transform, false);

            _gameplayController = gameplayRoot.AddComponent<GameplayController>();
            _gameplayController.BackToLobbyRequested += HandleBackToLobbyRequested;
            _gameplayController.RunStateChanged += HandleRunStateChanged;
            _gameplayController.RunCompleted += HandleRunCompleted;
            _gameplayController.RestartRequested += HandleRestartRequested;
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
            }

            if (_appTabsCanvasTransform != null)
            {
                _appTabsCanvasTransform.gameObject.SetActive(false);
                Destroy(_appTabsCanvasTransform.gameObject);
                _appTabsCanvasTransform = null;
            }

            _appTabsView = null;
        }

        private void HandleContinueClicked()
        {
            ContinueGame();
        }

        private void HandleNewGameClicked()
        {
            StartNewGame();
        }

        private void HandleBackToLobbyRequested()
        {
            SaveCurrentRunIfNeeded();
            ShowAppTabs(AppTabId.Main);
        }

        private void HandleRunStateChanged()
        {
            if (_gameplayController == null)
            {
                return;
            }

            _appSaveData.ActiveRun = _gameplayController.CreateRunSaveData();
            _saveService.Save(_appSaveData);
        }

        private void HandleRunCompleted(int finalScore)
        {
            _hasUnfinishedRun = false;
            if (finalScore > _appSaveData.BestScore)
            {
                _appSaveData.BestScore = finalScore;
                _gameplayController?.SetBestScore(_appSaveData.BestScore);
            }

            _appSaveData.ActiveRun = null;
            _saveService.Save(_appSaveData);
        }

        private void HandleRestartRequested()
        {
            StartNewGame();
        }

        private void SaveCurrentRunIfNeeded()
        {
            if (_gameplayController != null && _hasUnfinishedRun)
            {
                _appSaveData.ActiveRun = _gameplayController.CreateRunSaveData();
            }

            _saveService?.Save(_appSaveData);
        }

        private bool HasValidActiveRun()
        {
            RunSaveData run = _appSaveData?.ActiveRun;
            return run != null &&
                   run.Columns > 0 &&
                   run.Cells != null &&
                   run.Cells.Length > 0;
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
