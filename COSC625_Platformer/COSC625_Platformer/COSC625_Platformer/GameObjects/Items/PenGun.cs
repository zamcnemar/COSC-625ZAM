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

namespace COSC625_Platformer.GameObjects.Items
{
    class PenGun : Item
    {
        ColorChanger colorChanger;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        public PenGun(Level level, Vector2 position)
        {
            this.level = level;
            this.basePosition = position;
            active = true;
            PointValue = 100;

            colorChanger = new ColorChanger(
            .25f, // Time between color changes
            true, // Is the color changer active?
            Color.Violet, Color.Yellow);
            Color = colorChanger.CurrentColor;

            LoadContent();
        }

        public override void OnCollected(Player collectedBy)
        {
            base.OnCollected(collectedBy);
            collectedBy.gotPenGun();
        }

        public override void Update(GameTime gameTime)
        {
            colorChanger.Update(gameTime);
            base.Update(gameTime);

        }


        public void LoadContent()
        {
            spriteTexture = Level.Content.Load<Texture2D>("Sprites/Items/Pengun");
            origin = new Vector2(spriteTexture.Width / 2.0f, spriteTexture.Height / 2.0f);
            collectedSound = Level.Content.Load<SoundEffect>("Sounds/GemCollected");
            Size = new Rectangle(0, 0, (int)(spriteTexture.Width * Scale), (int)(spriteTexture.Height * Scale));
        }


        public override void Draw(SpriteBatch spritebatch, Color color)
        {

            Boundary = new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Size.Width, this.Size.Height);
            spritebatch.Draw(spriteTexture, Position, null, colorChanger.CurrentColor, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
            //base.Draw(spritebatch, colorChanger.CurrentColor);

        }

    }
}
