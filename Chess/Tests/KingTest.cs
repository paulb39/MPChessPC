using Chess.Board;
using Chess.Board.Figures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chess.Tests
{
    [TestClass]
    public class KingTest
    {
        [TestMethod]
        public void TestGetModelName()
        {
            var king = new King(new FigurePosition('a', 1), FigureColors.White);
            Assert.AreEqual("WhiteKing", king.GetModelName());

            king = new King(new FigurePosition('a', 1), FigureColors.Black);
            Assert.AreEqual("BlackKing", king.GetModelName());
        }
    }
}