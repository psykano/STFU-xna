using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace STFU
{
    class TrailParticleEmitter
    {
        public bool Running { get; protected set; }
        public DisplayEntity DisplayEntity { get; set; }
        protected List<Particle> particles;
        protected Timer emissionTimer;
        protected float emissionDelay;
        protected float particleLifetime;

        public TrailParticleEmitter()
        {
            emissionTimer = new Timer();
        }

        public void Initialize(int maxParticles, float emissionDelay, float particleLifetime)
        {
            this.emissionDelay = emissionDelay;
            this.particleLifetime = particleLifetime;
            particles = new List<Particle>();
            for (int i = 0; i < maxParticles; i++)
            {
                Particle particle = new Particle();
                particles.Add(particle);
            }
        }

        public void EmitParticleWithDisplayEntity(DisplayEntity displayEntity)
        {
            foreach (Particle particle in particles)
            {
                if (!particle.Active)
                {
                    particle.ActivateWithDisplayEntity(displayEntity, particleLifetime, true, true, 0.6f, 0.7f);
                    break;
                }
            }
        }

        public void Run()
        {
            if (!Running)
            {
                Running = true;
                emissionTimer.Reset();
            }
        }

        public void Stop()
        {
            if (Running)
            {
                Running = false;
            }
        }

        public void Update(float dt)
        {
            if (Running)
            {
                emissionTimer.Update(dt);
                if (emissionTimer.IsTimeUp())
                {
                    emissionTimer.SetAccumulatedDelay(emissionDelay);
                    EmitParticleWithDisplayEntity(this.DisplayEntity);
                }
            }

            foreach (Particle particle in particles)
            {
                if (particle.Active)
                    particle.Update(dt);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Particle particle in particles)
            {
                if (particle.Active)
                    particle.Draw(spriteBatch);
            }
        }

        public void DrawOpaque(SpriteBatch spriteBatch)
        {
            foreach (Particle particle in particles)
            {
                if (particle.Active)
                    particle.DrawOpaque(spriteBatch);
            }
        }

        // Used for debugging
        public int CountParticles()
        {
            int i = 0;
            foreach (Particle particle in particles)
            {
                if (particle.Active)
                    i++;
            }
            return i;
        }
    }
}
