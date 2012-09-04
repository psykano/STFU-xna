/*
 * Copyright (c) 2012 Chris Johns
 */
// original idea from http://rabidlion.com/?p=31

using System;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;

namespace STFU
{
    /// <summary>
    /// An rectangular object.
    /// </summary>
    class PhysicsObject : IDisposable
    {
        protected World world;
        protected float width;
        protected float height;
        public Body body { get; set; }
        public Fixture fixture; // this needs to be refactored out of child classes ***
        protected Category collisionCategory;

        public PhysicsObject(Entity owner, World world, Vector2 position, float width, float height, float density)
        {
            this.world = world;
            this.width = width;
            this.height = height;

            SetUpPhysics(owner, world, position, width, height, density);
        }

        protected virtual void SetUpPhysics(Entity owner, World world, Vector2 position, float width, float height, float density)
        {
            body = BodyFactory.CreateRectangle(world, ConvertUnits.ToSimUnits(width), ConvertUnits.ToSimUnits(height), density, ConvertUnits.ToSimUnits(position));
            body.BodyType = BodyType.Dynamic;
            body.Restitution = 0.3f;
            body.Friction = 0.5f;
            body.UserData = owner;
            
            fixture = body.FixtureList[0];
        }

        protected virtual Vector2 extents
        {
            get
            {
                return new Vector2(ConvertUnits.ToSimUnits(width * 0.5f), ConvertUnits.ToSimUnits(height * 0.5f));
            }
        }

        public virtual Vector2 Position
        {
            get
            {
                return body.Position;
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

        public virtual bool ContainsThisBody(Body _body)
        {
            if (body == _body)
                return true;
            return false;
        }
    }
}

