using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Chess.Board.Figures;
using Chess.Screens;

namespace Chess.Board
{
    //TODO refactor (less code)
    public class BoardState
    {
        #region Constants, structs and fields

        public const int FiguresNumber = 32;

        private readonly SortedList<int, Figure> figures = new SortedList<int, Figure>();

        private readonly SortedList<int, Figure> figuresCopy = new SortedList<int, Figure>();

        public FigureColors CurrentMoveColor { get; private set; }

        // save the position of pawn that must be transformed
        private FigurePosition pawnToPositionForTransformation;

        public FigurePosition EnPassant { get; private set; }

        /// <summary>
        /// This is the number of halfmoves since the last pawn advance or capture. 
        /// This is used to determine if a draw can be claimed under the fifty-move rule.
        /// </summary>
        private int? halfmoveClock;

        /// <summary>
        /// The number of the full move. It starts at 1, and is incremented after Black's move.
        /// </summary>
        private int fullmoveNumber = 1;

        private readonly FigureColors? computerColor;

        private readonly Dictionary<FigureColors, Dictionary<CastlingTypes, bool>> castling =
            new Dictionary<FigureColors, Dictionary<CastlingTypes, bool>>();

        private readonly GameplayScreen gameplayScreen;

        public bool GetCastling(FigureColors color, CastlingTypes types)
        {
            return castling[color][types];
        }

        private void SetCastling(FigureColors color, CastlingTypes types, bool value)
        {
            castling[color][types] = value;
        }

        #endregion

        public BoardState(FigureColors? computerColor, GameplayScreen gameplayScreen)
        {
            CurrentMoveColor = FigureColors.White;
            this.computerColor = computerColor;
            this.gameplayScreen = gameplayScreen;

            AddFigures();

            castling.Add(FigureColors.White, new Dictionary<CastlingTypes, bool>
                                                 {
                                                     {CastlingTypes.Kingside, true},
                                                     {CastlingTypes.Queenside, true}
                                                 });
            castling.Add(FigureColors.Black, new Dictionary<CastlingTypes, bool>
                                                 {
                                                     {CastlingTypes.Kingside, true},
                                                     {CastlingTypes.Queenside, true}
                                                 });
        }

        private void AddFigures()
        {
            for (var k = 0; k < 2; k++)
            {
                var color = (k == 0) ? FigureColors.White : FigureColors.Black;

                figures.Add(16*k, new Rook(new FigurePosition('a', 7*k + 1), color));
                figures.Add(16*k + 1, new Rook(new FigurePosition('h', 7*k + 1), color));

                figures.Add(16*k + 2, new Knight(new FigurePosition('b', 7*k + 1), color));
                figures.Add(16*k + 3, new Knight(new FigurePosition('g', 7*k + 1), color));

                figures.Add(16*k + 4, new Bishop(new FigurePosition('c', 7*k + 1), color));
                figures.Add(16*k + 5, new Bishop(new FigurePosition('f', 7*k + 1), color));

                figures.Add(16*k + 6, new Queen(new FigurePosition('d', 7*k + 1), color));
                figures.Add(16*k + 7, new King(new FigurePosition('e', 7*k + 1), color));

                for (var x = 'a'; x <= 'h'; x++)
                {
                    figures.Add(16*k + 8 + (x - 'a'), new Pawn(new FigurePosition(x, 5*k + 2), color));
                }
            }
        }

        #region Figures move

        /// <summary>
        /// Change position of figure
        /// </summary>
        /// <param name="figureNumber">Number of figure.</param>
        /// <param name="to">Destination of figure.</param>
        /// <param name="computerMove"> </param>
        /// <param name="figureForPawnPromotionForComputer"> </param>
        /// <returns>True, if move was performed.</returns>
        public bool PlayerMove(int figureNumber, FigurePosition to,
                               bool computerMove = false,
                               Type figureForPawnPromotionForComputer = null)
        {
            if (!to.IsValid())
                return false;

            Figure movingFigure;
            if (!figures.TryGetValue(figureNumber, out movingFigure))
                return false;
            
            var from = movingFigure.Position;
            if (to == from)
                return false;

            Figure attackedFigure = null;
            int? attackedFigureNumber = FindFigureNumber(to, false);
            if (attackedFigureNumber.HasValue)
                if (!figures.TryGetValue(attackedFigureNumber.Value, out attackedFigure))
                    return false;

            if (movingFigure.Color != CurrentMoveColor) // if you try to move enemy piece?
                return false;

            // Do not move computer's figures
            if (movingFigure.Color == computerColor && !computerMove)
                return false;

            if (attackedFigureNumber.HasValue)
                if (movingFigure.Color == attackedFigure.Color)
                    return false;

            // Create copy of chessboard to test the future possible position
            figuresCopy.Clear();
            for (var i = 0; i < figures.Count; i++)
                figuresCopy.Add(figures.Keys[i],
                                figures.Values[i]);

            #region Figures rules

            if (!movingFigure.CanMove(to, this))
            {
                return false;
            }
            if (movingFigure is Pawn)
            {
                var pawn = (Pawn) movingFigure;
                if (to.Y == 8 || to.Y == 1)
                {
                    pawnToPositionForTransformation = to;
                }
                if (EnPassant == to)
                {
                    figuresCopy.Remove(FindFigureNumber(EnPassant + new Vector(0, -pawn.MoveVector()), true).Value);
                }
            } // casleing?
            else if (movingFigure is King)
            {
                if (Math.Abs(to.X - from.X) == 2)
                {
                    var line = CurrentMoveColor == FigureColors.White ? 1 : 8;
                    if (to.X > from.X)
                    {
                        var rookNumber = FindFigureNumber(new FigurePosition('h', line), true).Value;
                        figuresCopy.Remove(rookNumber);
                        figuresCopy.Add(rookNumber, new Rook(new FigurePosition('f', line), CurrentMoveColor));
                    }
                    else
                    {
                        var rookNumber = FindFigureNumber(new FigurePosition('a', line), true).Value;
                        figuresCopy.Remove(rookNumber);
                        figuresCopy.Add(rookNumber, new Rook(new FigurePosition('d', line), CurrentMoveColor));
                    }
                }
            }

            #endregion

            #region Make move on copy boardState

            // Move figure on test copy of chessboard to check on attacks on King
            // Need to delete figure that positions on 'to' if it exists
            if (attackedFigureNumber.HasValue)
            {
                figuresCopy.Remove(attackedFigureNumber.Value); // got piece?
            } else
            {
                Console.WriteLine("attackedFigureNumber is FALSE");
            }
                

            // Move figure to new position
            var figureType = figuresCopy.
                Values[figuresCopy.IndexOfKey(figureNumber)].GetType();
            figuresCopy.Remove(figureNumber);
            figuresCopy.Add(figureNumber, (Figure) Activator.CreateInstance(figureType, to, CurrentMoveColor));

            #endregion

            /* Check if king on attack cell on copy boardState */
            if (CanAttackOnCopyBoard(GetKingPositionOnCopy(CurrentMoveColor))) // king can be attacked?
            {
                return false;
            }
            else
            {
                Console.WriteLine("CanAttackOnCopyBoard is false");
            }

            #region Save copy boardState

            // Can make move on "real" chessboard
            figures.Clear();
            for (int i = 0; i < figuresCopy.Count; i++)
                figures.Add(figuresCopy.Keys[i],
                            figuresCopy.Values[i]);

            #endregion

            /* Move performed */

            /* Change 3D model of figure if pawn promotion occured */
            var movedFigure = figures[figureNumber];
            if (movedFigure is Pawn)
            {
                if (movedFigure.Position.Y == 1 || movedFigure.Position.Y == 8)
                {
                    if (computerMove)
                    {
                        ChangePawnOnBoard(figureForPawnPromotionForComputer ?? typeof (Queen));
                    }
                    else
                    {
                        gameplayScreen.ShowFiguresBox();
                    }
                }
            }

            var isAnyFigureCaptured = attackedFigureNumber.HasValue;
            var isPawnMoved = movedFigure is Pawn;
            // En passant
            if (movedFigure is Pawn && Math.Abs(to.X - from.X) == 2 && to.Y == from.Y)
                EnPassant = new FigurePosition(from.X, (from.Y + to.Y)/2);
            else
                EnPassant = null;

            // Castle
            if (movedFigure is King)
            {
                SetCastling(CurrentMoveColor, CastlingTypes.Queenside, false);
                SetCastling(CurrentMoveColor, CastlingTypes.Kingside, false);
            }
            if (movedFigure is Rook)
            {
                if (from.X == 'a')
                {
                    SetCastling(CurrentMoveColor, CastlingTypes.Queenside, false);
                }
                if (from.X == 'h')
                {
                    SetCastling(CurrentMoveColor, CastlingTypes.Kingside, false);
                }
            }

            // Alternate move. Fullmove number
            if (CurrentMoveColor == FigureColors.White)
                CurrentMoveColor = FigureColors.Black;
            else
            {
                fullmoveNumber++;
                CurrentMoveColor = FigureColors.White;
            }

            // Halmove clock
            if (isPawnMoved || isAnyFigureCaptured)
                halfmoveClock = 0;
            else if (halfmoveClock.HasValue)
                halfmoveClock++;

            return true;
        }

        public void ComputerMove(String notation)
        {
            Trace.WriteLine(notation);
            /* Null move? */
            if (notation == "0000")
            {
                /* End of the game for computer */
                return;
            }
            /* Convert notation to coordinates */
            var from = new FigurePosition(notation[0], int.Parse(notation[1].ToString(CultureInfo.InvariantCulture)));
            var to = new FigurePosition(notation[2], int.Parse(notation[3].ToString(CultureInfo.InvariantCulture)));
            Type promotionFigureType = null;
            if (notation.Length > 4)
            {
                /* Pawn promotion */
                switch (notation[4])
                {
                    case 'n':
                        promotionFigureType = typeof (Knight);
                        break;
                    case 'b':
                        promotionFigureType = typeof (Bishop);
                        break;
                    case 'r':
                        promotionFigureType = typeof (Rook);
                        break;
                    case 'q':
                        promotionFigureType = typeof (Queen);
                        break;
                }
            }
            var figureNumber = FindFigureNumber(from, false).Value;
            var moved = PlayerMove(figureNumber, to, true, promotionFigureType);
            Debug.Assert(moved);
        }

        public void ChangePawnOnBoard(Type figureType)
        {
            var pawnNumber = FindFigureNumber(pawnToPositionForTransformation, false);
            if (!pawnNumber.HasValue)
                return;
            figures.Remove(pawnNumber.Value);
            if (pawnToPositionForTransformation.Y == 8)
                figures.Add(pawnNumber.Value,
                            (Figure)
                            Activator.CreateInstance(figureType, pawnToPositionForTransformation, FigureColors.White));
            else
                figures.Add(pawnNumber.Value,
                            (Figure)
                            Activator.CreateInstance(figureType, pawnToPositionForTransformation, FigureColors.Black));

            gameplayScreen.ReloadFigureModel(pawnNumber.Value, GetFigure(pawnNumber.Value).GetModelName());
        }

        /// <summary>
        /// Check cell on attacking by another figur.
        /// </summary>
        /// <param name="to">Checking cell position.</param>
        /// <returns>True if attacked, false if not.</returns>
        public bool CanAttackOnCopyBoard(FigurePosition to)
        {
            if (!to.IsValid())
                return false;

            foreach (var figure in figuresCopy.Values)
            {
                // check color
                if (figure.Color == CurrentMoveColor)
                    continue;
                var from = figure.Position;
                // Cell not at attack, it occupied by enemy
                if (from == to)
                    return false;
                // check only-attack-rules
                if (figure.CanAttack(to, this))
                {
                    return true;
                }
            }

            return false;
        }

        public string GenerateFenState()
        {
            var fen = "";

            // Position of figures
            var fenLinesOfFigures = new string[8];
            for (var y = 8; y >= 1; y--)
            {
                var fenLine = "";
                var blankSquares = 0;
                for (var x = 'a'; x <= 'h'; x++)
                {
                    var number = FindFigureNumber(new FigurePosition(x, y), false);
                    if (number.HasValue)
                    {
                        var figure = figures[number.Value];
                        var fenFigure = figure.GetFenName();
                        if (blankSquares > 0)
                        {
                            fenLine += blankSquares;
                            blankSquares = 0;
                        }
                        fenLine += fenFigure;
                    }
                    else
                    {
                        blankSquares++;
                    }
                }
                if (blankSquares > 0)
                    fenLine += blankSquares;
                fenLinesOfFigures[8 - y] = fenLine;
            }
            fen += String.Join("/", fenLinesOfFigures);

            // Current color
            fen += " " + (CurrentMoveColor == FigureColors.White ? 'w' : 'b');

            // Castle
            fen += " ";
            var castle = "";
            if (GetCastling(FigureColors.White, CastlingTypes.Kingside))
            {
                castle += 'K';
            }
            if (GetCastling(FigureColors.White, CastlingTypes.Queenside))
            {
                castle += 'Q';
            }
            if (GetCastling(FigureColors.Black, CastlingTypes.Kingside))
            {
                castle += 'k';
            }
            if (GetCastling(FigureColors.Black, CastlingTypes.Queenside))
            {
                castle += 'q';
            }
            fen += castle.Length > 0 ? castle : "-";

            // En passant
            fen += " " + (EnPassant != null ? EnPassant.ToString() : "-");

            // Halfmove
            fen += " " + (halfmoveClock ?? 0);

            // Fullmove
            fen += " " + fullmoveNumber;

            return fen;
        }

        #endregion

        /// <summary>
        /// Find figure by it number.
        /// </summary>
        /// <param name="figureID">Number of figure.</param>
        /// <returns>Type's type.</returns>
        public Figure GetFigure(int figureID)
        {
            Figure figure;
            if (figures.TryGetValue(figureID, out figure))
                return figure;
            return null;
        }

        /// <summary>
        /// Find figure's position by it number.
        /// </summary>
        /// <param name="figureID">Number of figure.</param>
        /// <returns>Position on chessboard.</returns>
        public FigurePosition FigurePosition(int figureID)
        {
            Figure figure;
            if (figures.TryGetValue(figureID, out figure))
                return figure.Position;
            return null;
        }

        /// <summary>
        /// Find figure's color by it number.
        /// </summary>
        /// <param name="figureNumber">Number of figure.</param>
        /// <returns>Type's color.</returns>
        public FigureColors? FigureColor(int figureNumber)
        {
            Figure fpc;
            if (figures.TryGetValue(figureNumber, out fpc))
                return fpc.Color;
            return null;
        }

        /// <summary>
        /// Check if figure exists at this moment.
        /// </summary>
        /// <param name="figureNumber">Number of figure.</param>
        /// <returns>True, if exists, and false - otherwise.</returns>
        public bool CheckFigureExistence(int figureNumber)
        {
            return figures.ContainsKey(figureNumber);
        }

        /// <summary>
        /// Find figure number on boardState
        /// </summary>
        /// <param name="position">Type position</param>
        /// <param name="findOnCopy">If true - search on boardState temporary copy</param>
        /// <returns>Type number or null</returns>
        public int? FindFigureNumber(FigurePosition position, bool findOnCopy)
        {
            Figure fpc;
            var figuresOnBoard = findOnCopy ? figuresCopy : figures;
            for (int i = 0; i < FiguresNumber; i++)
                if (figuresOnBoard.TryGetValue(i, out fpc))
                    if (fpc.Position == position)
                        return i;
            return null;
        }

        private FigurePosition GetKingPositionOnCopy(FigureColors color)
        {
            Figure figure;
            for (var i = 0; i < FiguresNumber; i++)
                if (figuresCopy.TryGetValue(i, out figure))
                {
                    if (figure is King && figure.Color == color)
                        return figure.Position;
                }
            throw new Exception(string.Format("Can't find {0} king", color));
        }

        public bool IsPositionOccupied(FigurePosition position, bool afterMove)
        {
            return FindFigureNumber(position, afterMove).HasValue;
        }
    }
}