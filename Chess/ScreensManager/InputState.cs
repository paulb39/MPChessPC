using Microsoft.Xna.Framework.Input;

namespace Chess.ScreensManager
{
    /// <summary>
    /// Helper for reading input from keyboard and mouse. This class tracks both
    /// the current and previous state of both input devices, and implements query
    /// methods for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    public class InputState
    {
        #region Fields

        private KeyboardState currentKeyboardState;
        private MouseState currentMouseState;

        private KeyboardState lastKeyboardState;
        private MouseState lastMouseState;

        public KeyboardState CurrentKeyboardState
        {
            get { return currentKeyboardState; }
        }

        public MouseState CurrentMouseState
        {
            get { return currentMouseState; }
        }

        public KeyboardState LastKeyboardState
        {
            get { return lastKeyboardState; }
        }

        public MouseState LastMouseState
        {
            get { return lastMouseState; }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructs a new input state.
        /// </summary>
        public InputState()
        {
            currentKeyboardState = Keyboard.GetState();
            currentMouseState = Mouse.GetState();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads the latest state of the keyboard and mouse.
        /// </summary>
        public void Update()
        {
            lastKeyboardState = CurrentKeyboardState;
            lastMouseState = CurrentMouseState;

            currentKeyboardState = Keyboard.GetState();
            currentMouseState = Mouse.GetState();
        }

        /// <summary>
        /// Helper for checking if a key was newly pressed during this update. The
        /// controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When a keypress
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsNewKeyPress(Keys key)
        {
            return (currentKeyboardState.IsKeyDown(key) &&
                    lastKeyboardState.IsKeyUp(key));
        }

        /// <summary>
        /// Checks for a "menu select" input action.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When the action
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsMenuSelect()
        {
            return (IsNewKeyPress(Keys.Space) || IsNewKeyPress(Keys.Enter));
        }

        /// <summary>
        /// Checks for a "menu cancel" input action.
        /// The controllingPlayer parameter specifies which player to read input for.
        /// If this is null, it will accept input from any player. When the action
        /// is detected, the output playerIndex reports which player pressed it.
        /// </summary>
        public bool IsMenuCancel()
        {
            return IsNewKeyPress(Keys.Escape);
        }

        /// <summary>
        /// Checks for a "menu up" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuUp()
        {
            return IsNewKeyPress(Keys.Up);
        }

        /// <summary>
        /// Checks for a "menu down" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsMenuDown()
        {
            return IsNewKeyPress(Keys.Down);
        }

        /// <summary>
        /// Checks for a "pause the game" input action.
        /// The controllingPlayer parameter specifies which player to read
        /// input for. If this is null, it will accept input from any player.
        /// </summary>
        public bool IsPauseGame()
        {
            return IsNewKeyPress(Keys.Escape);
        }

        public bool IsLeftButtonPressed()
        {
            return (currentMouseState.LeftButton == ButtonState.Pressed) &&
                   (lastMouseState.LeftButton == ButtonState.Released);
        }

        public bool IsLeftButtonPressing()
        {
            return (currentMouseState.LeftButton == ButtonState.Pressed) &&
                   (lastMouseState.LeftButton == ButtonState.Pressed);
        }

        public bool IsLeftButtonReleased()
        {
            return (currentMouseState.LeftButton == ButtonState.Released) &&
                   (lastMouseState.LeftButton == ButtonState.Pressed);
        }

        public bool IsRightButtonPressed()
        {
            return (currentMouseState.RightButton == ButtonState.Pressed) &&
                   (lastMouseState.RightButton == ButtonState.Released);
        }

        public bool IsRightButtonPressing()
        {
            return (currentMouseState.RightButton == ButtonState.Pressed) &&
                   (lastMouseState.RightButton == ButtonState.Pressed);
        }

        public bool IsMiddleButtonPressing()
        {
            return (currentMouseState.MiddleButton == ButtonState.Pressed) &&
                   (lastMouseState.MiddleButton == ButtonState.Pressed);
        }

        public int ScrollWheelChange()
        {
            return currentMouseState.ScrollWheelValue -
                   lastMouseState.ScrollWheelValue;
        }

        #endregion
    }
}