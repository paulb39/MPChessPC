using Chess.ScreensManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chess.Screens
{
    internal sealed class SelectableText
    {
        private string text;

        public string Text
        {
            get { return text; }
            set { text = value; } 
        }

        public event EventHandler<EventArgs> Selected;

        internal void OnSelectEntry()
        {
            if (Selected != null)
                Selected(this, new EventArgs());
        }

        public SelectableText(string txt)
        {
            text = txt;
        }

        public void Draw(GameScreen screen, Vector2 position,
                 bool isSelected, GameTime gameTime) 
        {
            // Draw the selected entry in yellow, otherwise white.
            Color color = isSelected ? Color.Yellow : Color.White;

            // Pulsate the size of the selected menu entry.
            double time = gameTime.TotalGameTime.TotalSeconds;

            float pulsate = (float)Math.Sin(time * 6) + 1;

            float scale = 1 + pulsate * 0.05f * selectionFade;

            // Modify the alpha to fade text out during transitions.
            color = new Color(color.R, color.G, color.B, screen.TransitionAlpha);

            // Draw text, centered on the middle of each line.
            ScreenManager screenManager = screen.ScreenManager;
            SpriteBatch spriteBatch = screenManager.SpriteBatch;
            SpriteFont font = screenManager.Font;

            Vector2 origin = new Vector2(0, font.LineSpacing / 2);

            spriteBatch.DrawString(font, text, position, color, 0,
                                   origin, scale, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Queries how much space this menu entry requires.
        /// </summary>
        public int GetHeight(GameScreen screen)
        {
            return screen.ScreenManager.Font.LineSpacing;
        }

        public Vector2 GetSize(GameScreen screen)
        {
            return screen.ScreenManager.Font.MeasureString(text);
        }
    }
}
