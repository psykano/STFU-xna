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
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using EasyConfig;

namespace STFU
{
    /// <summary>
    /// Creates enemies: creates their entity, controller and representation.
    /// </summary>
    class EnemyFactory
    {
        // walker
        private const string walkerGeneralSettings = "WalkerGeneral";

        public static WalkerEnemy CreateWalkerEntity(World world, EnemyEvent enemyEvent, Vector2 enemyPosition)
        {
            ConfigFile configFile = Enemy.GetEnemyConfigFile();
            int health = configFile.SettingGroups[walkerGeneralSettings].Settings["health"].GetValueAsInt();
            float hitDelay = configFile.SettingGroups[walkerGeneralSettings].Settings["hitDelay"].GetValueAsFloat();
            float recoveryDelay = configFile.SettingGroups[walkerGeneralSettings].Settings["recoveryDelay"].GetValueAsFloat();

            WalkerEnemy enemy = new WalkerEnemy(world, enemyEvent);
            enemy.SetUpEnemy(enemyPosition, health, hitDelay, recoveryDelay);

            return enemy;
        }

        public static WalkerEnemyRepresentation CreateWalkerRepresentation(WalkerEnemy enemy, EnemyEvent enemyEvent, ContentManager content)
        {
            WalkerEnemyRepresentation enemyRepresentation = new WalkerEnemyRepresentation(enemy, enemyEvent);
            enemyRepresentation.LoadContent(content);

            return enemyRepresentation;
        }

        public static WalkerEnemyController CreateWalkerController(WalkerEnemy enemy)
        {
            WalkerEnemyController enemyController = new WalkerEnemyController(enemy);

            return enemyController;
        }

        // bat
        private const string batGeneralSettings = "BatGeneral";

        public static BatEnemy CreateBatEntity(World world, EnemyEvent enemyEvent, Vector2 enemyPosition)
        {
            ConfigFile configFile = Enemy.GetEnemyConfigFile();
            int health = configFile.SettingGroups[batGeneralSettings].Settings["health"].GetValueAsInt();
            float hitDelay = configFile.SettingGroups[batGeneralSettings].Settings["hitDelay"].GetValueAsFloat();
            float recoveryDelay = configFile.SettingGroups[batGeneralSettings].Settings["recoveryDelay"].GetValueAsFloat();

            BatEnemy enemy = new BatEnemy(world, enemyEvent);
            enemy.SetUpEnemy(enemyPosition, health, hitDelay, recoveryDelay);

            return enemy;
        }

        public static BatEnemyRepresentation CreateBatRepresentation(BatEnemy enemy, EnemyEvent enemyEvent, ContentManager content)
        {
            BatEnemyRepresentation enemyRepresentation = new BatEnemyRepresentation(enemy, enemyEvent);
            enemyRepresentation.LoadContent(content);

            return enemyRepresentation;
        }

        public static BatEnemyController CreateBatController(BatEnemy enemy)
        {
            BatEnemyController enemyController = new BatEnemyController(enemy);

            return enemyController;
        }
    }
}
