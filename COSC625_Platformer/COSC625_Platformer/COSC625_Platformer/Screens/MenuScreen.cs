using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using COSC625_Platformer.GameObjects;

namespace COSC625_Platformer.Screens
{
    public enum MenuState
    {
        Main,
        Help,
        PopUp,
        Options,
        GameOver
    }

    public class MenuScreen
    {
        List<Text> menuEntries, helpContents, optionEntries, popupEntries, gameoverEntries;
        public static MenuState mState = MenuState.Main;
        MenuState current, previous;

        int selection;
        Texture2D popWindow;
        Texture2D titleScreen;
        Texture2D gbackground;

        public MenuScreen()
        {
            selection = 0;

            #region Menu Items
            menuEntries = new List<Text>();
            menuEntries.Add(new Text("Start Game", new Vector2(550, 450)));
            menuEntries.Add(new Text("Options", new Vector2(550, 30.0f)));
            menuEntries.Add(new Text("View Controls", new Vector2(550, 30.0f)));
            menuEntries.Add(new Text("Quit Game", new Vector2(550, 30.0f)));
            #endregion

            #region Controls
            helpContents = new List<Text>();
            helpContents.Add(new Text("Return to Main Menu", new Vector2(1000, 680)));
            #endregion

            #region Options
            optionEntries = new List<Text>();
            optionEntries.Add(new Text("Number of Lives: ", new Vector2(550, 450)));
            optionEntries.Add(new Text("Number of Continues", new Vector2(550, 30.0f)));
            optionEntries.Add(new Text("Return to Main Menu", new Vector2(550, 30.0f)));
            #endregion

            #region PopUp
            popupEntries = new List<Text>();
            popupEntries.Add(new Text("Return to Main Menu", new Vector2(550, 330)));
            popupEntries.Add(new Text("Quit", new Vector2(550, 30.0f)));
            #endregion

            #region Game Over
            gameoverEntries = new List<Text>();
            gameoverEntries.Add(new Text("Restart", new Vector2(590, 600)));
            gameoverEntries.Add(new Text("Quit to Main Menu", new Vector2(590, 30.0f)));
            #endregion
        }

        public void UpdateTextPositioning()
        {
            #region Menu
            for (int i = 1; i < menuEntries.Count; i++)
                menuEntries[i].Position += new Vector2(0, menuEntries[i - 1].Position.Y + menuEntries[i - 1].Size.Y);
            #endregion

            #region Help
            for (int i = 1; i < helpContents.Count; i++)
                helpContents[i].Position += new Vector2(0, helpContents[i - 1].Position.Y + helpContents[i - 1].Size.Y);
            #endregion

            #region Options
            for (int i = 1; i < optionEntries.Count; i++)
                optionEntries[i].Position += new Vector2(0, optionEntries[i - 1].Position.Y + optionEntries[i - 1].Size.Y);
            #endregion

            #region PopUp
            for (int i = 1; i < popupEntries.Count; i++)
                popupEntries[i].Position += new Vector2(0, popupEntries[i - 1].Position.Y + popupEntries[i - 1].Size.Y);
            #endregion

            #region Game Over
            for (int i = 1; i < gameoverEntries.Count; i++)
                gameoverEntries[i].Position += new Vector2(0, gameoverEntries[i - 1].Position.Y + gameoverEntries[i - 1].Size.Y);
            #endregion

            popWindow = Game1.content.Load<Texture2D>("Menu/popupmenuquit");
            titleScreen = Game1.content.Load<Texture2D>("Menu/titlescreenwtitle");
            gbackground = Game1.content.Load<Texture2D>("Menu/genericbackground");
        }

        public void Update(GameTime gametime)
        {
            #region Main
            if (mState == MenuState.Main)
            {
                if (ScreenManager.controls.Down(PlayerIndex.One))
                {
                    if (selection < menuEntries.Count - 1)
                        selection++;
                    else
                        selection = 0;
                }
                else if (ScreenManager.controls.Up(PlayerIndex.One))
                {
                    if (selection > 0)
                        selection--;
                    else
                        selection = menuEntries.Count - 1;
                }
                else if (ScreenManager.controls.Quit)
                    ScreenManager.isExiting = true;
                else if (ScreenManager.controls.AButton(PlayerIndex.One))
                {
                    switch (selection)
                    {
                        case 0: ScreenManager.gameState = GameState.Play; break;
                        case 1: mState = MenuState.Options; break;
                        case 2: mState = MenuState.Help; break;
                        case 3: mState = MenuState.PopUp; break;
                    }
                }
                for (int i = 0; i < menuEntries.Count; i++)
                {
                    if (i == selection)
                    {
                        if (!menuEntries[i].Active)
                            menuEntries[i].Active = true;
                    }
                    else
                    {
                        if (menuEntries[i].Active)
                            menuEntries[i].Active = false;
                    }
                    menuEntries[i].Update();
                }
            }
            #endregion

            #region Help
            else if (mState == MenuState.Help)
            {
                if (ScreenManager.controls.AButton(PlayerIndex.One) || ScreenManager.controls.Back(PlayerIndex.One))
                {
                    mState = MenuState.Main;
                }
                if (!helpContents[0].Active)
                {
                    helpContents[0].Active = true;
                    helpContents[0].Update();
                }
            }
            #endregion

            #region Options
            else if (mState == MenuState.Options)
            {
                if (ScreenManager.controls.Down(PlayerIndex.One))
                {
                    if (selection < optionEntries.Count - 1)
                        selection++;
                    else
                        selection = 0;
                }
                else if (ScreenManager.controls.Up(PlayerIndex.One))
                {
                    if (selection > 0)
                        selection--;
                    else
                        selection = optionEntries.Count - 1;
                }
                else if (ScreenManager.controls.AButton(PlayerIndex.One))
                {
                    switch (selection)
                    {
                        case 0: //break;
                        case 1: //break;
                        case 2: mState = MenuState.Main; break;
                    }
                }
                for (int i = 0; i < optionEntries.Count; i++)
                {
                    if (i == selection)
                    {
                        if (!optionEntries[i].Active)
                            optionEntries[i].Active = true;
                    }
                    else
                    {
                        if (optionEntries[i].Active)
                            optionEntries[i].Active = false;
                    }
                    optionEntries[i].Update();
                }
            }
            #endregion

            #region PopUp
            else if (mState == MenuState.PopUp)
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
                else if (ScreenManager.controls.AButton(PlayerIndex.One))
                {
                    switch (selection)
                    {
                        case 0: mState = MenuState.Main; break;
                        case 1: ScreenManager.isExiting = true; break;
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

            #region Game Over
            else if (mState == MenuState.GameOver)
            {
                if (ScreenManager.controls.Down(PlayerIndex.One))
                {
                    if (selection < gameoverEntries.Count - 1)
                        selection++;
                    else
                        selection = 0;
                }
                else if (ScreenManager.controls.Up(PlayerIndex.One))
                {
                    if (selection > 0)
                        selection--;
                    else
                        selection = gameoverEntries.Count - 1;
                }
                else if (ScreenManager.controls.AButton(PlayerIndex.One))
                {
                    switch (selection)
                    {
                        case 0: ScreenManager.gameState = GameState.Play; break;
                        case 1: mState = MenuState.Main; break;
                    }
                }
                for (int i = 0; i < gameoverEntries.Count; i++)
                {
                    if (i == selection)
                    {
                        if (!gameoverEntries[i].Active)
                            gameoverEntries[i].Active = true;
                    }
                    else
                    {
                        if (gameoverEntries[i].Active)
                            gameoverEntries[i].Active = false;
                    }
                    gameoverEntries[i].Update();
                }
            }
            #endregion

            MenuTransitionManagement();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            #region Menu
            if (mState == MenuState.Main)
            {
                spriteBatch.Draw(titleScreen, new Rectangle(0, 0, titleScreen.Width, titleScreen.Height), Color.White);
                foreach (Text t in menuEntries)
                    t.Draw(spriteBatch);
            }
            #endregion

            #region Help
            else if (mState == MenuState.Help)
            {
                spriteBatch.Draw(gbackground, new Rectangle(0, 0, titleScreen.Width, titleScreen.Height), Color.White);
                foreach (Text t in helpContents)
                    t.Draw(spriteBatch);
            }
            #endregion

            #region Options
            else if (mState == MenuState.Options)
            {
                spriteBatch.Draw(gbackground, new Rectangle(0, 0, titleScreen.Width, titleScreen.Height), Color.White);
                foreach (Text t in optionEntries)
                    t.Draw(spriteBatch);
            }
            #endregion

            #region PopUp
            else if (mState == MenuState.PopUp)
            {
                spriteBatch.Draw(titleScreen, new Rectangle(0, 0, titleScreen.Width, titleScreen.Height), Color.White);
                foreach (Text t in menuEntries)
                    t.Draw(spriteBatch);

                spriteBatch.Draw(popWindow, new Rectangle(500, 200, popWindow.Width, popWindow.Height), Color.White);
                foreach (Text t in popupEntries)
                    t.Draw(spriteBatch);
            }
            #endregion

            #region Game Over
            else if (mState == MenuState.GameOver)
            {
                foreach (Text t in gameoverEntries)
                    t.Draw(spriteBatch);
            }
            #endregion
        }

        private void MenuTransitionManagement()
        {
            previous = current;
            current = mState;

            if (current != previous)
                selection = 0;
        }
    }
}
