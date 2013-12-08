using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using COSC625_Platformer.GameObjects;
using COSC625_Platformer.Screens;
using COSC625_Platformer.Levels;

namespace COSC625_Platformer
{
    /// <summary>
    /// Facing direction along the X axis.
    /// </summary>
    public enum FaceDirection
    {
        Left = -1,
        Right = 1,
    }

    /// <summary>
    /// A monster who is impeding the progress of our fearless adventurer.
    /// </summary>
    public class Enemy : Sprite
    {
        // GameObjects
        protected GameObject arm;
        protected GameObject[] bullets;
        protected int numbullets = 1;

        protected String spriteSet;

        // Animations
        protected Animation runAnimation;
        protected Animation idleAnimation;
        protected Animation dieAnimation;
        protected SpriteEffects flip = SpriteEffects.None;
        protected AnimationPlayer sprite;

        // Sounds
        protected SoundEffect killedSound;
        protected SoundEffect jumpSound;
        protected SoundEffect fallSound;
        protected SoundEffect bulletDeflect;

        // Constants for controling horizontal movement
        protected const float MoveAcceleration = 13000.0f;
        protected const float MaxMoveSpeed = 1750.0f;
        protected const float GroundDragFactor = 0.48f;
        protected const float AirDragFactor = 0.58f;

        // Constants for controlling vertical movement
        protected float MaxJumpTime = 0.35f;
        protected float JumpLaunchVelocity = -3500.0f;
        protected float GravityAcceleration = 3400.0f;
        protected float MaxFallSpeed = 550.0f;
        protected float JumpControlPower = 0.14f;

        public bool canShoot { get; set; }

        protected int maxHealth = 1;
        protected int health = 1;


        /// <summary>
        /// Gets whether or not the player's feet are on the ground.
        /// </summary>
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        protected bool isOnGround;

        // Jumping state
        protected bool isJumping;
        protected bool wasJumping;
        protected float jumpTime;

        // Shooting Restrictions
        public bool isShooting;
        protected float MaxShootDelay = .75f;
        public float ShootTime = 0.0f;

        /// <summary>
        /// The direction this enemy is facing and moving along the X axis.
        /// </summary>
        protected FaceDirection direction = FaceDirection.Left;

        /// <summary>
        /// How long this enemy has been waiting before turning around.
        /// </summary>
        protected float waitTime;

        /// <summary>
        /// How long to wait before turning around.
        /// </summary>
        protected float MaxWaitTime = 0.5f;

        /// <summary>
        /// The speed at which this enemy moves along the X axis.
        /// </summary>
        protected float MoveSpeed = 64.0f;

        public Level Level
        {
            get { return level; }
        }
        protected Level level;

        public bool IsAlive { get; set; }

        //Removes the defeated enemies from the updating after a certain amount of time.
        protected const float deathTimeMax = 3.0f;
        public float deathTime = deathTimeMax;

        /// <summary>
        /// Position in world space of the bottom center of this enemy.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        protected Vector2 position;

        protected float previousBottom;

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        protected Vector2 velocity;


        protected Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this enemy in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        /// <summary>
        /// returns the enemy's vision rectangle.
        /// </summary>
        protected Rectangle SpotlightRectangle
        {
            get
            {
                int left = (int)Math.Round
                     (Position.X - sprite.Origin.X) +
                     localBounds.X;
                int top = (int)Math.Round
                     (Position.Y - sprite.Origin.Y) +
                     localBounds.Y;

                if ((int)direction == 1)
                    return new Rectangle(
                         left + localBounds.Width,
                         top,
                         spotlightTexture.Width,
                         (spotlightTexture.Height));
                else
                    return new Rectangle(
                         left - spotlightTexture.Width,
                         top,
                         spotlightTexture.Width,
                         (spotlightTexture.Height));
            }
        }

        protected bool iSeeYou;

        public Texture2D spotlightTexture;

        public virtual void OnKilled(Player killedBy)
        {
            IsAlive = false;
            killedSound.Play();
        }


        public virtual void OnHurt(Player hurtby, int dmgAmt)
        {
            this.health -= dmgAmt;
            
            if (health < 1)
            {
                this.OnKilled(hurtby);
            }
            else
                killedSound.Play();
        }
        public Enemy()
        {

        }

        #region Load

        /// <summary>
        /// Loads a particular enemy sprite sheet and sounds.
        /// </summary>
        public virtual void LoadContent()
        {
            LoadGun();
            spotlightTexture = level.Content.Load<Texture2D>("Overlays/spotlight2");
        }

        protected void LoadGun()
        {
            if (canShoot)
            {
                // Load game objects
                arm = new GameObject(Level.Content.Load<Texture2D>("Sprites/Player/Arm_Gun"));

                // Temp bullet count = 12
                bullets = new GameObject[numbullets];
                for (int i = 0; i < numbullets; i++)
                {
                    bullets[i] = new GameObject(Level.Content.Load<Texture2D>("Sprites/Player/Bullet"));
                }

            }
        }

        #endregion

        #region Update

        /// <summary>
        /// Enemy Update Method
        /// </summary>
        public virtual void Update(GameTime gameTime)
        {

        }



        /// <summary>
        /// Detects and resolves all collisions between the enemy and his neighboring
        /// tiles. When a collision is detected, There is some special logic for the Y axis to
        /// handle platforms which behave differently depending on direction of movement.
        /// </summary>
        protected void HandleCollisions()
        {

            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // Reset flag to search for ground collision.
            isOnGround = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                    isOnGround = true;

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            }

                        }
                    }
                }//end X
            }//end Y

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }//end check collisions


        #endregion

        #region Draw
        /// <summary>
        /// Draws the animated enemy, aiming, bullets.
        /// </summary>
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (deathTime < deathTimeMax)//!IsAlive)
            {
                sprite.PlayAnimation(dieAnimation);
            }
            else if (GameScreen.allPlayersDead() ||
                      Level.ReachedExit ||
                      Level.TimeRemaining == TimeSpan.Zero ||
                      waitTime > 0)
            {
                sprite.PlayAnimation(idleAnimation);
            }
            else
            {
                sprite.PlayAnimation(runAnimation);
            }

            // Draw facing the way the enemy is moving.
            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(gameTime, spriteBatch, Position, flip, Color.White);

            if (IsAlive)
            {
                if (canShoot)
                {
                    foreach (GameObject bullet in bullets)
                    {
                        if (bullet.alive)
                        {
                            spriteBatch.Draw(bullet.sprite,
                                bullet.position, Color.White);
                        }
                    }

                    spriteBatch.Draw(
                        arm.sprite,
                        arm.position,
                        null,
                        Color.White,
                        arm.rotation,
                        arm.center,
                        1.0f,
                        flip,
                        0);
                }
            }

        }
        #endregion

    }//end class

}//end namespace
