/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics.Contacts;

namespace STFU
{
    /// <summary>
    /// An falling platform is essentially static until a player or enemy lands on or hits it, in which case
    /// it falls down until it hits the ground after a short delay.
    /// </summary>
    class FallingPlatformTile : PlatformTile
    {
        public bool Fallen { get; set; }
        public bool CollidedWithLivingEntity { get; set; }

        protected Vector2 spawnPosition;

        private Timer fallTimer;
        private float fallDelay;
        private Timer resetTimer;
        private float resetDelay;

        // Constructor
        public FallingPlatformTile(World world, Vector2 position, float width, float fallDelay, float resetDelay)
            : base(world, width)
        {
            this.Fallen = false;
            this.CollidedWithLivingEntity = false;
            spawnPosition = position;
            fallTimer = new Timer();
            this.fallDelay = fallDelay;
            resetTimer = new Timer();
            this.resetDelay = resetDelay;

            SetUpPlatformTile(spawnPosition);
        }

        public void Reset()
        {
            if (physicsLine.body != null)
            {
                physicsLine.Dispose();
            }

            this.Fallen = false;
            this.CollidedWithLivingEntity = false;
            fallTimer.Reset();
            resetTimer.Reset();

            SetUpPlatformTile(spawnPosition);

            this.Visible = true;
        }

        public override void SetUpPlatformTile(Vector2 position)
        {
            base.SetUpPlatformTile(position);
            physicsLine.body.BodyType = BodyType.Kinematic;
            physicsLine.body.OnCollision += new OnCollisionEventHandler(onCollision);
        }

        public override void Update(float dt)
        {
            if (!this.Fallen)
            {
                if (!fallTimer.IsReset())
                {
                    fallTimer.Update(dt);

                    if (fallTimer.IsTimeUp())
                    {
                        fallAway();
                    }
                }
            }
            else
            {
                if (!resetTimer.IsReset())
                {
                    resetTimer.Update(dt);

                    if (resetTimer.IsTimeUp())
                    {
                        Reset();
                    }
                }
            }
        }

        protected bool onCollision(Fixture fix1, Fixture fix2, Contact contact)
        {
            if (fix2.IsSensor)
            {
                return false;
            }

            if (this.Fallen)
            {
                if (fix2.CollisionCategories == GameConstants.GroundCollisionCategory)
                {
                    // We need to check if the falling platform is actually colliding with the ground since farseer isn't accurate enough for our purposes...

                    Entity ground = fix2.Body.UserData as Entity;

                    float leftPlatformPoint = this.Position.X - this.Width * 0.5f;
                    float rightPlatformPoint = this.Position.X + this.Width * 0.5f;
                    float leftGroundPoint = ground.Position.X - ground.Width * 0.5f;
                    float rightGroundPoint = ground.Position.X + ground.Width * 0.5f;

                    if (rightPlatformPoint >= leftGroundPoint && rightPlatformPoint <= rightGroundPoint)
                        die();
                    else if (leftPlatformPoint >= leftGroundPoint && leftPlatformPoint <= rightGroundPoint)
                        die();
                }

                return false;
            }

            if (!this.CollidedWithLivingEntity)
            {
                if (onCollisionWithLivingEntity(fix1, fix2, contact))
                {
                    this.CollidedWithLivingEntity = true;
                    fallTimer.SetDelay(fallDelay);
                }
            }

            return true;
        }

        protected bool onCollisionWithLivingEntity(Fixture fix1, Fixture fix2, Contact contact)
        {
            LivingEntity entity = fix2.Body.UserData as LivingEntity;

            if (entity != null)
            {
                if (entity.OnCollisionWithOneWayPlatform(fix2, fix1))
                    return true;
            }

            return false;
        }

        protected void fallAway()
        {
            this.Fallen = true;
            resetTimer.SetDelay(resetDelay);

            // this is so it falls
            physicsLine.body.BodyType = BodyType.Dynamic;
        }

        protected void die()
        {
            physicsLine.body.OnCollision -= new OnCollisionEventHandler(onCollision);
            physicsLine.Dispose();
            this.Visible = false;
        }
    }
}
