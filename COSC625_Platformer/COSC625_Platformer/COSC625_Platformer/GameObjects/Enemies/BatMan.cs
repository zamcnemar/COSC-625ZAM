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
    class BatMan : Enemy
    {

        public BatMan(Level level, Vector2 position)
        {
            this.level = level;
            this.position = position;
            this.IsAlive = true;
            this.canShoot = true;
            this.isShooting = true;
            this.spriteSet = "BadGuy";
            this.MoveSpeed *= 4;
            this.MaxWaitTime = .10f;

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
                pacingAI(gameTime, elapsed);

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


        #endregion


        #region Load

        public override void LoadContent()
        {
            base.LoadContent();

            spriteSet = "Sprites/" + spriteSet + "/";

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
