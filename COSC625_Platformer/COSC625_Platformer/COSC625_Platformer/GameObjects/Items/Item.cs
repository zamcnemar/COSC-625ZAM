using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using COSC625_Platformer.Levels;

namespace COSC625_Platformer.GameObjects.Items
{
    class Item : Sprite
    {
        private Texture2D texture;
        private Vector2 origin;
        private SoundEffect collectedSound;

        public readonly int PointValue;
        public bool IsPowerUp { get; private set; }
        public readonly Color Color;

        // The item is animated from a base position along the Y axis.
        private Vector2 basePosition;
        private float bounce;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        /// <summary>
        /// Gets the current position of this item in world space. OK
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return basePosition + new Vector2(0.0f, bounce);
            }
        }

        /// <summary>
        /// Gets a circle which bounds this item in world space. OK
        /// </summary>
        public Circle BoundingCircle
        {
            get
            {
                return new Circle(Position, Tile.Width / 3.0f);
            }
        }

                /// <summary>
        /// Constructs a new item.
        /// </summary>
        public Item(Level level, Vector2 position, bool isPowerUp)
        {
            this.level = level;
            this.basePosition = position;

            IsPowerUp = isPowerUp;
            if (IsPowerUp)
            {
                PointValue = 100;
                Color = Color.Green;
            }
            else
            {
                PointValue = 30;
                Color = Color.Blue;
            }

            LoadContent();
        }

        /// <summary>
        /// Loads the item texture and collected sound.
        /// </summary>
        public void LoadContent()
        {
            texture = Level.Content.Load<Texture2D>("Sprites/gem");
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
            collectedSound = Level.Content.Load<SoundEffect>("Sounds/gemCollected");
        }

        /// <summary>
        /// Bounces up and down in the air to entice players to collect them.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // Bounce control constants
            const float BounceHeight = 0.18f;
            const float BounceRate = 3.0f;
            const float BounceSync = -0.75f;

            // Bounce along a sine curve over time.
            // Include the X coordinate so that neighboring items bounce in a nice wave pattern.            
            double t = gameTime.TotalGameTime.TotalSeconds * BounceRate + Position.X * BounceSync;
            bounce = (float)Math.Sin(t) * BounceHeight * texture.Height;
        }

        /// <summary>
        /// Called when this item has been collected by a player and removed from the level.
        /// </summary>
        /// <param name="collectedBy">
        /// The player who collected this item. Although currently not used, this parameter would be
        /// useful for creating special powerup items. For example, a item could make the player invincible.
        /// </param>
        public void OnCollected(Player collectedBy)
        {
            collectedSound.Play();

            if (IsPowerUp)
                collectedBy.PowerUp();
        }

        /// <summary>
        /// Draws a item in the appropriate color.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }

        
    }
}
