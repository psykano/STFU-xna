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
    /// A piece of the map to stand on, like the ground, but one-way: an entity can fall down onto it but can also fly upward through it.
    /// </summary>
    abstract class PlatformTile : Entity
    {
        protected PhysicsLine physicsLine;

        // Constructor
        public PlatformTile(World world, float width)
            : base(world)
        {
            this.Width = ConvertUnits.ToSimUnits(width - 0.1f); // the 0.1f fixes a weird bug in farseer
        }

        public virtual void SetUpPlatformTile(Vector2 position)
        {
            // Make the ground in the physics world
            Vector2 startPosition = new Vector2(position.X - this.ScreenWidth * 0.5f, position.Y);
            Vector2 endPosition = new Vector2(position.X + this.ScreenWidth * 0.5f, position.Y);
            physicsLine = new PhysicsLine(this, this.world, startPosition, endPosition);
            physicsLine.body.Restitution = 0f;
            physicsLine.CollisionCategory = GameConstants.GroundCollisionCategory | GameConstants.PlatformCollisionCategory;
        }

        public override Vector2 Position
        {
            get
            {
                return physicsLine.Position;
            }
        }
    }
}
