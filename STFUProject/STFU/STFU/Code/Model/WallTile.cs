/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using FarseerPhysics.Dynamics;

namespace STFU
{
    /// <summary>
    /// A wall tile is a part of the background.
    /// </summary>
    class WallTile : Entity
    {
        // Constructor
        public WallTile(World world, Vector2 position, float width, float height, float rotation)
            : base(world)
        {
            this.Position = ConvertUnits.ToSimUnits(position);
            this.Width = ConvertUnits.ToSimUnits(width - 0.1f); // the 0.1f _might_ fix a weird bug in farseer
            this.Height = ConvertUnits.ToSimUnits(height);
            this.Rotation = rotation;
        }
    }
}
