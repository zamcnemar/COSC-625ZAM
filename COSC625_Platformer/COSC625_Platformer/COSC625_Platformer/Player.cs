using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using COSC625_Platformer.GameObjects;
using COSC625_Platformer.Screens;
using COSC625_Platformer.Levels;

namespace COSC625_Platformer
{
    /// <summary>
    /// Our fearless adventurer!
    /// </summary>
    class Player
    {
        // GameObjects
        GameObject arm;
        GameObject[] bullets;
        
        GameObject[] bullets2;
        public PlayerIndex controller = PlayerIndex.One;

        // Animations
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation celebrateAnimation;
        private Animation dieAnimation;
        private Animation attackAnimation;
        private SpriteEffects flip = SpriteEffects.None;
        private AnimationPlayer sprite;

        // Sounds
        private SoundEffect killedSound;
        private SoundEffect jumpSound;
        private SoundEffect fallSound;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        public bool IsAlive
        {
            get { return isAlive; }
        }
        bool isAlive;

        // Powerup state
        private const float MaxPowerUpTime = 6.0f;
        private float powerUpTime;
        public bool IsPoweredUp
        {
            get { return powerUpTime > 0.0f; }
        }
        private readonly Color[] poweredUpColors = {
                                    Color.Red,
                                    Color.Blue,
                                    Color.Orange,
                                    Color.Yellow,
                                                   };
        private SoundEffect powerUpSound;

        // Physics state
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

        // Input configuration
        private const float MoveStickScale = 1.0f;

        /// <summary>
        /// Gets whether or not the player's feet are on the ground.
        /// </summary>
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        /// <summary>
        /// Current user movement input.
        /// </summary>
        private float movement;

        // Jumping state
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;
	
        // Attacking state
        public bool isAttacking;
        const float MaxAttackTime = 0.33f;
        public float AttackTime;

        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this player in world space.
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

        public Rectangle MeleeRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;
                if (flip == SpriteEffects.FlipHorizontally)
                    return new Rectangle(
                        (left + localBounds.Width),
                        top,
                        localBounds.Width,
                        localBounds.Height);
                else
                    return new Rectangle(
                        (left - localBounds.Width),
                        top,
                        localBounds.Width,
                        localBounds.Height);
            }
        }

        /// <summary>
        /// Constructors a new player.
        /// </summary>
        public Player(Level level, Vector2 position)
        {
            this.level = level;

            LoadContent();

            Reset(position);
        }

        /// <summary>
        /// Loads the player sprite sheet and sounds.
        /// </summary>
        public void LoadContent()
        {
            // Load game objects
            arm = new GameObject(Level.Content.Load<Texture2D>("Sprites/Player/Arm_Gun"));

            // Temp bullet count = 12
            bullets = new GameObject[12];
           
            bullets2 = new GameObject[12];
            for (int i = 0; i < 12; i++)
            {
                bullets[i] = new GameObject(Level.Content.Load<Texture2D>("Sprites/Player/Bullet"));
               
                bullets2[i] = new GameObject(Level.Content.Load<Texture2D>("Sprites/Player/Bullet"));
            }

            // Load animated textures.
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Idle - Ninja"), 0.1f, true);
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run - Ninja"), 0.1f, true);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Jump - Ninja"), 0.1f, false);
            celebrateAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Celebrate - Ninja"), 0.1f, false);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Die - Ninja"), 0.1f, false);
            attackAnimation = new Animation(level.Content.Load<Texture2D>("Sprites/Player/Attack"), 0.1f, false);


            // Calculate bounds within texture size.            
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            // Load sounds.            
            killedSound = Level.Content.Load<SoundEffect>("Sounds/PlayerKilled");
            jumpSound = Level.Content.Load<SoundEffect>("Sounds/PlayerJump");
            fallSound = Level.Content.Load<SoundEffect>("Sounds/PlayerFall");
            powerUpSound = Level.Content.Load<SoundEffect>("Sounds/PowerUp");
        }

        /// <summary>
        /// Resets the player to life.
        /// </summary>
        /// <param name="position">The position to come to life at.</param>
        public void Reset(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            isAlive = true;
            sprite.PlayAnimation(idleAnimation);
            powerUpTime = 0.0f;
        }

        /// <summary>
        /// Handles input, performs physics, and animates the player sprite.
        /// </summary>
        /// <remarks>
        /// We pass in all of the input states so that our game is only polling the hardware
        /// once per frame. We also pass the game's orientation because when using the accelerometer,
        /// we need to reverse our motion when the orientation is in the LandscapeRight orientation.
        /// </remarks>
        public void Update(GameTime gameTime)
        {
            GetInput();

            DoAttack(gameTime);

            ApplyPhysics(gameTime);

            if (IsPoweredUp)
                powerUpTime = Math.Max(0.0f, powerUpTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
 
            if (IsAlive)
            {
                if (isAttacking)
                {
                    sprite.PlayAnimation(attackAnimation);
                }
                else
                {
                    if (Math.Abs(Velocity.X) - 0.02f > 0)
                    {
                        sprite.PlayAnimation(runAnimation);
                    }
                    else if (isJumping)
                    {
                        sprite.PlayAnimation(jumpAnimation);
                    }
                    else
                    {
                        sprite.PlayAnimation(idleAnimation);
                    }
                }
            }

            // Clear input.
            movement = 0.0f;
            isJumping = false;

            if (isOnGround)
                numberOfJumps = 0;

            if (flip == SpriteEffects.FlipHorizontally)
                arm.position = new Vector2(position.X + 5, position.Y - 60);
            else
                arm.position = new Vector2(position.X - 5, position.Y - 60);

            UpdateBullets();
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

                        if (ScreenManager.controls.UpRight(controller))
                        {
                            float armCos = (float)Math.Cos(arm.rotation - MathHelper.PiOver2);
                            float armSin = (float)Math.Sin(arm.rotation - MathHelper.PiOver2);

                            //float armCos = (float)Math.Cos(45.0);
                            //float armSin = (float)Math.Sin(45.0);

                            // bullet.position = Vector2FromAngle(.785,true);

                            bullet.position = new Vector2(
                               arm.position.X + 42 * armCos,
                               arm.position.Y + 42 * armSin);



                            bullet.Velocity = new Vector2(
                                (float)Math.Cos(arm.rotation - MathHelper.PiOver4 + MathHelper.Pi + MathHelper.PiOver2),
                                (float)Math.Sin(arm.rotation - MathHelper.PiOver4 + MathHelper.Pi + MathHelper.PiOver2)) * 15.0f;

                           
                        }


                        else if (ScreenManager.controls.UpLeft(controller))
                        {
                            float armCos = (float)Math.Cos(arm.rotation - MathHelper.PiOver2);
                            float armSin = (float)Math.Sin(arm.rotation - MathHelper.PiOver2);

                            //float armCos = (float)Math.Cos(45.0);
                            //float armSin = (float)Math.Sin(45.0);

                            // bullet.position = Vector2FromAngle(.785,true);

                            bullet.position = new Vector2(
                               arm.position.X + 42 * armCos,
                               arm.position.Y + 42 * armSin);



                            bullet.Velocity = new Vector2(
                                (float)Math.Cos(arm.rotation - MathHelper.PiOver4 - (2 * MathHelper.Pi)),
                                (float)Math.Sin(arm.rotation - MathHelper.PiOver4 - (2 * MathHelper.Pi))) * 15.0f;

                            Console.WriteLine("You are pressing left and up");
                        }



 


                        else if (flip == SpriteEffects.FlipHorizontally) //Facing right
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
                                (float)Math.Cos(arm.rotation -  MathHelper.PiOver2),
                                (float)Math.Sin(arm.rotation -  MathHelper.PiOver2)) * 15.0f;

                            

                        }


                        else //Facing left
                        {
                            float armCos = (float)Math.Cos(arm.rotation + MathHelper.PiOver2);
                            float armSin = (float)Math.Sin(arm.rotation + MathHelper.PiOver2);

                            //Set the initial position of our bullet at the end of our gun arm
                            //42 is obtained be taking the width of the Arm_Gun texture / 2
                            //and   subtracting the width of the Bullet texture / 2. ((96/2)-(12/2))
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

        
       

        private void UpdateBullets()
        {
                
            // Check all of our bullets
            foreach (GameObject bullet in bullets)
            {
                
                // Only update them if they are alive
                if (bullet.alive)
                {

                    // Move our bullet based on it's velocity
                    bullet.position += bullet.Velocity;

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

                    // Check for collisions with the enemies
                    foreach (Enemy enemy in level.enemies)
                    {
                        if (bulletRect.Intersects(enemy.BoundingRectangle) && enemy.IsAlive)
                        {
                            enemy.OnKilled(this);
                            bullet.alive = false;
                        }
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

       


        /// <summary>
        /// Gets player horizontal movement and jump commands from input.
        /// </summary>
        private void GetInput()
        {
            // Get analog horizontal movement.
            movement = ScreenManager.controls.ControllerState(controller).ThumbSticks.Left.X * MoveStickScale;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // If any digital horizontal movement input is found, override the analog movement.
            if (ScreenManager.controls.Left(controller))
            {
                movement = -1.25f;// 9.28.13 // Z - made the guy move more with 
            }
            else if (ScreenManager.controls.Right(controller))
            {
                movement = 1.25f;
            }

            // Check if the player wants to jump.
            isJumping = ScreenManager.controls.Jump(controller);
         

            // Arm Rotation
            // 9.28.13 - Z - Noticed movement is slower with the Thumbstics than with pad or keyboard.
            // I want to change the arm rotation stuff to reflect what you press with left stick so it shoots
            // in the direction you are aiming similar to that of Contra or Super Metroid without affecting
            // movement. Thinking 90 degree angles for that, so that it gives us cardinal direction aiming.
            arm.rotation = (float)Math.Atan2(ScreenManager.controls.ControllerState(controller).ThumbSticks.Left.X, ScreenManager.controls.ControllerState(controller).ThumbSticks.Left.Y);

            if (flip == SpriteEffects.FlipHorizontally) //Facing right
            {
                // If we try to aim behind our head then flip the
                // character around so he doesn't break his arms!
                if (arm.rotation < 0)
                    flip = SpriteEffects.None;

                // If we aren't rotating our arm then set it to the
                // default position. Aiming in front of us.
                if (arm.rotation == 0 && Math.Abs(ScreenManager.controls.ControllerState(controller).ThumbSticks.Left.Length()) < 0.5f)
                    arm.rotation = MathHelper.PiOver2;
            }
            else // Facing left
            {
                // Once again, if we try to aim behind us then 
                // flip our character.
                if (arm.rotation > 0)
                    flip = SpriteEffects.FlipHorizontally;
            }

            // If we're not rotating our arm, default it to 
            //aim in the same direction we're facing.
            if (arm.rotation == 0 && Math.Abs(ScreenManager.controls.ControllerState(controller).ThumbSticks.Left.Length()) < 0.5f)
                arm.rotation = -MathHelper.PiOver2;

            // Shoot = RightTrigger
            if (ScreenManager.controls.Fire(controller))
            {
                FireBullet();
            }

            

            if (ScreenManager.controls.Attack(controller))
            {
                if (AttackTime == 0.0f)
                {
                    isAttacking = true;
                    AttackTime = MaxAttackTime;
                }
            }

            //Pause Game
            if (ScreenManager.controls.Start(controller))
                ScreenManager.gameState = GameState.Pause;
        }

        /// <summary>
        /// Updates the player's velocity and position based on input, gravity, etc.
        /// </summary>
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, gameTime);

            // Apply pseudo-drag horizontally.
            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply velocity.
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions();

            // If the collision stopped us from moving, reset the velocity to zero.
            if (Position.X == previousPosition.X)
                velocity.X = 0;

            if (Position.Y == previousPosition.Y)
                velocity.Y = 0;
        }

        /// <summary>
        /// Calculates the Y velocity accounting for jumping and
        /// animates accordingly.
        /// </summary>
        /// <remarks>
        /// During the accent of a jump, the Y velocity is completely
        /// overridden by a power curve. During the decent, gravity takes
        /// over. The jump velocity is controlled by the jumpTime field
        /// which measures time into the accent of the current jump.
        /// </remarks>
        /// <param name="velocityY">
        /// The player's current velocity along the Y axis.
        /// </param>
        /// <returns>
        /// A new Y velocity if beginning or continuing a jump.
        /// Otherwise, the existing Y velocity.
        /// </returns>
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
                    sprite.PlayAnimation(jumpAnimation);
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

        private void DoAttack(GameTime gameTime)
        {
           // If the player wants to attack
           if (isAttacking)
           {
               // Begin or continue an attack
               if (AttackTime > 0.0f)
               {
                   AttackTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
               }
                else
               {
                   isAttacking = false;
               }
           }
           else
           {
               //Continues not attack or cancels an attack in progress
               AttackTime = 0.0f;
           }
       }           

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

            //For each potentially colliding movable tile.
            foreach (var movableTile in level.movableTiles)
            {
                // Reset flag to search for movable tile collision.
                movableTile.PlayerIsOn = false;

                //check to see if player is on tile.
                if ((BoundingRectangle.Bottom == movableTile.BoundingRectangle.Top + 1) &&
                    (BoundingRectangle.Left >= movableTile.BoundingRectangle.Left - (BoundingRectangle.Width / 2) &&
                    BoundingRectangle.Right <= movableTile.BoundingRectangle.Right + (BoundingRectangle.Width / 2)))
                {
                    movableTile.PlayerIsOn = true;
                }

                bounds = HandleCollision(bounds, movableTile.Collision, movableTile.BoundingRectangle);
            }

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
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }


        private Rectangle HandleCollision(Rectangle bounds, TileCollision collision, Rectangle tileBounds)
        {
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
                else if(collision == TileCollision.Impassable) // Ignore platforms.
                {
                    // Resolve the collision along the X axis.
                    Position = new Vector2(Position.X + depth.X, Position.Y);
 
                    // Perform further collisions with the new bounds.
                    bounds = BoundingRectangle;
                }
            }
            return bounds;
        }



        /// <summary>
        /// Called when the player has been killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This parameter is null if the player was
        /// not killed by an enemy (fell into a hole).
        /// </param>
        public void OnKilled(Enemy killedBy)
        {
            isAlive = false;

            if (killedBy != null)
                killedSound.Play();
            else
                fallSound.Play();

            sprite.PlayAnimation(dieAnimation);
        }

        /// <summary>
        /// Called when this player reaches the level's exit.
        /// </summary>
        public void OnReachedExit()
        {
            sprite.PlayAnimation(celebrateAnimation);
        }

        /// <summary>
        /// Draws the animated player.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Flip the sprite to face the way we are moving.
            if (Velocity.X > 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (Velocity.X < 0)
                flip = SpriteEffects.None;

            // Calculate  a tint color based on power up state.
            Color color;
            if (IsPoweredUp)
            {
                float t = ((float)gameTime.TotalGameTime.TotalSeconds + powerUpTime / MaxPowerUpTime) * 20.0f;
                int colorIndex = (int)t % poweredUpColors.Length;
                color = poweredUpColors[colorIndex];
            }
            else
            {
                color = Color.White;
            }

            // Draw that sprite.
            sprite.Draw(gameTime, spriteBatch, Position, flip, color);

            // Draw the bullets
            foreach (GameObject bullet in bullets)
            {
                if (bullet.alive)
                {
                    spriteBatch.Draw(bullet.sprite,
                        bullet.position, Color.White);
                }
            }

            if (IsAlive)
            {
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

        public void PowerUp()
        {
            powerUpTime = MaxPowerUpTime;
            powerUpSound.Play();
        }

    }
}
