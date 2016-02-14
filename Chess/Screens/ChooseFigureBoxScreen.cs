using System;
using Chess.Board.Figures;
using Chess.Localization;
using Chess.ScreensManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Chess.Screens
{
    /// <summary>
    /// A popup message box screen, used to choose figure
    /// for pawn transformation.
    /// </summary>
    internal class ChooseFigureBoxScreen : GameScreen
    {
        #region Fields

        private SpriteFont font;

        //Rectangle backgroundRectangle;
        private Vector2 messagePosition;
        private Vector2 queenPosition;
        private Vector2 rookPosition;
        private Vector2 bishopPosition;
        private Vector2 knightPosition;

        private Rectangle queenRectangle;
        private Rectangle rookRectangle;
        private Rectangle bishopRectangle;
        private Rectangle knightRectangle;

        private readonly string message = Strings.choosefigurebox_message;
        private readonly string queen = Strings.choosefigurebox_queen;
        private readonly string rook = Strings.choosefigurebox_rook;
        private readonly string bishop = Strings.choosefigurebox_bishop;
        private readonly string knight = Strings.choosefigurebox_knight;

        private Texture2D gradientTexture;

        public event EventHandler<FigureEventArgs> FigureChosen;

        #endregion

        #region Initialization

        public ChooseFigureBoxScreen()
        {
            IsPopup = true;

            TransitionOnTime = TimeSpan.FromSeconds(0.2);
            TransitionOffTime = TimeSpan.FromSeconds(0.2);
        }

        /// <summary>
        /// Loads graphics content for this screen. This uses the shared ContentManager
        /// provided by the Game class, so the content will remain loaded forever.
        /// Whenever a subsequent MessageBoxScreen tries to load this same content,
        /// it will just get back another reference to the already loaded data.
        /// </summary>
        public override void LoadContent()
        {
            var content = ScreenManager.Game.Content;

            gradientTexture = content.Load<Texture2D>(@"Menu\gradient");

            font = ScreenManager.Font;

            // Center the message text in the viewport.
            var viewport = ScreenManager.GraphicsDevice.Viewport;

            var viewportSize = new Vector2(viewport.Width, viewport.Height);

            var messageSize = font.MeasureString(message);
            var queenSize = font.MeasureString(queen);
            var rookSize = font.MeasureString(rook);
            var bishopSize = font.MeasureString(bishop);
            var knightSize = font.MeasureString(knight);

            messagePosition = (viewportSize - new Vector2(messageSize.X,
                                                          messageSize.Y + queenSize.Y + rookSize.Y + bishopSize.Y +
                                                          knightSize.Y))/2.0f;
            queenPosition = new Vector2((viewport.Width - queenSize.X)/2.0f, messagePosition.Y + messageSize.Y);
            rookPosition = new Vector2((viewport.Width - rookSize.X)/2.0f,
                                       messagePosition.Y + messageSize.Y + queenSize.Y);
            bishopPosition = new Vector2((viewport.Width - bishopSize.X)/2.0f,
                                         messagePosition.Y + messageSize.Y + queenSize.Y + rookSize.Y);
            knightPosition = new Vector2((viewport.Width - knightSize.X)/2.0f,
                                         messagePosition.Y + messageSize.Y + queenSize.Y + rookSize.Y + bishopSize.Y);

            queenRectangle = new Rectangle(
                (int) queenPosition.X, (int) queenPosition.Y, (int) queenSize.X, (int) queenSize.Y);
            rookRectangle = new Rectangle(
                (int) rookPosition.X, (int) rookPosition.Y, (int) rookSize.X, (int) rookSize.Y);
            bishopRectangle = new Rectangle(
                (int) bishopPosition.X, (int) bishopPosition.Y, (int) bishopSize.X, (int) bishopSize.Y);
            knightRectangle = new Rectangle(
                (int) knightPosition.X, (int) knightPosition.Y, (int) knightSize.X, (int) knightSize.Y);
        }

        #endregion

        public override void HandleInput(InputState input)
        {
            if (input.IsNewKeyPress(Keys.Q))
            {
                if (FigureChosen != null)
                    FigureChosen(this, new FigureEventArgs(typeof (Queen)));

                ExitScreen();
            }
            else if (input.IsNewKeyPress(Keys.R))
            {
                if (FigureChosen != null)
                    FigureChosen(this, new FigureEventArgs(typeof (Rook)));

                ExitScreen();
            }
            else if (input.IsNewKeyPress(Keys.B))
            {
                if (FigureChosen != null)
                    FigureChosen(this, new FigureEventArgs(typeof (Bishop)));

                ExitScreen();
            }
            else if (input.IsNewKeyPress(Keys.K))
            {
                if (FigureChosen != null)
                    FigureChosen(this, new FigureEventArgs(typeof (Knight)));

                ExitScreen();
            }

            if (input.IsLeftButtonPressed())
            {
                if (queenRectangle.Contains(new Point(input.CurrentMouseState.X, input.CurrentMouseState.Y)))
                {
                    if (FigureChosen != null)
                        FigureChosen(this, new FigureEventArgs(typeof (Queen)));

                    ExitScreen();
                }
                else if (rookRectangle.Contains(new Point(input.CurrentMouseState.X, input.CurrentMouseState.Y)))
                {
                    if (FigureChosen != null)
                        FigureChosen(this, new FigureEventArgs(typeof (Rook)));

                    ExitScreen();
                }
                else if (bishopRectangle.Contains(new Point(input.CurrentMouseState.X, input.CurrentMouseState.Y)))
                {
                    if (FigureChosen != null)
                        FigureChosen(this, new FigureEventArgs(typeof (Bishop)));

                    ExitScreen();
                }
                else if (knightRectangle.Contains(new Point(input.CurrentMouseState.X, input.CurrentMouseState.Y)))
                {
                    if (FigureChosen != null)
                        FigureChosen(this, new FigureEventArgs(typeof (Knight)));

                    ExitScreen();
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            var spriteBatch = ScreenManager.SpriteBatch;

            // Darken down any other screens that were drawn beneath the popup.
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha*2/3);

            // Fade the popup alpha during transitions.
            var color = new Color(255, 255, 255, TransitionAlpha);

            // Draw the message box text.
            spriteBatch.Begin();
            spriteBatch.DrawString(font, message, messagePosition, color);
            spriteBatch.DrawString(font, queen, queenPosition, color);
            spriteBatch.DrawString(font, rook, rookPosition, color);
            spriteBatch.DrawString(font, bishop, bishopPosition, color);
            spriteBatch.DrawString(font, knight, knightPosition, color);
            spriteBatch.End();
        }
    }
}