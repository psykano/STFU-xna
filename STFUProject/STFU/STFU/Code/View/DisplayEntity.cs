/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace STFU
{
    /// <summary>
    /// A display entity is the base class for a frame and an animation since they share
    /// common properties like position, width and height.
    /// </summary>
    abstract class DisplayEntity
    {
        public Vector2 Position { get; set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }

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
    }
}
