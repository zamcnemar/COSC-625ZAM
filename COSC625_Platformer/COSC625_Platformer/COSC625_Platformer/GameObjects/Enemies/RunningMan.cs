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
    class RunningMan : Enemy
    {

        public RunningMan(Level level, Vector2 position)
        {
            this.level = level;
            this.position = position;
            this.IsAlive = true;
            this.canShoot = false;
            this.isShooting = false;
            this.spriteSet = "BadGuy";
            this.MoveSpeed *= 6;

            this.MaxJumpTime = 0.35f;
            this.JumpLaunchVelocity = -3500.0f;
            this.GravityAcceleration = 20000.0f;
            this.MaxFallSpeed = 2000f;
            this.JumpControlPower = 0.14f;

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
                RunningAI(gameTime, elapsed);

                if (isOnGround)
                    numberOfJumps = 0;
            }
        }

        /// <summary>
        /// Enemy Character runs until they hit a wall and turns around and continues running
        /// The character ignores all clif
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="elapsedTime"></param>
        protected void RunningAI(GameTime gameTime, float elapsedTime)
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
                if (Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable)
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
            base.Draw(gameTime, spritebatch);
        }

        #endregion

    }
}
