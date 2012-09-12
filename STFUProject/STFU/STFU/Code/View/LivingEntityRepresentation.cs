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
    /// A living entity representation is the visual representation(sorry!) of the living entity on the screen.
    /// </summary>
    abstract class LivingEntityRepresentation : EntityRepresentation
    {
        protected LivingEntity livingEntity;

        // for blinking
        protected bool blink;
        protected Timer blinkTimer;
        protected const float blinkDelay = 0.0314f;

        // for shaking
        protected Random randomShake;
        protected Vector2 shakePositionOffset;
        protected Timer shakeTimer;
        protected const float shakeDelay = 0.02f;
        protected const float shakeIntensity = 5.0f;

        public LivingEntityRepresentation(LivingEntity livingEntity)
        {
            this.livingEntity = livingEntity;
            blink = false;
            blinkTimer = new Timer();
            randomShake = new Random();
            shakePositionOffset = Vector2.Zero;
            shakeTimer = new Timer();
        }

        public override void Update(float dt)
        {
            checkBlink(dt);
            checkShake(dt);
        }

        protected void checkBlink(float dt)
        {
            if (livingEntity.Health.Recovering || livingEntity.Health.Dead)
            {
                doBlink(dt);
            }
            else
            {
                dontBlink();
            }
        }

        protected void doBlink(float dt)
        {
            blinkTimer.Update(dt);

            if (blinkTimer.IsTimeUp())
            {
                blinkTimer.SetDelay(blinkDelay);
                toggleBlink();
            }
        }

        protected void dontBlink()
        {
            blink = false;
            blinkTimer.Reset();
        }

        private void toggleBlink()
        {
            if (blink)
                blink = false;
            else
                blink = true;
        }

        protected void checkShake(float dt)
        {
            if (livingEntity.Health.Hit)
            {
                shakeTimer.Update(dt);

                if (shakeTimer.IsTimeUp())
                {
                    shakeTimer.SetAccumulatedDelay(shakeDelay);
                    doShake();
                }
            }
            else
            {
                shakePositionOffset = Vector2.Zero;
            }
        }

        protected void doShake()
        {
            float randomDouble = (float)randomShake.NextDouble();
            randomDouble *= 0.6f;
            randomDouble += 0.4f;

            if (shakePositionOffset.X < 0)
                shakePositionOffset.X = randomDouble * shakeIntensity;
            else
                shakePositionOffset.X = -randomDouble * shakeIntensity;
        }
    }
}
