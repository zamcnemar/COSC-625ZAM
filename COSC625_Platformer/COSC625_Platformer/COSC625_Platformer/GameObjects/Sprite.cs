using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using COSC625_Platformer.Screens;

namespace COSC625_Platformer.GameObjects
{
    public class Sprite
    {
        public Vector2 Position = new Vector2(0, 0);
        public Texture2D spriteTexture;
        public Rectangle Size;
        public float Scale = 1.0f;
        public int speed;
        public bool active = false;
        public Rectangle Boundary;

        public virtual void LoadContent(ContentManager thecontentmanager, String assetname)
        {
            spriteTexture = thecontentmanager.Load<Texture2D>(assetname);
            Size = new Rectangle(0, 0, (int)(spriteTexture.Width * Scale), (int)(spriteTexture.Height * Scale));
        }

        //allows for specialized classes to overide the loadContent method above when the classes contain
        //a LoadContent method that contains a specific image.
        public virtual void LoadContent(ContentManager thecontentmanager)
        {

        }

        public virtual void Draw(SpriteBatch spritebatch)
        {
            Boundary = new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Size.Width, this.Size.Height);
            spritebatch.Draw(spriteTexture, Position, new Rectangle(0, 0, spriteTexture.Width, spriteTexture.Height), Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }

        public virtual void Draw(SpriteBatch spritebatch, Color color)
        {
            Boundary = new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Size.Width, this.Size.Height);
            spritebatch.Draw(spriteTexture, Position, new Rectangle(0, 0, spriteTexture.Width, spriteTexture.Height), color, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }

    }
}
