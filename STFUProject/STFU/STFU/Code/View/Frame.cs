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
        protected string name;

        public void Initialize(SpriteSheet spriteSheet, Vector2 position, string name, Color color, float scale, SpriteEffects spriteEffects)
        {
            // Keep a local copy of the values passed in
            SpriteSheet = spriteSheet;
            Position = position;
            this.name = name;
            this.Color = color;
            this.Opacity = 1f;
            this.Scale = scale;
            SpriteEffects = spriteEffects;
            LayerDepth = 0.2f; // default layer depth

            SourceRect = spriteSheet.Map[name];
            Width = SourceRect.Width;
            Height = SourceRect.Height;
            origin = new Vector2(Width * 0.5f, Height * 0.5f);
        }

        // Draw the Frame
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!isVisible())
                return;

            DestinationRect = new Rectangle((int)Position.X, (int)Position.Y, (int)(Width * Scale), (int)(Height * Scale));

            spriteBatch.Draw(SpriteSheet.Sheet, DestinationRect, SourceRect, Color * Opacity, Rotation, origin, SpriteEffects, LayerDepth);
        }
    }
}
