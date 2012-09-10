/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Common;
using EasyConfig;

namespace STFU
{
    /// <summary>
    /// A player can be in only one of these states at a time.
    /// </summary>
    public enum State
    {
        Jumping,
        WallJumping,
        Falling,
        Running,
        Idle,
        None
    }

    /// <summary>
    /// A player is what is controlled by a user when playing the game.
    /// </summary>
    class Player : LivingEntity
    {
        public PlayerIndex PlayerIndex { get; set; }
        private PlayerEvent playerEvent;

        public PhysicsContainer<PlayerPhysicsCharacter> PhysicsContainer { get; protected set; }

        public PlayerWeapon Gun { get; private set; }
        public PlayerWeapon Sword { get; private set; }
        public State State { get; private set; }

        private bool landed;
        private int onDeath;

        // abilities (resources)
        private bool wallJumpEnabled;
        private bool wallSlideEnabled;
        private bool shootEnabled;
        private bool canJump;
        private bool canWallJump;
        private float jumpTimer;
        private float jumpStateBufferTimer;

        // Resources
        private float screenWidth;
        private float screenHeight;
        private float jumpDelay;
        private float jumpStateBufferDelay;
        private float hitDelay;
        private float recoveryDelay;
        private float respawnDelay;
        private float swordDistance;
        private float addedSwordDistanceWhenRunning;
        private float addedSwordDistanceWhenDashing;

        public const string SettingsIni = "Settings/PlayerSettings.ini";
        private const string initSettings = "Init";

        public bool Dashing { get; private set; }
        private bool canDash;
        private float dashTimer;
        private float dashDelay;
        private Direction lastDashDirection;

        // Constructor
        public Player(World world, PlayerIndex playerIndex, PlayerEvent playerEvent)
            : base(world)
        {
            // Load resources
            ConfigFile configFile = new ConfigFile(SettingsIni);
            screenWidth = configFile.SettingGroups[initSettings].Settings["screenWidth"].GetValueAsFloat();
            screenHeight = configFile.SettingGroups[initSettings].Settings["screenHeight"].GetValueAsFloat();
            jumpDelay = configFile.SettingGroups[initSettings].Settings["jumpDelay"].GetValueAsFloat();
            jumpStateBufferDelay = configFile.SettingGroups[initSettings].Settings["jumpStateBufferDelay"].GetValueAsFloat();
            hitDelay = configFile.SettingGroups[initSettings].Settings["hitDelay"].GetValueAsFloat();
            recoveryDelay = configFile.SettingGroups[initSettings].Settings["recoveryDelay"].GetValueAsFloat();
            respawnDelay = configFile.SettingGroups[initSettings].Settings["respawnDelay"].GetValueAsFloat();
            swordDistance = configFile.SettingGroups[initSettings].Settings["swordDistance"].GetValueAsFloat();
            addedSwordDistanceWhenRunning = configFile.SettingGroups[initSettings].Settings["addedSwordDistanceWhenRunning"].GetValueAsFloat();
            addedSwordDistanceWhenDashing = configFile.SettingGroups[initSettings].Settings["addedSwordDistanceWhenDashing"].GetValueAsFloat();
            dashDelay = configFile.SettingGroups[initSettings].Settings["dashDelay"].GetValueAsFloat();

            this.PlayerIndex = playerIndex;
            this.playerEvent = playerEvent;
            this.PhysicsContainer = new PhysicsContainer<PlayerPhysicsCharacter>();
            wallJumpEnabled = false;
            wallSlideEnabled = false;
            shootEnabled = false;
            canJump = false;
            canWallJump = false;

            // Accessories:
            this.Gun = null;
            this.Gun = new PlayerWeapon(world, this);
            this.Gun.SetUpWeapon(2, 0.3f, 20, 5, 200f, 1f, true);
            this.Sword = null;
            this.Sword = new PlayerWeapon(world, this);
            this.Sword.SetUpWeapon(1, 0.8f, 10, 36, swordDistance, 3f, false);
            this.Sword.RotateWhenShooting(40f, 0, 180);
        }

        public void SetUpPlayer(Vector2 playerStartPosition, int health)
        {
            setUpLivingEntity(playerStartPosition, screenWidth, screenHeight, health, hitDelay, recoveryDelay, respawnDelay);
        }

        public override void Spawn()
        {
            spawnLivingEntity();

            Idle();

            // Physics:
            this.PhysicsContainer.Object = null;
            this.PhysicsContainer.Object = new PlayerPhysicsCharacter(this, world, this.SpawnPosition, this.ScreenWidth, this.ScreenHeight, 1f,
                new OnCollision(onCollision), new OnSeparation(onSeparation));

            setWallSlideForPhysics();

            Category collisionCategory = GameConstants.PlayerCollisionCategory;
            if (this.PlayerIndex == PlayerIndex.One)
            {
                collisionCategory |= GameConstants.Player1CollisionCategory;
            }
            else if (this.PlayerIndex == PlayerIndex.Two)
            {
                collisionCategory |= GameConstants.Player2CollisionCategory;
            }
            else if (this.PlayerIndex == PlayerIndex.Three)
            {
                collisionCategory |= GameConstants.Player3CollisionCategory;
            }
            else if (this.PlayerIndex == PlayerIndex.Four)
            {
                collisionCategory |= GameConstants.Player4CollisionCategory;
            }
            this.PhysicsContainer.Object.CollisionCategory = collisionCategory;

            this.FacingRight = true;
            jumpTimer = 0f;
            jumpStateBufferTimer = 0f;
            onDeath = 0;

            this.Dashing = false;
            canDash = false;
            dashTimer = 0f;
        }

        protected override void updateWhenAlive(float dt)
        {
            // this bit is to do that Megaman thing where you can walk on spikes when recovering from being hit
            if (!this.Health.CheckInvulnerable())
            {
                this.PhysicsContainer.Object.RayCastIncludesDeath = false;

                if (onDeath > 0)
                {
                    Die();
                    return;
                }
            }
            else
            {
                this.PhysicsContainer.Object.RayCastIncludesDeath = true;
            }

            // Timers:
            if (jumpTimer < 0)
            {
                jumpTimer += dt;
                if (jumpTimer >= 0)
                {
                    StopJumping();
                }
            }

            if (jumpStateBufferTimer < 0)
            {
                jumpStateBufferTimer += dt;
            }

            if (dashTimer < 0)
            {
                dashTimer += dt;
            }
            else
            {
                StopDashing();
            }

            // Update the physics character
            this.PhysicsContainer.Object.Update(dt);

            // As an extra precaution, make sure we have a positive velocity
            if (!this.PhysicsContainer.Object.OnGround && this.PhysicsContainer.Object.body.LinearVelocity.Y > 0.1f)
            {
                Fall();
            }

            if (this.PhysicsContainer.Object.OnGround)
            {
                if (this.State == State.Jumping)
                {
                    None();
                }
                else if (this.PhysicsContainer.Object.brakeJoint.Enabled == true)
                {
                    Idle();
                }

                if (!landed)
                {
                    onLanding();
                }

                canJump = true;
                canWallJump = false;
                canDash = true;
            }
            else 
            {
                landed = false;

                if (wallJumpEnabled)
                {
                    if (this.PhysicsContainer.Object.OnWall())
                    {
                        canWallJump = true;
                    }
                    else
                    {
                        canWallJump = false;
                    }
                }

                // if we hit a wall while dashing in the air, we stop dashing
                if (this.Dashing)
                {
                    if (this.PhysicsContainer.Object.OnWall())
                        StopDashing();
                }
                // can't start a dash unless we're on the ground
                canDash = false;

                // check which way we should be facing while in the air
                // actually, this implements an interesting gameplay element when disabled
                /*
                if (!this.Gun.Active)
                {
                    if (this.PhysicsContainer.Object.body.LinearVelocity.X > changeDirectionVelocity)
                    {
                        this.FacingRight = true;
                    }
                    else if (this.PhysicsContainer.Object.body.LinearVelocity.X < -changeDirectionVelocity)
                    {
                        this.FacingRight = false;
                    }
                }
                 */

                if (this.PhysicsContainer.Object.wallSlideEnabled)
                {
                    if (this.PhysicsContainer.Object.OnRightWall && this.State == State.Falling)
                    {
                        this.FacingRight = false;
                    }
                    else if (this.PhysicsContainer.Object.OnLeftWall && this.State == State.Falling)
                    {
                        this.FacingRight = true;
                    }
                }
            }

            // Update the gun
            this.Gun.Update(dt);
            updateWeaponPosition(this.Gun, Vector2.Zero);

            // Update the sword
            this.Sword.Update(dt);
            Vector2 swordOffset = new Vector2(0, ConvertUnits.ToSimUnits(-6));
            updateWeaponPosition(this.Sword, swordOffset);
        }

        protected override void updateWhenDead(float dt)
        {
            // Update the accessories
            this.Gun.Update(dt);
            this.Sword.Update(dt);
        }

        public override void Die()
        {
            base.Die();
            this.PhysicsContainer.Object.Disable();
        }

        public override void Respawn()
        {
            base.Respawn();
            this.PhysicsContainer.Object.Dispose();
            Spawn();
        }

        private void updateWeaponPosition(PlayerWeapon weapon, Vector2 offset)
        {
            Vector2 wepPos = Position;

            if (this.FacingRight)
            {
                wepPos.X += offset.X;
            }
            else
            {
                wepPos.X -= offset.X;
            }
            wepPos.Y += offset.Y;

            weapon.Position = wepPos;
        }

        public override Vector2 Position
        {
            get
            {
                // we actually need to do this since there's an offset with the player's physics body
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

        private void gotHitByEnemy()
        {
            // No knock-back but no collisions with enemies for a short recovery period
            this.Health.GotHit(1);
        }

        protected bool onCollision(Fixture fix1, Fixture fix2, Contact contact)
        {
            if (fix2.CollisionCategories.HasFlag(GameConstants.DeathCollisionCategory))
            {
                onDeath++; // used in update
                if (!this.Health.CheckInvulnerable())
                {
                    Die();
                    return false;
                }
            }
            else if (fix2.CollisionCategories.HasFlag(GameConstants.EnemyCollisionCategory))
            {
                if (this.Health.CheckInvulnerable())
                {
                    return false;
                }

                gotHitByEnemy();

                return false;
            }

            if (fix2.CollisionCategories.HasFlag(GameConstants.PlatformCollisionCategory))
            {
                if (fix1.Body.LinearVelocity.Y < 0)
                {
                    return false;
                }
            }

            return true;
        }

        private void onSeparation(Fixture fix1, Fixture fix2)
        {
            if (fix2.CollisionCategories.HasFlag(GameConstants.DeathCollisionCategory))
            {
                onDeath--; // used in update
            }
        }

        public void onLanding()
        {
            landed = true;
            this.Gun.ChangeRotationFromDownToForward();
        }

        // States:
        private bool changeState(State newState)
        {
            if (jumpStateBufferTimer >= 0 && newState != this.State)
            {
                this.State = newState;
                playerEvent.changeState = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        protected void None()
        {
            if (changeState(State.None))
            {
                Console.WriteLine("none");
            }
        }

        protected void Jumping()
        {
            if (changeState(State.Jumping))
            {
                Console.Out.WriteLine("jumping");
                jumpTimer = -jumpDelay;
                jumpStateBufferTimer = -jumpStateBufferDelay;
            }
        }

        protected void WallJumping()
        {
            if (changeState(State.WallJumping))
            {
                Console.Out.WriteLine("wall jumping");
            }
        }

        protected void Falling()
        {
            if (changeState(State.Falling))
            {
                Console.Out.WriteLine("falling");
                canJump = false;
            }
        }

        protected void Running()
        {
            if (changeState(State.Running))
            {
                //Console.Out.WriteLine("running");
            }
        }

        protected void Idle()
        {
            if (changeState(State.Idle))
            {
                Console.Out.WriteLine("idle");
            }
        }

        // Activities:
        public void MoveLeft(float speedPercent, float dt)
        {
            if (this.PhysicsContainer.Object.OnGround && this.State != State.Jumping)
            {
                if (!this.Dashing)
                {
                    this.FacingRight = false;
                    Running();
                    this.PhysicsContainer.Object.RunLeft(speedPercent);
                }
            }
            else
            {
                this.PhysicsContainer.Object.FloatLeft(speedPercent, dt);
            }
        }

        public void MoveRight(float speedPercent, float dt)
        {
            if (this.PhysicsContainer.Object.OnGround && this.State != State.Jumping)
            {
                if (!this.Dashing)
                {
                    this.FacingRight = true;
                    Running();
                    this.PhysicsContainer.Object.RunRight(speedPercent);
                }
            }
            else
            {
                this.PhysicsContainer.Object.FloatRight(speedPercent, dt);
            }
        }

        public void StopMoving(float dt)
        {
            if (this.PhysicsContainer.Object.OnGround && this.State != State.Jumping)
            {
                if (!this.Dashing)
                {
                    this.PhysicsContainer.Object.StopRunning();
                }
            }
            else
            {
                this.PhysicsContainer.Object.StopFloating(dt);
            }
        }

        public void Jump(float dt)
        {
            if (canJump)
            {
                Jumping();
                this.PhysicsContainer.Object.Jump(dt);
            }
        }

        public bool StartJumping(float dt)
        {
            if (canWallJump)
            {
                canWallJump = false;
                WallJumping();
                this.PhysicsContainer.Object.WallJump();

                // make sure the player is facing away from the wall when he wall jumps
                if (this.PhysicsContainer.Object.OnRightWall)
                {
                    this.FacingRight = false;
                }
                else if (this.PhysicsContainer.Object.OnLeftWall)
                {
                    this.FacingRight = true;
                }

                canJump = true;
            }

            Jump(dt);

            if (this.State == State.Jumping)
                return true;
            else
                return false;
        }

        public void KeepJumping(float dt)
        {
            if (this.State == State.Jumping)
            {
                Jump(dt);
            }
        }

        public void StopJumping()
        {
            // so that the player can't jump again [in the air] after shooting through a platform
            if (!this.PhysicsContainer.Object.OnGround)
            {
                canJump = false;
                if (this.State == State.Jumping)
                {
                    this.PhysicsContainer.Object.StopJumping();
                }
            }
        }

        public void Fall()
        {
            Falling();
            this.PhysicsContainer.Object.Fall();
        }

        public void StartDashing(Direction direction)
        {
            if (!this.Dashing && canDash)
            {
                this.Dashing = true;
                this.PhysicsContainer.Object.Dash();
                lastDashDirection = direction;
                Dash(lastDashDirection);
                dashTimer = -dashDelay;
            }
        }

        public void KeepDashing(Direction direction)
        {
            if (this.Dashing)
            {
                if (direction != Direction.None)
                    lastDashDirection = direction;
                Dash(lastDashDirection);
            }
        }

        public void Dash(Direction direction)
        {
            if (this.PhysicsContainer.Object.OnGround)
            {
                Idle();

                if (direction == Direction.Right)
                {
                    this.PhysicsContainer.Object.DashRight();
                }
                else if (direction == Direction.Left)
                {
                    this.PhysicsContainer.Object.DashLeft();
                }
                else
                {
                    if (this.FacingRight)
                    {
                        this.PhysicsContainer.Object.DashRight();
                    }
                    else
                    {
                        this.PhysicsContainer.Object.DashLeft();
                    }
                }
            }
        }

        public void StopDashing()
        {
            if (this.Dashing)
            {
                // you can't stop dashing while in the air
                if (this.PhysicsContainer.Object.OnGround || this.PhysicsContainer.Object.OnWall())
                {
                    this.Dashing = false;
                    this.PhysicsContainer.Object.StopDashing();
                }
                else
                {
                    // stop the dash once landed by ending the timer
                    dashTimer = 0f;
                }
            }
        }

        public void ShootSword(Direction direction)
        {
            if (checkShootingWeapons())
            {
                // don't do anything since we're already shooting
            }

            else if (this.Sword.CanShoot() && shootEnabled)
            {
                shootWeapon(this.Sword);

                float velocityX = Math.Abs(this.PhysicsContainer.Object.body.LinearVelocity.X);
                if (velocityX > 1)
                {
                    if (this.Dashing)
                        this.Sword.SetBulletDistance(swordDistance + addedSwordDistanceWhenDashing);
                    else
                        this.Sword.SetBulletDistance(swordDistance + addedSwordDistanceWhenRunning);
                }
                else
                {
                    this.Sword.SetBulletDistance(swordDistance);
                }

                if (direction == Direction.Right && !this.PhysicsContainer.Object.OnWall())
                {
                    this.FacingRight = true;
                    this.Sword.ShootRight();
                }
                else if (direction == Direction.Left && !this.PhysicsContainer.Object.OnWall())
                {
                    this.FacingRight = false;
                    this.Sword.ShootLeft();
                }
                else
                {
                    shootWeaponWherePlayerFacing(this.Sword);
                }
            }
        }

        public void DontShootSword()
        {
            this.Sword.DontShoot();
        }

        public void ShootGun(Direction direction)
        {
            if (checkShootingWeapons())
            {
                // don't do anything since we're already shooting
            }

            else if (this.Gun.CanShoot() && shootEnabled)
            {
                shootWeapon(this.Gun);

                if (direction == Direction.Right && !this.PhysicsContainer.Object.OnWall())
                {
                    this.FacingRight = true;
                    this.Gun.ShootRight();
                }
                else if (direction == Direction.Left && !this.PhysicsContainer.Object.OnWall())
                {
                    this.FacingRight = false;
                    this.Gun.ShootLeft();
                }
                else if (direction == Direction.Up)
                {
                    this.Gun.ShootUpAndRotate();
                }
                else if (direction == Direction.Down)
                {
                    if (!this.PhysicsContainer.Object.OnGround)
                        this.Gun.ShootDownAndRotate();
                    else
                        shootWeaponWherePlayerFacing(this.Gun);
                }
                else
                {
                    shootWeaponWherePlayerFacing(this.Gun);
                }
            }
        }

        public void DontShootGun()
        {
            this.Gun.DontShoot();
        }

        private void shootWeapon(PlayerWeapon weapon)
        {
            this.playerEvent.shootWeapon = true;

            // check whether this weapon should be active
            bool weaponActiveState = false;
            if (weapon.Active)
                weaponActiveState = true;

            // deactivate all other weapons
            deactivateWeapons();

            //.. except this weapon
            if (weaponActiveState)
                weapon.Active = true;
        }

        private void shootWeaponWherePlayerFacing(PlayerWeapon weapon)
        {
            if (this.FacingRight)
            {
                weapon.ShootRight();
            }
            else
            {
                weapon.ShootLeft();
            }
        }

        private bool checkShootingWeapons()
        {
            if (this.Gun.Shooting || this.Sword.Shooting)
            {
                return true;
            }

            return false;
        }

        private void deactivateWeapons()
        {
            this.Gun.Active = false;
            this.Sword.Active = false;
        }

        public bool WeaponActive()
        {
            if (this.Gun.Active)
                return true;
            if (this.Sword.Active)
                return true;

            return false;
        }

        public bool WeaponActiveAndVertical()
        {
            if (this.Gun.Active && this.Gun.Vertical)
                return true;
            if (this.Sword.Active && this.Sword.Vertical)
                return true;

            return false;
        }

        // Abilities:
        public void enableWallJumping()
        {
            wallJumpEnabled = true;
        }

        public void disableWallJumping()
        {
            wallJumpEnabled = false;
        }

        public void enableWallSliding()
        {
            wallSlideEnabled = true;
            setWallSlideForPhysics();
        }

        public void disableWallSliding()
        {
            wallSlideEnabled = false;
            setWallSlideForPhysics();
        }

        public void setWallSlideForPhysics()
        {
            if (this.PhysicsContainer.Object != null)
            {
                if (wallSlideEnabled)
                {
                    this.PhysicsContainer.Object.wallSlideEnabled = true;
                }
                else
                {
                    this.PhysicsContainer.Object.wallSlideEnabled = false;
                }
            }
        }

        public void enableShooting()
        {
            shootEnabled = true;
        }

        public void disableShooting()
        {
            shootEnabled = false;
        }
        
        public override Vector2 ScreenPosition
        {
            get
            {
                Vector2 screenPosition = base.ScreenPosition;
                screenPosition.X += 0.5f; // fixes a bug to do with displaying the character
                return screenPosition;
            }
        }

        public void Dispose()
        {
            this.PhysicsContainer.Object.Dispose();
            this.Gun.Dispose();
            this.Sword.Dispose();
        }
    }
}
