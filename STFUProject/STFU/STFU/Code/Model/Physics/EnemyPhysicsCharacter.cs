/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Common;

namespace STFU
{
    /// <summary>
    /// An enemy physics character represents an enemy in the physics world.
    /// </summary>
    class EnemyPhysicsCharacter : CompositePhysicsCharacter
    {
        public float RunSpeed { get; set; }
        public float FlySpeed { get; set; }
        public float PlayerAngle { get; set; }

        public EnemyPhysicsCharacter(Entity owner, World world, Vector2 position, float width, float height, float density, OnCollision onCollision, OnSeparation onSeparation)
            : base(owner, world, position, width, height, density, onCollision, onSeparation)
        {
        }

        protected override void performChecks()
        {
            checkForGround();
            checkForCeiling(false, true);
            checkForWall(false, true);
            checkForCharOnHead(false, true); // if this is commented out, enemies slip off the heads of others
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            // Put updates here
            
            // perform this check at the end
            adjustForEdgeCatching();
        }

        /* old
        protected bool rayCastForPlayer(Vector2 rayStart, Vector2 rayEnd)
        {
            bool rayHitSomething = false;
            world.RayCast((fixture, point, normal, fraction) =>
            {
                if (fixture == null || collidedWithSelf(fixture.Body) || fixture.IsSensor)
                {
                    return -1;
                }
                else
                {
                    if (collidedWithPlayer(fixture.CollisionCategories))
                    {
                        rayHitSomething = true;
                    }
                    else
                    {
                        rayHitSomething = false;
                    }
                }
                return fraction;
            }, rayStart, rayEnd);

            return rayHitSomething;
        }
         */

        protected bool rayCastForPlayer(Vector2 rayStart, Vector2 rayEnd)
        {
            return rayCastSight(rayStart, rayEnd, false, true, false, false);
        }

        protected bool RayCastForGround(Vector2 rayStart, Vector2 rayEnd)
        {
            return rayCast(rayStart, rayEnd, true, false, false, false);
        }

        protected bool RayCastForWall(Vector2 rayStart, Vector2 rayEnd)
        {
            return rayCast(rayStart, rayEnd, true, false, true, false);
        }

        public bool CheckForPlayerHorizontally(bool facingRight, float distance)
        {
            float halfWidth = extents.X;

            if (!facingRight)
            {
                distance = -distance;
                halfWidth = -halfWidth;
            }

            // in the middle of the enemy
            Vector2 rayStart = new Vector2(Position.X, Position.Y);
            Vector2 rayEnd = rayStart + new Vector2(distance + halfWidth, 0);
            if (rayCastForPlayer(rayStart, rayEnd))
                return true;

            /* probably don't need this
            // a bit lower
            rayStart.Y += extents.Y * 0.5f;
            rayEnd.Y += extents.Y * 0.5f;
            if (rayCastForPlayer(rayStart, rayEnd))
                return true;

            // a bit higher
            rayStart.Y -= extents.Y;
            rayEnd.Y -= extents.Y;
            if (rayCastForPlayer(rayStart, rayEnd))
                return true;
             */

            return false;
        }

        /* old; slow
        public bool CheckForPlayerDiagonally(bool facingRight, float distance, float angle, float angleOffset)
        {
            float addExtentToDistance = (extents.X > extents.Y) ? extents.X : extents.Y;
            distance += addExtentToDistance;

            // Convert angles from degrees to radians
            angle = GameConstants.DegreesToRadians(angle);
            angleOffset = GameConstants.DegreesToRadians(angleOffset);

            float angleInterval = GameConstants.DegreesToRadians(10);
            int angleCounter = (int)Math.Ceiling((angleOffset * 2) / angleInterval);

            // in the middle of the enemy
            Vector2 rayStart = new Vector2(Position.X, Position.Y);
            Vector2 rayEnd;

            // the starting angle
            angle = angle - angleOffset;
            
            // check from the starting angle to the final angle, which is angle + angleOffset
            for (int i = 0; i <= angleCounter; i++)
            {
                rayEnd = rayStart + GameConstants.VectorFromAngle(angle, distance, facingRight);
                if (rayCastForPlayer(rayStart, rayEnd))
                {
                    playerAngle = angle;
                    return true;
                }

                angle += angleInterval;
            }

            return false;
        }
         */

        public bool CheckForPlayerDiagonally(bool facingRight, float distance, float angle)
        {
            float addExtentToDistance = (extents.X > extents.Y) ? extents.X : extents.Y;
            distance += addExtentToDistance;

            // Convert angles from degrees to radians
            angle = GameConstants.DegreesToRadians(angle);

            // in the middle of the enemy
            Vector2 rayStart = new Vector2(Position.X, Position.Y);
            Vector2 rayEnd = rayStart + GameConstants.VectorFromAngle(angle, distance, facingRight);
            if (rayCastForPlayer(rayStart, rayEnd))
            {
                this.PlayerAngle = angle;
                return true;
            }

            return false;
        }

        public bool CheckForGroundAhead(bool facingRight, float distance)
        {
            float halfWidth = extents.X;
            float halfHeight = extents.Y;
            float distanceDown = ConvertUnits.ToSimUnits(4); // check 4 pixels down
            float offset = ConvertUnits.ToSimUnits(1); // 1 pixel offset so that the cracks in between the ground are bypassed
            if (!facingRight)
            {
                distance = -distance;
                halfWidth = -halfWidth;
                offset = -offset;
            }
            
            Vector2 rayStart = new Vector2(Position.X + halfWidth, Position.Y); // at the center of the enemy's forward edge
            Vector2 rayEnd = rayStart + new Vector2(distance + offset, halfHeight + distanceDown);

            return RayCastForGround(rayStart, rayEnd);
        }

        public bool CheckForWallAhead(bool facingRight, float distance)
        {
            float halfWidth = extents.X;
            float halfHeight = extents.Y;
            float offset = ConvertUnits.ToSimUnits(1); // 1 pixel offset so that the cracks in between the wall are bypassed
            if (!facingRight)
            {
                distance = -distance;
                halfWidth = -halfWidth;
                offset = -offset;
            }

            // in the middle of the enemy
            Vector2 rayStart = new Vector2(Position.X + halfWidth, Position.Y); // at the center of the enemy's forward edge
            Vector2 rayEnd = rayStart + new Vector2(distance + offset, 0);
            if (RayCastForWall(rayStart, rayEnd))
                return true;

            /* probably don't need this
            // a bit lower
            rayStart.Y += extents.Y * 0.5f;
            rayEnd.Y += extents.Y * 0.5f;
            if (RayCastForWall(rayStart, rayEnd))
                return true;

            // a bit higher
            rayStart.Y -= extents.Y;
            rayEnd.Y -= extents.Y;
            if (RayCastForWall(rayStart, rayEnd))
                return true;
             */

            return false;
        }

        // Movement:
        private float getSpeedFor(float speed, float speedPercent)
        {
            return speedPercent * speed;
        }

        public void RunLeft(float speedPercent)
        {
            moveOnGround();
            motor.MotorSpeed = -getSpeedFor(this.RunSpeed, speedPercent);
        }

        public void RunRight(float speedPercent)
        {
            moveOnGround();
            motor.MotorSpeed = getSpeedFor(this.RunSpeed, speedPercent);
        }

        public void StopRunning()
        {
            // don't brake yet as we need to stop rolling first
            motor.MotorSpeed = 0;
        }

        public void StopFlying()
        {
            body.LinearVelocity = new Vector2(0, body.LinearVelocity.Y);
            wheelBody.LinearVelocity = new Vector2(0, wheelBody.LinearVelocity.Y);
        }

        public void FlapLeft(float speedPercent)
        {
            moveInAir();
            float speed = getSpeedFor(this.FlySpeed, speedPercent);
            Vector2 flyVelocity = new Vector2(-speed*0.5f, -speed);

            body.LinearVelocity = flyVelocity;
        }

        public void FlapRight(float speedPercent)
        {
            moveInAir();
            float speed = getSpeedFor(this.FlySpeed, speedPercent);
            Vector2 flyVelocity = new Vector2(speed*0.5f, -speed);

            body.LinearVelocity = flyVelocity;
        }

        public void DiveLeft(float speedPercent, float angle)
        {
            body.LinearVelocity = GameConstants.VectorFromAngle(angle, getSpeedFor(this.FlySpeed, speedPercent), false);
        }

        public void DiveRight(float speedPercent, float angle)
        {
            body.LinearVelocity = GameConstants.VectorFromAngle(angle, getSpeedFor(this.FlySpeed, speedPercent), true);
        }

        public void Float(float speedPercent)
        {
            moveInAir();
            float speed = getSpeedFor(this.FlySpeed, speedPercent);
            body.LinearVelocity = new Vector2(0, -speed);
        }

        protected override void moveInAir()
        {
            base.moveInAir();
            wheelBody.LinearVelocity = Vector2.Zero;
        }

        protected override bool collidedWithGroundFixture(Fixture fix2)
        {
            if (fix2.CollisionCategories.HasFlag(GameConstants.EnemyCollisionCategory))
            {
                return true;
            }

            return base.collidedWithGroundFixture(fix2);
        }
    }
}
