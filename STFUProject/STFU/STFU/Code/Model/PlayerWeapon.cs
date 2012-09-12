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
    /// <summary>
    /// A player weapon is used by a player to harm enemies and bosses.
    /// </summary>
    class PlayerWeapon : Weapon<PlayerBullet>
    {
        public bool Vertical { get; private set; }

        private Player player;
        private PlayerBullet bulletToRemove;
        private float activeTimer;
        private const float activeDelay = 1.0f;

        private float impulse;
        private float bulletWidth;
        private float bulletHeight;
        private float bulletDistance;
        private float bulletDensity;
        private bool bulletGoneOnCollision;

        private bool rotate;
        private bool rotateBackward;
        private float rotateSpeed;
        private float startRotation;
        private float endRotation;

        // Constructor
        public PlayerWeapon(World world, Player player)
            : base(world)
        {
            this.player = player;
            this.Bullets = new List<PlayerBullet>();
            this.Active = false;
            this.Shooting = false;
            this.Vertical = false;
        }

        public void SetUpWeapon(int maxBullets, float impulse, float bulletWidth, float bulletHeight, float bulletDistance, float bulletDensity, bool bulletGoneOnCollision)
        {
            MaxBullets = maxBullets;
            this.impulse = impulse;
            this.bulletWidth = bulletWidth;
            this.bulletHeight = bulletHeight;
            this.bulletDistance = bulletDistance;
            this.bulletDensity = bulletDensity;
            this.bulletGoneOnCollision = bulletGoneOnCollision;

            // doesn't rotate by default
            rotate = false;
            rotateBackward = false;
        }

        public void RotateWhenShooting(float rotateSpeed, float startRotation, float endRotation)
        {
            rotate = true;
            this.rotateSpeed = rotateSpeed;
            this.startRotation = GameConstants.DegreesToRadians(startRotation);
            this.endRotation = GameConstants.DegreesToRadians(endRotation);
        }

        public override void Update(float dt)
        {
            // Timers:
            if (activeTimer < 0)
            {
                activeTimer += dt;
                if (activeTimer >= 0)
                {
                    this.Active = false;
                }
            }

            // Update bullets
            bulletToRemove = null;
            foreach (PlayerBullet bullet in this.Bullets)
            {
                bullet.Update(dt);

                // Check to remove any bullets
                if (bullet.Done)
                {
                    bulletToRemove = bullet;
                    break;
                }
            }
            if (bulletToRemove != null)
            {
                this.Bullets.Remove(bulletToRemove);
            }

            if (rotate)
            {
                if (!rotateBackward)
                {
                    this.Rotation += rotateSpeed * dt;
                    if (this.Rotation > endRotation)
                    {
                        this.Rotation = endRotation;
                    }
                }
                else
                {
                    this.Rotation -= rotateSpeed * dt;
                    if (this.Rotation < startRotation)
                    {
                        this.Rotation = startRotation;
                    }
                }
            }
        }

        public bool CanShoot()
        {
            if (this.GetBulletCount() < MaxBullets)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected void shoot(Vector2 positionOffset, Vector2 shotImpulse, float rotation)
        {
            if (!this.Shooting)
            {
                this.Active = true;
                activeTimer = -activeDelay;
                this.Shooting = true;
                //
                PlayerBullet bullet = new PlayerBullet(world, player, ConvertUnits.ToDisplayUnits(Position) + positionOffset, bulletWidth, bulletHeight, bulletDistance, rotation, bulletDensity, bulletGoneOnCollision);
                this.Bullets.Add(bullet);
                this.LastBulletShot = bullet;

                // make bullet fly
                bullet.Fly(shotImpulse);

                // make sure bullets knockback things near the player that shot them, otherwise they might go through and not cause much (if any) knockback
                PlayerBullet muzzleBullet = new PlayerBullet(world, player, ConvertUnits.ToDisplayUnits(Position), bulletWidth, bulletHeight, 0f, rotation, bulletDensity*0.6f, bulletGoneOnCollision);
                muzzleBullet.Visible = false;
                this.Bullets.Add(muzzleBullet);
                muzzleBullet.Fly(shotImpulse * 0.8f);
            }
        }

        public void ShootHorizontally(float impulse)
        {
            this.Vertical = false;

            Vector2 positionOffset = new Vector2(bulletWidth * 0.5f, 0);
            if (impulse < 0)
            {
                positionOffset.X = -positionOffset.X;
            }

            shoot(positionOffset, new Vector2(impulse, 0), 0);
        }

        public void ShootVertically(float impulse)
        {
            this.Vertical = true;

            Vector2 positionOffset = new Vector2(0, bulletWidth * 0.5f);
            if (impulse < 0)
            {
                positionOffset.Y = -positionOffset.Y;
            }

            float rotation = (float)Math.PI * 0.5f;
            if (impulse < 0)
            {
                rotation = -rotation;
            }

            shoot(positionOffset, new Vector2(0, impulse), rotation);
        }

        public void ShootRight()
        {
            if (!rotate)
            {
                this.Rotation = 0;
            }
            else
            {
                toggleRotateBackward();
            }
            ShootHorizontally(impulse);
        }

        public void ShootLeft()
        {
            if (!rotate)
            {
                this.Rotation = 0;
            }
            else
            {
                toggleRotateBackward();
            }
            ShootHorizontally(-impulse);
        }

        public void ShootDown()
        {
            ShootVertically(impulse);
        }

        public void ShootDownAndRotate()
        {
            this.Rotation = (float)Math.PI * 0.5f;
            ShootDown();
        }

        public void ShootUp()
        {
            ShootVertically(-impulse);
        }

        public void ShootUpAndRotate()
        {
            this.Rotation = -(float)Math.PI * 0.5f;
            ShootUp();
        }

        public void DontShoot()
        {
            this.Shooting = false;
        }

        public void SetBulletDistance(float bulletDistance)
        {
            this.bulletDistance = bulletDistance;
        }

        private void toggleRotateBackward()
        {
            if (this.Active)
            {
                if (rotateBackward)
                {
                    rotateBackward = false;
                    this.Rotation = startRotation;
                }
                else
                {
                    rotateBackward = true;
                    this.Rotation = endRotation;
                }
            }
            else
            {
                rotateBackward = false;
                this.Rotation = startRotation;
            }
        }

        public void ChangeRotationFromDownToForward()
        {
            if (this.Rotation == (float)Math.PI * 0.5f)
            {
                this.Vertical = false;
                this.Rotation = 0f;
            }
        }
    }
}
