using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using COSC625_Platformer.GameObjects;
using COSC625_Platformer.Levels;

namespace COSC625_Platformer.Screens
{
    public enum LevelState
    {
        Level1
    }

    public class GameScreen
    {
        public static LevelState levelState;

        public static List<Player> players;

        // Level content.        

        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D diedOverlay;

        // Meta-level game state.
        public static int Levelindex
        {
            get { return levelIndex; }
        }
        private static int levelIndex = -1;

        private Level level;


        // When the time remaining is less than the warning time, it blinks on the hud
        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

        // The number of levels in the Levels directory of our content. We assume that
        // levels in our content are 0-based and that all numbers under this constant
        // have a level file present. This allows us to not need to check for the file
        // or handle exceptions, both of which can add unnecessary time to level loading.
        private const int numberOfLevels = 4;

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        /// 

        public GameScreen()
        {
            players = new List<Player>();
        }
        public void LoadContent()
        {
            // Load overlay textures
            winOverlay = Game1.content.Load<Texture2D>("Overlays/you_win");
            loseOverlay = Game1.content.Load<Texture2D>("Overlays/you_lose");
            diedOverlay = Game1.content.Load<Texture2D>("Overlays/you_died");

            /*
            try
            {
                MediaPlayer.Volume = .30f;
                MediaPlayer.IsRepeating = true;
                MediaPlayer.Play(Game1.content.Load<Song>("Sounds/NG42"));
            }
            catch { }
            */
             
            LoadNextLevel();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            // Handle polling for our input and handling high-level input
            HandleInput();

            // update our level, passing down the GameTime along with all of our input states
            level.Update(gameTime);
        }

        private void HandleInput()
        {
            // Exit the game when back is pressed.
            if (ScreenManager.controls.Quit)
                ScreenManager.isExiting = true;

            // Perform the appropriate action to advance the game and
            // to get the player back to playing.
            foreach (Player p in players)
            {
                    if (ScreenManager.controls.AButton(p.controller))
                    {
                        if (allPlayersOutofLives())
                        {
                            ScreenManager.gameState = GameState.Menu;
                        }
                        else if (level.TimeRemaining == TimeSpan.Zero)
                        {
                            if (level.ReachedExit)
                            {
                                LoadNextLevel();
                                break;
                            }
                            else
                                ReloadCurrentLevel();
                        }
                        else if (!p.IsAlive && p.lives > 0)
                        {
                            level.StartNewLife(p);
                        }
                    }
                }
        }

        private void LoadNextLevel()
        {
            // move to the next level
            if ((levelIndex = (levelIndex + 1)) >= numberOfLevels)
            {
                ScreenManager.gameState = GameState.Menu;
            }

            else
            {
                // Unloads the content for the current level before loading the next one.
                if (level != null)
                    level.Dispose();

                // Load the level.
                string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
                using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                    level = new Level(fileStream, levelIndex);
            }
        }

        private void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();
        }

        /// <summary>
        /// Draws the game from background to foreground.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            level.Draw(gameTime, spriteBatch);

            DrawHud(spriteBatch);
        }

        private void DrawHud(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            Rectangle titleSafeArea = spriteBatch.GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X, titleSafeArea.Y);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                         titleSafeArea.Y + titleSafeArea.Height / 2.0f);

            // Draw time remaining. Uses modulo division to cause blinking when the
            // player is running out of time.
            string timeString = "TIME: " + level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");
            Color timeColor;
            if (level.TimeRemaining > WarningTime ||
                level.ReachedExit ||
                (int)level.TimeRemaining.TotalSeconds % 2 == 0)
            {
                timeColor = Color.Yellow;
            }
            else
            {
                timeColor = Color.Red;
            }
            DrawShadowedString(spriteBatch, ScreenManager.spriteFont, timeString, hudLocation, timeColor);

            // Draw score
            float timeHeight = ScreenManager.spriteFont.MeasureString(timeString).Y;
            DrawShadowedString(spriteBatch, ScreenManager.spriteFont, "SCORE: " + level.Score.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Yellow);

            // Draw Lives 
            float livesHeight = ScreenManager.spriteFont.MeasureString(timeString).Y;
            DrawShadowedString(spriteBatch, ScreenManager.spriteFont, "Lives: " + players[0].lives.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 2.4f), Color.Yellow);

            // Determine the status overlay message to show.
            Texture2D status = null;
            if (level.TimeRemaining == TimeSpan.Zero)
            {
                if (level.ReachedExit)
                {
                    status = winOverlay;
                }
                else
                {
                    status = loseOverlay;
                }
            }

            else if (allPlayersOutofLives())
            {
                status = loseOverlay;
            }

            else if (allPlayersDead())
            {
                status = diedOverlay;
            }

            if (status != null)
            {
                // Draw status message.
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                spriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }

            spriteBatch.End();
        }

        private void DrawShadowedString(SpriteBatch spriteBatch, SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }

        public static Boolean allPlayersOutofLives()
        {
            foreach (Player p in players)
            {
                if (p.IsAlive || p.lives > 0)
                    return false;
            }
            return true;
        }

        public static Boolean allPlayersDead()
        {
            foreach (Player p in players)
            {
                if (p.IsAlive)
                    return false;
            }
            return true;
        }

        public static Player activePlayer()
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].IsAlive)
                    return (players[i]);
            }
            return players[0];
        }
    }
}
