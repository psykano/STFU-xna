/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;
using FarseerPhysics.DebugViews;
using MyDataTypes;
using EasyConfig;

namespace STFU
{
    /// <summary>
    /// This is basically a quick (and dirty) way to actually play the game while all the different components are still being created.
    /// Eventually it will be replaced with a state management system when enough is made such that you can win the game.
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        PhysicsSystem physicsSystem;
        
        Map map;
        MapRepresentation mapRepresentation;

        public Viewport defaultViewport;
        Viewport topLeftViewport;
        Viewport topRightViewport;
        Viewport bottomLeftViewport;
        Viewport bottomRightViewport;

        DebugViewXNA DebugView;

        EntityManager entityManager;
        RepresentationManager representationManager;
        ControllerManager controllerManager;

        FixedTimeStepSystem playersFixedTimeStep;
        FixedTimeStepSystem fixedTimeStep;

        Texture2D tmpTex;

        int numPlayers;

        private float gameSpeed;
        private int defaultResolutionHeight;
        private bool enableDebugging;
        public bool ShowDebugView { get; set; }

        private const string gameSettingsIni = "Settings/GameSettings.ini";
        private const string videoSettings = "Video";
        private const string generalSettings = "General";
        private const string engineSettings = "Engine";

        private const float fixedTimestep = 1/30f;
        private const int maxSteps = 1;

        public Game1()
        {
            ConfigFile configFile = new ConfigFile(gameSettingsIni);

            // engine settings
            gameSpeed = configFile.SettingGroups[engineSettings].Settings["gameSpeed"].GetValueAsFloat();
            defaultResolutionHeight = configFile.SettingGroups[engineSettings].Settings["defaultResolutionHeight"].GetValueAsInt();
            enableDebugging = configFile.SettingGroups[engineSettings].Settings["enableDebugging"].GetValueAsBool();
            this.ShowDebugView = false;

            // video settings
            graphics = new GraphicsDeviceManager(this);
            graphics.IsFullScreen = configFile.SettingGroups[videoSettings].Settings["Fullscreen"].GetValueAsBool();
            if (graphics.IsFullScreen)
            {
                graphics.PreferredBackBufferWidth = configFile.SettingGroups[videoSettings].Settings["Width"].GetValueAsInt();
                graphics.PreferredBackBufferHeight = configFile.SettingGroups[videoSettings].Settings["Height"].GetValueAsInt();
            }
            else
            {
                // default resolution and windowed resolution
                graphics.PreferredBackBufferWidth = 1280;
                graphics.PreferredBackBufferHeight = 720;
            }
            SetVsync(configFile.SettingGroups[videoSettings].Settings["Vsync"].GetValueAsBool());
            graphics.ApplyChanges();

            if (enableDebugging)
            {
                Components.Add(new DebugComponent(this, Content, graphics));
            }
            
            Content.RootDirectory = "Content";
            physicsSystem = new PhysicsSystem();
            ConvertUnits.SetDisplayUnitToSimUnitRatio(64f);

            // players get updated twice as much as all other entities
            playersFixedTimeStep = new FixedTimeStepSystem(fixedTimestep, maxSteps, new SingleStep(PlayersSingleStep), new PostStepping(PlayersPostStepping));
            fixedTimeStep = new FixedTimeStepSystem(fixedTimestep * 2f, maxSteps, new SingleStep(SingleStep), new PostStepping(PostStepping));

            numPlayers = 1;
        }

        public void SetVsync(bool enable)
        {
            // it's odd... we need to enable these together otherwise there's input lag (and probably other nasty things)
            if (enable)
            {
                IsFixedTimeStep = true;
                graphics.SynchronizeWithVerticalRetrace = true;
            }
            else
            {
                IsFixedTimeStep = false;
                graphics.SynchronizeWithVerticalRetrace = false;
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // for debug view
            DebugView = new DebugViewXNA(physicsSystem.world);
            DebugView.DefaultShapeColor = Color.White;
            DebugView.SleepingShapeColor = Color.LightGray;
            DebugView.AppendFlags(DebugViewFlags.ContactPoints);
            DebugView.AppendFlags(DebugViewFlags.DebugPanel);
            DebugView.RemoveFlags(DebugViewFlags.Joint);
            DebugView.LoadContent(GraphicsDevice, Content);
            
            // for splitscreen
            SetUpSplitScreen();

            entityManager = new EntityManager();
            representationManager = new RepresentationManager(Content);
            controllerManager = new ControllerManager();

            LevelMap levelMap = Content.Load<LevelMap>("simplelevelmap");
            map = new Map(physicsSystem.world, levelMap, entityManager, representationManager, controllerManager);
            mapRepresentation = new MapRepresentation(map);
            mapRepresentation.LoadContent(Content);
            representationManager.Add(mapRepresentation);

            GameVariables.NumPlayers = numPlayers;
            GameVariables.LevelHeight = map.LevelMap.Height;
            GameVariables.EnemyRespawnDelay = 10f;
            GameVariables.CamCulling = new Vector2(640 * 0.5f, 360 * 0.5f);

            // create the players
            CreatePlayer((int)PlayerIndex.One);
            if (numPlayers > 1)
            {
                CreatePlayer((int)PlayerIndex.Two);
            }
            if (numPlayers > 2)
            {
                CreatePlayer((int)PlayerIndex.Three);
            }
            if (numPlayers > 3)
            {
                CreatePlayer((int)PlayerIndex.Four);
            }

            map.SetUpTiles();


            // test for a slanted platform
            /*
            Vector2 position = new Vector2(180, 270);
            Vertices vertices = new Vertices();
            vertices.Add(ConvertUnits.ToSimUnits(new Vector2(290, 480)));
            vertices.Add(ConvertUnits.ToSimUnits(new Vector2(578, 384)));
            vertices.Add(ConvertUnits.ToSimUnits(new Vector2(578, 480)));
            Body body = BodyFactory.CreatePolygon(physicsSystem.world, vertices, 1f);
            body.BodyType = BodyType.Static;
            body.Restitution = 0f;
            body.Friction = 0.5f;
            body.CollisionCategories = GameConstants.PlatformCollisionCategory;
             */

            // test for a thin platform
            //Vector2 position = new Vector2(300, 400);
            /*
            Body body = BodyFactory.CreateEdge(physicsSystem.world, ConvertUnits.ToSimUnits(new Vector2(300, 400)), ConvertUnits.ToSimUnits(new Vector2(500, 400)));
            //StaticPhysicsObject physicsObject = new StaticPhysicsObject(this, physicsSystem.world, position, 64, 1);
            //Body body = physicsObject.body;
            body.BodyType = BodyType.Static;
            body.Restitution = 0f;
            body.Friction = 0.5f;
            body.CollisionCategories = GameConstants.PlatformCollisionCategory;
             */
            //oneWayTile = new OneWayGroundTile(physicsSystem.world, position, 64);
            /*
            Vector2 startPosition = new Vector2(268, 400);
            Vector2 endPosition = new Vector2(332, 400);
            Body body = BodyFactory.CreateEdge(physicsSystem.world, ConvertUnits.ToSimUnits(startPosition), ConvertUnits.ToSimUnits(endPosition));
            body.BodyType = BodyType.Static;
            body.Restitution = 0f;
            body.Friction = 0.5f;
             */

            // set player spawn positions from map
            List<Player> players = entityManager.Players;
            foreach (Player player in players)
            {
                player.SpawnPosition = map.PlayerSpawn;
            }

            // spawn the living entities
            entityManager.SpawnLivingEntities();

            tmpTex = Content.Load<Texture2D>("blank");
        }

        public void SetUpSplitScreen()
        {
            float targetAspectRatio = 16 / 9f;
            // figure out the largest area that fits in this resolution at the desired aspect ratio     
            int width = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int height = (int)(width / targetAspectRatio + .5f);
            if (height > GraphicsDevice.PresentationParameters.BackBufferHeight)
            {
                height = GraphicsDevice.PresentationParameters.BackBufferHeight;
                width = (int)(height * targetAspectRatio + .5f);
            }

            // set up the new viewport centered in the backbuffer     
            GraphicsDevice.Viewport = new Viewport
            {
                X = GraphicsDevice.PresentationParameters.BackBufferWidth / 2 - width / 2,
                Y = GraphicsDevice.PresentationParameters.BackBufferHeight / 2 - height / 2,
                Width = width,
                Height = height,
                MinDepth = 0,
                MaxDepth = 1
            };

            defaultViewport = GraphicsDevice.Viewport;
            topLeftViewport = defaultViewport;
            topRightViewport = defaultViewport;
            bottomLeftViewport = defaultViewport;
            bottomRightViewport = defaultViewport;

            if (numPlayers == 2)
            {
                topLeftViewport.Width = topLeftViewport.Width / 2 - 2;
                topLeftViewport.Height = topLeftViewport.Height / 2 - 2;
                topRightViewport.Width = topRightViewport.Width / 2 - 2;
                topRightViewport.Height = topRightViewport.Height / 2 - 2;

                topLeftViewport.X += defaultViewport.Width / 4;
                topRightViewport.X += topLeftViewport.X;
                topRightViewport.Y += defaultViewport.Height / 2 + 2;
            }
            else if (numPlayers >= 3)
            {
                topLeftViewport.Width = topLeftViewport.Width / 2 - 2;
                topLeftViewport.Height = topLeftViewport.Height / 2 - 2;
                topRightViewport.Width = topRightViewport.Width / 2 - 2;
                topRightViewport.Height = topRightViewport.Height / 2 - 2;

                topRightViewport.X += topLeftViewport.Width + 2;
                bottomLeftViewport.Width = topLeftViewport.Width;
                bottomLeftViewport.Height = topLeftViewport.Height;
                bottomLeftViewport.Y += bottomLeftViewport.Height + 2;
                bottomRightViewport.Width = topRightViewport.Width;
                bottomRightViewport.Height = topRightViewport.Height;
                bottomRightViewport.X += topRightViewport.X;
                bottomRightViewport.Y += bottomRightViewport.Height + 2;
            }
        }

        public void ResetPlayerCameras()
        {
            PlayerRepresentation playerRepresentation;

            playerRepresentation = representationManager.GetPlayerRepresentationWithIndex(PlayerIndex.One);
            playerRepresentation.SetUpCamera(topLeftViewport, map.LevelMap.Width, map.LevelMap.Height);

            if (numPlayers > 1)
            {
                playerRepresentation = representationManager.GetPlayerRepresentationWithIndex(PlayerIndex.Two);
                playerRepresentation.SetUpCamera(topRightViewport, map.LevelMap.Width, map.LevelMap.Height);
            }
            if (numPlayers > 2)
            {
                playerRepresentation = representationManager.GetPlayerRepresentationWithIndex(PlayerIndex.Three);
                playerRepresentation.SetUpCamera(bottomLeftViewport, map.LevelMap.Width, map.LevelMap.Height);
            }
            if (numPlayers > 3)
            {
                playerRepresentation = representationManager.GetPlayerRepresentationWithIndex(PlayerIndex.Four);
                playerRepresentation.SetUpCamera(bottomRightViewport, map.LevelMap.Width, map.LevelMap.Height);
            }
        }

        private void CreatePlayer(int playerIndex)
        {
            PlayerEvent playerEvent = new PlayerEvent();

            Player player = PlayerFactory.CreatePlayer(physicsSystem.world, (PlayerIndex)playerIndex, playerEvent, Vector2.Zero);
            entityManager.Add(player);

            Viewport playerViewport = defaultViewport;
            if ((PlayerIndex)playerIndex == PlayerIndex.One)
                playerViewport = topLeftViewport;
            else if ((PlayerIndex)playerIndex == PlayerIndex.Two)
                playerViewport = topRightViewport;
            else if ((PlayerIndex)playerIndex == PlayerIndex.Three)
                playerViewport = bottomLeftViewport;
            else if ((PlayerIndex)playerIndex == PlayerIndex.Four)
                playerViewport = bottomRightViewport;
            PlayerRepresentation playerRepresentation = PlayerFactory.CreatePlayerRepresentation(player, playerEvent, Content, playerViewport, map.LevelMap.Width, map.LevelMap.Height);
            representationManager.Add(playerRepresentation);

            PlayerController playerController = PlayerFactory.CreatePlayerController(player);
            controllerManager.Add(playerController);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here

            // Memory management
            if (spriteBatch != null)
            {
                try
                {
                    spriteBatch.Dispose();
                    spriteBatch = null;
                }
                catch { }
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            // Update logic
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            dt *= gameSpeed;
            
            physicsSystem.Update(dt);
            // after this, we use the dt from the physics engine since there might be slowdown

            // update the players first
            playersFixedTimeStep.Update(physicsSystem.Dt);
            fixedTimeStep.Update(physicsSystem.Dt);

            // Update the representations at a variable rate
            representationManager.Update(physicsSystem.Dt);
        }

        public void PlayersSingleStep(float dt)
        {
            entityManager.UpdatePlayers(dt);
            controllerManager.UpdatePlayerControllers(dt);
        }

        public void SingleStep(float dt)
        {
            entityManager.Update(dt);
            controllerManager.Update(dt);
        }

        public void PlayersPostStepping()
        {
        }

        public void PostStepping()
        {
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // physics debugging view
            if (this.ShowDebugView)
            {
                GraphicsDevice.Viewport = defaultViewport;
                GraphicsDevice.Clear(new Color(50, 50, 50));

                GraphicsDevice.Viewport = topLeftViewport;
                DrawDebugScene(0);
                if (numPlayers > 1)
                {
                    GraphicsDevice.Viewport = topRightViewport;
                    DrawDebugScene(1);
                }
                if (numPlayers > 2)
                {
                    GraphicsDevice.Viewport = bottomLeftViewport;
                    DrawDebugScene(2);
                }
                if (numPlayers > 3)
                {
                    GraphicsDevice.Viewport = bottomRightViewport;
                    DrawDebugScene(3);
                }
            }


            // NO DEBUG
            /*
            GraphicsDevice.Viewport = defaultViewport;
            Color color = new Color(200, 200, 200);
            GraphicsDevice.Clear(color);

            GraphicsDevice.Viewport = leftViewport;
            //DrawScene(gameTime, Camera1.ViewMatrix, halfprojectionMatrix);
            spriteBatch.Begin(SpriteSortMode.BackToFront,
                        BlendState.AlphaBlend,
                        null,
                        null,
                        null,
                        null,
                        player1Representation.Cam.View);
            mapRepresentation.Draw(spriteBatch);
            player1Representation.Draw(spriteBatch);
            player2Representation.Draw(spriteBatch);
            enemyRepresentation.Draw(spriteBatch);
            spriteBatch.End();



            GraphicsDevice.Viewport = rightViewport;
            //DrawScene(gameTime, Camera2.ViewMatrix, halfprojectionMatrix);
            spriteBatch.Begin(SpriteSortMode.BackToFront,
                        BlendState.AlphaBlend,
                        null,
                        null,
                        null,
                        null,
                        player2Representation.Cam.View);
            mapRepresentation.Draw(spriteBatch);
            player1Representation.Draw(spriteBatch);
            player2Representation.Draw(spriteBatch);
            enemyRepresentation.Draw(spriteBatch);
            spriteBatch.End();
             */

            if (!this.ShowDebugView)
            {
                GraphicsDevice.Viewport = defaultViewport;
                //Color color = new Color(210, 210, 210); // old
                Color color = new Color(30, 30, 30); // the color of the line for splitscreen
                GraphicsDevice.Clear(color);

                GraphicsDevice.Viewport = topLeftViewport;
                DrawScene(0);
                if (numPlayers > 1)
                {
                    GraphicsDevice.Viewport = topRightViewport;
                    DrawScene(1);
                }
                if (numPlayers > 2)
                {
                    GraphicsDevice.Viewport = bottomLeftViewport;
                    DrawScene(2);

                    // todo ***
                    //if (numPlayers == 3)
                    //{
                    //    GraphicsDevice.Viewport = bottomRightViewport;
                    //    DrawActionScene();
                    //}
                }
                if (numPlayers > 3)
                {
                    GraphicsDevice.Viewport = bottomRightViewport;
                    DrawScene(3);
                }
            }

            base.Draw(gameTime);
        }

        protected void DrawScene(int playerIndex)
        {
            // get the current player representation
            PlayerRepresentation playerRepresentation = representationManager.GetPlayerRepresentationWithIndex((PlayerIndex)playerIndex);
            
            // depends on the current resolution
            playerRepresentation.Cam.Zoom = resolutionRatio();
            if (numPlayers == 1)
            {
                playerRepresentation.Cam.Zoom *= 2;
            }


            // Draw the parallax background

            playerRepresentation.Cam.Parallax = 0.66f;
            spriteBatch.Begin(SpriteSortMode.BackToFront,
                        BlendState.AlphaBlend,
                        SamplerState.PointClamp,
                        null,
                        null,
                        null,
                        playerRepresentation.Cam.ParallaxView);
            
            GameVariables.CamPosition = playerRepresentation.Cam.ParallaxPosition;
            representationManager.MapRepresentation.DrawBackground(spriteBatch);
            spriteBatch.End();


            // Draw the other things

            playerRepresentation.Cam.Parallax = 1f;
            spriteBatch.Begin(SpriteSortMode.BackToFront,
                        BlendState.AlphaBlend,
                        SamplerState.PointClamp,
                        null,
                        null,
                        null,
                        playerRepresentation.Cam.View);

            GameVariables.CamPosition = playerRepresentation.Cam.Position;
            representationManager.Draw(spriteBatch);
            spriteBatch.End();
        }

        protected void DrawDebugScene(int playerIndex)
        {
            // get the current player representation
            PlayerRepresentation playerRepresentation = representationManager.GetPlayerRepresentationWithIndex((PlayerIndex)playerIndex);

            playerRepresentation.Cam.Zoom = resolutionRatio();
            if (numPlayers == 1)
            {
                playerRepresentation.Cam.Zoom *= 2;
            }

            //Matrix _projection = Matrix.CreateOrthographicOffCenter(0f, ConvertUnits.ToSimUnits(GraphicsDevice.Viewport.Width), ConvertUnits.ToSimUnits(GraphicsDevice.Viewport.Height), 0f, 0f, 1f);
            Matrix _projection = playerRepresentation.Cam.SimProjection;
            Matrix _view = playerRepresentation.Cam.SimView;
            DebugView.RenderDebugData(ref _projection, ref _view);
        }

        protected float resolutionRatio()
        {
            return (float)defaultViewport.Height / defaultResolutionHeight;
        }

        // Drop-in
        public void AddPlayer()
        {
            if (numPlayers < 4)
            {
                numPlayers++;
                GameVariables.NumPlayers = numPlayers;
            }
            else
                return;

            PlayerIndex playerIndex;
            if (numPlayers > 3)
                playerIndex = PlayerIndex.Four;
            else if (numPlayers > 2)
                playerIndex = PlayerIndex.Three;
            else if (numPlayers > 1)
                playerIndex = PlayerIndex.Two;
            else
                return;

            CreatePlayer((int)playerIndex);
            Player player = entityManager.GetPlayerWithIndex(playerIndex);
            Player playerOne = entityManager.GetPlayerWithIndex(PlayerIndex.One);
            player.SpawnPosition = playerOne.SpawnPosition;
            player.Spawn();

            SetUpSplitScreen();
            ResetPlayerCameras();
        }

        // Drop-out
        public void SubtractPlayer() // here *** so refactor this then refactor the above, [AddPlayer()]
        {
            if (numPlayers < 2)
                return;

            PlayerIndex playerIndex;
            if (numPlayers > 3)
                playerIndex = PlayerIndex.Four;
            else if (numPlayers > 2)
                playerIndex = PlayerIndex.Three;
            else if (numPlayers > 1)
                playerIndex = PlayerIndex.Two;
            else
                return;

            Player player = entityManager.GetPlayerWithIndex(playerIndex);
            entityManager.Remove(player);
            PlayerRepresentation playerRepresentation = representationManager.GetPlayerRepresentationWithIndex(playerIndex);
            representationManager.Remove(playerRepresentation);
            PlayerController playerController = controllerManager.GetPlayerControllerWithIndex(playerIndex);
            controllerManager.Remove(playerController);

            player.Dispose();
            
            numPlayers--;
            GameVariables.NumPlayers = numPlayers;

            SetUpSplitScreen();
            ResetPlayerCameras();
        }
    }





    class DebugComponent : DrawableGameComponent
    {
        int frameRate = 0;
        int frameCounter = 0;
        TimeSpan elapsedTime = TimeSpan.Zero;
        SpriteFont spriteFont;
        ContentManager content;
        SpriteBatch spriteBatch;
        GraphicsDeviceManager graphics;
        Game1 game;
        bool debugKeyPressed = false;
        string debugString;
        Timer debugTextTimer;
        bool debugText = true;

        private const float debugTextDelay = 3f;

        public DebugComponent(Game1 game, ContentManager content, GraphicsDeviceManager graphics)
            : base(game)
        {
            this.game = game;
            this.content = content;
            this.graphics = graphics;
            debugTextTimer = new Timer();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = content.Load<SpriteFont>("HUDFont");
        }

        protected override void UnloadContent()
        {
            content.Unload();
        }

        public override void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                frameRate = frameCounter;
                frameCounter = 0;
            }

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            HandleDebugInput(dt);
            debugTextTimer.Update(dt);
        }

        public override void Draw(GameTime gameTime)
        {
            ++frameCounter;

            string fps = string.Format("fps: {0}", frameRate);

            if (debugText)
            {
                GraphicsDevice.Viewport = game.defaultViewport;

                spriteBatch.Begin();

                spriteBatch.DrawString(spriteFont, fps, new Vector2(5, 5), Color.White);

                if (!debugTextTimer.IsTimeUp())
                {
                    spriteBatch.DrawString(spriteFont, debugString, new Vector2(5, 37), Color.White);
                }

                spriteBatch.End();
            }
        }

        protected void HandleDebugInput(float dt)
        {
            KeyboardState keyState = Keyboard.GetState();

            if (!debugKeyPressed)
            {
                if (keyState.IsKeyDown(Keys.F1))
                {
                    toggleDebugText();
                }
                else if (keyState.IsKeyDown(Keys.F4))
                {
                    //graphics.ToggleFullScreen();
                    toggleFullScreen();
                }
                else if (keyState.IsKeyDown(Keys.F5))
                {
                    debugTextTimer.SetDelay(debugTextDelay);
                    toggleVSync();
                }
                else if (keyState.IsKeyDown(Keys.F6))
                {
                    debugTextTimer.SetDelay(debugTextDelay);
                    toggleResolution();
                }
                else if (keyState.IsKeyDown(Keys.F10))
                {
                    toggleDebugView();
                }
                else if (keyState.IsKeyDown(Keys.Add))
                {
                    toggleChangeNumPlayers(true);
                }
                else if (keyState.IsKeyDown(Keys.Subtract))
                {
                    toggleChangeNumPlayers(false);
                }
            }

            if (keyState.GetPressedKeys().Length == 0)
            {
                debugKeyPressed = false;
            }
            else
            {
                debugKeyPressed = true;
            }
        }
        protected void toggleDebugText()
        {
            if (debugText)
                debugText = false;
            else
                debugText = true;
        }
        protected void toggleFullScreen()
        {
            
            if (graphics.IsFullScreen)
            {
                graphics.IsFullScreen = false;
                graphics.PreferredBackBufferWidth = 1280;
                graphics.PreferredBackBufferHeight = 720;
                graphics.ApplyChanges();

                game.SetUpSplitScreen();
                game.ResetPlayerCameras();
            }
            else
            {
                graphics.IsFullScreen = true;
                graphics.ApplyChanges();
            }
        }
        protected void toggleVSync()
        {
            if (graphics.SynchronizeWithVerticalRetrace)
            {
                game.SetVsync(false);
                debugString = "VSync off";
            }
            else
            {
                game.SetVsync(true);
                debugString = "VSync on";
            }

            graphics.ApplyChanges();
        }
        protected void toggleResolution()
        {
            if (graphics.IsFullScreen)
            {
                if (graphics.PreferredBackBufferHeight == 720)
                {
                    graphics.PreferredBackBufferWidth = 2560;
                    graphics.PreferredBackBufferHeight = 1440;
                }
                else
                {
                    graphics.PreferredBackBufferWidth = 1280;
                    graphics.PreferredBackBufferHeight = 720;
                }

                graphics.ApplyChanges();
                game.SetUpSplitScreen();
                game.ResetPlayerCameras();

                debugString = string.Format("{0} x {1}", game.Window.ClientBounds.Width, game.Window.ClientBounds.Height);
            }
            else
            {
                debugString = "change resolution in \n fullscreen (F4) only";
            }
        }
        protected void toggleDebugView()
        {
            if (game.ShowDebugView)
                game.ShowDebugView = false;
            else
                game.ShowDebugView = true;
        }
        protected void toggleChangeNumPlayers(bool add)
        {
            if (add)
                game.AddPlayer();
            else
                game.SubtractPlayer();
        }
    }
}
