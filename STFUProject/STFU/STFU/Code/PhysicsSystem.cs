/*
 * Copyright (c) 2012 Chris Johns
 */
// ported from http://www.unagames.com/blog/daniele/2010/06/fixed-time-step-implementation-box2d

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace STFU
{
    /// <summary>
    /// This controls the physics engine.
    /// Note that it is updated at a fixed rate. This is to remove issues with either a step too large or a step too small
    /// as well as to reduce issues with having a variable step (such as shaking bodies)
    /// Also note that we use a small timestep interval to minimize tunneling issues.
    /// </summary>
    class PhysicsSystem
    {
        private float fixedTimestepAccumulator;
        private float fixedTimestepAccumulatorRatio;
        private int velocityIterations;
        private int positionIterations;
        public World world { get; private set; }
        public float Dt { get; set; }

        private const float FIXED_TIMESTEP = 1f / 120f;
        // Maximum number of steps in order to avoid degrading to a halt
        private const int MAX_STEPS = 4; // since 120/4 = 30. the game will run at a slower speed below 30fps.

        // Constructor
        public PhysicsSystem()
        {
            fixedTimestepAccumulator = 0;
            fixedTimestepAccumulatorRatio = 0;
            velocityIterations = 8;
            positionIterations = 2;
            FarseerPhysics.Settings.VelocityIterations = velocityIterations;
            FarseerPhysics.Settings.PositionIterations = positionIterations;
            this.Dt = FIXED_TIMESTEP;

            // Define the gravity
            Vector2 gravity = new Vector2(0, 22f);

            // Create the world
            world = new World(gravity);
            world.AutoClearForces = false;
        }

        public void Update(float dt)
        {
            fixedTimestepAccumulator += dt;
            int nSteps = (int)Math.Floor(fixedTimestepAccumulator / FIXED_TIMESTEP);

            if (nSteps > 0)
            {
                fixedTimestepAccumulator -= nSteps * FIXED_TIMESTEP;
            }

            fixedTimestepAccumulatorRatio = fixedTimestepAccumulator / FIXED_TIMESTEP;

            // This is similar to clamp "dt":
	        //	dt = std::min (dt, MAX_STEPS * FIXED_TIMESTEP)
	        // but it allows above calculations of fixedTimestepAccumulator_ and
	        // fixedTimestepAccumulatorRatio_ to remain unchanged.
	        int nStepsClamped = Math.Min(nSteps, MAX_STEPS);
            for (int i = 0; i < nStepsClamped; i++)
	        {
		        singleStep(FIXED_TIMESTEP);
	        }
            
            this.Dt = nStepsClamped * FIXED_TIMESTEP;

            postStepping();
        }

        private void singleStep(float dt)
        {
            // In singleStep_() the CollisionManager could fire custom
            // callbacks that uses the smoothed states. So we must be sure
            // to reset them correctly before firing the callbacks.
            resetSmoothStates();

            world.Step(dt);
        }

        private void postStepping()
        {
            // We "smooth" positions and orientations using
            // fixedTimestepAccumulatorRatio_ (alpha).
            smoothStates(); // or rather, don't smooth the positions? ***

            world.ClearForces();
        }

        private void smoothStates()
        {
            float oneMinusRatio = 1f - fixedTimestepAccumulatorRatio;

            foreach (Body body in world.BodyList)
            {
                if (body.BodyType == BodyType.Static)
                {
                    continue;
                }

                Entity entity = (Entity)body.UserData;
                entity.SmoothedPosition = fixedTimestepAccumulatorRatio * entity.Position + oneMinusRatio * entity.PreviousPosition;
                entity.SmoothedRotation = fixedTimestepAccumulatorRatio * entity.Rotation + oneMinusRatio * entity.PreviousRotation;
            }
        }

        private void resetSmoothStates()
        {
            foreach (Body body in world.BodyList)
            {
                if (body.BodyType == BodyType.Static)
                {
                    continue;
                }

                Entity entity = (Entity)body.UserData;
                entity.SmoothedPosition = entity.PreviousPosition = entity.Position;
                entity.SmoothedRotation = entity.PreviousRotation = entity.Rotation;
            }
        }
    }
}
