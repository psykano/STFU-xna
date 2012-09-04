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
    /// Platform that moves in a clockwise motion around a point.
    /// </summary>
    class ClockwiseMovingPlatform : CircularMovingPlatform
    {
        private const int distanceWidthMult = 2;
        private const float speed = 1.25f;

        // Constructor
        public ClockwiseMovingPlatform(World world, Vector2 position, float width, bool reverse)
            : base(world, position, position + Vector2.UnitX * width * distanceWidthMult, width, speed, true, reverse)
        {
        }
    }

    /// <summary>
    /// Platform that moves in a circular motion around a point.
    /// The platform doesn't rotate and so remains facing upright.
    /// </summary>
    class CircularMovingPlatform : MovingPlatformTile
    {
        private bool clockwise;

        // Constructor
        public CircularMovingPlatform(World world, Vector2 initialPosition, Vector2 finalPosition, float width, float speed, bool clockwise, bool reverse)
            : base(world, initialPosition, finalPosition, width, speed)
        {
            Vector2 startPosition;
            if (!reverse)
            {
                startPosition = initialPosition;
            }
            else
            {
                startPosition = finalPosition;
            }
            SetUpPlatformTile(startPosition);

            this.clockwise = clockwise;
        }

        public override void Update(float dt)
        {
            // first, get the centre point of the circle
            Vector2 centrePoint = (finalPosition + initialPosition) * 0.5f;

            float dx = centrePoint.X - this.Position.X;
            float dy = centrePoint.Y - this.Position.Y;

            // get the normal to the current point on the circle
            Vector2 normal;
            if (clockwise)
                normal = new Vector2(dy, -dx);
            else
                normal = new Vector2(-dy, dx);

            // normalize then multiply by the maxVelocity
            float normalLength = (float)Math.Sqrt(normal.X * normal.X + normal.Y * normal.Y);
            normal /= normalLength;
            normal *= maxSpeed;

            float radius = GameConstants.DistanceBetweenTwoPoints(initialPosition, centrePoint);

            Vector2 toCentrePoint = new Vector2(centrePoint.X - this.Position.X, centrePoint.Y - this.Position.Y);

            if (GameConstants.DistanceBetweenTwoPoints(this.Position, centrePoint) > radius)
            {
                physicsLine.body.LinearVelocity = (normal + toCentrePoint) * 0.5f;
            }
            else
            {
                physicsLine.body.LinearVelocity = normal;
            }
        }
    }
}
