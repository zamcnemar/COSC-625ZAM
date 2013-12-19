using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using COSC625_Platformer.GameObjects;
using Microsoft.Xna.Framework.Audio;

namespace COSC625_Platformer.Screens
{
    public enum PauseState
    {
        Main,
        Help,
        PopUp
    }

    public class PauseScreen
    {
        List<Text> pauseEntries, helpContents, popupEntries;
        PauseState pauseState = PauseState.Main;
        PauseState current, previous;

        int selection = 0;
        Texture2D popWindow;
        Texture2D titleScreen;
        Texture2D gbackground;

        public PauseScreen()
        {
            selection = 0;

            #region Pause Items
            pauseEntries = new List<Text>();
            pauseEntries.Add(new Text("Return to Game", new Vector2(550, 450)));
            pauseEntries.Add(new Text("View Controls", new Vector2(550, 30.0f)));
            pauseEntries.Add(new Text("Quit to Main Menu", new Vector2(550, 30.0f)));
            #endregion

            #region Help Contents
            helpContents = new List<Text>();
            helpContents.Add(new Text("Return to Pause Menu", new Vector2(1000, 680)));
            #endregion

            #region PopUp
            popupEntries = new List<Text>();
            popupEntries.Add(new Text("Return to Pause Menu", new Vector2(550, 330)));
            popupEntries.Add(new Text("Quit to Main Menu", new Vector2(550, 30.0f)));
            #endregion
        }

        public void UpdateTextPositioning()
        {
            #region Pause
            for (int i = 1; i < pauseEntries.Count; i++)
                pauseEntries[i].Position += new Vector2(0, pauseEntries[i - 1].Position.Y + pauseEntries[i - 1].Size.Y);
            #endregion

            #region Help
            for (int i = 1; i < helpContents.Count; i++)
                helpContents[i].Position += new Vector2(0, helpContents[i - 1].Position.Y + helpContents[i - 1].Size.Y);
            #endregion

            #region PopUp
            for (int i = 1; i < popupEntries.Count; i++)
                popupEntries[i].Position += new Vector2(0, popupEntries[i - 1].Position.Y + helpContents[i - 1].Size.Y);
            #endregion

            popWindow = Game1.content.Load<Texture2D>("Menu/popupmenuquit");
            titleScreen = Game1.content.Load<Texture2D>("Menu/titlescreenwtitle");
            gbackground = Game1.content.Load<Texture2D>("Menu/genericbackground");
        }

        public void Update(GameTime gametime)
        {
            #region Main
            if (pauseState == PauseState.Main)
            {
                if (ScreenManager.controls.Down(PlayerIndex.One))
                {
                    if (selection < pauseEntries.Count - 1)
                        selection++;
                    else
                        selection = 0;
                }
                else if (ScreenManager.controls.Up(PlayerIndex.One))
                {
                    if (selection > 0)
                        selection--;
                    else
                        selection = pauseEntries.Count - 1;
                }
                else if (ScreenManager.controls.Quit)
                    ScreenManager.isExiting = true;
                else if (ScreenManager.controls.AButton(PlayerIndex.One))
                {
                    switch (selection)
                    {
                        case 0: ScreenManager.gameState = GameState.Play; break;
                        case 1: pauseState = PauseState.Help; break;
                        case 2: pauseState = PauseState.PopUp; break;
                    }
                }
                for (int i = 0; i < pauseEntries.Count; i++)
                {
                    if (i == selection)
                    {
                        if (!pauseEntries[i].Active)
                            pauseEntries[i].Active = true;
                    }
                    else
                    {
                        if (pauseEntries[i].Active)
                            pauseEntries[i].Active = false;
                    }
                    pauseEntries[i].Update();
                }
            }
            #endregion

            #region Help
            else if (pauseState == PauseState.Help)
            {
                if (ScreenManager.controls.AButton(PlayerIndex.One) || ScreenManager.controls.Back(PlayerIndex.One))
                {
                    pauseState = PauseState.Main;
                }
                if (!helpContents[0].Active)
                {
                    helpContents[0].Active = true;
                    helpContents[0].Update();
                }
            }
            #endregion

            #region PopUp
            if (pauseState == PauseState.PopUp)
            {
                if (ScreenManager.controls.Down(PlayerIndex.One))
                {
                    if (selection < popupEntries.Count - 1)
                        selection++;
                    else
                        selection = 0;
                }
                else if (ScreenManager.controls.Up(PlayerIndex.One))
                {
                    if (selection > 0)
                        selection--;
                    else
                        selection = popupEntries.Count - 1;
                }
                else if (ScreenManager.controls.Quit)
                    ScreenManager.isExiting = true;
                else if (ScreenManager.controls.AButton(PlayerIndex.One))
                {
                    switch (selection)
                    {
                        case 0: pauseState = PauseState.Main; break;
                        case 1: ScreenManager.gameState = GameState.Menu;
                            MenuScreen.mState = MenuState.Main; break;
                    }
                }
                for (int i = 0; i < popupEntries.Count; i++)
                {
                    if (i == selection)
                    {
                        if (!popupEntries[i].Active)
                            popupEntries[i].Active = true;
                    }
                    else
                    {
                        if (popupEntries[i].Active)
                            popupEntries[i].Active = false;
                    }
                    popupEntries[i].Update();
                }
            }
            #endregion

            PauseTransitionManagement();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            #region Menu
            if (pauseState == PauseState.Main)
            {
                spriteBatch.Draw(titleScreen, new Rectangle(0, 0, titleScreen.Width, titleScreen.Height), Color.White);
                foreach (Text t in pauseEntries)
                    t.Draw(spriteBatch);
            }
            #endregion

            #region Help
            else if (pauseState == PauseState.Help)
            {
                spriteBatch.Draw(titleScreen, new Rectangle(0, 0, titleScreen.Width, titleScreen.Height), Color.White);
                foreach (Text t in helpContents)
                    t.Draw(spriteBatch);
            }
            #endregion

            #region PopUp
            else if (pauseState == PauseState.PopUp)
            {
                spriteBatch.Draw(titleScreen, new Rectangle(0, 0, titleScreen.Width, titleScreen.Height), Color.White);
                foreach (Text t in pauseEntries)
                    t.Draw(spriteBatch);

                spriteBatch.Draw(popWindow, new Rectangle(500, 200, popWindow.Width, popWindow.Height), Color.White);
                foreach (Text t in popupEntries)
                    t.Draw(spriteBatch);
            }
            #endregion
        }

        private void PauseTransitionManagement()
        {
            previous = current;
            current = pauseState;

            if (current != previous)
                selection = 0;
        }
    }
}
