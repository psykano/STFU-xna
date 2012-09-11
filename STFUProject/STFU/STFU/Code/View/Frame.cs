/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace STFU
{
    /// <summary>
    /// A single frame is used to draw an entity to the screen.
    /// </summary>
    class Frame : DisplayEntity
    {
        public float Rotation { get; set; }
        private Color color;
        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                color = value;
            }
        }
        public SpriteEffects SpriteEffects { get; set; }
        public float LayerDepth { get; set; }
        protected SpriteSheet spriteSheet;
        protected Rectangle sourceRect;
        protected string name;
        protected float scale;
        protected Vector2 origin;

        public void Initialize(SpriteSheet spriteSheet, Vector2 position, string name, Color color, float scale, SpriteEffects spriteEffects)
        {
            // Keep a local copy of the values passed in
            this.spriteSheet = spriteSheet;
            Position = position;
            this.name = name;
            this.Color = color;
            this.scale = scale;
            SpriteEffects = spriteEffects;
            LayerDepth = 0.2f; // default layer depth

            sourceRect = spriteSheet.Map[name];
            Width = sourceRect.Width;
            Height = sourceRect.Height;
            origin = new Vector2(Width * 0.5f, Height * 0.5f);
        }

        // Draw the Frame
        public void Draw(SpriteBatch spriteBatch)
        {
            /*
            Rectangle sourceRect = spriteSheet.Map[name];
            Width = (int)(sourceRect.Width * scale);
            Height = (int)(sourceRect.Height * scale);
             */

            if (!isVisible())
                return;

            Rectangle destinationRect = new Rectangle((int)Position.X, (int)Position.Y, (int)(Width * scale), (int)(Height * scale));

            spriteBatch.Draw(spriteSheet.Sheet, destinationRect, sourceRect, Color, Rotation, origin, SpriteEffects, LayerDepth);
        }

        // Set the opacity
        public float Opacity { get; set; }

        // Set the alpha
        public float Alpha
        {
            get
            {
                return (Color.A / 255f);
            }
            set
            {
                if (value > 1f)
                    value = 1f;
                else if (value < 0f)
                    value = 0f;
                color.A = (byte)(value * 255f);
            }
        }
    }
}
