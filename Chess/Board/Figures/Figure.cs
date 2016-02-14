namespace Chess.Board.Figures
{
    public abstract class Figure
    {
        public FigurePosition Position { get; private set; }

        public FigureColors Color { get; private set; }

        protected Figure(FigurePosition position, FigureColors color)
        {
            Position = position;
            Color = color;
        }

        public abstract bool CanMove(FigurePosition to, BoardState boardState, bool afterMove = false);

        public abstract bool CanAttack(FigurePosition to, BoardState boardState, bool afterMove = true);

        public string GetModelName()
        {
            return Color + GetType().Name;
        }

        public virtual char GetFenName()
        {
            var firstChar = GetType().Name[0];
            return Color == FigureColors.White ? char.ToUpperInvariant(firstChar) : char.ToLowerInvariant(firstChar);
        }
    }
}