using UnityEngine;

namespace Game.Core.Bootstrap
{
    [DisallowMultipleComponent]
    public sealed class GameBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            // Stage 0 / Stage 1 foundation only.
            // Later stages can initialize the first runtime objects here.
        }
    }
}
