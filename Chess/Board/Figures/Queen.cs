namespace Chess.Board.Figures
{
    internal class Queen : Figure
    {
        public Queen(FigurePosition position, FigureColors color)
            : base(position, color)
        {
        }

        public override bool CanMove(FigurePosition to, BoardState boardState, bool afterMove = false)
        {
            return
                new Bishop(Position, Color).CanMove(to, boardState, afterMove) ||
                new Rook(Position, Color).CanMove(to, boardState, afterMove);
        }

        public override bool CanAttack(FigurePosition to, BoardState boardState, bool afterMove = true)
        {
            return CanMove(to, boardState, afterMove);
        }
    }
}