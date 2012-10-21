/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace STFU
{
    /// <summary>
    /// A map representation is the visual representation(sorry!) of the map on the screen.
    /// This includes things like the ground, platforms but doesn't include other entities like
    /// enemies or players.
    /// </summary>
    class MapRepresentation : EntityRepresentation
    {
        protected Map map;
        protected Rectangle sourceRect;

        protected Frame wallFrame;
        protected Frame checkpointPoleFrame;
        protected Frame checkpointFlagFrame;
        protected Frame groundFrame;
        protected Frame spikeFrame;
        protected Frame staticPlatformFrame;
        protected Frame movingPlatformFrame;
        protected Frame fallingPlatformFrame;

        private const string wallFrameName = "wall";
        private const string checkpointPoleFrameName = "checkpointpole";
        private const string checkpointFlagFrameName = "checkpointflag";
        private const string groundFrameName = "ground";
        private const string spikeFrameName = "spike";
        private const string staticPlatformFrameName = "staticplatform";
        private const string movingPlatformFrameName = "movingplatform";
        private const string fallingPlatformFrameName = "fallingplatform";

        protected Random random;

        // Constructor
        public MapRepresentation(Map map)
            : base()
        {
            this.map = map;
            this.random = new Random();
        }

        public override void LoadContent(ContentManager content)
        {
            spriteSheet = new SpriteSheet();
            spriteSheet.Sheet = content.Load<Texture2D>("tilesheet");
            spriteSheet.Map = content.Load<Dictionary<string, Rectangle>>("tilesheetmap");
            sourceRect = spriteSheet.Map[groundFrameName];

            float tilePaddingScale = 1.04f;

            wallFrame = new Frame();
            int randomNumberR = random.Next(10);
            int randomNumberG = random.Next(10);
            int randomNumberB = random.Next(10);
            Color wallColor = new Color(225 - randomNumberR, 225 - randomNumberG, 225 - randomNumberB);
            wallFrame.Initialize(spriteSheet, Vector2.Zero, wallFrameName, wallColor, tilePaddingScale, SpriteEffects.None);
            wallFrame.LayerDepth = 0.3f;

            checkpointPoleFrame = new Frame();
            checkpointPoleFrame.Initialize(spriteSheet, Vector2.Zero, checkpointPoleFrameName, Color.White, 1f, SpriteEffects.None);
            checkpointPoleFrame.LayerDepth = 0.25f;

            checkpointFlagFrame = new Frame();
            checkpointFlagFrame.Initialize(spriteSheet, Vector2.Zero, checkpointFlagFrameName, Color.White, 1f, SpriteEffects.None);
            checkpointFlagFrame.LayerDepth = 0.25f;
            
            groundFrame = new Frame();
            groundFrame.Initialize(spriteSheet, Vector2.Zero, groundFrameName, Color.White, tilePaddingScale, SpriteEffects.None);
            groundFrame.LayerDepth = 0.1f;

            spikeFrame = new Frame();
            spikeFrame.Initialize(spriteSheet, Vector2.Zero, spikeFrameName, Color.Red, 1f, SpriteEffects.None);
            spikeFrame.LayerDepth = 0.1f;

            staticPlatformFrame = new Frame();
            staticPlatformFrame.Initialize(spriteSheet, Vector2.Zero, staticPlatformFrameName, Color.White, tilePaddingScale, SpriteEffects.None);
            staticPlatformFrame.LayerDepth = 0.25f;

            movingPlatformFrame = new Frame();
            movingPlatformFrame.Initialize(spriteSheet, Vector2.Zero, movingPlatformFrameName, Color.White, tilePaddingScale, SpriteEffects.None);
            movingPlatformFrame.LayerDepth = 0.25f;

            fallingPlatformFrame = new Frame();
            fallingPlatformFrame.Initialize(spriteSheet, Vector2.Zero, fallingPlatformFrameName, Color.White, tilePaddingScale, SpriteEffects.None);
            fallingPlatformFrame.LayerDepth = 0.25f;
        }

        public override void Update(float dt)
        {
            //
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the tiles
            foreach (Entity tile in map.Tiles)
            {
                if (!tile.Visible)
                    continue;

                if (tile is CheckpointTile)
                {
                    checkpointPoleFrame.Position = tile.ScreenPosition;
                    checkpointPoleFrame.Rotation = tile.ScreenRotation;
                    checkpointPoleFrame.Draw(spriteBatch);

                    CheckpointTile checkpointTile = tile as CheckpointTile;
                    if (checkpointTile.Activated)
                    {
                        checkpointFlagFrame.Position = tile.ScreenPosition;
                        checkpointFlagFrame.Rotation = tile.ScreenRotation;
                        checkpointFlagFrame.Draw(spriteBatch);
                    }
                }
                if (tile is GroundTile)
                {
                    groundFrame.Position = tile.ScreenPosition;
                    groundFrame.Rotation = tile.ScreenRotation;
                    groundFrame.Draw(spriteBatch);
                }
                else if (tile is SpikeTile)
                {
                    spikeFrame.Position = tile.ScreenPosition;
                    spikeFrame.Rotation = tile.ScreenRotation;
                    spikeFrame.Draw(spriteBatch);
                }
                else if (tile is StaticPlatformTile)
                {
                    staticPlatformFrame.Position = tile.ScreenPosition;
                    staticPlatformFrame.Rotation = tile.ScreenRotation;
                    staticPlatformFrame.Draw(spriteBatch);
                }
                else if (tile is MovingPlatformTile)
                {
                    movingPlatformFrame.Rotation = tile.ScreenRotation;
                    movingPlatformFrame.Position = tile.ScreenPosition;
                    // check if it's a big tile
                    if (tile.ScreenWidth <= map.LevelMap.TileWidth)
                    {
                        movingPlatformFrame.Draw(spriteBatch);
                    }
                    else
                    {
                        movingPlatformFrame.Position -= Vector2.UnitX * tile.ScreenWidth * 0.25f;
                        movingPlatformFrame.Draw(spriteBatch);
                        movingPlatformFrame.Position += Vector2.UnitX * tile.ScreenWidth * 0.5f;
                        movingPlatformFrame.Draw(spriteBatch);
                    }
                }
                else if (tile is FallingPlatformTile)
                {
                    fallingPlatformFrame.Position = tile.ScreenPosition;
                    fallingPlatformFrame.Rotation = tile.ScreenRotation;
                    fallingPlatformFrame.Draw(spriteBatch);
                }
            }
        }

        public void DrawBackground(SpriteBatch spriteBatch)
        {
            // Draw the wall
            foreach (WallTile wallTile in map.WallTiles)
            {
                wallFrame.Position = wallTile.ScreenPosition;
                wallFrame.Rotation = wallTile.ScreenRotation;
                wallFrame.Draw(spriteBatch);
            }
        }
    }
}
