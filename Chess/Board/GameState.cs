using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.Globalization;
using Chess.Board.Figures;
using Chess.Screens;

namespace Chess.Board
{
    public class GameState
    {
        public static FigureColors CurrentPlayerMove { get; set; }

        public static GameState savedGameState { get; set; }

        public static bool isAnonymousLogin { get; set; }
    }
}
