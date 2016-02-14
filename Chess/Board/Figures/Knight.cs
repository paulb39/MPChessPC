using System;

namespace Chess.Board.Figures
{
    internal class Knight : Figure
    {
        public Knight(FigurePosition position, FigureColors color)
            : base(position, color)
        {
        }

        public override bool CanMove(FigurePosition to, BoardState boardState, bool afterMove = false)
        {
            if (Math.Abs(to.X - Position.X) == 2)
            {
                return Math.Abs(to.Y - Position.Y) == 1;
            }
            if (Math.Abs(to.X - Position.X) == 1)
            {
                return Math.Abs(to.Y - Position.Y) == 2;
            }
            return false;
        }

        public override bool CanAttack(FigurePosition to, BoardState boardState, bool afterMove = true)
        {
            return CanMove(to, boardState, afterMove);
        }

        public override char GetFenName()
        {
            return Color == FigureColors.White ? 'N' : 'n';
        }
    }
}