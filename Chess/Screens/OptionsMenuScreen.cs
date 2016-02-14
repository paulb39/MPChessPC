using System;
using System.Globalization;
using Chess.Board;
using Chess.Core;
using Chess.Localization;
using Chess.ScreensManager;

namespace Chess.Screens
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    internal class OptionsMenuScreen : MenuScreen
    {
        private readonly MenuEntry backOption = new MenuEntry();
        private readonly MenuEntry computerColorOption = new MenuEntry();

        private readonly CircularArray<string> cultures = new CircularArray<string>(new[] {"en-US", "ru-RU", "uk-UA"});
        private readonly MenuEntry languageOption = new MenuEntry();
        private readonly MenuEntry levelOption = new MenuEntry();

        /// <summary>
        /// After change of language needed to update main screen
        /// </summary>
        private readonly MainMenuScreen mainMenuScreen;

        public OptionsMenuScreen(MainMenuScreen mainMenuScreen)
            : base(Strings.optionsmenu_title)
        {
            this.mainMenuScreen = mainMenuScreen;
            /* Needed for access to global properties */
            ScreenManager = mainMenuScreen.ScreenManager;

            /* Initialize options */
            InitializeCultureOption();
            UpdateAllTexts();

            /* Click events */
            levelOption.Selected += (sender, e) =>
                                        {
                                            ScreenManager.GameLevelsArray.MoveNext();
                                            UpdateLevelText();
                                        };
            computerColorOption.Selected += (sender, e) =>
                                                {
                                                    ScreenManager.ComputerColorArray.MoveNext();
                                                    UpdateComputerColorText();
                                                };
            languageOption.Selected += (sender, e) =>
                                           {
                                               cultures.MoveNext();
                                               Strings.Culture = new CultureInfo(cultures.Current);
                                               UpdateAllTexts();
                                           };
            backOption.Selected += OnCancel;

            /* Add entries to the menu */
            MenuEntries.Add(levelOption);
            MenuEntries.Add(computerColorOption);
            MenuEntries.Add(languageOption);
            MenuEntries.Add(backOption);
        }

        private void InitializeCultureOption()
        {
            cultures.Index = cultures.IndexOf(Strings.Culture.Name);
            if (cultures.Index == -1)
                cultures.MoveNext();
        }

        private void UpdateLevelText()
        {
            string levelString;
            switch (ScreenManager.GameLevelsArray.Current)
            {
                case ScreenManager.GameLevels.Easy:
                    levelString = Strings.optionsmenu_easy;
                    break;
                case ScreenManager.GameLevels.Normal:
                    levelString = Strings.optionsmenu_normal;
                    break;
                case ScreenManager.GameLevels.Hard:
                    levelString = Strings.optionsmenu_hard;
                    break;
                default:
                    throw new Exception("Unknown game level");
            }
            levelOption.Text = string.Format("{0}: {1}", Strings.optionsmenu_level, levelString);
        }

        private void UpdateComputerColorText()
        {
            string computerColorText;
            switch (ScreenManager.ComputerColorArray.Current)
            {
                case FigureColors.White:
                    computerColorText = Strings.optionsmenu_white;
                    break;
                case FigureColors.Black:
                    computerColorText = Strings.optionsmenu_black;
                    break;
                case null:
                    computerColorText = Strings.optionsmenu_none;
                    break;
                default:
                    throw new Exception("Unknown figure color");
            }
            computerColorOption.Text = string.Format("{0}: {1}", Strings.optionsmenu_color, computerColorText);
        }

        private void UpdateAllTexts()
        {
            languageOption.Text = string.Format("{0}: {1}", Strings.optionsmenu_language,
                                                Strings.Culture.Parent.NativeName);
            backOption.Text = Strings.optionsmenu_back;
            UpdateLevelText();
            UpdateComputerColorText();
            mainMenuScreen.UpdateStringsNames();
        }
    }
}