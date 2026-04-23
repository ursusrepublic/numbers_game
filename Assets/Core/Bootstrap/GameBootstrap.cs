using Game.App;
using Game.Core;
using TMPro;
using UnityEngine;

namespace Game.Core.Bootstrap
{
    [DisallowMultipleComponent]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("App")]
        [SerializeField] private AppMode _appMode = AppMode.Player;
        [SerializeField] private bool _showSafeAreaDebugOverlay;

        [Header("Board Settings")]
        [SerializeField] [Min(1)] private int _boardColumns = 9;
        [SerializeField] [Min(1)] private int _initialRows = 3;
        [SerializeField] [Min(0)] private int _startingPairs = 8;
        [SerializeField] private int _randomSeed;

        [Header("Additions Settings")]
        [SerializeField] [Min(0)] private int _startingAdditions = 5;

        [Header("Fonts")]
        [SerializeField] private TMP_FontAsset _regularFont;
        [SerializeField] private TMP_FontAsset _boldFont;

        [Header("Icons")]
        [SerializeField] private Texture2D _plusIconTexture;
        [SerializeField] private Texture2D _hintIconTexture;
        [SerializeField] private Texture2D _mainTabIconTexture;
        [SerializeField] private Texture2D _dailyTabIconTexture;
        [SerializeField] private Texture2D _journeyTabIconTexture;
        [SerializeField] private Texture2D _meTabIconTexture;

        private void Awake()
        {
            if (transform.Find("AppRoot") != null)
            {
                return;
            }

            var appRoot = new GameObject("AppRoot");
            appRoot.transform.SetParent(transform, false);

            var appFlowController = appRoot.AddComponent<AppFlowController>();
            appFlowController.Initialize(
                _appMode,
                _boardColumns,
                _initialRows,
                _startingPairs,
                _randomSeed,
                _startingAdditions,
                _regularFont,
                _boldFont,
                _showSafeAreaDebugOverlay,
                _plusIconTexture,
                _hintIconTexture,
                _mainTabIconTexture,
                _dailyTabIconTexture,
                _journeyTabIconTexture,
                _meTabIconTexture);
        }
    }
}
