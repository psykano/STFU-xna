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
using FarseerPhysics.Dynamics.Contacts;

namespace STFU
{
    /// <summary>
    /// An entity that has health and so can be attacked and die.
    /// </summary>
    abstract class LivingEntity : Entity
    {
        public bool FacingRight { get; set; }
        public Health Health { get; protected set; }
        public Vector2 SpawnPosition { get; set; }

        public LivingEntity(World world)
            : base(world)
        {
        }

        protected void setUpLivingEntity(Vector2 spawnPosition, float width, float height, int health, float hitDelay, float recoveryDelay, float respawnDelay)
        {
            this.SpawnPosition = spawnPosition;
            this.Width = ConvertUnits.ToSimUnits(width);
            this.Height = ConvertUnits.ToSimUnits(height);
            this.Health = new Health(health, hitDelay, recoveryDelay, respawnDelay);
        }

        protected void spawnLivingEntity()
        {
            // reset smoothed properties
            this.SmoothedPosition = this.SpawnPosition;
            this.SmoothedRotation = 0;

            // reset health
            this.Health.ResetHealth();
        }

        public virtual void Spawn()
        {
        }

        public override void Update(float dt)
        {
            checkForDead();
            checkForRespawn();
            this.Health.Update(dt);

            if (this.Health.Dead)
            {
                updateWhenDead(dt);
            }
            else
            {
                updateWhenAlive(dt);
            }
        }

        protected virtual void updateWhenDead(float dt)
        {
        }

        protected virtual void updateWhenAlive(float dt)
        {
        }

        public virtual void Die()
        {
            if (this.Health.Dead)
                return;

            this.Health.Die();
        }

        protected void checkForDead()
        {
            // Check if we don't have any health left
            if (this.Health.Hitpoints <= 0)
            {
                Die();
            }

            // Check if we fell to our doom
            if (this.Position.Y - this.Height*0.5f > GameVariables.GetSimLevelHeight())
            {
                Die();
            }
        }

        protected void checkForRespawn()
        {
            if (this.Health.CheckRespawn())
            {
                Respawn();
            }
        }

        public virtual void Respawn()
        {
        }

        public virtual bool Controllable()
        {
            if (this.Health.Dead)
            {
                return false;
            }

            return true;
        }
        
        public override Vector2 ScreenPosition
        {
            get
            {
                return ConvertUnits.ToDisplayUnits(SmoothedPosition);
            }
        }
        public override float ScreenRotation
        {
            get
            {
                return SmoothedRotation;
            }
        }
    }
}
