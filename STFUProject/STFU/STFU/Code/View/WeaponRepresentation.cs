/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace STFU
{
    /// <summary>
    /// A weapon representation is the visual representation(sorry!) of the weapon on the screen.
    /// </summary>
    class WeaponRepresentation<TWeapon> : EntityRepresentation
        where TWeapon : Weapon
    {
        public Animation WeaponAnimation { get; set; }
        protected LivingEntity trackingEntity;
        protected TWeapon weapon;
        protected Vector2 weaponOffset;
        protected List<BulletRepresentation> bulletRepresentations { get; set; }

        public WeaponRepresentation(LivingEntity trackingEntity, TWeapon weapon, Vector2 weaponOffset)
        {
            this.WeaponAnimation = new Animation();
            this.trackingEntity = trackingEntity;
            this.weapon = weapon;
            this.weaponOffset = weaponOffset;
            bulletRepresentations = new List<BulletRepresentation>();
        }

        public void InitializeAnimation(SpriteSheet spriteSheet, Vector2 position, string name, int frameTime, Color color, float scale, SpriteEffects spriteEffects, bool looping)
        {
            this.WeaponAnimation.Initialize(spriteSheet, position, name, frameTime, color, scale, spriteEffects, looping);
        }

        public void InitializeBullets(SpriteSheet spriteSheet, Vector2 position, string name, int frameTime, Color color, float scale, SpriteEffects spriteEffects, bool looping)
        {
            for (int i = 0; i < weapon.MaxBullets; i++)
            {
                BulletRepresentation bulletRepresentation = new BulletRepresentation();
                bulletRepresentation.InitializeAnimation(spriteSheet, position, name, frameTime, color, scale, spriteEffects, looping);
                bulletRepresentations.Add(bulletRepresentation);
            }
        }

        public override void Update(float dt)
        {
            // Update the weapon
            if (weapon.Active)
            {
                this.WeaponAnimation.Active = true;

                if (trackingEntity.FacingRight)
                {
                    this.WeaponAnimation.Position = trackingEntity.ScreenPosition + weaponOffset;
                    this.WeaponAnimation.Rotation = trackingEntity.ScreenRotation + weapon.ScreenRotation;
                }
                else
                {
                    this.WeaponAnimation.Position = trackingEntity.ScreenPosition + Vector2.UnitY * weaponOffset.Y - Vector2.UnitX * weaponOffset.X;
                    this.WeaponAnimation.Rotation = -(trackingEntity.ScreenRotation + weapon.ScreenRotation);
                }

                this.WeaponAnimation.Update(dt);

                SpriteEffects weaponSpriteEffects = SpriteEffects.None;
                if (!trackingEntity.FacingRight)
                {
                    weaponSpriteEffects = SpriteEffects.FlipHorizontally;
                }
                this.WeaponAnimation.SpriteEffects = weaponSpriteEffects;
            }
            else
            {
                this.WeaponAnimation.Active = false;
            }

            // Update the bullets
            foreach (BulletRepresentation bulletRepresentation in bulletRepresentations)
            {
                bulletRepresentation.Update(dt);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the weapon
            if (this.WeaponAnimation.Active)
            {
                this.WeaponAnimation.Draw(spriteBatch);
            }
        }

        public void DrawBullets(SpriteBatch spriteBatch)
        {
            // Draw the bullets
            foreach (BulletRepresentation bulletRepresentation in bulletRepresentations)
            {
                bulletRepresentation.Draw(spriteBatch);
            }
        }

        public void AddBullet()
        {
            foreach (BulletRepresentation bulletRepresentation in bulletRepresentations)
            {
                if (bulletRepresentation.Bullet == null)
                {
                    bulletRepresentation.Bullet = weapon.LastBulletShot;
                    break;
                }
            }
        }

        public void BulletsLayerDepth(float depth)
        {
            foreach (BulletRepresentation bulletRepresentation in bulletRepresentations)
            {
                bulletRepresentation.BulletAnimation.LayerDepth = depth;
            }
        }
    }
}
