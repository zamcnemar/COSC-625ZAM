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
    class SuperBeast : Enemy
    {
        /// <summary>
        /// The Enemy Enumeration states that control the enemy logic.
        /// </summary>
        public enum AI
        {
            Pacing,
            Prowling,
            Charging,
            Unprowling,
            Shooting
        }

        /// <summary>
        /// The direction this enemy is facing and moving along the X axis.
        /// </summary>
        protected AI currentAI = AI.Pacing;

        Animation prowlAnimation;
        Animation chargeAnimation;
        Animation unprowlAnimation;


        float baseMovespeed = 64.0f;

        public static float maxChaseWaitTime = 1.0f;

        protected float chaseWaitTime = maxChaseWaitTime;

        public static float maxShootWaitTime = 0.5f;

        protected float shootWaitTime = maxShootWaitTime;

        public SuperBeast(Level level, Vector2 position)
        {
            this.level = level;
            this.position = position;
            this.IsAlive = true;
            this.spriteSet = "SuperBeast";
            this.MoveSpeed = baseMovespeed * 2;
            this.MaxWaitTime = 0.001f;
            this.maxHealth = 10;
            this.health = 10;

            LoadContent();
        }

        public override void OnKilled(Player killedBy)
        {
            base.OnKilled(killedBy);
        }

        public override void OnHurt(Player hurtby, int dmgAmt)
        {
            base.OnHurt(hurtby, dmgAmt);
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

                if (currentAI == AI.Pacing)
                {
                    pacingAI(gameTime, elapsed);
                }

                if (currentAI == AI.Prowling)
                {
                    prowlAI(gameTime, elapsed);
                }

                if (currentAI == AI.Charging)
                {
                    chargeAI(gameTime, elapsed);
                }

                if (currentAI == AI.Unprowling)
                {
                    unprowlAI(gameTime, elapsed);
                }

                if (currentAI == AI.Shooting)
                {
                    shootemAI(gameTime, elapsed);
                }

                if (SpotlightRectangle.Intersects(Level.Player.BoundingRectangle))
                {
                    if (currentAI != AI.Charging && currentAI == AI.Pacing)
                    {
                        currentAI = AI.Prowling;
                    }
                    iSeeYou = true;
                }
                else
                {
                    iSeeYou = false;
                }
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

        protected void turnAroundAI(GameTime gameTime, float elapsedTime)
        {
            // wait and turn at impassible blocks.
            if (waitTime > 0)
            {
                // Wait for some amount of time.
                waitTime = Math.Max(0.0f, waitTime - elapsedTime);
                if (waitTime <= 0.0f)
                {
                    // Then turn around.
                    direction = (FaceDirection)(-(int)direction);
                    currentAI = AI.Unprowling;
                    
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="elapsedTime"></param>
        protected void prowlAI(GameTime gameTime, float elapsedTime)
        {
            MoveSpeed = 0;
            
            if (chaseWaitTime <= 0.0f && currentAI != AI.Charging)
            {
                currentAI = AI.Charging;
                chaseWaitTime = maxChaseWaitTime;
            }
            else
                chaseWaitTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="elapsedTime"></param>
        protected void chargeAI(GameTime gameTime, float elapsedTime)
        {
            MoveSpeed = baseMovespeed * 8;
            pacingAI(gameTime, elapsedTime);

            if (currentAI == AI.Charging && waitTime > 0)
            {
                turnAroundAI(gameTime, elapsedTime);
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="elapsedTime"></param>
        protected void unprowlAI(GameTime gameTime, float elapsedTime)
        {
            MoveSpeed = 0;

            if (chaseWaitTime <= 0.0f && currentAI != AI.Charging)
            {
                currentAI = AI.Shooting;
                chaseWaitTime = maxChaseWaitTime;
            }
            else
                chaseWaitTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;


            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="elapsedTime"></param>
        protected void shootemAI(GameTime gameTime, float elapsedTime)
        {

            if (shootWaitTime <= 0.0f && currentAI != AI.Charging)
            {
                currentAI = AI.Prowling;
                shootWaitTime = maxShootWaitTime;
            }
            else
            {
                shootWaitTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                // shoot the missle

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


        #endregion


        #region Load

        public override void LoadContent()
        {
            base.LoadContent();

            spriteSet = "Sprites/Enemies/" + spriteSet + "/";

            // Load animations.
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Run"), 0.20f, true);
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.15f, true);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Die"), 0.07f, false);
            prowlAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Prowl"), 0.15f, false);
            chargeAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Charge"), 0.05f, true);
            unprowlAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "UnProwl"), 0.05f, false);



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
            //Draws a visual representation of the enemy's field of view.
            if (iSeeYou)
                spritebatch.Draw(spotlightTexture, SpotlightRectangle, null, Color.Red);
            else
                spritebatch.Draw(spotlightTexture, SpotlightRectangle, null, Color.Green);

            if (deathTime < deathTimeMax)//!IsAlive)
            {
                sprite.PlayAnimation(dieAnimation);
            }
            else if (!Level.Player.IsAlive ||
                      Level.ReachedExit ||
                      Level.TimeRemaining == TimeSpan.Zero)
            {
                sprite.PlayAnimation(idleAnimation);
            }
            else if (currentAI == AI.Pacing)
            {
                    sprite.PlayAnimation(runAnimation);
            }
            else if (currentAI == AI.Prowling)
            {
                sprite.PlayAnimation(prowlAnimation);
            }
            else if (currentAI == AI.Charging)
            {
                sprite.PlayAnimation(chargeAnimation);
            }
            else if (currentAI == AI.Unprowling)
            {
                sprite.PlayAnimation(unprowlAnimation);
            }
            else if (currentAI == AI.Shooting)
            {

            }

            // Draw facing the way the enemy is moving.
            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(gameTime, spritebatch, Position, flip, Color.White);


        }

        #endregion

    }
}
