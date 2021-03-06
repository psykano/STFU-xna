﻿/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Common;
using EasyConfig;

namespace STFU
{
    /// <summary>
    /// A player physics character represents a player in the physics world.
    /// </summary>
    class PlayerPhysicsCharacter : CompositePhysicsCharacter
    {
        public bool wallSlideEnabled;

        private Vector2 jumpForce;
        private Vector2 airControlForce;

        // Resources
        private float moveSpeed;
        private float airControlSpeed;
        private float jumpImpulse;
        private float wallJumpImpulse;
        private float airControlAbility;
        private float terminalVelocity;
        private float slideTerminalVelocity;

        private Timer onGroundTimer;
        private Timer onLeftWallTimer;
        private Timer onRightWallTimer;
        private bool enableOnGroundDelay;
        private bool enableOnLeftWallDelay;
        private bool enableOnRightWallDelay;
        private bool prevOnGround;
        private bool prevOnLeftWall;
        private bool prevOnRightWall;
        private bool nextOnGround;
        private bool nextOnLeftWall;
        private bool nextOnRightWall;
        private const float onGroundOrWallDelay = 0.05f;

        private float runSpeed;
        private float runAirControlSpeed;
        private float dashMultiplier;
        private const float dashFrictionOffsetVelX = 1.7f; // the 1.7 accounts for friction when dashing

        private const string physicsSettings = "PlayerPhysicsCharacter";

        public PlayerPhysicsCharacter(Entity owner, World world, Vector2 position, float width, float height, float density, OnCollision onCollision, OnSeparation onSeparation)
            : base(owner, world, position, width, height, density, onCollision, onSeparation)
        {
            // Load resources
            ConfigFile configFile = PhysicsSystem.GetPhysicsConfigFile();
            runSpeed = configFile.SettingGroups[physicsSettings].Settings["runSpeed"].GetValueAsFloat();
            runAirControlSpeed = configFile.SettingGroups[physicsSettings].Settings["airControlSpeed"].GetValueAsFloat();
            jumpImpulse = configFile.SettingGroups[physicsSettings].Settings["jumpImpulse"].GetValueAsFloat();
            wallJumpImpulse = configFile.SettingGroups[physicsSettings].Settings["wallJumpImpulse"].GetValueAsFloat();
            airControlAbility = configFile.SettingGroups[physicsSettings].Settings["airControlAbility"].GetValueAsFloat();
            terminalVelocity = configFile.SettingGroups[physicsSettings].Settings["terminalVelocity"].GetValueAsFloat();
            slideTerminalVelocity = configFile.SettingGroups[physicsSettings].Settings["slideTerminalVelocity"].GetValueAsFloat();
            dashMultiplier = configFile.SettingGroups[physicsSettings].Settings["dashMultiplier"].GetValueAsFloat();

            wallSlideEnabled = false;
            onGroundTimer = new Timer();
            onLeftWallTimer = new Timer();
            onRightWallTimer = new Timer();
            enableOnGroundDelay = true;
            enableOnLeftWallDelay = true;
            enableOnRightWallDelay = true;

            moveSpeed = runSpeed;
            airControlSpeed = runAirControlSpeed;
        }

        protected override void performChecks()
        {
            // we don't care about the ceiling
            checkForGround();
            checkForWall(false, false);
            checkForCharOnHead(true, false);

            // record the previous values to the next values
            prevOnGround = nextOnGround;
            prevOnLeftWall = nextOnLeftWall;
            prevOnRightWall = nextOnRightWall;

            // record the next values
            nextOnGround = onGroundPhysics;
            nextOnLeftWall = OnLeftWall;
            nextOnRightWall = OnRightWall;

            if (onGroundPhysics)
                enableOnGroundDelay = true;
            if (OnLeftWall)
                enableOnLeftWallDelay = true;
            if (OnRightWall)
                enableOnRightWallDelay = true;

            onGroundDelay = false;
            if (enableOnGroundDelay && !onGroundPhysics)
            {
                if (prevOnGround && !nextOnGround)
                {
                    if (body.LinearVelocity.Y >= 0) // so that it doesn't happen when shooting upwards off a platform
                    {
                        onGroundTimer.SetDelay(onGroundOrWallDelay);
                    }
                }

                if (!onGroundPhysics && !onGroundTimer.IsTimeUp())
                {
                    onGroundDelay = true;
                }
            }

            if (enableOnLeftWallDelay && !OnLeftWall)
            {
                if (prevOnLeftWall && !nextOnLeftWall)
                {
                    onLeftWallTimer.SetDelay(onGroundOrWallDelay);
                }

                if (!OnLeftWall && !onLeftWallTimer.IsTimeUp())
                {
                    OnLeftWall = true;
                }
            }

            if (enableOnRightWallDelay && !OnRightWall)
            {
                if (prevOnRightWall && !nextOnRightWall)
                {
                    onRightWallTimer.SetDelay(onGroundOrWallDelay);
                }

                if (!OnRightWall && !onRightWallTimer.IsTimeUp())
                {
                    OnRightWall = true;
                }
            }
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            //Console.WriteLine("velx: {0}", body.LinearVelocity.X);

            onGroundTimer.Update(dt);
            onLeftWallTimer.Update(dt);
            onRightWallTimer.Update(dt);

            if (body.LinearVelocity.Y > terminalVelocity)
            {
                body.LinearVelocity = new Vector2(body.LinearVelocity.X, terminalVelocity);
            }

            if (wallSlideEnabled)
            {
                if (OnWall())
                {
                    // this helps keep the player on the wall when doing a wall slide
                    if (!OnGround && !checkEdgeCatching())
                    {
                        if (OnLeftWall)
                        {
                            body.LinearVelocity = new Vector2(-0.02f, body.LinearVelocity.Y); // don't go lower than -0.02, otherwise there's "edge-catching"
                        }
                        else if (OnRightWall)
                        {
                            body.LinearVelocity = new Vector2(0.02f, body.LinearVelocity.Y); // ^^ same with higher than 0.02
                        }
                    }

                    if (body.LinearVelocity.Y >= slideTerminalVelocity)
                    {
                        body.LinearVelocity = new Vector2(body.LinearVelocity.X, slideTerminalVelocity);
                    }
                }
            }

            // perform this check at the end
            adjustForEdgeCatching();
        }

        protected bool RayCastForEnemy(Vector2 rayStart, Vector2 rayEnd)
        {
            bool rayHitSomething = false;
            world.RayCast((fixture, point, normal, fraction) =>
            {
                if (fixture == null || collidedWithSelf(fixture.Body) || fixture.IsSensor)
                {
                    return -1;
                }
                else
                {
                    if (collidedWithEnemy(fixture.CollisionCategories))
                    {
                        rayHitSomething = true;
                    }
                    else
                    {
                        rayHitSomething = false;
                    }
                }
                return fraction;
            }, rayStart, rayEnd);

            return rayHitSomething;
        }

        public bool CheckForEnemyHorizontally(bool facingRight, float distance)
        {
            float halfWidth = extents.X;

            if (!facingRight)
            {
                distance = -distance;
                halfWidth = -halfWidth;
            }

            // in the middle of the player
            Vector2 rayStart = new Vector2(Position.X - halfWidth, Position.Y); // at the back of the player
            Vector2 rayEnd = rayStart + new Vector2(distance + 2*halfWidth, 0);
            if (RayCastForEnemy(rayStart, rayEnd))
                return true;

            // a bit lower
            rayStart.Y += extents.Y * 0.5f;
            rayEnd.Y += extents.Y * 0.5f;
            if (RayCastForEnemy(rayStart, rayEnd))
                return true;

            // a bit higher
            rayStart.Y -= extents.Y;
            rayEnd.Y -= extents.Y;
            if (RayCastForEnemy(rayStart, rayEnd))
                return true;

            return false;
        }

        // Movement:
        private float getSpeedFor(float speed, float speedPercent)
        {
            return speedPercent * speed;
        }

        public void RunLeft(float speedPercent)
        {
            moveOnGround();
            motor.MotorSpeed = -getSpeedFor(moveSpeed, speedPercent);
        }

        public void RunRight(float speedPercent)
        {
            moveOnGround();
            motor.MotorSpeed = getSpeedFor(moveSpeed, speedPercent);
        }

        public void StopRunning()
        {
            // don't brake yet as we need to stop rolling first
            motor.MotorSpeed = 0;
        }

        public void Dash()
        {
            //moveSpeed = dashMultiplier * runSpeed;
            airControlSpeed = dashMultiplier * runAirControlSpeed;

            // so the player's isn't running while dashing
            StopRunning();
        }

        public void DashLeft()
        {
            body.LinearVelocity = new Vector2(-airControlSpeed - dashFrictionOffsetVelX, body.LinearVelocity.Y);
        }

        public void DashRight()
        {
            body.LinearVelocity = new Vector2(airControlSpeed + dashFrictionOffsetVelX, body.LinearVelocity.Y);
        }

        public void StopDashing()
        {
            //moveSpeed = runSpeed;
            airControlSpeed = runAirControlSpeed;

            // so that the player stops short after a dash
            body.LinearVelocity = new Vector2(0, body.LinearVelocity.Y);
        }

        public void FloatLeft(float speedPercent, float dt)
        {
            moveInAir();
            if (!OnLeftWall)
            {
                airControlForce.X = body.LinearVelocity.X - getSpeedFor(airControlAbility, speedPercent) * dt;
                airControlForce.Y = body.LinearVelocity.Y;

                if (airControlForce.X < -airControlSpeed)
                {
                    airControlForce.X = -airControlSpeed;
                }

                body.LinearVelocity = airControlForce;
            }
        }

        public void FloatRight(float speedPercent, float dt)
        {
            moveInAir();
            if (!OnRightWall)
            {
                airControlForce.X = body.LinearVelocity.X + getSpeedFor(airControlAbility, speedPercent) * dt;
                airControlForce.Y = body.LinearVelocity.Y;

                if (airControlForce.X > airControlSpeed)
                {
                    airControlForce.X = airControlSpeed;
                }

                body.LinearVelocity = airControlForce;
            }
        }

        public void StopFloating(float dt)
        {
            moveInAir();

            // physics won't naturally handle it the way we want
            body.LinearVelocity = new Vector2(body.LinearVelocity.X - body.LinearVelocity.X / 0.6f * dt, body.LinearVelocity.Y);
        }

        public void Jump(float dt)
        {
            enableOnGroundDelay = false;

            moveInAir();
            jumpForce.Y = jumpImpulse;
            body.LinearVelocity = new Vector2(body.LinearVelocity.X, jumpForce.Y);
        }

        public void WallJump()
        {
            if (!OnGround)
            {
                enableOnLeftWallDelay = false;
                enableOnRightWallDelay = false;

                // to correct for consecutive wall jumping
                body.LinearVelocity = new Vector2(0, body.LinearVelocity.Y);

                if (OnRightWall)
                {
                    Vector2 impulse = new Vector2(-wallJumpImpulse, 0);
                    body.ApplyLinearImpulse(impulse);
                }
                else if (OnLeftWall)
                {
                    Vector2 impulse = new Vector2(wallJumpImpulse, 0);
                    body.ApplyLinearImpulse(impulse);
                }
            }
        }

        public void StopJumping()
        {
            // again, physics will naturally handle it
        }

        public void Fall()
        {
            // physics will naturally handle it
        }

        protected override bool collidedWithGroundFixture(Fixture fix2)
        {
            if (fix2.CollisionCategories.HasFlag(GameConstants.PlayerCollisionCategory))
            {
                return true;
            }

            return base.collidedWithGroundFixture(fix2);
        }
    }
}
