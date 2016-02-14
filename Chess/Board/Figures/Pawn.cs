using System;

namespace Chess.Board.Figures
{
    internal class Pawn : Figure
    {
        public Pawn(FigurePosition position, FigureColors color)
            : base(position, color)
        {
        }

        private bool Moved()
        {
            if (Color == FigureColors.White)
            {
                return Position.Y > 2;
            }
            return Position.Y < 7;
        }

        public int MoveVector()
        {
            return Color == FigureColors.White ? 1 : -1;
        }

        public override bool CanMove(FigurePosition to, BoardState boardState, bool afterMove = false)
        {
            if ((to.X == Position.X) && (to.Y - Position.Y == 1*MoveVector()))
            {
                return !boardState.IsPositionOccupied(to, false);
            }
            if ((to.X == Position.X) && (to.Y - Position.Y == 2*MoveVector()))
            {
                if (Moved())
                {
                    return false;
                }
                if (boardState.IsPositionOccupied(Position + new Vector(0, MoveVector()), false))
                {
                    return false;
                }
                if (boardState.IsPositionOccupied(to, false))
                {
                    return false;
                }
                return true;
            }
            if ((Math.Abs(to.X - Position.X) == 1) && (to.Y - Position.Y == 1*MoveVector()))
            {
                if (boardState.IsPositionOccupied(to, false))
                {
                    return true;
                }
                if (boardState.EnPassant == to)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        public override bool CanAttack(FigurePosition to, BoardState boardState, bool afterMove = true)
        {
            return Math.Abs(to.X - Position.X) == 1 && to.Y - Position.Y == MoveVector();
        }
    }
}