/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Common;
using EasyConfig;

namespace STFU
{
    public enum WalkerBehavior
    {
        Walking,
        Running,
        Idle
    }

    /// <summary>
    /// A walker enemy has a medium rectangular frame and rolls on the ground for movement.
    /// It has vision to look for a player.
    /// It attacks by running (rolling quicker) toward a player.
    /// It has average hit points.
    /// </summary>
    class WalkerEnemy : Enemy<EnemyPhysicsCharacter>, IEnemyMovement
    {
        public EnemyBehavior<WalkerBehavior> Behavior { get; private set; }
        public EnemyVision Vision { get; private set; }
        public EnemyMovement Movement { get; private set; }

        // Resources
        private float walkPercent;
        private float sightForPlayerDistance;
        private float sightForGroundDistance;
        private float enemySightDelay;
        private float turnAroundTimerDelay;

        private const string initSettings = "WalkerInit";

        // Constructor
        public WalkerEnemy(World world, EnemyEvent enemyEvent) 
            : base(world, enemyEvent)
        {
            // Load resources
            ConfigFile configFile = GetEnemyConfigFile();
            screenWidth = configFile.SettingGroups[initSettings].Settings["screenWidth"].GetValueAsFloat();
            screenHeight = configFile.SettingGroups[initSettings].Settings["screenHeight"].GetValueAsFloat();
            walkPercent = configFile.SettingGroups[initSettings].Settings["walkPercent"].GetValueAsFloat();
            sightForPlayerDistance = configFile.SettingGroups[initSettings].Settings["sightForPlayerDistance"].GetValueAsFloat();
            sightForGroundDistance = configFile.SettingGroups[initSettings].Settings["sightForGroundDistance"].GetValueAsFloat();
            enemySightDelay = configFile.SettingGroups[initSettings].Settings["enemySightDelay"].GetValueAsFloat();
            turnAroundTimerDelay = configFile.SettingGroups[initSettings].Settings["turnAroundTimerDelay"].GetValueAsFloat();

            this.FacingRight = false;
            this.Behavior = new EnemyBehavior<WalkerBehavior>(enemyEvent);
            Vision = new EnemyVision(sightForPlayerDistance, enemySightDelay);
            Movement = new EnemyMovement(turnAroundTimerDelay);
        }

        public override void SetUpEnemy(Vector2 enemyStartPosition, int health, float hitDelay, float recoveryDelay)
        {
            base.SetUpEnemy(enemyStartPosition, health, hitDelay, recoveryDelay);
        }

        public override void Spawn()
        {
            base.Spawn();

            // Physics:
            this.PhysicsContainer.Object = null;
            this.PhysicsContainer.Object = new EnemyPhysicsCharacter(this, world, this.SpawnPosition, this.ScreenWidth, this.ScreenHeight, 1f,
                new OnCollision(onCollision), new OnSeparation(onSeparation));
            this.PhysicsContainer.Object.CollisionCategory = GameConstants.EnemyCollisionCategory;
            this.PhysicsContainer.Object.RunSpeed = 15;

            Vision.Reset();
            Movement.Reset();
        }

        protected override void updateWhenAlive(float dt)
        {
            base.updateWhenAlive(dt);

            Vision.Update(dt);
            Movement.Update(dt);

            // Check if he's supposed to be stopped
            if (Movement.Stopped)
            {
                // make sure the behavior is to idle
                Idle();
                this.StopMoving();
            }
        }

        protected override void onCollisionWithBullet()
        {
            gotHitByBullet();
        }

        public bool CheckForPlayer()
        {
            return Vision.CheckForPlayer(this.PhysicsContainer.Object.CheckForPlayerHorizontally(this.FacingRight, sightForPlayerDistance));
        }

        public bool CheckTrapped()
        {
            if (this.PhysicsContainer.Object.CheckIfStill())
            {
                // On a small platform
                if (!this.PhysicsContainer.Object.CheckForGroundAhead(true, sightForGroundDistance + 0.1f) && !this.PhysicsContainer.Object.CheckForGroundAhead(false, sightForGroundDistance + 0.1f))
                {
                    return true;
                }

                // Between two walls
                if (this.PhysicsContainer.Object.OnRightWall && this.PhysicsContainer.Object.OnLeftWall)
                {
                    return true;
                }

                // Between a wall and a platform
                if (!this.PhysicsContainer.Object.CheckForGroundAhead(true, sightForGroundDistance + 0.1f) && this.PhysicsContainer.Object.OnLeftWall)
                {
                    return true;
                }
                if (!this.PhysicsContainer.Object.CheckForGroundAhead(false, sightForGroundDistance + 0.1f) && this.PhysicsContainer.Object.OnRightWall)
                {
                    return true;
                }
            }

            return false;
        }

        public void Trapped()
        {
            this.Movement.Stop();
        }

        public bool CheckTurnAround()
        {
            // Don't turn around if he's falling
            if (IsFalling())
            {
                return Movement.CheckTurnAround(false);
            }

            if (this.PhysicsContainer.Object.CheckIfNotMovingVertically())
            {
                // Check if he's at the edge of the platform
                if (!this.PhysicsContainer.Object.CheckForGroundAhead(this.FacingRight, sightForGroundDistance))
                {
                    return Movement.CheckTurnAround(true);
                }

                // Check if he hit a wall
                if (this.FacingRight)
                {
                    if (this.PhysicsContainer.Object.OnRightWall)
                        return Movement.CheckTurnAround(true);
                }
                else
                {
                    if (this.PhysicsContainer.Object.OnLeftWall)
                        return Movement.CheckTurnAround(true);
                }
            }

            // Check if he hit an enemy?

            return Movement.CheckTurnAround(false);
        }
        
        public void TurnAround()
        {
            Movement.TurnAround(this);
        }

        public override Vector2 Position
        {
            get
            {
                // we actually need to do this since there's an offset with the enemy's physics body
                return this.PhysicsContainer.Object.Position;
            }
        }

        public override float Rotation
        {
            get
            {
                return this.PhysicsContainer.Object.body.Rotation;
            }
        }

        /*
        public override void Respawn()
        {
            base.Respawn();
            SetUpEnemy(this.SpawnPosition, EnemyFactory.WalkerWidth, EnemyFactory.WalkerHeight, EnemyFactory.WalkerHealth, EnemyFactory.WalkerHitDelay, EnemyFactory.WalkerRecoveryDelay);
        }
         */

        // Activities:
        public void WalkLeft()
        {
            if (this.PhysicsContainer.Object.OnGround)
            {
                this.FacingRight = false;
                this.PhysicsContainer.Object.RunLeft(walkPercent);
            }
        }

        public void WalkRight()
        {
            if (this.PhysicsContainer.Object.OnGround)
            {
                this.FacingRight = true;
                this.PhysicsContainer.Object.RunRight(walkPercent);
            }
        }

        public void RunLeft()
        {
            if (this.PhysicsContainer.Object.OnGround)
            {
                this.FacingRight = false;
                this.PhysicsContainer.Object.RunLeft(1f);
            }
        }

        public void RunRight()
        {
            if (this.PhysicsContainer.Object.OnGround)
            {
                this.FacingRight = true;
                this.PhysicsContainer.Object.RunRight(1f);
            }
        }

        public void StopMoving()
        {
            if (this.PhysicsContainer.Object.OnGround)
            {
                this.PhysicsContainer.Object.StopRunning();
            }
        }

        public bool IsFalling()
        {
            if (!this.PhysicsContainer.Object.OnGround)
                return true;
            else
                return false;
        }

        // Behaviors:
        private bool changeBehavior(WalkerBehavior newBehavior)
        {
            return this.Behavior.ChangeTo(newBehavior);
        }

        public void Walk()
        {
            changeBehavior(WalkerBehavior.Walking);
            Movement.Move();
            if (this.FacingRight)
                WalkRight();
            else
                WalkLeft();
        }

        public void Run()
        {
            changeBehavior(WalkerBehavior.Running);
            Movement.Move();
            if (this.FacingRight)
                RunRight();
            else
                RunLeft();
        }

        public void Idle()
        {
            changeBehavior(WalkerBehavior.Idle);
            Movement.Stop();
            Vision.Reset();
        }
    }
}
