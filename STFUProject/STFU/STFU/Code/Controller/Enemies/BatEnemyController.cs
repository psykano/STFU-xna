/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using Microsoft.Xna.Framework;
using TreeSharp;

namespace STFU
{
    /// <summary>
    /// A bat enemy controller is the AI for a bat enemy.
    /// It flies forward unless there's an obstacle at which point it changes direction. If there's nowhere
    /// to fly, it will float in place.
    /// It's always looking down-forward for a player and if it finds one it will swoop down at the player.
    /// </summary>
    class BatEnemyController : EnemyController<BatEnemy>
    {
        // Constructor
        public BatEnemyController(BatEnemy enemy)
        {
            this.enemy = enemy;

            tree = new TreeExecutor(Mode());
            tree.Start();
        }

        protected override void updateUponHit(float dt)
        {
            // do nothing
        }

        protected override void updateUponIdle(float dt)
        {
            tree.Tick();
        }

        // Sets mode based on specific behaviors
        protected Composite Mode()
        {
            return new PrioritySelector(
                new Decorator(ret => enemy.Behavior.Compare(BatBehavior.Diving),
                    Diving()),

                new Decorator(ret => !enemy.Behavior.Compare(BatBehavior.Diving),
                    Idle())
            );
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
                    FlyAround())
            );
        }

        protected PrioritySelector FlyAround()
        {
            return new PrioritySelector(
                new Decorator(ret => enemy.CheckForPlayer(),
                    DiveAtPlayer()),

                new Decorator(ret => !enemy.CheckForPlayer(),
                    new TSAction(ret => enemy.Fly()))
            );
        }

        protected PrioritySelector DiveAtPlayer()
        {
            return new PrioritySelector(
                new Decorator(ret => enemy.CheckAbleToDive(),
                    new TSAction(ret => enemy.Dive())),

                new Decorator(ret => !enemy.CheckAbleToDive(),
                    new TSAction(ret => enemy.Fly()))
            );
        }

        protected Composite Diving()
        {
            return new Decorator(ret => enemy.CheckStopDiving(),
                new TSAction(ret => enemy.Idle())
            );
        }
    }
}
