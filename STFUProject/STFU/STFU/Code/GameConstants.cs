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

namespace STFU
{
    public static class GameConstants
    {
        public const Category DefaultCollisionCategory = Category.Cat1;
        public const Category PlayerCollisionCategory = Category.Cat2;
        public const Category Player1CollisionCategory = Category.Cat3;
        public const Category Player2CollisionCategory = Category.Cat4;
        public const Category Player3CollisionCategory = Category.Cat5;
        public const Category Player4CollisionCategory = Category.Cat6;
        public const Category PlayerBulletCollisionCategory = Category.Cat7;
        public const Category Player1BulletCollisionCategory = Category.Cat8;
        public const Category Player2BulletCollisionCategory = Category.Cat9;
        public const Category Player3BulletCollisionCategory = Category.Cat10;
        public const Category Player4BulletCollisionCategory = Category.Cat11;
        public const Category EnemyCollisionCategory = Category.Cat12;
        public const Category DeathCollisionCategory = Category.Cat13;
        public const Category GroundCollisionCategory = Category.Cat29;
        public const Category PlatformCollisionCategory = Category.Cat30;
        public const Category IgnoreCollisionCategory = Category.Cat31;

        public static float DegreesToRadians(float degrees)
        {
            return MathHelper.ToRadians(degrees);
        }

        public static float RadiansToDegrees(float radians)
        {
            return MathHelper.ToDegrees(radians);
        }

        public static Vector2 VectorFromAngle(float angle, float distance, bool facingRight)
        {
            float x, y;

            x = distance * (float)Math.Cos(angle);
            if (!facingRight)
            {
                x = -x;
            }

            y = distance * (float)Math.Sin(angle);

            return new Vector2(x, y);
        }

        public static float DistanceBetweenTwoPoints(Vector2 p1, Vector2 p2)
        {
            return (p2 - p1).Length();
        }
    }
}
