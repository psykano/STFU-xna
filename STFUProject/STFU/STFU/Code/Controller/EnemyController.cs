/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using Microsoft.Xna.Framework;
using TreeSharp;

namespace STFU
{
    /// <summary>
    /// An enemy controller is the AI for an enemy.
    /// </summary>
    abstract class EnemyController : Controller
    {
        public EnemyController()
        {
        }
    }

    abstract class EnemyController<TEnemy> : EnemyController
        where TEnemy : Enemy
    {
        protected TEnemy enemy;
        protected TreeExecutor tree;

        // Constructor
        public EnemyController()
        {
        }

        public override void Update(float dt)
        {
            if (enemy.Controllable())
            {
                if (enemy.Health.Hit)
                {
                    updateUponHit(dt);
                }
                else
                {
                    updateUponIdle(dt);
                }
            }
        }

        protected virtual void updateUponHit(float dt)
        {
        }

        protected virtual void updateUponIdle(float dt)
        {
        }
    }
}
