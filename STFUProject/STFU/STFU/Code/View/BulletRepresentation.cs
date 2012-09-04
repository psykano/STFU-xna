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
    /// A bullet representation is the visual representation(sorry!) of a bullet on the screen.
    /// </summary>
    class BulletRepresentation : EntityRepresentation
    {
        public Animation BulletAnimation { get; set; }
        public Bullet Bullet { get; set; }

        public BulletRepresentation()
        {
            this.BulletAnimation = new Animation();
        }

        public void InitializeAnimation(SpriteSheet spriteSheet, Vector2 position, string name, int frameTime, Color color, float scale, SpriteEffects spriteEffects, bool looping)
        {
            this.BulletAnimation.Initialize(spriteSheet, position, name, frameTime, color, scale, spriteEffects, looping);
        }

        public override void Update(float dt)
        {
            if (this.Bullet == null)
            {
                this.BulletAnimation.Active = false;
            }
            else if (this.Bullet.Done)
            {
                this.Bullet = null;
            }
            else
            {
                this.BulletAnimation.Active = true;

                this.BulletAnimation.Position = this.Bullet.ScreenPosition;
                this.BulletAnimation.Rotation = this.Bullet.Rotation;

                SpriteEffects bulletSpriteEffects = SpriteEffects.None;
                if (this.Bullet.Direction.X < 0)
                {
                    bulletSpriteEffects = SpriteEffects.FlipHorizontally;
                }
                this.BulletAnimation.SpriteEffects = bulletSpriteEffects;

                this.BulletAnimation.Update(dt);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (this.BulletAnimation.Active)
            {
                this.BulletAnimation.Draw(spriteBatch);
            }
        }
    }
}
