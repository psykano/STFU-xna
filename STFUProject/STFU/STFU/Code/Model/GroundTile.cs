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
    /// An falling platform is essentially static until a player or enemy lands on or hits it, in which case
    /// it falls down until it hits the ground after a short delay.
    /// </summary>
    class GroundTile : Entity
    {
        protected StaticPhysicsObject physicsObj;
        protected float rotation;

        // Constructor
        public GroundTile(World world, Vector2 position, float width, float height, float rotation)
            : base(world)
        {
            this.Width = ConvertUnits.ToSimUnits(width - 0.1f); // the 0.1f _might_ fix a weird bug in farseer
            this.Height = ConvertUnits.ToSimUnits(height);
            this.rotation = rotation;

            // Make the ground in the physics world
            physicsObj = new StaticPhysicsObject(this, this.world, position, width, height);
            physicsObj.body.Restitution = 0f;
            physicsObj.CollisionCategory = GameConstants.GroundCollisionCategory;
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
                return physicsObj.body.Rotation + rotation;
            }
        }
    }
}
