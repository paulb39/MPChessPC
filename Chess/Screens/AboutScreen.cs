using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Screens
{
    internal class AboutScreen : MenuScreen
    {
        const string msgString = @"TODO this is an about page";
        private readonly MenuEntry backOption = new MenuEntry("Back");
        private readonly MenuEntry aboutMessage = new MenuEntry(msgString);

        public AboutScreen(MenuScreen loginScreen) : base("About")
        {
            ScreenManager = loginScreen.ScreenManager;

            backOption.Selected += OnCancel;

            MenuEntries.Add(aboutMessage); // stupid way of adding the message but meh
            MenuEntries.Add(backOption);

        }


    }
}
