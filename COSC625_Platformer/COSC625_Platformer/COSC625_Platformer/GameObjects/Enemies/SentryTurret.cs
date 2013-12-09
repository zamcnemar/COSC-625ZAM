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
    class SentryTurret : Enemy
    {
        /// <summary>
        /// The Enemy Enumeration states that control the enemy logic.
        /// </summary>
        public enum AI
        {
            Patrol,
            Spotted,
            Alert,
            Cooldown,
        }

        protected AI currentAI = AI.Patrol;

        Animation alertAnimation;
        Animation cooldownAnimation;

        float baseMovespeed = 0.0f;

        public static float maxChaseWaitTime = 1.0f;

        protected float chaseWaitTime = maxChaseWaitTime;

        public static float maxShootWaitTime = 0.5f;

        protected float shootWaitTime = maxShootWaitTime;

        public static float maxcooldownWaitTime = 0.5f;

        protected float cooldownWaitTime = maxcooldownWaitTime;

        public SentryTurret(Level level, Vector2 position)
        {
            this.level = level;
            this.position = position;
            this.IsAlive = true;
            this.spriteSet = "SentryTurret";
            this.MoveSpeed = 0;
            this.MaxWaitTime = .5f;
            this.maxHealth = 4;
            this.health = 4;
            this.canShoot = true;
            this.numbullets = 6;

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

            if(IsAlive)
                UpdateShooting(gameTime);

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (!IsAlive)
                deathTime -= elapsed;
            else  // AI Section
            {
                //ApplyPhysics(gameTime);

                if (currentAI == AI.Patrol)
                {
                    turnAroundAI(gameTime, elapsed);
                }

                if (currentAI == AI.Spotted)
                {
                    
                }

                if (currentAI == AI.Alert)
                {
                    shootemAI(gameTime, elapsed);
                }

                if (currentAI == AI.Cooldown)
                {
                    coolDownAI(gameTime, elapsed);
                }

                foreach (Player p in GameScreen.players)
                {
                    if (SpotlightRectangle.Intersects(p.BoundingRectangle))
                    {
                        if (currentAI == AI.Patrol)
                        {
                            if (SpotlightRectangle.Intersects(p.BoundingRectangle))
                            {
                                currentAI = AI.Alert;
                                iSeeYou = true;
                            }
                            else if (!SpotlightRectangle.Intersects(p.BoundingRectangle))
                            {
                                currentAI = AI.Cooldown;
                                iSeeYou = false;
                            }
                        }
                    }
                }//end for each
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
                    waitTime = MaxWaitTime;
                }
            }
        }

        protected void coolDownAI(GameTime gameTime, float elapsedTime)
        {
            foreach (Player p in GameScreen.players)
            {
                if (!SpotlightRectangle.Intersects(p.BoundingRectangle))
                {
                    if (cooldownWaitTime <= 0.0f)
                    {
                        cooldownWaitTime = maxcooldownWaitTime;
                    }
                    else
                    {
                        cooldownWaitTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                        currentAI = AI.Patrol;
                    }
                }
                else
                {
                    currentAI = AI.Alert;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="elapsedTime"></param>
        protected void shootemAI(GameTime gameTime, float elapsedTime)
        {

            foreach (Player p in GameScreen.players)
            {
                if (SpotlightRectangle.Intersects(p.BoundingRectangle))
                {
                    if (shootWaitTime <= 0.0f)
                    {
                        shootWaitTime = maxShootWaitTime;
                    }
                    else
                    {
                        shootWaitTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                    }
                    iSeeYou = true;
                }
                else if (!SpotlightRectangle.Intersects(p.BoundingRectangle))
                {
                    iSeeYou = false;
                    currentAI = AI.Cooldown;
                }
            }
        }

        public void UpdateShooting(GameTime gameTime)
        {

            aimGun();

            UpdateBullets();

            foreach (Player p in GameScreen.players)
            {
                if (p.IsAlive)
                    DoShoot(gameTime);
            }

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

                    foreach (Player p in GameScreen.players)
                    {
                        if (bulletRect.Intersects(p.BoundingRectangle) && p.IsAlive && p.IsPoweredUp == false)
                        {
                            p.OnKilled(this);
                            bullet.alive = false;
                        }
                        if (bulletRect.Intersects(p.BoundingRectangle) && p.IsAlive && p.IsPoweredUp == true)
                        {
                            bullet.alive = false;
                        }
                        if (bulletRect.Intersects(p.MeleeRectangle) && p.isAttacking)
                        {
                            bullet.alive = false;
                            bulletDeflect.Play(.5f, 0.0f, 0.0f);
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
            if (currentAI == AI.Alert)
            {
                if (ShootTime < 0.0f)
                {
                    ShootTime = MaxShootDelay;
                    //increment angle by 45 to spread by 3 bullets or 22.5 for 5 bullets
                    for (double angle = 45; angle <= 135; angle = angle + 45)
                    {
                        FireBullet(angle);
                    }
                }
                else
                {
                    ShootTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                }
            }
        }

        private void FireBullet(double angle)
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
                        //bullet.Velocity = new Vector2(
                        //    (float)Math.Cos(arm.rotation - MathHelper.PiOver2),
                        //    (float)Math.Sin(arm.rotation - MathHelper.PiOver2)) * 15.0f;

                        bullet.Velocity = new Vector2(
                            (float)Math.Cos(arm.rotation - MathHelper.ToRadians((float)angle)),
                            (float)Math.Sin(arm.rotation - MathHelper.ToRadians((float)angle))) * 15.0f;
                    }
                    else //Facing left
                    {
                        float armCos = (float)Math.Cos(arm.rotation + MathHelper.ToRadians((float)angle));
                        float armSin = (float)Math.Sin(arm.rotation + MathHelper.ToRadians((float)angle));
                        //float armCos = (float)Math.Cos(arm.rotation + MathHelper.PiOver2);
                        //float armSin = (float)Math.Sin(arm.rotation + MathHelper.PiOver2);

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
            LoadGun();
            spotlightTexture = level.Content.Load<Texture2D>("Overlays/spotlight2");

            spriteSet = "Sprites/Enemies/" + spriteSet + "/";

            // Load animations.
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "idle"), 0.20f, false);
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "idle"), 0.15f, true);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "die"), 0.07f, false);
            alertAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "alert"), 0.07f, false);
            cooldownAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "cooldown"), 0.07f, false);

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

        private void LoadGun()
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


        #region Draw

        public override void Draw(GameTime gameTime, SpriteBatch spritebatch)
        {
            //Draws a visual representation of the enemy's field of view.
            //if (iSeeYou)
            //spritebatch.Draw(spotlightTexture, SpotlightRectangle, null, Color.Red);
            //else
            //spritebatch.Draw(spotlightTexture, SpotlightRectangle, null, Color.Green);

            if (deathTime < deathTimeMax)//!IsAlive)
            {
                sprite.PlayAnimation(dieAnimation);
            }
            else if (GameScreen.allPlayersDead() ||
                      Level.ReachedExit ||
                      Level.TimeRemaining == TimeSpan.Zero)
            {
                sprite.PlayAnimation(idleAnimation);
            }
            else if (currentAI == AI.Patrol)
            {
                sprite.PlayAnimation(idleAnimation);
            }
            else if (currentAI == AI.Spotted)
            {
                sprite.PlayAnimation(alertAnimation);
            }
            else if (currentAI == AI.Alert)
            {
                sprite.PlayAnimation(alertAnimation);
            }
            else if (currentAI == AI.Cooldown)
            {
                sprite.PlayAnimation(cooldownAnimation);
            }

            // Draw facing the way the enemy is moving.
            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(gameTime, spritebatch, Position, flip, Color.White);

            if (IsAlive)
            {
                if (canShoot)
                {
                    foreach (GameObject bullet in bullets)
                    {
                        if (bullet.alive)
                        {
                            spritebatch.Draw(bullet.sprite,
                                bullet.position, Color.White);
                        }
                    }

                    spritebatch.Draw(
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

    }
}
