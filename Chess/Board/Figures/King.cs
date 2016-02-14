using System;
using System.Collections.Generic;

namespace Chess.Board.Figures
{
    public class King : Figure
    {
        public King(FigurePosition position, FigureColors color)
            : base(position, color)
        {
        }

        public override bool CanMove(FigurePosition to, BoardState boardState, bool afterMove = false)
        {
            if (Math.Abs(to.Y - Position.Y) > 1)
                return false;
            if (Math.Abs(to.X - Position.X) > 2)
                return false;
            if (Math.Abs(to.X - Position.X) == 2) //castleing??
            {
                var vectorsToCheck = new List<Vector>();
                if (to.X > Position.X)
                {
                    if (!boardState.GetCastling(Color, CastlingTypes.Kingside))
                        return false;
                    vectorsToCheck.Add(new Vector(1, 0));
                }
                else
                {
                    if (!boardState.GetCastling(Color, CastlingTypes.Queenside))
                        return false;
                    vectorsToCheck.Add(new Vector(-1, 0));
                    vectorsToCheck.Add(new Vector(-2, 0));
                }
                foreach (var vector in vectorsToCheck)
                {
                    if (boardState.IsPositionOccupied(Position + vector, afterMove))
                        return false;
                    /* Cell is attacked? */
                    if (boardState.CanAttackOnCopyBoard(Position + vector)) //king can be attacked at move?
                        return false;
                }
                if (boardState.IsPositionOccupied(to, afterMove))
                    return false;
            }
            return true;
        }

        public override bool CanAttack(FigurePosition to, BoardState boardState, bool afterMove = true)
        {
            return Math.Abs(to.X - Position.X) <= 1 && Math.Abs(to.Y - Position.Y) <= 1;
        }
    }
}
