using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace COSC625_Platformer.Screens
{

    public enum GameState
    {
        Menu,
        Play,
        Pause
    }

    public class ScreenManager:DrawableGameComponent
    {
        public static GameState gameState = GameState.Menu;
        public static Controls controls;
        public static SpriteFont spriteFont;
        public static bool isExiting = false;

        MenuScreen screen_Menu;
        GameScreen screen_Game;
        PauseScreen screen_Pause;
        SpriteBatch spritebatch;

        GameState previous, current;

        public ScreenManager(Game game) : base(game)
        {
            screen_Game = new GameScreen();
            screen_Menu = new MenuScreen();
            screen_Pause = new PauseScreen();
            controls = new Controls();
        }

        protected override void LoadContent()
        {
            spritebatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Game1.content.Load<SpriteFont>("Fonts/Hud");

            screen_Menu.UpdateTextPositioning();
            screen_Pause.UpdateTextPositioning();
            screen_Game.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            if (isExiting)
                this.Game.Exit();
            controls.Update();

            if (gameState == GameState.Menu)
                screen_Menu.Update(gameTime);
            else if (gameState == GameState.Play)
                screen_Game.Update(gameTime);
            else if (gameState == GameState.Pause)
                screen_Pause.Update(gameTime);

            TransitionManagement();
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            spritebatch.Begin();
            if (gameState == GameState.Menu)
                screen_Menu.Draw(spritebatch);
            else if (gameState == GameState.Play)
            {
                spritebatch.End();
                screen_Game.Draw(gameTime, spritebatch);
                spritebatch.Begin();
            }
            else if (gameState == GameState.Pause)
                screen_Pause.Draw(spritebatch);
            base.Draw(gameTime);
            spritebatch.End();
        }

        private void TransitionManagement()
        {
            previous = current;
            current = gameState;

            if (current == GameState.Menu && (previous == GameState.Play || previous == GameState.Pause))
            {
                screen_Game = new GameScreen();
                screen_Game.LoadContent();
                screen_Pause = new PauseScreen();
                screen_Pause.UpdateTextPositioning();
            }
        }
    }
}
