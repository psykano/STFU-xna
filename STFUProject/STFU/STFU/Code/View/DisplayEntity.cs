/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace STFU
{
    /// <summary>
    /// A display entity is the base class for a frame and an animation since they share
    /// common properties like position, width and height.
    /// </summary>
    abstract class DisplayEntity
    {
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public float Scale { get; set; }
        public float LayerDepth { get; set; }
        public SpriteEffects SpriteEffects { get; set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public SpriteSheet SpriteSheet { get; protected set; }
        public Rectangle SourceRect { get; protected set; }
        public Rectangle DestinationRect { get; protected set; }

        protected Vector2 origin;
        protected Color color;
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

        private Vector2 camPosition;
        private Vector2 camCulling;

        protected bool isVisible()
        {
            camPosition = GameVariables.CamPosition;
            camCulling = GameVariables.CamCulling;
            float halfWidth = this.Width * 0.5f;
            float halfHeight = this.Height * 0.5f;

            if (Position.X + halfWidth < camPosition.X - camCulling.X || Position.X - halfWidth > camPosition.X + camCulling.X)
            {
                return false;
            }
            if (Position.Y + halfHeight < camPosition.Y - camCulling.Y || Position.Y - halfHeight > camPosition.Y + camCulling.Y)
            {
                return false;
            }

            return true;
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

        // Override these

        public virtual void Draw(SpriteBatch spriteBatch)
        {
        }

        public virtual void DrawWithColor(SpriteBatch spriteBatch, Color drawColor)
        {
        }
    }
}
