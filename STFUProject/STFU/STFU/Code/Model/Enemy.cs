/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics.Contacts;
using EasyConfig;

namespace STFU
{
    /// <summary>
    /// An enemy harms players.
    /// </summary>
    abstract class Enemy : LivingEntity
    {
        private const string settingsIni = "Settings/EnemySettings.ini";

        // Constructor
        public Enemy(World world)
            : base(world)
        {
        }

        public static ConfigFile GetEnemyConfigFile()
        {
            return new ConfigFile(settingsIni);
        }
    }

    abstract class Enemy<TPhysicsChar> : Enemy
        where TPhysicsChar : PhysicsCharacter
    {
        protected EnemyEvent enemyEvent;
        protected float screenWidth;
        protected float screenHeight;

        public PhysicsContainer<TPhysicsChar> PhysicsContainer { get; protected set; }

        // Constructor
        public Enemy(World world, EnemyEvent enemyEvent) 
            : base(world)
        {
            this.enemyEvent = enemyEvent;
            this.PhysicsContainer = new PhysicsContainer<TPhysicsChar>();
        }

        public virtual void SetUpEnemy(Vector2 enemyStartPosition, int health, float hitDelay, float recoveryDelay)
        {
            setUpLivingEntity(enemyStartPosition, screenWidth, screenHeight, health, hitDelay, recoveryDelay, GameVariables.EnemyRespawnDelay);
        }

        public override void Spawn()
        {
            spawnLivingEntity();
        }

        protected override void updateWhenAlive(float dt)
        {
            // Update the physics character
            this.PhysicsContainer.Object.Update(dt);
        }

        protected bool onCollision(Fixture fix1, Fixture fix2, Contact contact)
        {
            if (fix2.CollisionCategories.HasFlag(GameConstants.DeathCollisionCategory))
            {
                Die();
                return false;
            }
            else if (fix2.CollisionCategories.HasFlag(GameConstants.PlayerBulletCollisionCategory))
            {
                if (this.Health.CheckInvulnerable())
                {
                    return false;
                }

                onCollisionWithBullet();
            }

            // Let the player class handle collisions with the players
            if (fix2.CollisionCategories.HasFlag(GameConstants.PlayerCollisionCategory))
            {
                return false;
            }

            return true;
        }

        protected void onSeparation(Fixture fix1, Fixture fix2)
        {
        }

        protected virtual void onCollisionWithBullet()
        {
        }

        protected void gotHitByBullet()
        {
            // Decrease health
            this.Health.GotHit(1);
        }

        public override void Die()
        {
            base.Die();
            this.PhysicsContainer.Object.Disable();
        }

        public override void Respawn()
        {
            base.Respawn();
            this.PhysicsContainer.Object.Dispose();
            Spawn();
        }
    }
}
