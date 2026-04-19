using Game.Gameplay.Core;
using UnityEngine;

namespace Game.Core.Bootstrap
{
    [DisallowMultipleComponent]
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Board Settings")]
        [SerializeField] [Min(1)] private int _boardColumns = 9;
        [SerializeField] [Min(1)] private int _initialRows = 3;
        [SerializeField] [Min(0)] private int _startingPairs = 8;
        [SerializeField] private int _randomSeed;

        [Header("Additions Settings")]
        [SerializeField] [Min(0)] private int _startingAdditions = 5;

        private void Awake()
        {
            if (transform.Find("GameplayRoot") != null)
            {
                return;
            }

            var gameplayRoot = new GameObject("GameplayRoot");
            gameplayRoot.transform.SetParent(transform, false);

            var gameplayController = gameplayRoot.AddComponent<GameplayController>();
            gameplayController.Initialize(
                _boardColumns,
                _initialRows,
                _startingPairs,
                _randomSeed,
                _startingAdditions);
        }
    }
}
