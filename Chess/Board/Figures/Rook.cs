namespace Chess.Board.Figures
{
    internal class Rook : Figure
    {
        public Rook(FigurePosition position, FigureColors color)
            : base(position, color)
        {
        }

        public override bool CanMove(FigurePosition to, BoardState boardState, bool afterMove = false)
        {
            if (to.Y == Position.Y)
            {
                if (to.X > Position.X) /* Right */
                {
                    for (var i = 1; i < to.X - Position.X; i++)
                        if (boardState.FindFigureNumber(Position + new Vector(i, 0), false).HasValue)
                            return false;
                    return true;
                }
                else /* Left */
                {
                    for (var i = -1; i > to.X - Position.X; i--)
                        if (boardState.FindFigureNumber(Position + new Vector(i, 0), false).HasValue)
                            return false;
                    return true;
                }
            }
            if (to.X == Position.X)
            {
                if (to.Y > Position.Y) /* Up */
                {
                    for (var i = 1; i < to.Y - Position.Y; i++)
                        if (boardState.FindFigureNumber(Position + new Vector(0, i), false).HasValue)
                            return false;
                    return true;
                }
                else /* Down */
                {
                    for (var i = -1; i > to.Y - Position.Y; i--)
                        if (boardState.FindFigureNumber(Position + new Vector(0, i), false).HasValue)
                            return false;
                    return true;
                }
            }
            return false;
        }

        public override bool CanAttack(FigurePosition to, BoardState boardState, bool afterMove = true)
        {
            return CanMove(to, boardState, afterMove);
        }
    }
}