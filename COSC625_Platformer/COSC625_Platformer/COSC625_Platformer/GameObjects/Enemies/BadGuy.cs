using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using COSC625_Platformer.Screens;
using COSC625_Platformer.Levels;

namespace COSC625_Platformer.GameObjects.Enemies
{
    class BadGuy : Enemy
    {

        public BadGuy(Level level, Vector2 position)
        {
            this.level = level;
            this.position = position;
            this.IsAlive = true;
            this.canShoot = true;
            this.isShooting = true;
            this.spriteSet = "BadGuy";
            this.MoveSpeed *= 2;

            LoadContent();
        }

        public override void OnKilled(Player killedBy)
        {
            base.OnKilled(killedBy);
        }

        #region Update

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!IsAlive)
                deathTime -= elapsed;
            else  // AI Section
            {
                ApplyPhysics(gameTime);
                UpdateShooting(gameTime);
                pacingAI(gameTime, elapsed);

                if (isOnGround)
                    numberOfJumps = 0;
            }
        }

        protected void pacingAI(GameTime gameTime, float elapsedTime)
        {
            // Calculate tile position based on the side we are walking towards.
            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);

            // wait and turn at impassible blocks.
            if (waitTime > 0)
            {
                // Wait for some amount of time.
                waitTime = Math.Max(0.0f, waitTime - elapsedTime);
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
                    velocity = new Vector2((int)direction * MoveSpeed * elapsedTime, 0.0f);
                    position = position + velocity;
                }
            }
        }


        public void UpdateShooting(GameTime gameTime)
        {

            aimGun();

            UpdateBullets();

            if (Level.Player.IsAlive)
                DoShoot(gameTime);

            if (flip == SpriteEffects.FlipHorizontally)
                arm.position = new Vector2(position.X + 5, position.Y - 60);
            else
                arm.position = new Vector2(position.X - 5, position.Y - 60);

        }


        /// <summary>
        /// Updates the Enemy's velocity and position based gravity, etc.
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
            this.HandleCollisions();

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

                    if (bulletRect.Intersects(level.Player.BoundingRectangle) && level.Player.IsAlive && level.Player.IsPoweredUp == false)
                    {
                        level.Player.OnKilled(this);
                        bullet.alive = false;
                    }
                    if (bulletRect.Intersects(level.Player.BoundingRectangle) && level.Player.IsAlive && level.Player.IsPoweredUp == true)
                    {
                        bullet.alive = false;
                    }
                    if (bulletRect.Intersects(level.Player.MeleeRectangle) && level.Player.isAttacking)
                    {
                        bullet.alive = false;
                        bulletDeflect.Play(.5f, 0.0f, 0.0f);
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
        private void aimGun()
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

        }

        private void DoShoot(GameTime gameTime)
        {
            if (isShooting)
            {
                if (ShootTime < 0.0f)
                {
                    ShootTime = MaxShootDelay;
                    FireBullet();
                }
                else
                {
                    ShootTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
            }
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



        #endregion
        

        #region Load

        public override void LoadContent()
        {
            base.LoadContent();

            spriteSet = "Sprites/Enemies/" + spriteSet + "/";

            // Load animations.
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Run"), 0.10f, true);
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.15f, true);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Die"), 0.07f, false);

            sprite.PlayAnimation(idleAnimation);

            // Calculate bounds within texture size.
            int width = (int)(idleAnimation.FrameWidth * 0.35);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.7);
            int top = idleAnimation.FrameHeight - height;
            this.localBounds = new Rectangle(left, top, width, height);

            // Load Sounds.
            killedSound = level.Content.Load<SoundEffect>("Sounds/MonsterKilled");
            jumpSound = Level.Content.Load<SoundEffect>("Sounds/PlayerJump");
            fallSound = Level.Content.Load<SoundEffect>("Sounds/PlayerFall");
            bulletDeflect = Level.Content.Load<SoundEffect>("Sounds/metal weapon");

        }

        #endregion


        #region Draw

        public override void Draw(GameTime gameTime, SpriteBatch spritebatch)
        {
            base.Draw(gameTime,spritebatch);
        }

        #endregion

    }
}
