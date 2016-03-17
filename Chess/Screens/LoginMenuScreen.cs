using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Screens
{
    internal class LoginMenuScreen : MenuScreen
    {
        public LoginMenuScreen() : base("Login")
        {
            //
            MenuEntry facebookLogin = new MenuEntry("FB Login");
            MenuEntry anonLogin = new MenuEntry("Anonymous Login");
            MenuEntry aboutPage = new MenuEntry("About");

            // Hook up menu event handlers. Login can probably be refactored 
            facebookLogin.Selected += loginFB;
            anonLogin.Selected += loginAnonymous;
            aboutPage.Selected += loadAbout;

            // Add entries to the menu.
            MenuEntries.Add(facebookLogin);
            MenuEntries.Add(anonLogin);
            MenuEntries.Add(aboutPage);
        }

        private void loadAbout(object sender, EventArgs e)
        {
            ScreenManager.AddScreen(new AboutScreen(this));
        }

        private void loginAnonymous(object sender, EventArgs e)
        {
            //TODO set username / send info to server
            ScreenManager.AddScreen(new MainMenuScreen());
        }

        private void loginFB(object sender, EventArgs e)
        {
            //TODO set username / send info to server
            ScreenManager.AddScreen(new MainMenuScreen());
        }
    }
}
