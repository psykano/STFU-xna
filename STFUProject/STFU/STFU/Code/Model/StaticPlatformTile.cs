/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Common;

namespace STFU
{
    /// <summary>
    /// A static platform doesn't ever move.
    /// </summary>
    class StaticPlatformTile : PlatformTile
    {
        // Constructor
        public StaticPlatformTile(World world, Vector2 position, float width)
            : base(world, width)
        {
            SetUpPlatformTile(position);
            physicsLine.body.BodyType = BodyType.Static;
        }
    }
}
