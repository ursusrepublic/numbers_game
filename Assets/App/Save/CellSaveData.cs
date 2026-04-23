using System;

namespace Game.App.Save
{
    [Serializable]
    public sealed class CellSaveData
    {
        public int Number;
        public bool IsMatched;
        public bool IsSelected;
    }
}
