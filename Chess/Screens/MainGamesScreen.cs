using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Screens
{
    internal class MainGamesScreen : MenuScreen
    {
        private MenuEntry btnNewGame = new MenuEntry("New Game");

        public MainGamesScreen() : base("Play") //screenmanager needed?
        {
            btnNewGame.Selected += BtnNewGame_Selected;
            MenuEntries.Add(btnNewGame);
        }

        private void BtnNewGame_Selected(object sender, EventArgs e)
        {
            //message box for if AI or player
           // ScreenManager.AddScreen(confirmExitMessageBox);         
            string message = "Do you want to play against AI or a human?";

            MessageBoxScreen msgBox = new MessageBoxScreen(message);
            msgBox.OkayMessage = "AI";
            msgBox.CancelMessage = "Human";

            msgBox.Accepted += AIChoosen;
            msgBox.Cancelled += humanChoosen;

            ScreenManager.AddScreen(msgBox);
        }

        private void humanChoosen(object sender, EventArgs e)
        {
            //TODO goto FB friend list - use pawn selection box logic?
        }

        private void AIChoosen(object sender, EventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, new GameplayScreen());
        }
    }
}
