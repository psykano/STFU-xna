/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace STFU
{
    /// <summary>
    /// The four-way directions for controlling the player.
    /// The analog stick on the controller is reduced to these four directions yet maintains some
    /// degrees of freedom. Keyboard controls for direction are digital, on the other hand.
    /// </summary>
    public enum Direction
    {
        Left,
        Right,
        Up,
        Down,
        None
    }

    /// <summary>
    /// A player controller takes a user's input and sends them in the form of commands to a player.
    /// </summary>
    class PlayerController : Controller
    {
        public PlayerIndex PlayerIndex
        {
            get
            {
                return player.PlayerIndex;
            }
        }

        protected KeyboardState keyState;
        protected GamePadState gamePadState;
        protected Player player;

        // keyboard buttons
        protected Keys leftKey;
        protected Keys rightKey;
        protected Keys upKey;
        protected Keys downKey;
        protected Keys jumpKey;
        protected Keys shootKey;
        protected Keys meleeKey;
        protected Keys dashKey;

        private bool jumpButtonEnabled;
        private bool dashButtonEnabled;

        // for gamepads
        private const float xThumbStickThreshold = 0.1f;
        private const float yThumbStickThreshold = 0.15f;

        // Constructor
        public PlayerController(Player player)
        {
            this.player = player;

            if (player.PlayerIndex == PlayerIndex.One)
            {
                leftKey = Keys.Left;
                rightKey = Keys.Right;
                upKey = Keys.Up;
                downKey = Keys.Down;
                jumpKey = Keys.N;
                shootKey = Keys.M;
                meleeKey = Keys.OemComma;
                dashKey = Keys.B;
            }
            else if (player.PlayerIndex == PlayerIndex.Two)
            {
                leftKey = Keys.A;
                rightKey = Keys.D;
                upKey = Keys.W;
                downKey = Keys.S;
                jumpKey = Keys.Tab;
                shootKey = Keys.Q;
                meleeKey = Keys.D1;
                dashKey = Keys.D2;
            }
            else if (player.PlayerIndex == PlayerIndex.Three)
            {
                // not set up yet
            }
            else if (player.PlayerIndex == PlayerIndex.Four)
            {
                // same as above
            }
        }

        public override void Update(float dt)
        {
            if (player.Controllable())
            {
                HandleInput(dt);
            }
        }

        protected void HandleInput(float dt)
        {
            keyState = Keyboard.GetState();
            gamePadState = GamePad.GetState(player.PlayerIndex);

            if (gamePadState.IsConnected)
            {
                if (gamePadState.ThumbSticks.Left.Y > yThumbStickThreshold)
                {
                    // shoot
                    HandleShoot(gamePadState.IsButtonDown(Buttons.X), Direction.Up);

                    // melee
                    HandleMelee(gamePadState.IsButtonDown(Buttons.B), Direction.Up);
                }
                else if (gamePadState.ThumbSticks.Left.Y < -yThumbStickThreshold)
                {
                    // shoot
                    HandleShoot(gamePadState.IsButtonDown(Buttons.X), Direction.Down);

                    // melee
                    HandleMelee(gamePadState.IsButtonDown(Buttons.B), Direction.Down);
                }

                if (gamePadState.ThumbSticks.Left.X < -xThumbStickThreshold)
                {
                    // move left
                    player.MoveLeft(-gamePadState.ThumbSticks.Left.X, dt);

                    // shoot left
                    HandleShoot(gamePadState.IsButtonDown(Buttons.X), Direction.Left);

                    // melee left
                    HandleMelee(gamePadState.IsButtonDown(Buttons.B), Direction.Left);

                    // dash left
                    HandleDash(gamePadState.IsButtonDown(Buttons.RightShoulder), Direction.Left);
                }
                else if (gamePadState.ThumbSticks.Left.X > xThumbStickThreshold)
                {
                    // move right
                    player.MoveRight(gamePadState.ThumbSticks.Left.X, dt);

                    // shoot right
                    HandleShoot(gamePadState.IsButtonDown(Buttons.X), Direction.Right);

                    // melee right
                    HandleMelee(gamePadState.IsButtonDown(Buttons.B), Direction.Right);

                    // dash right
                    HandleDash(gamePadState.IsButtonDown(Buttons.RightShoulder), Direction.Right);
                }
                else
                {
                    // stop
                    player.StopMoving(dt);

                    // shoot
                    HandleShoot(gamePadState.IsButtonDown(Buttons.X), Direction.None);

                    // melee
                    HandleMelee(gamePadState.IsButtonDown(Buttons.B), Direction.None);

                    // dash
                    HandleDash(gamePadState.IsButtonDown(Buttons.RightShoulder), Direction.None);
                }

                HandleJump(gamePadState.IsButtonDown(Buttons.A), dt);
            }
            else
            {
                if (keyState.IsKeyDown(upKey))
                {
                    // shoot
                    HandleShoot(keyState.IsKeyDown(shootKey), Direction.Up);

                    // melee
                    HandleMelee(keyState.IsKeyDown(meleeKey), Direction.Up);
                }
                else if (keyState.IsKeyDown(downKey))
                {
                    // shoot
                    HandleShoot(keyState.IsKeyDown(shootKey), Direction.Down);

                    // melee
                    HandleMelee(keyState.IsKeyDown(meleeKey), Direction.Down);
                }

                if (keyState.IsKeyDown(leftKey))
                {
                    // move left
                    player.MoveLeft(1f, dt);

                    // shoot left
                    HandleShoot(keyState.IsKeyDown(shootKey), Direction.Left);

                    // melee left
                    HandleMelee(keyState.IsKeyDown(meleeKey), Direction.Left);

                    // dash left
                    HandleDash(keyState.IsKeyDown(dashKey), Direction.Left);
                }
                else if (keyState.IsKeyDown(rightKey))
                {
                    // move right
                    player.MoveRight(1f, dt);

                    // shoot right
                    HandleShoot(keyState.IsKeyDown(shootKey), Direction.Right);

                    // melee right
                    HandleMelee(keyState.IsKeyDown(meleeKey), Direction.Right);

                    // dash right
                    HandleDash(keyState.IsKeyDown(dashKey), Direction.Right);
                }
                else
                {
                    // stop
                    player.StopMoving(dt);

                    // shoot
                    HandleShoot(keyState.IsKeyDown(shootKey), Direction.None);

                    // melee
                    HandleMelee(keyState.IsKeyDown(meleeKey), Direction.None);

                    // dash
                    HandleDash(keyState.IsKeyDown(dashKey), Direction.None);
                }

                HandleJump(keyState.IsKeyDown(jumpKey), dt);
            }

        }

        protected void HandleJump(bool isKeyDown, float dt)
        {
            if (isKeyDown)
            {
                if (jumpButtonEnabled)
                {
                    // start jumping
                    if (player.StartJumping(dt))
                    {
                        jumpButtonEnabled = false;
                    }
                }
                else
                {
                    // keep jumping
                    player.KeepJumping(dt);
                }
            }
            else
            {
                // stop jumping
                player.StopJumping();
                jumpButtonEnabled = true;
            }
        }

        protected void HandleDash(bool isKeyDown, Direction direction)
        {
            if (isKeyDown)
            {
                //Console.WriteLine("DASH KEY");
                //player.Dash();

                if (dashButtonEnabled)
                {
                    // start dashing
                    player.StartDashing(direction);
                    dashButtonEnabled = false;
                }
                else
                {
                    // keep dashing
                    player.KeepDashing(direction);
                }
            }
            else
            {
                // stop dashing
                player.StopDashing();
                dashButtonEnabled = true;
            }
        }

        protected void HandleShoot(bool isKeyDown, Direction direction)
        {
            if (isKeyDown)
            {
                player.ShootGun(direction);
            }
            else
            {
                player.DontShootGun();
            }
        }

        protected void HandleMelee(bool isKeyDown, Direction direction)
        {
            if (isKeyDown)
            {
                player.ShootSword(direction);
            }
            else
            {
                player.DontShootSword();
            }
        }
    }
}
