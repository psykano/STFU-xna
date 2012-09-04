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
            time = 0;
            time -= delay;
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
    }
}
