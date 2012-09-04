/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;

namespace STFU
{
    /// <summary>
    /// A line with no width or height, just two points.
    /// </summary>
    class PhysicsLine
    {
        protected World world;
        protected Vector2 startPosition;
        protected Vector2 endPosition;
        public Body body { get; set; }
        protected Category collisionCategory;

        public PhysicsLine(Entity owner, World world, Vector2 startPosition, Vector2 endPosition)
        {
            this.world = world;
            this.startPosition = startPosition;
            this.endPosition = endPosition;

            SetUpPhysics(owner, world, startPosition, endPosition);
        }

        protected virtual void SetUpPhysics(Entity owner, World world, Vector2 startPosition, Vector2 endPosition)
        {
            body = BodyFactory.CreateEdge(world, ConvertUnits.ToSimUnits(startPosition), ConvertUnits.ToSimUnits(endPosition));
            body.BodyType = BodyType.Dynamic;
            body.Restitution = 0.3f;
            body.Friction = 0.5f;
            body.UserData = owner;

            //body.Position = new Vector2((startPosition.X + endPosition.X) * 0.5f, (startPosition.Y + endPosition.Y) * 0.5f);
            //body.Position = new Vector2(0, -1);
        }

        protected virtual Vector2 extents
        {
            get
            {
                return new Vector2(ConvertUnits.ToSimUnits(Math.Abs(endPosition.X - startPosition.X) * 0.5f), ConvertUnits.ToSimUnits(Math.Abs(endPosition.Y - startPosition.Y) * 0.5f));
            }
        }

        protected virtual Vector2 origin
        {
            get
            {
                return new Vector2(ConvertUnits.ToSimUnits((startPosition.X + endPosition.X) * 0.5f), ConvertUnits.ToSimUnits((startPosition.Y + endPosition.Y) * 0.5f));
            }
        }

        public virtual Vector2 Position
        {
            get
            {
                return origin + body.Position;
            }
        }

        public virtual void Dispose()
        {
            if (body == null)
                return;
            body.Dispose();
        }

        public virtual void Disable()
        {
            body.Enabled = false;
        }

        public virtual Category CollisionCategory
        {
            get
            {
                return collisionCategory;
            }
            set
            {
                collisionCategory = value;
                body.CollisionCategories = value;
            }
        }
    }
}
