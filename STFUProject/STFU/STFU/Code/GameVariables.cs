/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace STFU
{
    public static class GameVariables
    {
        private static int numPlayer;
        public static int NumPlayers
        {
            get
            {
                return numPlayer;
            }
            set
            {
                numPlayer = value;
            }
        }

        private static float levelHeight;
        public static float LevelHeight
        {
            get
            {
                return levelHeight;
            }
            set
            {
                levelHeight = value;
            }
        }

        private static float enemyRespawnDelay;
        public static float EnemyRespawnDelay
        {
            get
            {
                return enemyRespawnDelay;
            }
            set
            {
                enemyRespawnDelay = value;
            }
        }

        public static float GetSimLevelHeight()
        {
            return ConvertUnits.ToSimUnits(LevelHeight);
        }

        private static Vector2 camPosition;
        public static Vector2 CamPosition
        {
            get
            {
                return camPosition;
            }
            set
            {
                camPosition = value;
            }
        }

        private static Vector2 camCulling;
        public static Vector2 CamCulling
        {
            get
            {
                return camCulling;
            }
            set
            {
                camCulling = value;
            }
        }
    }
}
