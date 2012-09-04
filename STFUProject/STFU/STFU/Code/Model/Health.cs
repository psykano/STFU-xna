/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace STFU
{
    /// <summary>
    /// Health is how alive the entity with it is... I don't really know how to describe it. If you've
    /// played a video game in your life you should know how getting hit and recovering work.
    /// </summary>
    class Health
    {
        public bool Hit { get; private set; }
        public bool Recovering { get; private set; }
        public int Hitpoints { get; private set; }
        private int defaultHitpoints;
        private Timer hitTimer;
        protected float hitDelay;
        private Timer recoveryTimer;
        protected float recoveryDelay;

        public bool Dead { get; private set; }
        public Timer RespawnTimer { get; private set; }
        protected float respawnDelay;

        public Health(int hitpoints, float hitDelay, float recoveryDelay, float respawnDelay)
        {
            defaultHitpoints = hitpoints;

            hitTimer = new Timer();
            this.hitDelay = hitDelay;
            recoveryTimer = new Timer();
            this.recoveryDelay = recoveryDelay;
            this.RespawnTimer = new Timer();
            this.respawnDelay = respawnDelay;

            ResetHealth();
        }

        public void Update(float dt)
        {
            if (Hit)
            {
                hitTimer.Update(dt);

                if (hitTimer.IsTimeUp())
                {
                    recovering();
                }
            }

            if (Recovering)
            {
                recoveryTimer.Update(dt);

                if (recoveryTimer.IsTimeUp())
                {
                    recoveredFromHit();
                }
            }

            if (Dead)
            {
                this.RespawnTimer.Update(dt);
            }
        }

        public void ResetHealth()
        {
            this.Hitpoints = defaultHitpoints;
            Hit = false;
            Recovering = false;
            Dead = false;
            hitTimer.Reset();
            recoveryTimer.Reset();
            this.RespawnTimer.Reset();
        }

        public void IncreaseHealth()
        {
            Hitpoints++;
        }

        public void GotHit(int healthLost)
        {
            Hit = true;
            hitTimer.SetDelay(hitDelay);

            // decrease health
            Hitpoints = Hitpoints - healthLost;
        }

        private void recovering()
        {
            Hit = false;
            Recovering = true;
            recoveryTimer.SetDelay(recoveryDelay);
        }

        private void recoveredFromHit()
        {
            Recovering = false;
        }

        public bool CheckInvulnerable()
        {
            //if (Recovering)
            if (this.Hit || this.Recovering)
            {
                return true;
            }

            return false;
        }

        public void Die()
        {
            Dead = true;
            this.RespawnTimer.SetDelay(respawnDelay);
        }

        public bool CheckRespawn()
        {
            if (Dead)
            {
                if (this.RespawnTimer.IsTimeUp())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
