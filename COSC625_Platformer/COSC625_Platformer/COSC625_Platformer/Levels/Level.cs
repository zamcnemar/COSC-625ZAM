using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.IO;
using COSC625_Platformer.GameObjects;
using COSC625_Platformer.Screens;
using COSC625_Platformer.GameObjects.Items;
using COSC625_Platformer.GameObjects.Enemies;

namespace COSC625_Platformer.Levels
{
    /// <summary>
    /// A uniform grid of tiles with collections of items and enemies.
    /// The level owns the player and controls the game's win and lose
    /// conditions as well as scoring.
    /// </summary>
    class Level : IDisposable
    {
        // Physical structure of the level.
        private Tile[,] tiles;
        private Layer[] layers;
        // The layer which entities are drawn on top of.
        private const int EntityLayer = 2;

        // Entities in the level.
        public Player Player
        {
            get { return player; }
        }
        Player player;

        public ContentManager Content = Game1.content;

        public List<MovableTile> movableTiles = new List<MovableTile>();

        private List<Item> items = new List<Item>();

        public List<Enemy> enemies = new List<Enemy>();

        // Key locations in the level.        
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);

        // Level game state.
        private Random random = new Random(354668); // Arbitrary, but constant seed

        // Camera Position relative to the level.
        //public float cameraPosition;
        //private float cameraPositionYAxis;

        public float cameraPosition { get; private set; }

        public float cameraPositionYAxis { get; private set; }

        public int Score
        {
            get { return score; }
        }
        int score;

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        public TimeSpan TimeRemaining
        {
            get { return timeRemaining; }
        }
        TimeSpan timeRemaining;

        private const int PointsPerSecond = 5;

        private SoundEffect exitReachedSound;

        ContentManager content;

        #region Loading

        /// <summary>
        /// Constructs a new level.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider that will be used to construct a ContentManager.
        /// </param>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        public Level(Stream fileStream, int levelIndex)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(Game1.content.ServiceProvider, "Content");

            timeRemaining = TimeSpan.FromMinutes(2.0);

            LoadTiles(fileStream);

            //Load background layer textures. For now, all levels must
            // use the same backgrounds and only use the left-most part of them.
            layers = new Layer[3];
            layers[0] = new Layer(Content, "Backgrounds/stars0", 0.2f);
            layers[1] = new Layer(Content, "Backgrounds/stars1", 0.5f);
            layers[2] = new Layer(Content, "Backgrounds/stars2", 0.8f);


            //used to use vertical test background - use commented section above instead
            //to use normal backgrounds
            /*layers = new Layer[3];
            layers[0] = new Layer(content, "Backgrounds/layer0", 0.8f);
            layers[1] = new Layer(content, "Backgrounds/layer0", 0.8f);
            layers[2] = new Layer(content, "Backgrounds/layer0", 0.8f);
            */
            // Load sounds.
            exitReachedSound = content.Load<SoundEffect>("Sounds/ExitReached");
        }

        /// <summary>
        /// Iterates over every tile in the structure file and loads its
        /// appearance and behavior. This method also validates that the
        /// file is well-formed with a player start point, exit, etc.
        /// </summary>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        private void LoadTiles(Stream fileStream)
        {
            // Load the level and ensure all of the lines are the same length.
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception(String.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    line = reader.ReadLine();
                }
            }

            // Allocate the tile grid.
            tiles = new Tile[width, lines.Count];

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // to load each tile.
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }

            // Verify that the level has a beginning and an end.
            if (Player == null)
                throw new NotSupportedException("A level must have a starting point.");
            if (exit == InvalidPosition)
                throw new NotSupportedException("A level must have an exit.");

        }

        /// <summary>
        /// Loads an individual tile's appearance and behavior.
        /// </summary>
        /// <param name="tileType">
        /// The character loaded from the structure file which
        /// indicates what should be loaded.
        /// </param>
        /// <param name="x">
        /// The X location of this tile in tile space.
        /// </param>
        /// <param name="y">
        /// The Y location of this tile in tile space.
        /// </param>
        /// <returns>The loaded tile.</returns>
        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                //LADDER
                case 'H':
                    return LoadTile("ladder0", TileCollision.Ladder);
                // Moving platform - Horzontal
                case 'M':
                    return LoadMovableTile(x, y, TileCollision.Platform);

                // Blank space
                case '.':
                    return new Tile(null, TileCollision.Passable);

                // Exit
                case 'X':
                    return LoadExitTile(x, y);

                // Item
                case 'G':
                    return LoadGemTile(x, y);

                case 'P':
                    return LoadPowerUpTile(x, y);

                // Floating platform
                case '-':
                    return LoadTile("Platform", TileCollision.Platform);

                // Floating platform
                case '_':
                    return LoadTile("pf1", TileCollision.Platform);

                // Various enemies
                case 'A':
                    return LoadZombieTile(x, y);

                // Platform block
                case '~':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Platform);

                // Passable block
                case ':':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Passable);

                // Player 1 start point
                case '0':
                    return LoadStartTile(x, y);

                // Impassable block
                case '#':
                    return LoadVarietyTile("BlockA", 7, TileCollision.Impassable);

                // Impassable Surface sprites

                case '5':
                    return LoadTile("bg1", TileCollision.Impassable);

                case '8':
                    return LoadTile("u1", TileCollision.Impassable);

                case '2':
                    return LoadTile("d1", TileCollision.Impassable);

                case '4':
                    return LoadTile("l1", TileCollision.Impassable);

                case '6':
                    return LoadTile("r1", TileCollision.Impassable);

                // Impassable Rounded Corners

                case '9':
                    return LoadTile("ur1", TileCollision.Impassable);

                case '7':
                    return LoadTile("ul1", TileCollision.Impassable);

                case '1':
                    return LoadTile("dl1", TileCollision.Impassable);

                case '3':
                    return LoadTile("dr1", TileCollision.Impassable);

                // Impassable Inverted connectors

                case 'f':
                    return LoadTile("cul1", TileCollision.Impassable);

                case 'g':
                    return LoadTile("cur1", TileCollision.Impassable);

                case 'v':
                    return LoadTile("cdl1", TileCollision.Impassable);

                case 'b':
                    return LoadTile("cdr1", TileCollision.Impassable);

                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        /// <summary>
        /// Creates a new tile. The other tile loading methods typically chain to this
        /// method after performing their special logic.
        /// </summary>
        /// <param name="name">
        /// Path to a tile texture relative to the Content/Tiles directory.
        /// </param>
        /// <param name="collision">
        /// The tile collision type for the new tile.
        /// </param>
        /// <returns>The new tile.</returns>
        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(content.Load<Texture2D>("Tiles/" + name), collision);
        }

        private Tile LoadMovableTile(int x, int y, TileCollision collision)
        {
            Point position = GetBounds(x, y).Center;
            movableTiles.Add(new MovableTile(this, new Vector2(position.X, position.Y), collision));

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Loads a tile with a random appearance.
        /// </summary>
        /// <param name="baseName">
        /// The content name prefix for this group of tile variations. Tile groups are
        /// name LikeThis0.png and LikeThis1.png and LikeThis2.png.
        /// </param>
        /// <param name="variationCount">
        /// The number of variations in this group.
        /// </param>
        private Tile LoadVarietyTile(string baseName, int variationCount, TileCollision collision)
        {
            int index = random.Next(variationCount);
            return LoadTile(baseName + index, collision);
        }


        /// <summary>
        /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private Tile LoadStartTile(int x, int y)
        {
            if (Player != null)
                throw new NotSupportedException("A level may only have one starting point.");

            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            player = new Player(this, start);

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Remembers the location of the level's exit.
        /// </summary>
        private Tile LoadExitTile(int x, int y)
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            exit = GetBounds(x, y).Center;

            return LoadTile("Exit", TileCollision.Passable);
        }

        /// <summary>
        /// Instantiates an enemy and puts him in the level.
        /// </summary>
        private Tile LoadZombieTile(int x, int y)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemies.Add(new Zombie(this, position));

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Instantiates a gem and puts it in the level.
        /// </summary>
        private Tile LoadGemTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            items.Add(new Gem(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Instantiates a star powerup and puts it in the level.
        /// </summary>
        private Tile LoadPowerUpTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            items.Add(new Star(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Unloads the level content.
        /// </summary>
        public void Dispose()
        {
            content.Unload();
        }

        #endregion

        #region Bounds and collision

        /// <summary>
        /// Gets the collision mode of the tile at a particular location.
        /// This method handles tiles outside of the levels boundries by making it
        /// impossible to escape past the left or right edges, but allowing things
        /// to jump beyond the top of the level and fall off the bottom.
        /// </summary>
        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        //LADDER
        public TileCollision GetTileCollisionBehindPlayer(Vector2 playerPosition)
        {
            int x = (int)playerPosition.X / Tile.Width;
            int y = (int)(playerPosition.Y - 1) / Tile.Height;

            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }
        //LADDER
        public TileCollision GetTileCollisionBelowPlayer(Vector2 playerPosition)
        {
            int x = (int)playerPosition.X / Tile.Width;
            int y = (int)(playerPosition.Y) / Tile.Height;

            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        /// <summary>
        /// Gets the bounding rectangle of a tile in world space.
        /// </summary>        
        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        /// <summary>
        /// Width of level measured in tiles.
        /// </summary>
        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        /// <summary>
        /// Height of the level measured in tiles.
        /// </summary>
        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates all objects in the world, performs collision between them,
        /// and handles the time limit with scoring.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Pause while the player is dead or time is expired.
            if (!Player.IsAlive || TimeRemaining == TimeSpan.Zero)
            {
                // Still want to perform physics on the player.
                Player.ApplyPhysics(gameTime);
            }
            else if (ReachedExit)
            {
                // Animate the time being converted into points.
                int seconds = (int)Math.Round(gameTime.ElapsedGameTime.TotalSeconds * 100.0f);
                seconds = Math.Min(seconds, (int)Math.Ceiling(TimeRemaining.TotalSeconds));
                timeRemaining -= TimeSpan.FromSeconds(seconds);
                score += seconds * PointsPerSecond;
            }
            else
            {
                timeRemaining -= gameTime.ElapsedGameTime;
                Player.Update(gameTime);
                UpdateItems(gameTime);
                // Falling off the bottom of the level kills the player.
                if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                    OnPlayerKilled(null);

                UpdateEnemies(gameTime);

                UpdateMovableTiles(gameTime);

                // The player has reached the exit if they are standing on the ground and
                // his bounding rectangle contains the center of the exit tile. They can only
                // exit when they have collected all of the items.
                if (Player.IsAlive &&
                    Player.IsOnGround &&
                    Player.BoundingRectangle.Contains(exit))
                {
                    OnExitReached();
                }
            }

            // Clamp the time remaining at zero.
            if (timeRemaining < TimeSpan.Zero)
                timeRemaining = TimeSpan.Zero;
        }

        private void UpdateMovableTiles(GameTime gameTime)
        {
            foreach (MovableTile tile in movableTiles)
            {
                tile.Update(gameTime);

                if (tile.PlayerIsOn)
                {
                    //Make player move with tile if the player is on top of tile
                    player.Position += tile.Velocity;
                }
            }
        }

        private void UpdateItems(GameTime gameTime)
        {
            for (int i = 0; i < items.Count; ++i)
            {
                Item item = items[i];

                item.Update(gameTime);

                if (item.Boundary.Intersects(Player.BoundingRectangle))
                {
                    items.RemoveAt(i--);
                    OnItemCollected(item, Player);
                }
            }
        }

        /// <summary>
        /// Animates each enemy and allow them to kill the player.
        /// </summary>
        
        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
            {
                enemy.Update(gameTime);

                // Touching an enemy results in the following.
                if (enemy.IsAlive && enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    // If the player is powered up kill the enemy.
                    if (Player.IsPoweredUp)
                    {
                        OnEnemyKilled(enemy, Player);
                    }
                    else
                    {
                        // otherwise the player is killed.
                        if (enemy.IsAlive && enemy.BoundingRectangle.Intersects(Player.MeleeRectangle))
                        {
                            if (Player.isAttacking)
                                OnEnemyKilled(enemy, Player);
                                
                        }

                        if (enemy.IsAlive && enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
                        {
                            OnPlayerKilled(enemy);
                        }
                    }

                }

            }
        }

        /// <summary>
        /// Called when an enemy is killed to determine who killed it.
        /// </summary>
        private void OnEnemyKilled(Enemy enemy, Player killedBy)
        {
            enemy.OnKilled(killedBy);

        }

        /// <summary>
        /// Called when a item is collected.
        /// </summary>
        /// <param name="item">The item that was collected.</param>
        /// <param name="collectedBy">The player who collected this item.</param>
        private void OnItemCollected(Item item, Player collectedBy)
        {
            score += item.PointValue;

            item.OnCollected(collectedBy);
        }

        /// <summary>
        /// Called when the player is killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This is null if the player was not killed by an
        /// enemy, such as when a player falls into a hole.
        /// </param>
        private void OnPlayerKilled(Enemy killedBy)
        {
            Player.OnKilled(killedBy);
        }

        /// <summary>
        /// Called when the player reaches the level's exit.
        /// </summary>
        private void OnExitReached()
        {
            Player.OnReachedExit();
            exitReachedSound.Play();
            reachedExit = true;
        }

        /// <summary>
        /// Restores the player to the starting point to try the level again.
        /// </summary>
        public void StartNewLife()
        {
            Player.Reset(start);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw everything in the level from background to foreground.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();
            for (int i = 0; i <= EntityLayer; ++i)
                layers[i].Draw(spriteBatch, cameraPosition, cameraPositionYAxis);
            spriteBatch.End();

            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraPosition, -cameraPositionYAxis, 0.0f);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, null, cameraTransform);

            DrawTiles(spriteBatch);

            foreach (MovableTile tile in movableTiles)
                tile.Draw(gameTime, spriteBatch);

            DrawItems(gameTime, spriteBatch);

            Player.Draw(gameTime, spriteBatch);

            DrawEnemies(gameTime, spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin();
            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                layers[i].Draw(spriteBatch, cameraPosition, cameraPositionYAxis);
            spriteBatch.End();
        }


        /// <summary>
        /// Camera Movement Update method that moves vertically and horizontally with the player.
        /// </summary>
        private void ScrollCamera(Viewport viewport)
        {
            const float ViewMargin = 0.35f;

            // Calculate the edges of the screen.
            float marginWidth = viewport.Width * ViewMargin;
            float marginLeft = cameraPosition + marginWidth;
            float marginRight = cameraPosition + viewport.Width - marginWidth;

            // Calculate the scrolling borders for the Y-axis
            const float TopMargin = 0.4f;
            const float BottomMargin = 0.4f;
            float marginTop = cameraPositionYAxis + viewport.Height * TopMargin;
            float marginBottom = cameraPositionYAxis + viewport.Height - viewport.Height * BottomMargin;

            // Calculate how far to scroll when the player is near the edges of the screen.
            float cameraMovement = 0.0f;
            if (Player.Position.X < marginLeft)
                cameraMovement = Player.Position.X - marginLeft;
            else if (Player.Position.X > marginRight)
                cameraMovement = Player.Position.X - marginRight;

            // Calculate how far to vertically scroll when the player is near the top or bottom of the screen.
            float cameraMovementY = 0.0f;
            if (Player.Position.Y < marginTop) // above the top margin
                cameraMovementY = Player.Position.Y - marginTop;
            else if (Player.Position.Y > marginBottom) // below the bottom margin
                cameraMovementY = Player.Position.Y - marginBottom;

            float maxCameraPositionYOffset = Tile.Height * Height - viewport.Height;

            // Update the camera position, but prevent scrolling off the ends of the level.
            float maxCameraPosition = Tile.Width * Width - viewport.Width;
            // x axis
            cameraPosition = MathHelper.Clamp(cameraPosition + cameraMovement, 0.0f, maxCameraPosition);
            // y axis
            cameraPositionYAxis = MathHelper.Clamp(cameraPositionYAxis + cameraMovementY, 0.0f, maxCameraPositionYOffset);
        }

        /// <summary>
        /// Draws each tile in the level.
        /// </summary>
        private void DrawTiles(SpriteBatch spriteBatch)
        {
            // Calculate the visible range of tiles.
            int left = (int)Math.Floor(cameraPosition / Tile.Width);
            int right = left + spriteBatch.GraphicsDevice.Viewport.Width / Tile.Width;
            right = Math.Min(right, Width - 1);

            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = left; x <= right; ++x)
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }

        /// <summary>
        ///  Draws each Enemy in the level if it falls within the set outer margins of the camera.
        ///  </summary>
        ///  9.28.13 - Z - Created method to limit the drawing for enemies to those within a range of the screen/camera.
        private void DrawEnemies(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Calculate the visible range of enemiese
            Viewport viewport = spriteBatch.GraphicsDevice.Viewport;
            const float ViewMargin = 0.35f;
            float marginWidth = viewport.Width * ViewMargin;
            float marginLeft = cameraPosition - marginWidth;
            float marginRight = cameraPosition + viewport.Width + marginWidth;

            foreach (Enemy enemy in enemies)
                if (enemy.Position.X > marginLeft && enemy.Position.X < marginRight)
                {
                    if(enemy.IsAlive || enemy.deathTime > 0)
                        enemy.Draw(gameTime, spriteBatch);
                }

        }

        /// <summary>
        ///  Draws each item in the level if it is within the set outer boundaries of the camera.
        ///  </summary>
        ///  9.28.13 - Z - Created method to limit the drawing for items to those within a range of the screen/camera.
        private void DrawItems(GameTime gameTime, SpriteBatch spriteBatch)
        {

            // Calculate the visible range of items on the screen.
            Viewport viewport = spriteBatch.GraphicsDevice.Viewport;
            const float ViewMargin = 0.35f;
            float marginWidth = viewport.Width * ViewMargin;
            float marginLeft = cameraPosition - marginWidth;
            float marginRight = cameraPosition + viewport.Width + marginWidth;


            foreach (Item item in items)
                if (item.Position.X > marginLeft && item.Position.X < marginRight)
                {
                    item.Draw(spriteBatch, item.Color);
                }

        }
        #endregion
    }
}
