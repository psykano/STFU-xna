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
    /// A platform that moves left and right.
    /// </summary>
    class HorizontalMovingPlatform : LinearMovingPlatform
    {
        private const int distanceWidthMult = 2;
        private const float speed = 1.25f;

        // Constructor
        public HorizontalMovingPlatform(World world, Vector2 position, float width, float nearTargetPositionDistance, bool reverse)
            : base(world, position, position + Vector2.UnitX * width * distanceWidthMult, nearTargetPositionDistance, width, speed, reverse)
        {
        }
    }

    /// <summary>
    /// A platform that moves up and down.
    /// </summary>
    class VerticalMovingPlatform : LinearMovingPlatform
    {
        private const int distanceWidthMult = 2;
        private const float speed = 1.25f;

        // Constructor
        public VerticalMovingPlatform(World world, Vector2 position, float width, float nearTargetPositionDistance, bool reverse)
            : base(world, position, position + Vector2.UnitY * width * distanceWidthMult, nearTargetPositionDistance, width, speed, reverse)
        {
        }
    }

    /// <summary>
    /// A platform that moves from one point to another, slowing down when nearing each point.
    /// </summary>
    class LinearMovingPlatform : MovingPlatformTile
    {
        protected bool movingTowardFinal;
        protected float nearTargetPositionDistance;

        private Vector2 velocity;
        private Vector2 maxVelocity;
        private float distanceToTargetPosition;

        // Constructor
        public LinearMovingPlatform(World world, Vector2 initialPosition, Vector2 finalPosition, float nearTargetPositionDistance, float width, float speed, bool reverse)
            : base(world, initialPosition, finalPosition, width, speed)
        {
            Vector2 startPosition;
            if (!reverse)
            {
                movingTowardFinal = true;
                startPosition = initialPosition;
            }
            else
            {
                movingTowardFinal = false;
                startPosition = finalPosition;
            }
            SetUpPlatformTile(startPosition);

            this.nearTargetPositionDistance = ConvertUnits.ToSimUnits(nearTargetPositionDistance);

            // calculate the maximum velocity
            // it depends on the angle between the initial and final position
            Vector2 displacementFromFinalToInitial = new Vector2(finalPosition.X - initialPosition.X, finalPosition.Y - initialPosition.Y);
            if (displacementFromFinalToInitial.X <= 0)
            {
                maxVelocity = new Vector2(0, speed);
            }
            else if (displacementFromFinalToInitial.Y <= 0)
            {
                maxVelocity = new Vector2(speed, 0);
            }
            else
            {
                double angle = Math.Atan2(displacementFromFinalToInitial.X, displacementFromFinalToInitial.Y);
                maxVelocity = new Vector2(speed * (float)Math.Sin(angle), speed * (float)Math.Cos(angle));
            }
        }

        public override void Update(float dt)
        {
            // check if we're moving outside the initial position or final positions
            if (this.Position.X < initialPosition.X || this.Position.Y < initialPosition.Y)
            {
                movingTowardFinal = true;
            }
            else if (this.Position.X > finalPosition.X || this.Position.Y > finalPosition.Y)
            {
                movingTowardFinal = false;
            }

            velocity = maxVelocity;

            // calculate the distance
            if (GameConstants.DistanceBetweenTwoPoints(this.Position, initialPosition) < nearTargetPositionDistance)
            {
                distanceToTargetPosition = GameConstants.DistanceBetweenTwoPoints(this.Position, initialPosition);
            }
            else if (GameConstants.DistanceBetweenTwoPoints(this.Position, finalPosition) < nearTargetPositionDistance)
            {
                distanceToTargetPosition = GameConstants.DistanceBetweenTwoPoints(this.Position, finalPosition);
            }
            else
            {
                distanceToTargetPosition = 0;
            }

            if (distanceToTargetPosition > 0)
            {
                velocity *= (float)Math.Sqrt(distanceToTargetPosition / nearTargetPositionDistance);
            }

            if (!movingTowardFinal)
            {
                velocity = -velocity;
            }

            physicsLine.body.LinearVelocity = velocity;
        }
    }
}
