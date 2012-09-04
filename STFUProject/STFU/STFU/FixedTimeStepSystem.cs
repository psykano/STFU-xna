/*
 * Copyright (c) 2012 Chris Johns
 */
// idea from http://www.unagames.com/blog/daniele/2010/06/fixed-time-step-implementation-box2d

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STFU
{
    public delegate void SingleStep(float dt);
    public delegate void PostStepping();

    /// <summary>
    /// Certain aspects of the game are updated at a fixed rate.
    /// </summary>
    class FixedTimeStepSystem
    {
        private float fixedTimestepAccumulator;
        private float fixedTimestepAccumulatorRatio;
        private float fixedTimeStep;
        private int maxSteps;
        private SingleStep singleStep;
        private PostStepping postStepping;

        // Constructor
        public FixedTimeStepSystem(float fixedTimeStep, int maxSteps, SingleStep singleStep, PostStepping postStepping)
        {
            fixedTimestepAccumulator = 0;
            fixedTimestepAccumulatorRatio = 0;
            this.fixedTimeStep = fixedTimeStep;
            this.maxSteps = maxSteps;
            this.singleStep = singleStep;
            this.postStepping = postStepping;
        }

        public void Update(float dt)
        {
            fixedTimestepAccumulator += dt;
            int nSteps = (int)Math.Floor(fixedTimestepAccumulator / fixedTimeStep);

            if (nSteps > 0)
            {
                fixedTimestepAccumulator -= nSteps * fixedTimeStep;
            }

            fixedTimestepAccumulatorRatio = fixedTimestepAccumulator / fixedTimeStep;

            int nStepsClamped = Math.Min(nSteps, maxSteps);
            for (int i = 0; i < nStepsClamped; i++)
            {
                singleStep(fixedTimeStep);
            }

            postStepping();
        }
    }
}
