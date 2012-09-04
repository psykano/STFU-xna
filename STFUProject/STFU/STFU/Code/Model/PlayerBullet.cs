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
    /// A player bullet is fired from a player's weapon.
    /// </summary>
    class PlayerBullet : Bullet
    {
        private Player player;
        private const float doneDelay = 0.025f;

        // Constructor
        public PlayerBullet(World world, Player player, Vector2 position, float width, float height, float distance, float rotation, float density, bool bulletGoneOnCollision)
            : base (world, position, width, height, distance, rotation, density, bulletGoneOnCollision)
        {
            this.player = player;

            // Set up collision category based on player index
            Category collisionCategory = GameConstants.PlayerBulletCollisionCategory;
            if (player.PlayerIndex == PlayerIndex.One)
            {
                collisionCategory |= GameConstants.Player1BulletCollisionCategory;
            }
            else if (player.PlayerIndex == PlayerIndex.Two)
            {
                collisionCategory |= GameConstants.Player2BulletCollisionCategory;
            }
            else if (player.PlayerIndex == PlayerIndex.Three)
            {
                collisionCategory |= GameConstants.Player3BulletCollisionCategory;
            }
            else if (player.PlayerIndex == PlayerIndex.Four)
            {
                collisionCategory |= GameConstants.Player4BulletCollisionCategory;
            }
            this.physicsObj.CollisionCategory = collisionCategory;

            // We want the bullet to disappear once it hits something
            // bullets don't collide with the player that shot it or itself or platforms
            Category bulletCollidesWith = ~player.PhysicsContainer.Object.CollisionCategory & ~collisionCategory;
            if (bulletGoneOnCollision)
            {
            }
            else
            {
                bulletCollidesWith &= ~GameConstants.GroundCollisionCategory;
            }
            this.physicsObj.body.CollidesWith = bulletCollidesWith;
        }

        protected override bool onCollision(Fixture fix1, Fixture fix2, Contact contact)
        {
            if (fix2.IsSensor)
            {
                return false;
            }

            if (collidedWithOneWayPlatform(fix1, fix2))
                if (!OnCollisionWithOneWayPlatform(fix1, fix2))
                    return false;

            // We don't want to try to remove the same physics body twice
            MarkDone();

            return true;
        }

        protected override bool onSilentCollision(Fixture fix1, Fixture fix2, Contact contact)
        {
            if (fix2.IsSensor)
            {
                return false;
            }

            if (fix2.CollisionCategories.HasFlag(GameConstants.PlatformCollisionCategory))
                return false;

            // This fixes a bug:
            //    sometimes it collides with a platform when it shouldn't at a low framerate
            if (fix2.CollisionCategories.HasFlag(GameConstants.GroundCollisionCategory))
                return false;

            // We don't want to try to remove the same physics body twice
            doneTimer.SetDelay(doneDelay);

            return true;
        }
    }
}
