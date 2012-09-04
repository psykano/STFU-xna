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
        protected bool blink;
        protected Timer blinkTimer;
        protected const float blinkDelay = 0.0314f;
        protected LivingEntity livingEntity;

        public LivingEntityRepresentation(LivingEntity livingEntity)
        {
            this.livingEntity = livingEntity;
            blink = false;
            blinkTimer = new Timer();
        }

        public override void Update(float dt)
        {
            checkBlink(dt);
        }

        protected void checkBlink(float dt)
        {
            if (livingEntity.Health.Hit || livingEntity.Health.Recovering || livingEntity.Health.Dead)
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
                toggleBlink();
                blinkTimer.SetDelay(blinkDelay);
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
    }
}
