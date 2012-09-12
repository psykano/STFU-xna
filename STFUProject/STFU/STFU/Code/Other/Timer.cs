/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using Microsoft.Xna.Framework;

namespace STFU
{
    /// <summary>
    /// A timer is... exactly what it sounds like. You set a delay; it goes off when it reaches 0; the usual.
    /// </summary>
    class Timer
    {
        private float time;
        private float delay;

        // Constructor
        public Timer()
        {
            Reset();
        }

        public void Update(float dt)
        {
            if (time < 0)
            {
                time += dt;
            }
        }

        public void Reset()
        {
            time = float.MaxValue;
        }

        public bool IsReset()
        {
            if (time >= float.MaxValue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetDelay(float delay)
        {
            this.delay = delay;
            time = -delay;
        }

        public bool IsTimeUp()
        {
            if (time >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public float Time()
        {
            return -time;
        }

        public float PercentFromTimeUp()
        {
            if (time >= 0)
            {
                return 0f;
            }
            else
            {
                return -time / delay;
            }
        }

        public void SetAccumulatedDelay(float delay)
        {
            this.delay = delay;
            if (IsReset())
            {
                time = 0;
            }
            time -= delay;
        }
    }
}
