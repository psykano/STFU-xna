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

namespace STFU
{
    public enum BatBehavior
    {
        Flying,
        Diving,
        Idle
    }

    /// <summary>
    /// A bat enemy has a smaller rectangular frame and flaps in the air for movement.
    /// It has vision to look for a player.
    /// It attacks by diving (swooping down) at a player.
    /// It has less than average hit points.
    /// </summary>
    class BatEnemy : Enemy<EnemyPhysicsCharacter>
    {
        public EnemyBehavior<BatBehavior> Behavior { get; private set; }
        public EnemyVision Vision { get; private set; }
        public EnemyMovement Movement { get; private set; }
        private const float flyPercent = 0.6f;
        private const float sightForPlayerDistance = 2f;
        private const float sightForWallDistance = 0.25f;
        private const float sightForPlayerAngle = 50f;
        private const float sightForPlayerAngleOffset = 40f;
        private const float enemySightDelay = 0.25f;
        private const float turnAroundTimerDelay = 0f;
        private const float divePixelOffset = 2f;

        private Timer flapTimer; // put this in movement somehow ***
        private float flapTimerDelay;
        private Timer diveTimer; // and this maybe? ***
        private float diveTimerDelay;

        // Constructor
        public BatEnemy(World world, EnemyEvent enemyEvent)
            : base(world, enemyEvent)
        {
            this.FacingRight = false;
            this.Behavior = new EnemyBehavior<BatBehavior>(enemyEvent);
            Vision = new EnemyVision(sightForPlayerDistance, enemySightDelay);
            Movement = new EnemyMovement(turnAroundTimerDelay);

            flapTimer = new Timer();
            flapTimerDelay = 0.25f;
            diveTimer = new Timer();
            diveTimerDelay = 0.5f;
        }

        public override void SetUpEnemy(Vector2 enemyStartPosition, float width, float height, int health, float hitDelay, float recoveryDelay)
        {
            base.SetUpEnemy(enemyStartPosition, width, height, health, hitDelay, recoveryDelay);
        }

        public override void Spawn()
        {
            base.Spawn();

            // Physics:
            this.PhysicsContainer.Object = null;
            this.PhysicsContainer.Object = new EnemyPhysicsCharacter(this, world, this.SpawnPosition, this.ScreenWidth, this.ScreenHeight, 1f,
                new OnCollision(onCollision), new OnSeparation(onSeparation));
            this.PhysicsContainer.Object.CollisionCategory = GameConstants.EnemyCollisionCategory;
            this.PhysicsContainer.Object.flySpeed = 15;

            Vision.Reset();
            Movement.Reset();

            flapTimer.Reset();
            diveTimer.Reset();
        }

        protected override void updateWhenAlive(float dt)
        {
            base.updateWhenAlive(dt);

            Vision.Update(dt);
            Movement.Update(dt);

            flapTimer.Update(dt);
            diveTimer.Update(dt);

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
            bool checkForPlayer = false;

            if (this.PhysicsContainer.Object.CheckForPlayerDiagonally(this.FacingRight, sightForPlayerDistance, sightForPlayerAngle))
            {
                this.StopMoving(); // find a better way to do this - set him Idle in the meantime ***
                checkForPlayer = true;
            }
            else if (this.PhysicsContainer.Object.CheckForPlayerDiagonally(this.FacingRight, sightForPlayerDistance, sightForPlayerAngle + sightForPlayerAngleOffset))
            {
                this.StopMoving(); // same as above ***
                checkForPlayer = true;
            }

            return Vision.CheckForPlayer(checkForPlayer);
        }

        public bool CheckTrapped()
        {
            if (this.PhysicsContainer.Object.CheckForWallAhead(this.FacingRight, sightForWallDistance + 0.1f) && this.PhysicsContainer.Object.CheckForWallAhead(!this.FacingRight, sightForWallDistance + 0.1f))
            {
                return true;
            }

            if (this.PhysicsContainer.Object.OnRightWall && this.PhysicsContainer.Object.OnLeftWall)
            {
                return true;
            }

            return false;
        }

        public void Trapped()
        {
            this.Movement.Stop();
        }

        public bool CheckTurnAround()
        {
            // Check if he's going to hit a wall ahead
            if (this.PhysicsContainer.Object.CheckForWallAhead(this.FacingRight, sightForWallDistance))
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

            return Movement.CheckTurnAround(false);
        }

        public void TurnAround()
        {
            Movement.TurnAround(this);
            this.StopMoving();
        }

        public bool CheckAbleToDive()
        {
            if (!this.PhysicsContainer.Object.OnGround && this.Position.Y <= ConvertUnits.ToSimUnits(this.SpawnPosition.Y + divePixelOffset))
            {
                return true;
            }

            return false;
        }

        public bool CheckStopDiving()
        {
            if (this.PhysicsContainer.Object.OnGround)
            {
                return true;
            }

            if (diveTimer.IsTimeUp())
            {
                return true;
            }

            return false;
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
            SetUpEnemy(this.SpawnPosition, EnemyFactory.BatWidth, EnemyFactory.BatHeight, EnemyFactory.BatHealth, EnemyFactory.BatHitDelay, EnemyFactory.BatRecoveryDelay);
        }
         */

        // Activities:
        private bool checkFlap()
        {
            if (this.Position.Y >= ConvertUnits.ToSimUnits(this.SpawnPosition.Y) || this.PhysicsContainer.Object.OnGround)
            {
                if (flapTimer.IsTimeUp())
                {
                    flapTimer.SetDelay(flapTimerDelay);
                    enemyEvent.flap = true;
                    return true;
                }
            }

            return false;
        }

        public void FlyLeft()
        {
            this.FacingRight = false;
            if (checkFlap())
            {
                this.PhysicsContainer.Object.FlapLeft(flyPercent);
            }
        }

        public void FlyRight()
        {
            this.FacingRight = true;
            if (checkFlap())
            {
                this.PhysicsContainer.Object.FlapRight(flyPercent);
            }
        }

        public void DiveLeft()
        {
            this.FacingRight = false;
            this.PhysicsContainer.Object.DiveLeft(1f, this.PhysicsContainer.Object.playerAngle);
        }

        public void DiveRight()
        {
            this.FacingRight = true;
            this.PhysicsContainer.Object.DiveRight(1f, this.PhysicsContainer.Object.playerAngle);
        }

        public void StopMoving()
        {
            this.PhysicsContainer.Object.StopFlying();
            if (checkFlap())
            {
                this.PhysicsContainer.Object.Float(flyPercent);
            }
        }

        // Behaviors:
        private bool changeBehavior(BatBehavior newBehavior)
        {
            return this.Behavior.ChangeTo(newBehavior);
        }

        public void Fly()
        {
            changeBehavior(BatBehavior.Flying);
            Movement.Move();
            if (this.FacingRight)
                FlyRight();
            else
                FlyLeft();
        }

        public void Dive()
        {
            changeBehavior(BatBehavior.Diving);
            Movement.Move();
            diveTimer.SetDelay(diveTimerDelay);
            if (this.FacingRight)
                DiveRight();
            else
                DiveLeft();
        }

        public void Idle()
        {
            changeBehavior(BatBehavior.Idle);
            Movement.Stop();
            Vision.Reset();
        }
    }
}
