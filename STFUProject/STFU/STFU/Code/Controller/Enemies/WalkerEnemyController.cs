/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using Microsoft.Xna.Framework;
using TreeSharp;

namespace STFU
{
    /// <summary>
    /// A walker enemy controller is the AI for a walker enemy.
    /// It walks forward unless there's an obstacle or lack of ground ahead at which point it changes direction. If
    /// there's nowhere to walk it will stay still.
    /// It's always looking forward for a player and if it finds one it will run forward. If it loses sight of the
    /// player while it's running it will continue to walk.
    /// </summary>
    class WalkerEnemyController : EnemyController<WalkerEnemy>
    {
        // Constructor
        public WalkerEnemyController(WalkerEnemy enemy)
        {
            this.enemy = enemy;

            tree = new TreeExecutor(Idle());
            tree.Start();
        }

        protected override void updateUponHit(float dt)
        {
            enemy.StopMoving();
        }

        protected override void updateUponIdle(float dt)
        {
            tree.Tick();
        }

        protected Composite Idle()
        {
            return new PrioritySelector(
                new Decorator(ret => enemy.CheckTrapped(),
                    new TSAction(ret => enemy.Trapped())),

                new Decorator(ret => !enemy.CheckTrapped(),
                    Move())
            );
        }

        protected Composite Move()
        {
            return new PrioritySelector(
                new Decorator(ret => enemy.CheckTurnAround(),
                    new TSAction(ret => enemy.TurnAround())),

                new Decorator(ret => !enemy.CheckTurnAround(),
                    WalkAround())
            );
        }

        protected PrioritySelector WalkAround()
        {
            return new PrioritySelector(
                new Decorator(ret => enemy.CheckForPlayer(),
                    new TSAction(ret => enemy.Run())),

                new Decorator(ret => !enemy.CheckForPlayer(),
                    new TSAction(ret => enemy.Walk()))
            );
        }
    }
}
