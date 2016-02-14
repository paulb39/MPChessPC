namespace Chess.Board.Figures
{
    internal class Bishop : Figure
    {
        public Bishop(FigurePosition position, FigureColors color)
            : base(position, color)
        {
        }

        public override bool CanMove(FigurePosition to, BoardState boardState, bool afterMove = false)
        {
            if (to.X - Position.X == to.Y - Position.Y)
            {
                if (to.Y - Position.Y > 0) /* Right-up */
                {
                    for (var i = 1; i < to.Y - Position.Y; i++)
                        if (boardState.IsPositionOccupied(Position + new Vector(i, i), afterMove))
                            return false;
                    return true;
                }
                else /* Left-down */
                {
                    for (var i = -1; i > to.Y - Position.Y; i--)
                        if (boardState.IsPositionOccupied(Position + new Vector(i, i), afterMove))
                            return false;
                    return true;
                }
            }
            if (to.X - Position.X == -(to.Y - Position.Y))
            {
                if (to.Y - Position.Y > 0) /* Left-up */
                {
                    for (var i = 1; i < to.Y - Position.Y; i++)
                        if (boardState.IsPositionOccupied(Position + new Vector(-i, i), afterMove))
                            return false;
                    return true;
                }
                else /* Right-down */
                {
                    for (var i = -1; i > to.Y - Position.Y; i--)
                        if (boardState.IsPositionOccupied(Position + new Vector(-i, i), afterMove))
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