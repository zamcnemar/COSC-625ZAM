using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using COSC625_Platformer.GameObjects;
using COSC625_Platformer.Levels;

namespace COSC625_Platformer
{
    /// <summary>
    /// Facing direction along the X axis.
    /// </summary>
    enum FaceDirection
    {
        Left = -1,
        Right = 1,
    }

    /// <summary>
    /// A monster who is impeding the progress of our fearless adventurer.
    /// </summary>
    class Enemy
    {
        // GameObjects
        GameObject arm;
        GameObject[] bullets;

        // Animations
        private Animation runAnimation;
        private Animation idleAnimation;
        private Animation dieAnimation;
        private SpriteEffects flip = SpriteEffects.None;
        private AnimationPlayer sprite;

        // Sounds
        private SoundEffect killedSound;
        private SoundEffect jumpSound;
        private SoundEffect fallSound;

        // Constants for controling horizontal movement
        private const float MoveAcceleration = 13000.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.14f;

        public bool canShoot { get; set; }

        /// <summary>
        /// Gets whether or not the player's feet are on the ground.
        /// </summary>
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        // Jumping state
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;

        /// <summary>
        /// The direction this enemy is facing and moving along the X axis.
        /// </summary>
        private FaceDirection direction = FaceDirection.Left;

        /// <summary>
        /// How long this enemy has been waiting before turning around.
        /// </summary>
        private float waitTime;

        /// <summary>
        /// How long to wait before turning around.
        /// </summary>
        private const float MaxWaitTime = 0.5f;

        /// <summary>
        /// The speed at which this enemy moves along the X axis.
        /// </summary>
        private const float MoveSpeed = 64.0f;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        public bool IsAlive { get; set; }

        /// <summary>
        /// Position in world space of the bottom center of this enemy.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        Vector2 position;

        private float previousBottom;

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        Vector2 velocity;


        private Rectangle localBounds;
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

        public void OnKilled(Player killedBy)
        {
            IsAlive = false;
            killedSound.Play();
        }

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public Enemy(Level level, Vector2 position, string spriteSet)
        {
            this.level = level;
            this.position = position;
            this.IsAlive = true;
            this.canShoot = true;

            LoadContent(spriteSet);
        }

        #region Load

        /// <summary>
        /// Loads a particular enemy sprite sheet and sounds.
        /// </summary>
        public void LoadContent(string spriteSet)
        {
            if(canShoot)
            {
                // Load game objects
                arm = new GameObject(Level.Content.Load<Texture2D>("Sprites/Player/Arm_Gun"));

                // Temp bullet count = 12
                bullets = new GameObject[1];
                for (int i = 0; i < 1; i++)
                {
                    bullets[i] = new GameObject(Level.Content.Load<Texture2D>("Sprites/Player/Bullet"));
                }

            }

            // Load animations.
            spriteSet = "Sprites/" + spriteSet + "/";
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Run"), 0.1f, true);
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.15f, true);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Die"), 0.07f, false);

            sprite.PlayAnimation(idleAnimation);

            // Calculate bounds within texture size.
            int width = (int)(idleAnimation.FrameWidth * 0.35);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.7);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            // Load Sounds.
            killedSound = level.Content.Load<SoundEffect>("Sounds/MonsterKilled");
            jumpSound = Level.Content.Load<SoundEffect>("Sounds/PlayerJump");
            fallSound = Level.Content.Load<SoundEffect>("Sounds/PlayerFall");
        }

        #endregion

        #region Update

        /// <summary>
        /// Paces back and forth along a platform, waiting at either end.
        /// </summary>
        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!IsAlive)
                return;

            ApplyPhysics(gameTime);

            if (canShoot)
            {
                aimGun();

                UpdateBullets();

                if (flip == SpriteEffects.FlipHorizontally)
                    arm.position = new Vector2(position.X + 5, position.Y - 60);
                else
                    arm.position = new Vector2(position.X - 5, position.Y - 60);
            }


            // Calculate tile position based on the side we are walking towards.
            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);

            // wait and turn at impassible blocks.
            if (waitTime > 0)
            {
                // Wait for some amount of time.
                waitTime = Math.Max(0.0f, waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (waitTime <= 0.0f)
                {
                    // Then turn around.
                    direction = (FaceDirection)(-(int)direction);
                }
            }
            else
            {
                // If we are about to run into a wall or off a cliff, start waiting.
                if (Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable ||
                    Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable)
                {
                    waitTime = MaxWaitTime;
                }
                else
                {
                    // Move in the current direction.
                    velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f);
                    position = position + velocity;
                } 
            }

            isJumping = false;

            if (isOnGround)
                numberOfJumps = 0;



        }


        /// <summary>
        /// Updates the player's velocity and position based on input, gravity, etc.
        /// </summary>
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            if (!IsOnGround)
                velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, gameTime);

            // Apply pseudo-drag horizontally.
            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            // Apply velocity.
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions();

            // If the collision stopped us from moving, reset the velocity to zero.
            if (Position.X == previousPosition.X)
                velocity.X = 0;

            if (Position.Y == previousPosition.Y)
            {
                velocity.Y = 0;
            }
        }



        private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (isJumping)
            {
                // Begin or continue a jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    if (jumpTime == 0.0f)
                        jumpSound.Play();

                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    sprite.PlayAnimation(runAnimation);
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump and has double jumps
                    if (velocityY > -MaxFallSpeed * 0.5f && !wasJumping && numberOfJumps < 1)
                    {
                        velocityY =
                            JumpLaunchVelocity * (0.5f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                        jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                        numberOfJumps++;
                    }
                    else
                    {
                        // Reached the apex of the jump
                        jumpTime = 0.0f;
                    }
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;

            return velocityY;
        }
        private int numberOfJumps = 0;


        private void UpdateBullets()
        {
            // Check all of our bullets
            foreach (GameObject bullet in bullets)
            {

                // Only update them if they are alive
                if (bullet.alive)
                {

                    // Move our bullet based on it's velocity
                    bullet.position += (bullet.Velocity) / 2;

                    // Rectangle the size of the screen so bullets that 
                    // leave the screen are deleted.
                    Rectangle screenRect = new Rectangle((int)level.cameraPosition, (int)level.cameraPositionYAxis, Game1.screenWidth, Game1.screenHeight);
                    if (!screenRect.Contains(new Point(
                        (int)bullet.position.X,
                        (int)bullet.position.Y)))
                    {
                        bullet.alive = false;
                        continue;
                    }

                    // Collision rectangle for each bullet - Will also be 
                    // used for collisions with enemies
                    Rectangle bulletRect = new Rectangle(
                        (int)bullet.position.X - bullet.sprite.Width * 2,
                        (int)bullet.position.Y - bullet.sprite.Height * 2,
                        bullet.sprite.Width * 4,
                        bullet.sprite.Height * 4);

                    if (bulletRect.Intersects(level.Player.BoundingRectangle) && level.Player.IsAlive)
                    {
                        level.Player.OnKilled(this);
                        bullet.alive = false;
                    }

                    // Everything below here can below deleted if you want
                    // your bullets to shoot through all tiles.

                    // Look for adjacent tiles to the bullet
                    Rectangle bounds = new Rectangle(
                        bulletRect.Center.X - 6,
                        bulletRect.Center.Y - 6,
                        bulletRect.Width / 4,
                        bulletRect.Height / 4);
                    int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
                    int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
                    int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
                    int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;


                    // Fore each potentially colliding tile
                    for (int y = topTile; y <= bottomTile; ++y)
                    {
                        for (int x = leftTile; x <= rightTile; ++x)
                        {
                            TileCollision collision = Level.GetCollision(x, y);

                            // If we collide with an impassible or Platform tile
                            // then delete our bullet
                            if (collision == TileCollision.Impassable ||
                                collision == TileCollision.Platform)
                            {
                                if (bulletRect.Intersects(bounds))
                                    bullet.alive = false;
                            }

                        }// End for loop x axis

                    }// End for loop y axis

                }// End IF alive

            }// End Foreach
        }

        // Aim Enemy Gun
        public void aimGun()
        {
            // if enemy is facing right, aim right
            if (flip == SpriteEffects.FlipHorizontally)//Facing right
            {
                if (direction == FaceDirection.Right)
                {
                    flip = SpriteEffects.None;
                    arm.rotation = MathHelper.PiOver2;
                }
            }
            else // Facing left
            {
                // if enemy is facing left, aim left.
                if (direction == FaceDirection.Left)
                {
                    flip = SpriteEffects.FlipHorizontally;
                    arm.rotation = -MathHelper.PiOver2;
                }
            }


            if (Level.Player.IsAlive)
                FireBullet();

        }

        private void FireBullet()
        {
            foreach (GameObject bullet in bullets)
            {
                // Find a bullet that isn't alive
                if (!bullet.alive)
                {
                    //And set it to alive
                    bullet.alive = true;

                    if (flip == SpriteEffects.FlipHorizontally) //Facing right
                    {
                        float armCos = (float)Math.Cos(arm.rotation - MathHelper.PiOver2);
                        float armSin = (float)Math.Sin(arm.rotation - MathHelper.PiOver2);

                        // Set the initial position of our bullets at the end of our gun arm
                        // 42 is obtained by taking the width of the Arm_Gun texture / 2
                        // and subtracting the width of the Bullet texture / 2. ((96/2)=(12/2))
                        bullet.position = new Vector2(
                            arm.position.X + 42 * armCos,
                            arm.position.Y + 42 * armSin);

                        // And give it a velocity of the direction we're aiming.
                        // Increae/decrease speed by changeing 15.0f
                        bullet.Velocity = new Vector2(
                            (float)Math.Cos(arm.rotation - MathHelper.PiOver2),
                            (float)Math.Sin(arm.rotation - MathHelper.PiOver2)) * 15.0f;
                    }
                    else //Facing left
                    {
                        float armCos = (float)Math.Cos(arm.rotation + MathHelper.PiOver2);
                        float armSin = (float)Math.Sin(arm.rotation + MathHelper.PiOver2);

                        //Set the initial position of our bullet at the end of our gun arm
                        //42 is obtained be taking the width of the Arm_Gun texture / 2
                        //and subtracting the width of the Bullet texture / 2. ((96/2)-(12/2))
                        bullet.position = new Vector2(
                            arm.position.X - 42 * armCos,
                            arm.position.Y - 42 * armSin);

                        //And give it a velocity of the direction we're aiming.
                        //Increase/decrease speed by changing 15.0f
                        bullet.Velocity = new Vector2(
                           -armCos,
                           -armSin) * 15.0f;
                    }

                    return;
                }// End if
            }// End foreach
        }// End FireBullets();

        /// <summary>
        /// Detects and resolves all collisions between the player and his neighboring
        /// tiles. When a collision is detected, the player is pushed away along one
        /// axis to prevent overlapping. There is some special logic for the Y axis to
        /// handle platforms which behave differently depending on direction of movement.
        /// </summary>
        private void HandleCollisions()
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
                }
            }

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }//end check collisions

        #endregion

        #region Draw
        /// <summary>
        /// Draws the animated enemy, aiming, bullets.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (!IsAlive)
            {
                sprite.PlayAnimation(dieAnimation);
            }
            else if (!Level.Player.IsAlive ||
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
