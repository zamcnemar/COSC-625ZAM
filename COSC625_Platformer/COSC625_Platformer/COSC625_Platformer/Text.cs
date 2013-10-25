using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using COSC625_Platformer.Screens;

namespace COSC625_Platformer
{
    public class Text
    {
        String text;
        Vector2 position;
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        public Vector2 Size
        {
            get { return ScreenManager.spriteFont.MeasureString(text); }
        }
        Color aColor, bColor, sColor;
        bool active = false;
        public bool Active
        {
            get { return active; }
            set { active = value; }
        }

        public Text(String text, Vector2 position)
        {
            this.text = text;
            this.position = position;
            this.sColor = Color.Blue;
            this.bColor = this.aColor = Color.White;
        }

        public void Update()
        {
            if (this.active && this.aColor == this.bColor)
                this.aColor = this.sColor;
            else if (!this.active && this.aColor == this.sColor)
                this.aColor = this.bColor;

        }

        public void Draw(SpriteBatch spritebatch)
        {
            spritebatch.DrawString(ScreenManager.spriteFont, this.text, this.position, this.aColor);
        }
    }
}
