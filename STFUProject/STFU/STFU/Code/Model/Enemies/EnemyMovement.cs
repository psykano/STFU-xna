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
    public interface IEnemyMovement
    {
        bool CheckTurnAround();
        void TurnAround();
    }

    /// <summary>
    /// Takes care of an enemy's movement: when to turn around, and when to stop/go.
    /// </summary>
    class EnemyMovement
    {
        public bool Stopped { get; private set; }
        private bool turnAround;
        private Timer turnAroundTimer;
        private float turnAroundTimerDelay;

        public EnemyMovement(float turnAroundTimerDelay)
        {
            this.turnAroundTimerDelay = turnAroundTimerDelay;
            turnAroundTimer = new Timer();
            Reset();
        }

        public void Reset()
        {
            turnAround = false;
            turnAroundTimer.Reset();
        }

        public void Update(float dt)
        {
            turnAroundTimer.Update(dt);
        }

        public bool CheckTurnAround(bool shouldTurnAround)
        {
            if (shouldTurnAround)
            {
                return true;
            }
            else
            {
                turnAround = false;
                return false;
            }
        }

        public void TurnAround(Enemy enemy)
        {
            Stop();

            if (!turnAround)
            {
                turnAround = true;
                turnAroundTimer.SetDelay(turnAroundTimerDelay);
            }

            if (turnAroundTimer.IsTimeUp())
            {
                turnAround = false;

                if (enemy.FacingRight)
                    enemy.FacingRight = false;
                else
                    enemy.FacingRight = true;
            }
        }

        public void Stop()
        {
            Stopped = true;
        }

        public void Move()
        {
            Stopped = false;
        }
    }
}
