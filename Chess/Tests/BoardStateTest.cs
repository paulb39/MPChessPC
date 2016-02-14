using Chess.Board;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chess.Tests
{
    [TestClass]
    public class BoardStateTest
    {
        [TestMethod]
        public void TestFenStartPosition()
        {
            var chessboardState = new BoardState(FigureColors.White, null);
            const string expected = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            Assert.AreEqual(expected, chessboardState.GenerateFenState());
        }
    }
}