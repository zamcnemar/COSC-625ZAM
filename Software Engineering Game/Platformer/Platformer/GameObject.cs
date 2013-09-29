using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Platformer
{
    /// <summary>
    /// A general usage class for potential objects like bullets and guns that need to be
    /// rotated, transposed or translated in the game.
    /// </summary>
    class GameObject
    {
        public Texture2D sprite;
        public Vector2 position;
        public float rotation;
        public Vector2 center;
        public Vector2 Velocity;
        public bool alive;

        public Rectangle rectangle 
        {
            get
            {
                int left = (int)position.X;
                int width = sprite.Width;
                int top = (int)position.Y;
                int height = sprite.Height;
                return new Rectangle(left, top, width, height);
            }
        }

        public GameObject(Texture2D loadedTexture)
        {
            rotation = 0.0f;
            position = Vector2.Zero;
            sprite = loadedTexture;
            center = new Vector2(sprite.Width / 2, sprite.Height / 2);
            Velocity = Vector2.Zero;
            alive = false;
        }

    }
}
