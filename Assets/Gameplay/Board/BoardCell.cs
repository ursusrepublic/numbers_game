namespace Game.Gameplay.Board
{
    public sealed class BoardCell
    {
        public BoardCell(int index, int row, int column, int number)
        {
            Index = index;
            Row = row;
            Column = column;
            Number = number;
        }

        public int Index { get; }

        public int Row { get; }

        public int Column { get; }

        public int Number { get; }

        public bool IsMatched { get; set; }

        public bool IsSelected { get; set; }
    }
}
