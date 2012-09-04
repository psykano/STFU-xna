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
using MyDataTypes;

namespace STFU
{
    /// <summary>
    /// The layout for the current level.
    /// It's in ascii and the character definitions can be found right below.
    /// </summary>
    class Map
    {
        public const char PLACEHOLDER = '.';
        public const char SPAWN_POINT = 's';
        public const char CHECKPOINT = 'c';
        public const char GROUND = '1';
        public const char SPIKE = 'x';
        public const char STATIC_PLATFORM = '-';
        public const char HORIZONTAL_PLATFORM = 'h';
        public const char BIG_HORIZONTAL_PLATFORM = 'H';
        public const char REVERSE_HORIZONTAL_PLATFORM = 'j';
        public const char BIG_REVERSE_HORIZONTAL_PLATFORM = 'J';
        public const char VERTICAL_PLATFORM = 'y';
        public const char BIG_VERTICAL_PLATFORM = 'Y';
        public const char REVERSE_VERTICAL_PLATFORM = 'u';
        public const char BIG_REVERSE_VERTICAL_PLATFORM = 'U';
        public const char CLOCKWISE_PLATFORM = '[';
        public const char BIG_CLOCKWISE_PLATFORM = '{';
        public const char REVERSE_CLOCKWISE_PLATFORM = ']';
        public const char BIG_REVERSE_CLOCKWISE_PLATFORM = '}';
        public const char FALLING_PLATFORM = '_';
        public const char WALKER_ENEMY = 'w';
        public const char BAT_ENEMY = 'b';

        protected World world;
        public LevelMap LevelMap { get; set; }
        EntityManager entityManager;
        RepresentationManager representationManager;
        ControllerManager controllerManager;
        public List<WallTile> WallTiles { get; set; }
        public List<Entity> Tiles { get; set; }
        public Vector2 PlayerSpawn { get; set; }
        protected Random random;

        private const int spikeTileSizeOffset = -8;

        public Map(World world, LevelMap levelMap, EntityManager entityManager, RepresentationManager representationManager, ControllerManager controllerManager)
        {
            this.world = world;
            this.LevelMap = levelMap;
            this.entityManager = entityManager;
            this.representationManager = representationManager;
            this.controllerManager = controllerManager;
            this.WallTiles = new List<WallTile>();
            this.Tiles = new List<Entity>();
            this.random = new Random();
        }

        public void SetUpTiles()
        {
            setUpWallTiles();
            using (StringReader strReader = new StringReader(this.LevelMap.MapString))
            {
                int x = 0;
                int y = 0;
                string line;
                while ((line = strReader.ReadLine()) != null)
                {
                    if (line.Length > 0)
                    {
                        foreach (char c in line)
                        {
                            if (c.Equals('0')) // empty
                            {
                                // so that there is a border around the map
                                if (x == 0)
                                {
                                    addGroundTileAt(-1, y);
                                }
                                if (y == 0)
                                {
                                    addGroundTileAt(x, -1);
                                }
                                x++;
                            }
                            else if (c.Equals(SPAWN_POINT)) // players' spawn point
                            {
                                PlayerSpawn = calculateTilePosition(x, y);
                                x++;
                            }
                            else if (checkIfTile(c, x, y))
                            {
                                x++;
                            }
                            else if (checkIfEnemy(c, x, y))
                            {
                                x++;
                            }
                            else if (c.Equals(PLACEHOLDER))
                            {
                                x++;
                            }
                            else
                            {
                                // it's blank or something
                            }
                        }
                        // again, border
                        if (x > 0)
                            addGroundTileAt(x, y);

                        y++;
                        x = 0;
                    }
                }
            }
        }

        private void setUpWallTiles()
        {
            for (int x = 0; x < LevelMap.Width / LevelMap.TileWidth; x++)
            {
                for (int y = 0; y < LevelMap.Height / LevelMap.TileHeight; y++)
                {
                    Vector2 tilePosition = calculateTilePosition(x, y);
                    WallTile wallTile = new WallTile(world, tilePosition, this.LevelMap.TileWidth, this.LevelMap.TileHeight, randomTileRotation());
                    this.WallTiles.Add(wallTile);
                }
            }
        }

        private float randomTileRotation()
        {
            int randomNumber = random.Next(4);
            float tileRotation = 0;
            if (randomNumber == 0)
                tileRotation = 0;
            else if (randomNumber == 1)
                tileRotation = (float)(Math.PI * 0.5f);
            else if (randomNumber == 2)
                tileRotation = (float)Math.PI;
            else if (randomNumber == 3)
                tileRotation = (float)((3f * Math.PI) * 0.5f);

            return tileRotation;
        }

        private Vector2 calculateTilePosition(int x, int y)
        {
            return new Vector2((x * this.LevelMap.TileWidth) + this.LevelMap.TileWidth * 0.5f, (y * this.LevelMap.TileHeight) + this.LevelMap.TileHeight * 0.5f);
        }

        private Vector2 calculatePlatformTilePosition(int x, int y)
        {
            return calculateTilePosition(x, y) - Vector2.UnitY * this.LevelMap.TileHeight * 0.5f;
        }

        private Vector2 calculateMovingPlatformPosition(int x, int y, bool big)
        {
            Vector2 position = calculatePlatformTilePosition(x, y);
            if (big)
            {
                position.X += this.LevelMap.TileWidth * 0.5f;
            }
            return position;
        }

        private int calculateMovingPlatformWidth(bool big)
        {
            int width = this.LevelMap.TileWidth;
            if (big)
            {
                width *= 2;
            }
            return width;
        }

        private bool checkIfTile(char c, int x, int y)
        {
            switch (c) // refactor this? ***
            {
                case CHECKPOINT:
                    {
                        addCheckpointTileAt(x, y);
                        return true;
                    }
                case GROUND:
                    {
                        addGroundTileAt(x, y);
                        return true;
                    }
                case SPIKE:
                    {
                        addSpikeTileAt(x, y);
                        return true;
                    }
                case STATIC_PLATFORM:
                    {
                        addStaticPlatformTileAt(x, y);
                        return true;
                    }
                case HORIZONTAL_PLATFORM:
                    {
                        addHorizontalPlatformTileAt(x, y, false, false);
                        return true;
                    }
                case BIG_HORIZONTAL_PLATFORM:
                    {
                        addHorizontalPlatformTileAt(x, y, true, false);
                        return true;
                    }
                case REVERSE_HORIZONTAL_PLATFORM:
                    {
                        addHorizontalPlatformTileAt(x, y, false, true);
                        return true;
                    }
                case BIG_REVERSE_HORIZONTAL_PLATFORM:
                    {
                        addHorizontalPlatformTileAt(x, y, true, true);
                        return true;
                    }
                case VERTICAL_PLATFORM:
                    {
                        addVerticalPlatformTileAt(x, y, false, false);
                        return true;
                    }
                case BIG_VERTICAL_PLATFORM:
                    {
                        addVerticalPlatformTileAt(x, y, true, false);
                        return true;
                    }
                case REVERSE_VERTICAL_PLATFORM:
                    {
                        addVerticalPlatformTileAt(x, y, false, true);
                        return true;
                    }
                case BIG_REVERSE_VERTICAL_PLATFORM:
                    {
                        addVerticalPlatformTileAt(x, y, true, true);
                        return true;
                    }
                case CLOCKWISE_PLATFORM:
                    {
                        addClockwisePlatformTileAt(x, y, false, false);
                        return true;
                    }
                case BIG_CLOCKWISE_PLATFORM:
                    {
                        addClockwisePlatformTileAt(x, y, true, false);
                        return true;
                    }
                case REVERSE_CLOCKWISE_PLATFORM:
                    {
                        addClockwisePlatformTileAt(x, y, false, true);
                        return true;
                    }
                case BIG_REVERSE_CLOCKWISE_PLATFORM:
                    {
                        addClockwisePlatformTileAt(x, y, true, true);
                        return true;
                    }
                case FALLING_PLATFORM:
                    {
                        addFallingPlatformTileAt(x, y);
                        return true;
                    }
                default:
                    {
                        break;
                    }
            }

            return false;
        }

        private void addTile(Entity tile)
        {
            this.Tiles.Add(tile);
        }

        private void addDynamicTile(Entity tile)
        {
            addTile(tile);
            entityManager.Add(tile);
        }

        private void addCheckpointTileAt(int x, int y)
        {
            Vector2 tilePosition = calculateTilePosition(x, y);

            CheckpointTile checkPointTile = new CheckpointTile(world, tilePosition, this.LevelMap.TileWidth * 3f, this.LevelMap.TileHeight * 3f, entityManager);
            addTile(checkPointTile);
        }

        private void addGroundTileAt(int x, int y)
        {
            Vector2 tilePosition = calculateTilePosition(x, y);
            GroundTile groundTile = new GroundTile(world, tilePosition, this.LevelMap.TileWidth, this.LevelMap.TileHeight, randomTileRotation());
            addTile(groundTile);
        }

        /*
        private void addGroundLowerInclineTileAt(int x, int y)
        {
            Vector2 tilePosition = calculateTilePosition(x, y);
            //GroundTile spikeTile = new SpikeTile(world, tilePosition, this.LevelMap.TileWidth + spikeTileSizeOffset, this.LevelMap.TileHeight + spikeTileSizeOffset);
            //addTile(groundTile);
        }

        private void addGroundUpperInclineTileAt(int x, int y)
        {
        }

        private void addGroundLowerDeclineTileAt(int x, int y)
        {
        }

        private void addGroundUpperDeclineTileAt(int x, int y)
        {
        }
         */

        private void addSpikeTileAt(int x, int y)
        {
            Vector2 tilePosition = calculateTilePosition(x, y);
            SpikeTile spikeTile = new SpikeTile(world, tilePosition, this.LevelMap.TileWidth + spikeTileSizeOffset, this.LevelMap.TileHeight + spikeTileSizeOffset);
            addTile(spikeTile);
        }

        private void addStaticPlatformTileAt(int x, int y)
        {
            Vector2 tilePosition = calculatePlatformTilePosition(x, y);
            StaticPlatformTile platformTile = new StaticPlatformTile(world, tilePosition, this.LevelMap.TileWidth);
            addTile(platformTile);
        }

        private void addHorizontalPlatformTileAt(int x, int y, bool big, bool reverse)
        {
            Vector2 tilePosition = calculateMovingPlatformPosition(x, y, big);
            int tileWidth = calculateMovingPlatformWidth(big);
            
            HorizontalMovingPlatform platformTile = new HorizontalMovingPlatform(world, tilePosition, tileWidth, this.LevelMap.TileWidth, reverse);
            addDynamicTile(platformTile);
        }

        private void addVerticalPlatformTileAt(int x, int y, bool big, bool reverse)
        {
            Vector2 tilePosition = calculateMovingPlatformPosition(x, y, big);
            int tileWidth = calculateMovingPlatformWidth(big);

            VerticalMovingPlatform platformTile = new VerticalMovingPlatform(world, tilePosition, tileWidth, this.LevelMap.TileWidth, reverse);
            addDynamicTile(platformTile);
        }

        private void addClockwisePlatformTileAt(int x, int y, bool big, bool reverse)
        {
            Vector2 tilePosition = calculateMovingPlatformPosition(x, y, big);
            int tileWidth = calculateMovingPlatformWidth(big);
            
            //CircularMovingPlatform platformTile = new CircularMovingPlatform(world, tilePosition, finalTilePosition, tileWidth, 1.25f, true, false);
            ClockwiseMovingPlatform platformTile = new ClockwiseMovingPlatform(world, tilePosition, tileWidth, reverse);
            addDynamicTile(platformTile);
        }

        /*
        private void addReverseClockwisePlatformTileAt(int x, int y, bool big)
        {
            Vector2 tilePosition = calculateMovingPlatformPosition(x, y, big);
            int tileWidth = calculateMovingPlatformWidth(big);

            ClockwiseMovingPlatform platformTile = new ClockwiseMovingPlatform(world, tilePosition, tileWidth, true);
            addDynamicTile(platformTile);
        }
         */

        private void addFallingPlatformTileAt(int x, int y)
        {
            Vector2 tilePosition = calculatePlatformTilePosition(x, y);
            int tileWidth = this.LevelMap.TileWidth;

            FallingPlatformTile platformTile = new FallingPlatformTile(world, tilePosition, tileWidth, 0.35f, 5f);
            addDynamicTile(platformTile);
        }

        private bool checkIfEnemy(char c, int x, int y)
        {
            switch (c) // refactor this? ***
            {
                case WALKER_ENEMY:
                    {
                        spawnWalkerEnemyAt(x, y);
                        return true;
                    }
                case BAT_ENEMY:
                    {
                        spawnBatEnemyAt(x, y);
                        return true;
                    }
                default:
                    {
                        break;
                    }
            }

            return false;
        }

        private void spawnEnemy(Enemy enemy, EnemyRepresentation enemyRepresentation, EnemyController enemyController) 
        {
            entityManager.Add(enemy);
            representationManager.Add(enemyRepresentation);
            controllerManager.Add(enemyController);
        }

        private void spawnWalkerEnemyAt(int x, int y)
        {
            EnemyEvent enemyEvent = new EnemyEvent();
            Vector2 tilePosition = calculateTilePosition(x, y);
            WalkerEnemy enemy = EnemyFactory.CreateWalkerEntity(world, enemyEvent, tilePosition);
            WalkerEnemyRepresentation enemyRepresentation = EnemyFactory.CreateWalkerRepresentation(enemy, enemyEvent, representationManager.Content);
            WalkerEnemyController enemyController = EnemyFactory.CreateWalkerController(enemy);
            spawnEnemy(enemy, enemyRepresentation, enemyController);
        }

        private void spawnBatEnemyAt(int x, int y)
        {
            EnemyEvent enemyEvent = new EnemyEvent();
            Vector2 tilePosition = calculateTilePosition(x, y);
            BatEnemy enemy = EnemyFactory.CreateBatEntity(world, enemyEvent, tilePosition);
            BatEnemyRepresentation enemyRepresentation = EnemyFactory.CreateBatRepresentation(enemy, enemyEvent, representationManager.Content);
            BatEnemyController enemyController = EnemyFactory.CreateBatController(enemy);
            spawnEnemy(enemy, enemyRepresentation, enemyController);
        }
    }
}
