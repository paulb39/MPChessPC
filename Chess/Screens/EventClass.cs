using System;

namespace Chess.Screens
{
    /// <summary>
    /// Using to send a selected figure type
    /// in choose figure box.
    /// </summary>
    internal class FigureEventArgs : EventArgs
    {
        public Type FigureType { get; private set; }

        public FigureEventArgs(Type figureType)
        {
            FigureType = figureType;
        }
    }
}