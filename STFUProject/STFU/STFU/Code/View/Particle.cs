/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace STFU
{
    /// <summary>
    /// A single frame is used to draw an entity to the screen.
    /// </summary>
    class Particle : DisplayEntity
    {
        public bool Active { get; set; }
        protected Timer lifeTimer;
        protected bool fadeAway;
        protected bool alphaAway;
        protected float initialOpacity;
        protected float initialAlpha;

        public Particle()
        {
            this.Active = false;
            lifeTimer = new Timer();
        }

        protected void reset()
        {
            lifeTimer.Reset();
        }

        public void ActivateWithDisplayEntity(DisplayEntity displayEntity, float lifetime)
        {
            ActivateWithDisplayEntity(displayEntity, lifetime, false, false, 1f, 1f);
        }

        public void ActivateWithDisplayEntity(DisplayEntity displayEntity, float lifetime, bool fadeAway, bool alphaAway)
        {
            ActivateWithDisplayEntity(displayEntity, lifetime, fadeAway, alphaAway, 1f, 1f);
        }

        public void ActivateWithDisplayEntity(DisplayEntity displayEntity, float lifetime, bool fadeAway, bool alphaAway, float initialOpacity, float initialAlpha)
        {
            this.Active = true;
            lifeTimer.SetDelay(lifetime);
            this.fadeAway = fadeAway;
            this.alphaAway = alphaAway;
            this.initialOpacity = initialOpacity;
            this.initialAlpha = initialAlpha;

            // Extract drawing information from the display entity
            SpriteSheet = displayEntity.SpriteSheet;
            SourceRect = displayEntity.SourceRect;
            Position = displayEntity.Position;
            Scale = displayEntity.Scale;
            Color = displayEntity.Color;
            Rotation = displayEntity.Rotation;
            SpriteEffects = displayEntity.SpriteEffects;
            LayerDepth = displayEntity.LayerDepth +0.001f;
            Width = SourceRect.Width;
            Height = SourceRect.Height;
            origin = new Vector2(Width * 0.5f, Height * 0.5f);
        }

        public void Update(float dt)
        {
            lifeTimer.Update(dt);

            if (fadeAway)
            {
                Opacity = lifeTimer.PercentFromTimeUp() * initialOpacity;
            }

            if (alphaAway)
            {
                Alpha = lifeTimer.PercentFromTimeUp() * initialAlpha;
            }

            if (lifeTimer.IsTimeUp())
            {
                this.Active = false;
            }
        }

        // Draw the particle
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!isVisible())
                return;

            DestinationRect = new Rectangle((int)Position.X, (int)Position.Y, (int)(Width * Scale), (int)(Height * Scale));

            spriteBatch.Draw(SpriteSheet.Sheet, DestinationRect, SourceRect, Color * Opacity, Rotation, origin, SpriteEffects, LayerDepth);
        }
    }
}
