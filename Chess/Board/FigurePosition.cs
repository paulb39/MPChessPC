using System.Globalization;

namespace Chess.Board
{
    public class FigurePosition
    {
        public int Y { get; private set; }

        public char X { get; private set; }

        public FigurePosition(char x, int y)
        {
            X = x;
            Y = y;
        }

        public bool IsValid()
        {
            return X >= 'a' && X <= 'h' && Y >= 1 && Y <= 8;
        }

        public static FigurePosition operator +(FigurePosition position, Vector vector)
        {
            return new FigurePosition((char) (position.X + vector.X), position.Y + vector.Y);
        }

        public static bool operator ==(FigurePosition position1, FigurePosition position2)
        {
            if (ReferenceEquals(position1, position2))
            {
                return true;
            }
            if (((object) position1 == null) || ((object) position2 == null))
            {
                return false;
            }
            return position1.X == position2.X && position1.Y == position2.Y;
        }

        public static bool operator !=(FigurePosition position1, FigurePosition position2)
        {
            return !(position1 == position2);
        }

        public override string ToString()
        {
            return X.ToString(CultureInfo.InvariantCulture) + Y.ToString(CultureInfo.InvariantCulture);
        }

        public override bool Equals(object obj)
        {
            var position = obj as FigurePosition;
            if (position == null)
            {
                return false;
            }
            return X == position.X && Y == position.Y;
        }

        public override int GetHashCode()
        {
            return X ^ Y;
        }
    }
}