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
    class Gem : Item
    {

        public Gem(Level level, Vector2 position, int randomIndex)
        {
            this.level = level;
            this.basePosition = position;
            active = true;
            PointValue = 30;
            Color = Color.LightSteelBlue;
            basename = "gold";
            this.randomIndex = randomIndex;



            LoadContent();
        }

        public override void OnCollected(Player collectedBy)
        {
            base.OnCollected(collectedBy);
        }

        public override void Update(GameTime gameTime)
        {

            base.Update(gameTime);

        }


        public void LoadContent()
        {
            spriteTexture = Level.Content.Load<Texture2D>("Sprites/Items/" + basename + randomIndex);
            //spriteTexture = Level.Content.Load<Texture2D>("Sprites/Gem");
            origin = new Vector2(spriteTexture.Width / 2.0f, spriteTexture.Height / 2.0f);
            collectedSound = Level.Content.Load<SoundEffect>("Sounds/gemCollected");
            Size = new Rectangle(0, 0, (int)(spriteTexture.Width * Scale), (int)(spriteTexture.Height * Scale));
        }


        public override void Draw(SpriteBatch spritebatch, Color color)
        {
            base.Draw(spritebatch, color);

        }
    }
}
