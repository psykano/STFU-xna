/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;

namespace STFU
{
    /// <summary>
    /// An entity is the base class for everything material in the game.
    /// </summary>
    abstract class Entity
    {
        protected World world;
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visible { get; set; }

        public Entity(World world)
        {
            this.world = world;
            this.Visible = true;
        }

        public virtual void Update(float dt)
        {
        }

        public virtual Vector2 Position { get; set; }
        public virtual float Rotation { get; set; }

        public virtual Vector2 ScreenPosition
        {
            get
            {
                return ConvertUnits.ToDisplayUnits(Position);
            }
        }
        public virtual float ScreenRotation
        {
            get
            {
                return Rotation;
            }
        }

        public float ScreenWidth
        {
            get
            {
                return ConvertUnits.ToDisplayUnits(Width);
            }
        }
        public float ScreenHeight
        {
            get
            {
                return ConvertUnits.ToDisplayUnits(Height);
            }
        }

        public Vector2 PreviousPosition { get; set; }
        public Vector2 SmoothedPosition { get; set; }
        public float PreviousRotation { get; set; }
        public float SmoothedRotation { get; set; }

        // needs to be refactored since it's the same as in PhysicsCharacter ***
        protected bool collidedWithOneWayPlatform(Fixture fix1, Fixture fix2)
        {
            if (fix2.CollisionCategories.HasFlag(GameConstants.PlatformCollisionCategory))
            {
                return true;
            }

            return false;
        }

        public bool OnCollisionWithOneWayPlatform(Fixture fix1, Fixture fix2)
        {
            // as a precaution to help make sure we don't bounce off the platform by being stuck halfway under
            Entity platform = fix2.Body.UserData as Entity;
            if (fix1.Body.Position.Y > platform.Position.Y) // since fix1.Body.Position is _not_ necessarily the same as this.Position
                return false;

            if (fix1.Body.LinearVelocity.Y < -0.01f) // 0.01 to account for error and so stationary bodies get picked up by moving platforms
            {
                return false;
            }

            return true;
        }
    }
}
