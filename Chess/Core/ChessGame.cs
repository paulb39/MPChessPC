using System;
using System.Globalization;
using Chess.Localization;
using Chess.Screens;
using Chess.ScreensManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chess.Core
{
    /// <summary>
    /// This main game class is extremely simple: all the interesting
    /// stuff happens in the ScreenManager component.
    /// </summary>
    public class ChessGame : Game
    {
        #region Fields

        private readonly GraphicsDeviceManager graphics;
        private readonly ScreenManager screenManager;

        #endregion

        private ChessGame()
        {
            IsFixedTimeStep = false;
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this)
                           {
                               PreferMultiSampling = true,
                               PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8
                           };

            // Tell the resource manager what language to use when loading strings.
            Strings.Culture = CultureInfo.CurrentCulture;

            // Create the screen manager component.
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);
            // Activate the first screens.
            screenManager.AddScreen(new BackgroundScreen());
            screenManager.AddScreen(new MainMenuScreen());

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += WindowSizeChanged;
        }

        private void WindowSizeChanged(object sender, EventArgs e)
        {
            graphics.PreferredBackBufferWidth = GraphicsDevice.Viewport.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.Viewport.Height;
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            IsMouseVisible = true;
            base.Initialize();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            // The real drawing happens inside the screen manager component.
            base.Draw(gameTime);
        }

        private static void Main()
        {
            using (var game = new ChessGame())
            {
                game.Run();
            }
        }
    }
}