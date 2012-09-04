/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Common;

namespace STFU
{
    /// <summary>
    /// A spike is an instant kill for living entities that touch it (with the exception of bosses).
    /// </summary>
    class SpikeTile : Entity
    {
        protected StaticPhysicsObject physicsObj;

        // Constructor
        public SpikeTile(World world, Vector2 position, float width, float height)
            : base(world)
        {
            this.Width = ConvertUnits.ToSimUnits(width);
            this.Height = ConvertUnits.ToSimUnits(height);

            // Make the spike in the physics world
            physicsObj = new StaticPhysicsObject(this, this.world, position, width, height);
            physicsObj.body.Restitution = 0f;
            physicsObj.CollisionCategory = GameConstants.GroundCollisionCategory | GameConstants.DeathCollisionCategory;
        }

        public override Vector2 Position
        {
            get
            {
                return physicsObj.Position;
            }
        }

        public override float Rotation
        {
            get
            {
                return physicsObj.body.Rotation;
            }
        }
    }
}
