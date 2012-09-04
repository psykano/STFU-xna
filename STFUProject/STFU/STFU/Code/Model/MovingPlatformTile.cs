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
    /// An platform that moves; a platform that is never static.
    /// </summary>
    class MovingPlatformTile : PlatformTile
    {
        protected Vector2 initialPosition;
        protected Vector2 finalPosition;
        protected float maxSpeed;

        // Constructor
        public MovingPlatformTile(World world, Vector2 initialPosition, Vector2 finalPosition, float width, float speed)
            : base(world, width)
        {
            if (initialPosition.X > finalPosition.X || initialPosition.Y > finalPosition.Y)
            {
                throw new Exception("Error initialPosition > finalPosition: please reverse the values for initial and final positions");
            }

            this.initialPosition = ConvertUnits.ToSimUnits(initialPosition);
            this.finalPosition = ConvertUnits.ToSimUnits(finalPosition);
            maxSpeed = speed;
        }

        public override void SetUpPlatformTile(Vector2 position)
        {
            base.SetUpPlatformTile(position);
            physicsLine.body.BodyType = BodyType.Kinematic;
        }
    }
}
