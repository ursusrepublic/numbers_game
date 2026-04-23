using UnityEngine;

namespace Game.Gameplay.Stage
{
    public sealed class StageState
    {
        private int _clearedBoardCount;

        public int ClearedBoardCount => _clearedBoardCount;

        public int Stage => _clearedBoardCount + 1;

        public int Multiplier => Mathf.Clamp(_clearedBoardCount, 1, 8);

        public void AdvanceAfterBoardClear()
        {
            _clearedBoardCount++;
        }

        public void Restore(int clearedBoardCount)
        {
            _clearedBoardCount = Mathf.Max(0, clearedBoardCount);
        }
    }
}
