using Chess.Board;
using Chess.Board.Figures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chess.Tests
{
    [TestClass]
    public class KnightTest
    {
        [TestMethod]
        public void TestFenChar()
        {
            Figure knight = new Knight(new FigurePosition('a', 1), FigureColors.White);
            Assert.AreEqual('N', knight.GetFenName());
            knight = new Knight(new FigurePosition('a', 1), FigureColors.Black);
            Assert.AreEqual('n', knight.GetFenName());
        }
    }
}