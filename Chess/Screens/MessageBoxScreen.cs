using System;
using Chess.Localization;
using Chess.ScreensManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Chess.Screens
{
    /// <summary>
    /// A popup message box screen, used to display "are you sure?"
    /// confirmation messages.
    /// </summary>
    internal class MessageBoxScreen : GameScreen
    {
        #region Fields

        private readonly string message;
        private readonly string ok = Strings.messagebox_yes;
        private readonly string cancel = Strings.messagebox_no;
        private Texture2D gradientTexture;

        #endregion

        #region Events

        public event EventHandler<EventArgs> Accepted;
        public event EventHandler<EventArgs> Cancelled;

        #endregion

        #region Initialization

        public MessageBoxScreen(string message)
        {
            this.message = message;

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
            ContentManager content = ScreenManager.Game.Content;

            gradientTexture = content.Load<Texture2D>(@"Menu\gradient");
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// Responds to user input, accepting or cancelling the message box.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input.IsMenuSelect() || input.IsNewKeyPress(Keys.Y))
            {
                // Raise the accepted event, then exit the message box.
                if (Accepted != null)
                    Accepted(this, new EventArgs());

                ExitScreen();
            }
            else if (input.IsMenuCancel() || input.IsNewKeyPress(Keys.N))
            {
                // Raise the cancelled event, then exit the message box.
                if (Cancelled != null)
                    Cancelled(this, new EventArgs());

                ExitScreen();
            }

            SpriteFont font = ScreenManager.Font;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            Vector2 messageSize = font.MeasureString(message);
            Vector2 okSize = font.MeasureString(ok);
            Vector2 cancelSize = font.MeasureString(cancel);

            Vector2 messagePosition = (viewportSize - messageSize - okSize.Y*Vector2.UnitY)/2.0f;
            Vector2 okPosition = new Vector2(
                messagePosition.X + messageSize.X/4.0f - okSize.X/2.0f,
                messagePosition.Y + messageSize.Y);
            Vector2 cancelPosition = new Vector2(
                messagePosition.X + 3*messageSize.X/4.0f - cancelSize.X/2.0f,
                messagePosition.Y + messageSize.Y);

            Rectangle okRectangle = new Rectangle(
                (int) okPosition.X, (int) okPosition.Y, (int) okSize.X, (int) okSize.Y);
            Rectangle cancelRectangle = new Rectangle(
                (int) cancelPosition.X, (int) cancelPosition.Y, (int) cancelSize.X, (int) cancelSize.Y);
            if (input.IsLeftButtonPressed())
            {
                if (okRectangle.Contains(new Point(input.CurrentMouseState.X, input.CurrentMouseState.Y)))
                {
                    // Raise the accepted event, then exit the message box.
                    if (Accepted != null)
                        Accepted(this, new EventArgs());

                    ExitScreen();
                }
                else if (cancelRectangle.Contains(new Point(input.CurrentMouseState.X, input.CurrentMouseState.Y)))
                {
                    // Raise the cancelled event, then exit the message box.
                    if (Cancelled != null)
                        Cancelled(this, new EventArgs());

                    ExitScreen();
                }
            }
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draws the message box.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            SpriteFont font = ScreenManager.Font;

            // Darken down any other screens that were drawn beneath the popup.
            ScreenManager.FadeBackBufferToBlack(TransitionAlpha*2/3);

            // Center the message text in the viewport.
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            Vector2 messageSize = font.MeasureString(message);
            Vector2 okSize = font.MeasureString(ok);
            Vector2 cancelSize = font.MeasureString(cancel);

            Vector2 messagePosition = (viewportSize - messageSize - okSize.Y*Vector2.UnitY)/2.0f;
            Vector2 okPosition = new Vector2(
                messagePosition.X + messageSize.X/4.0f - okSize.X/2.0f,
                messagePosition.Y + messageSize.Y);
            Vector2 cancelPosition = new Vector2(
                messagePosition.X + 3*messageSize.X/4.0f - cancelSize.X/2.0f,
                messagePosition.Y + messageSize.Y);

            // The background includes a border somewhat larger than the text itself.
            const int hPad = 32;
            const int vPad = 16;

            Rectangle backgroundRectangle = new Rectangle((int) messagePosition.X - hPad,
                                                          (int) messagePosition.Y - vPad,
                                                          (int) messageSize.X + hPad*2,
                                                          (int) (messageSize.Y + okSize.Y) + vPad*2);

            // Fade the popup alpha during transitions.
            Color color = new Color(255, 255, 255, TransitionAlpha);

            spriteBatch.Begin();

            // Draw the background rectangle.
            spriteBatch.Draw(gradientTexture, backgroundRectangle, color);

            // Draw the message box text.
            spriteBatch.DrawString(font, message, messagePosition, color);
            spriteBatch.DrawString(font, ok, okPosition, color);
            spriteBatch.DrawString(font, cancel, cancelPosition, color);

            spriteBatch.End();
        }

        #endregion
    }
}