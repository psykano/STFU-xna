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
    /// Takes care of an enemy's vision in terms of looking for a player: when saw/lost sight of a player.
    /// </summary>
    class EnemyVision
    {
        public float SightForPlayerDistance { get; set; }
        public bool EnemySeesPlayer { get; private set; }
        private Timer enemySawPlayerTimer;
        private float enemySightDelay;

        public EnemyVision(float sightForPlayerDistance, float enemySightDelay)
        {
            this.SightForPlayerDistance = sightForPlayerDistance;
            this.enemySightDelay = enemySightDelay;
            enemySawPlayerTimer = new Timer();
            Reset();
        }

        public void Reset()
        {
            EnemySeesPlayer = false;
            enemySawPlayerTimer.Reset();
        }

        public void Update(float dt)
        {
            enemySawPlayerTimer.Update(dt);
        }

        public bool CheckForPlayer(bool playerInSight)
        {
            if (playerInSight)
            {
                if (!EnemySeesPlayer)
                {
                    EnemySeesPlayer = true;
                    enemySawPlayerTimer.SetDelay(enemySightDelay);
                }

                if (enemySawPlayerTimer.IsTimeUp())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (EnemySeesPlayer)
                {
                    EnemySeesPlayer = false;
                    enemySawPlayerTimer.SetDelay(enemySightDelay);
                }

                if (enemySawPlayerTimer.IsTimeUp())
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}
