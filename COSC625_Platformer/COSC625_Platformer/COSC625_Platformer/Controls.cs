using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using COSC625_Platformer.Screens;

namespace COSC625_Platformer
{
    public class Controls
    {
        public KeyboardState CurrentKeyboardState, LastKeyboardState;
        public GamePadState CurrentPlayerOneState, LastPlayerOneState, CurrentPlayerTwoState, LastPlayerTwoState,
                            CurrentPlayerThreeState, LastPlayerThreeState, CurrentPlayerFourState, LastPlayerFourState;

        public GamePadState ControllerState(PlayerIndex index)
        {
            if (index == PlayerIndex.One)
                return CurrentPlayerOneState;
            else if (index == PlayerIndex.Two)
                return CurrentPlayerTwoState;
            else if (index == PlayerIndex.Three)
                return CurrentPlayerThreeState;
            else
                return CurrentPlayerFourState;
        }

        public bool Quit
        {
            get { return IsNewKeyPress(Keys.Escape); }
        }
        public bool AButton(PlayerIndex index)
        {
            return (IsNewButtonPress(Buttons.A, index) || IsNewKeyPress(Keys.Space));
        }
        public bool BButton(PlayerIndex index)
        {
            return (IsButtonPress(Buttons.B, index));
        }
        public bool YButton(PlayerIndex index)
        {
            return (IsNewButtonPress(Buttons.Y, index));
        }
        public bool XButton(PlayerIndex index)
        {
            return (IsNewButtonPress(Buttons.X, index));
        }
        public bool Start(PlayerIndex index)
        {
            return (IsNewKeyPress(Keys.Enter) || IsNewButtonPress(Buttons.Start, index));
        }
        public bool Back(PlayerIndex index)
        {
            return (IsNewKeyPress(Keys.Back) || IsNewButtonPress(Buttons.Back, index));
        }
        public bool Up(PlayerIndex index)
        {
            if (ScreenManager.gameState == GameState.Menu || ScreenManager.gameState == GameState.Pause)
                return (IsNewKeyPress(Keys.Up) || IsNewButtonPress(Buttons.LeftThumbstickUp, index) || IsNewButtonPress(Buttons.DPadUp, index));
            else
                return (IsKeyPress(Keys.Up) || IsButtonPress(Buttons.LeftThumbstickUp, index) || IsButtonPress(Buttons.DPadUp, index));
        }
        public bool Down(PlayerIndex index)
        {
            if (ScreenManager.gameState == GameState.Menu || ScreenManager.gameState == GameState.Pause)
                return (IsNewKeyPress(Keys.Down) || IsNewButtonPress(Buttons.LeftThumbstickDown, index) || IsNewButtonPress(Buttons.DPadDown, index));
            else
                return (IsKeyPress(Keys.Down) || IsButtonPress(Buttons.LeftThumbstickDown, index) || IsButtonPress(Buttons.DPadDown, index));
        }

        public bool Right(PlayerIndex index)
        {
            return (IsKeyPress(Keys.Right) || IsButtonPress(Buttons.LeftThumbstickRight, index) || IsButtonPress(Buttons.DPadRight, index));
        }

        public bool Left(PlayerIndex index)
        {
            return (IsKeyPress(Keys.Left) || IsButtonPress(Buttons.LeftThumbstickLeft, index) || IsButtonPress(Buttons.DPadLeft, index));
        }

        public bool Fire(PlayerIndex index)
        {
            return (IsNewKeyPress(Keys.Z) || IsNewButtonPress(Buttons.RightTrigger, index));
        }

        public bool Attack(PlayerIndex index)
        {
            return (IsNewKeyPress(Keys.F) || IsNewButtonPress(Buttons.X, index));
        }

        public bool Jump(PlayerIndex index)
        {
            return (IsKeyPress(Keys.Space) || IsButtonPress(Buttons.A, index));
        }

        public void Update()
        {
            //Since we need to update the current keyboard state it actually holds the last state.
            //So set the last keyboard as the current keyboard state
            LastKeyboardState = CurrentKeyboardState;
            CurrentKeyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

            LastPlayerOneState = CurrentPlayerOneState;
            CurrentPlayerOneState = GamePad.GetState(PlayerIndex.One);

            LastPlayerTwoState = CurrentPlayerTwoState;
            CurrentPlayerTwoState = GamePad.GetState(PlayerIndex.Two);

            LastPlayerThreeState = CurrentPlayerThreeState;
            CurrentPlayerThreeState = GamePad.GetState(PlayerIndex.Three);

            LastPlayerFourState = CurrentPlayerFourState;
            CurrentPlayerFourState = GamePad.GetState(PlayerIndex.Four);
        }

        private bool IsKeyPress(Keys keys)
        {
            return CurrentKeyboardState.IsKeyDown(keys);
        }

        private bool IsNewKeyPress(Keys keys)
        {
            return (LastKeyboardState.IsKeyUp(keys)) && (CurrentKeyboardState.IsKeyDown(keys));
        }

        private bool IsButtonPress(Buttons button, PlayerIndex index)
        {
            if (index == PlayerIndex.One)
                return CurrentPlayerOneState.IsButtonDown(button);
            else if (index == PlayerIndex.Two)
                return CurrentPlayerTwoState.IsButtonDown(button);
            else if (index == PlayerIndex.Three)
                return CurrentPlayerThreeState.IsButtonDown(button);
            else
                return CurrentPlayerFourState.IsButtonDown(button);
        }

        private bool IsNewButtonPress(Buttons button, PlayerIndex index)
        {
            if (index == PlayerIndex.One)
                return (LastPlayerOneState.IsButtonUp(button)) && (CurrentPlayerOneState.IsButtonDown(button));
            else if (index == PlayerIndex.Two)
                return (LastPlayerTwoState.IsButtonUp(button)) && (CurrentPlayerTwoState.IsButtonDown(button));
            else if (index == PlayerIndex.Three)
                return (LastPlayerThreeState.IsButtonUp(button)) && (CurrentPlayerThreeState.IsButtonDown(button));
            else
                return (LastPlayerFourState.IsButtonUp(button)) && (CurrentPlayerFourState.IsButtonDown(button));
        }

    }
}
