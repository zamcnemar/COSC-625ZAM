using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace COSC625_Platformer.GameObjects
{
    public sealed class ColorChanger
    {
        private readonly Color[] colors;
        private float elapsed;
        private int nextColorIndex = 1;
        private readonly float timeBetweenColors;

        /// <summary>
        /// Gets the current color of the object
        /// </summary
        /// 
        public Color CurrentColor
        {
            get;
            private set;
        }

        /// <summary>
        /// Get or Set whether or not the ColorChanger is active
        /// </summary
        /// 
        public bool IsActive { get; set; }

        public ColorChanger(float timeBetweenColors, bool isActive, params Color[] colors)
        {
            this.colors = colors;
            this.timeBetweenColors = timeBetweenColors;
            this.IsActive = isActive;

            Reset(IsActive);
        }

        public void Reset(bool isActive)
        {
            //Set our initial color. If no colors OR we're not active
            //we set the CurrentColor to White. Otherwise we'll use
            //the first color in our list of 'colors'
            if (!IsActive || colors.Length <= 0)
                CurrentColor = Color.White;
            else
                CurrentColor = colors[0];

            nextColorIndex = 1;
            elapsed = 0f;
            IsActive = isActive;
        }

        public void Update(GameTime gameTime)
        {
            //If we deactivate the ColorChanger or we only
            //use a single color then we don't need to update
            if (!IsActive || colors.Length == 1)
                return;

            //Keep track of the time so we know when to change colors
            elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;

            //If it's time to change colors, do so!
            if (elapsed >= timeBetweenColors)
            {
                //If we've passed the last color, start at the beginning
                if (++nextColorIndex >= colors.Length)
                    nextColorIndex = 0;

                //Reset the time so we can start over again
                elapsed = 0f;
            }

            //Interpolate our CurrentColor through our list of colors!
            CurrentColor = Color.Lerp(
                CurrentColor,
                colors[nextColorIndex],
                elapsed / timeBetweenColors);
        }
    }

}
