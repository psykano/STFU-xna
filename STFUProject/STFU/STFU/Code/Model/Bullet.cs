/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics.Contacts;

namespace STFU
{
    /// <summary>
    /// A bullet is what gets fired from a weapon.
    /// In the physics world, it's a rectangle.
    /// </summary>
    class Bullet : Entity, IDisposable
    {
        public bool Done { get; protected set; }
        public Vector2 Direction { get; protected set; }
        protected PhysicsObject physicsObj;
        protected Vector2 initialPosition;
        protected float distance;
        protected Timer doneTimer;

        // Constructor
        public Bullet(World world, Vector2 position, float width, float height, float distance, float rotation, float density, bool bulletGoneOnCollision)
            : base(world)
        {
            this.initialPosition = position;
            this.Width = ConvertUnits.ToSimUnits(width);
            this.Height = ConvertUnits.ToSimUnits(height);
            this.distance = distance;
            this.Done = false;
            doneTimer = new Timer();

            // Make the bullet in the physics world
            this.physicsObj = new PhysicsObject(this, this.world, position, width, height, density);
            this.physicsObj.body.IgnoreGravity = true;
            this.physicsObj.body.Rotation = rotation;
            this.physicsObj.body.FixedRotation = true;

            // We want the bullet to disappear once it hits something
            if (bulletGoneOnCollision)
            {
                this.physicsObj.body.OnCollision += new OnCollisionEventHandler(onCollision);
            }
            else
            {
                this.physicsObj.body.OnCollision += new OnCollisionEventHandler(onSilentCollision);
            }
        }

        public override Vector2 Position
        {
            get
            {
                return this.physicsObj.Position;
            }
        }

        public override float Rotation
        {
            get
            {
                return this.physicsObj.body.Rotation;
            }
        }

        protected virtual bool onCollision(Fixture fix1, Fixture fix2, Contact contact)
        {
            return true;
        }

        protected virtual bool onSilentCollision(Fixture fix1, Fixture fix2, Contact contact)
        {
            return true;
        }

        public override void Update(float dt)
        {
            // horizontally
            if (Math.Abs(Position.X - ConvertUnits.ToSimUnits(initialPosition.X)) > ConvertUnits.ToSimUnits(distance))
            {
                MarkDone();
            }
            // vertically
            else if (Math.Abs(Position.Y - ConvertUnits.ToSimUnits(initialPosition.Y)) > ConvertUnits.ToSimUnits(distance))
            {
                MarkDone();
            }

            if (!doneTimer.IsReset())
            {
                doneTimer.Update(dt);

                if (doneTimer.IsTimeUp())
                {
                    MarkDone();
                    doneTimer.Reset();
                }
            }
        }

        public void MarkDone()
        {
            if (!this.Done)
            {
                this.Done = true;
                Dispose();
            }
        }

        public void Fly(Vector2 impulse)
        {
            this.Direction = impulse;
            this.physicsObj.body.ApplyLinearImpulse(impulse * this.physicsObj.fixture.Shape.Density);
        }

        public void Dispose()
        {
            this.physicsObj.Dispose();
        }
    }
}
