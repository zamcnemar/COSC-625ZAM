using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using COSC625_Platformer.Levels;

namespace COSC625_Platformer.GameObjects.Items
{
    class Item : Sprite
    {
        protected SoundEffect collectedSound;
        public int PointValue;
        public Color Color;
        public Vector2 origin;
    
        // The item is animated from a base position along the Y axis.
        protected float bounce;
        protected Vector2 basePosition;

        // Bounce control constants
        protected const float BounceHeight = 0.18f;
        protected const float BounceRate = 3.0f;
        protected const float BounceSync = -0.75f;

        /// <summary>
        /// Gets the current position of this item in world space.
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return basePosition + new Vector2(0.0f, bounce);
            }
        }

        /// <summary>
        /// Gets a circle which bounds this item in world space.
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
        public Item()
        {
            

        }

        /// <summary>
        /// Bounces up and down in the air to entice players to collect them.
        /// </summary>
        public virtual void Update(GameTime gameTime)
        {
            // Bounce along a sine curve over time.
            // Include the X coordinate so that neighboring items bounce in a nice wave pattern.            
            double t = gameTime.TotalGameTime.TotalSeconds * BounceRate + Position.X * BounceSync;
            bounce = (float)Math.Sin(t) * BounceHeight * spriteTexture.Height;
        }

        /// <summary>
        /// Called when this item has been collected by a player and removed from the level.
        /// </summary>
        /// <param name="collectedBy">
        /// The player who collected this item. Although currently not used, this parameter would be
        /// useful for creating special powerup items. For example, a item could make the player invincible.
        /// </param>
        public virtual void OnCollected(Player collectedBy)
        {
            collectedSound.Play();
        }

       
        /// <summary>
        /// Draws a item in the appropriate color.
        /// </summary>
        public override void Draw(SpriteBatch spritebatch, Color color)
        {

            Boundary = new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Size.Width, this.Size.Height);
            spritebatch.Draw(spriteTexture, Position, null, Color, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);

        }
        
        
    }
}
