/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using EasyConfig;

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

        // gamepad buttons
        protected Buttons jumpButton;
        protected Buttons shootButton;
        protected Buttons meleeButton;
        protected Buttons dashButton;

        private bool jumpButtonEnabled;
        private bool dashButtonEnabled;

        // for gamepads
        private const float xThumbStickThreshold = 0.1f;
        private const float yThumbStickThreshold = 0.15f;

        // settings 
        private const string inputSettingsIni = "Settings/InputSettings.ini";
        private const string playerOneInput = "PlayerOne";
        private const string playerTwoInput = "PlayerTwo";
        private const string playerThreeInput = "PlayerThree";
        private const string playerFourInput = "PlayerFour";

        // Constructor
        public PlayerController(Player player)
        {
            this.player = player;

            ConfigFile configFile = new ConfigFile(inputSettingsIni);
            string playerInput = "";

            if (player.PlayerIndex == PlayerIndex.One)
            {
                playerInput = playerOneInput;
            }
            else if (player.PlayerIndex == PlayerIndex.Two)
            {
                playerInput = playerTwoInput;
            }
            else if (player.PlayerIndex == PlayerIndex.Three)
            {
                playerInput = playerThreeInput;
            }
            else if (player.PlayerIndex == PlayerIndex.Four)
            {
                playerInput = playerFourInput;
            }

            // keyboard
            leftKey = parsePlayerInput<Keys>(configFile, playerInput, "leftKey");
            rightKey = parsePlayerInput<Keys>(configFile, playerInput, "rightKey");
            upKey = parsePlayerInput<Keys>(configFile, playerInput, "upKey");
            downKey = parsePlayerInput<Keys>(configFile, playerInput, "downKey");
            jumpKey = parsePlayerInput<Keys>(configFile, playerInput, "jumpKey");
            shootKey = parsePlayerInput<Keys>(configFile, playerInput, "shootKey");
            meleeKey = parsePlayerInput<Keys>(configFile, playerInput, "meleeKey");
            dashKey = parsePlayerInput<Keys>(configFile, playerInput, "dashKey");

            // gamepad
            jumpButton = parsePlayerInput<Buttons>(configFile, playerInput, "jumpButton");
            shootButton = parsePlayerInput<Buttons>(configFile, playerInput, "shootButton");
            meleeButton = parsePlayerInput<Buttons>(configFile, playerInput, "meleeButton");
            dashButton = parsePlayerInput<Buttons>(configFile, playerInput, "dashButton");
        }

        private T parsePlayerInput<T>(ConfigFile configFile, string playerInput, string key) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            T value;

            try
            {
                int intValue = configFile.SettingGroups[playerInput].Settings[key].GetValueAsInt();
                value = (T)Convert.ChangeType(intValue, Type.GetTypeCode(typeof(T)));
                return value;
            }
            catch
            {
            }

            try
            {
                value = (T)Enum.Parse(typeof(T), configFile.SettingGroups[playerInput].Settings[key].GetValueAsString(), true);
                return value;
            }
            catch
            {
            }

            // if all else fails
            value = (T)Convert.ChangeType(0, Type.GetTypeCode(typeof(T)));
            return value;
        }

        public override void Update(float dt)
        {
            if (player.Controllable())
            {
                HandleInput(dt);
            }
            else
            {
                // stop
                player.StopMoving(dt);
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
                    HandleShoot(gamePadState.IsButtonDown(shootButton), Direction.Up);

                    // melee
                    HandleMelee(gamePadState.IsButtonDown(meleeButton), Direction.Up);
                }
                else if (gamePadState.ThumbSticks.Left.Y < -yThumbStickThreshold)
                {
                    // shoot
                    HandleShoot(gamePadState.IsButtonDown(shootButton), Direction.Down);

                    // melee
                    HandleMelee(gamePadState.IsButtonDown(meleeButton), Direction.Down);
                }

                if (gamePadState.ThumbSticks.Left.X < -xThumbStickThreshold)
                {
                    // move left
                    player.MoveLeft(-gamePadState.ThumbSticks.Left.X, dt);

                    // shoot left
                    HandleShoot(gamePadState.IsButtonDown(shootButton), Direction.Left);

                    // melee left
                    HandleMelee(gamePadState.IsButtonDown(meleeButton), Direction.Left);

                    // dash left
                    HandleDash(gamePadState.IsButtonDown(dashButton), Direction.Left);
                }
                else if (gamePadState.ThumbSticks.Left.X > xThumbStickThreshold)
                {
                    // move right
                    player.MoveRight(gamePadState.ThumbSticks.Left.X, dt);

                    // shoot right
                    HandleShoot(gamePadState.IsButtonDown(shootButton), Direction.Right);

                    // melee right
                    HandleMelee(gamePadState.IsButtonDown(meleeButton), Direction.Right);

                    // dash right
                    HandleDash(gamePadState.IsButtonDown(dashButton), Direction.Right);
                }
                else
                {
                    // stop
                    player.StopMoving(dt);

                    // shoot
                    HandleShoot(gamePadState.IsButtonDown(shootButton), Direction.None);

                    // melee
                    HandleMelee(gamePadState.IsButtonDown(meleeButton), Direction.None);

                    // dash
                    HandleDash(gamePadState.IsButtonDown(dashButton), Direction.None);
                }

                HandleJump(gamePadState.IsButtonDown(jumpButton), dt);
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
