/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace STFU
{
    /// <summary>
    /// An enemy representation is the visual representation(sorry!) of the enemy on the screen.
    /// </summary>
    abstract class EnemyRepresentation : LivingEntityRepresentation
    {
        public EnemyRepresentation(Enemy enemy)
            : base(enemy)
        {
        }
    }

    abstract class EnemyRepresentation<TEnemy> : EnemyRepresentation
        where TEnemy : Enemy
    {
        protected TEnemy enemy;
        protected EnemyEvent enemyEvent;
        protected Color color;
        protected SpriteEffects spriteEffects;
        protected Animation currentAnimation;
        protected Frame currentEyesFrame;
        protected Vector2 eyesOffset;
        private const float showRespawningTime = 1.5f;

        // Constructor
        public EnemyRepresentation(TEnemy enemy, EnemyEvent enemyEvent)
            : base(enemy)
        {
            this.enemy = enemy;
            this.enemyEvent = enemyEvent;
            this.color = Color.Red; // enemies are red by default
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            if (enemy.Health.Dead && enemy.Health.RespawnTimer.Time() < showRespawningTime)
            {
                updateWhenDead(dt);
            }
            else
            {
                updateWhenAlive(dt);
            }
        }

        protected virtual void updateWhenAlive(float dt)
        {
        }

        protected virtual void updateWhenDead(float dt)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the enemy
            if (!blink)
            {
                if (!enemy.Health.Dead)
                {
                    // Draw the current animation and eyes
                    currentAnimation.Draw(spriteBatch);
                    if (currentEyesFrame != null)
                    {
                        currentEyesFrame.Draw(spriteBatch);
                        currentEyesFrame.LayerDepth = currentAnimation.LayerDepth - 0.001f;
                    }
                }
                else
                {
                    // Draw the enemy respawning
                    if (enemy.Health.RespawnTimer.Time() < showRespawningTime)
                    {
                        currentAnimation.Draw(spriteBatch);
                        if (currentEyesFrame != null)
                        {
                            currentEyesFrame.Draw(spriteBatch);
                            currentEyesFrame.LayerDepth = currentAnimation.LayerDepth - 0.001f;
                        }
                    }
                }
            }
        }

        protected virtual void CheckEvents()
        {
            if (enemyEvent.changeBehavior)
            {
                enemyEvent.changeBehavior = false;

                // reset the current animation
                currentAnimation.Reset();
            }
        }

        protected void updateCurrentAnimation(float dt)
        {
            /*
            // shake enemy if hit
            if (enemy.Health.Hit)
            {
                shakeTimer.Update(dt);

                if (shakeTimer.IsTimeUp())
                {
                    shakeTimer.SetAccumulatedDelay(shakeDelay);
                    shakeEnemy();
                }
            }
            else
            {
                shakePositionOffset = Vector2.Zero;
            }
             */

            currentAnimation.Position = enemy.ScreenPosition + shakePositionOffset;
            currentAnimation.Rotation = enemy.ScreenRotation;
            currentAnimation.Opacity = 1f;
            currentAnimation.Update(dt);
            currentAnimation.SpriteEffects = spriteEffects;

            // update eyes
            if (currentEyesFrame != null)
            {
                adjustEyesOffset();
                currentEyesFrame.Position = enemy.ScreenPosition + eyesOffset + shakePositionOffset;
                currentEyesFrame.Rotation = enemy.ScreenRotation;
                currentEyesFrame.Opacity = 1f;
                currentEyesFrame.SpriteEffects = spriteEffects;
            }
        }

        protected void updateCurrentAnimationWhenDead(float dt)
        {
            currentAnimation.Position = enemy.SpawnPosition;
            currentAnimation.Rotation = 0;

            currentAnimation.Opacity = 1f - (enemy.Health.RespawnTimer.Time() / showRespawningTime);

            currentAnimation.Update(dt);
            currentAnimation.SpriteEffects = spriteEffects;

            // update eyes
            if (currentEyesFrame != null)
            {
                adjustEyesOffset();
                currentEyesFrame.Position = enemy.SpawnPosition + eyesOffset;
                currentEyesFrame.Rotation = 0;
                currentEyesFrame.Opacity = currentAnimation.Opacity;
                currentEyesFrame.SpriteEffects = spriteEffects;
            }
        }

        protected virtual void adjustEyesOffset()
        {
        }

        /*
        private void shakeEnemy()
        {
            float randomDouble = (float)randomShake.NextDouble();
            randomDouble *= 0.6f;
            randomDouble += 0.4f;

            if (shakePositionOffset.X < 0)
                shakePositionOffset.X = randomDouble * shakeIntensity;
            else
                shakePositionOffset.X = -randomDouble * shakeIntensity;
        }
         */
    }
}
