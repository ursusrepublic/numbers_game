namespace Game.Gameplay.Board
{
    public enum BoardPositionType
    {
        RowBoundary,
        Horizontal,
        Vertical,
        Diagonal,
    }

    public enum BoardValueType
    {
        SameValue,
        SumToTen,
    }

    public sealed class BoardMatchInfo
    {
        public BoardMatchInfo(
            int firstIndex,
            int secondIndex,
            BoardPositionType positionType,
            BoardValueType valueType,
            bool isAdjacent)
        {
            FirstIndex = firstIndex;
            SecondIndex = secondIndex;
            PositionType = positionType;
            ValueType = valueType;
            IsAdjacent = isAdjacent;
        }

        public int FirstIndex { get; }

        public int SecondIndex { get; }

        public BoardPositionType PositionType { get; }

        public BoardValueType ValueType { get; }

        public bool IsAdjacent { get; }
    }
}
