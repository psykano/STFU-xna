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

namespace STFU
{
    /// <summary>
    /// Creates enemies: creates their entity, controller and representation.
    /// </summary>
    class EnemyFactory
    {
        // walker
        public const float WalkerWidth = 30f;
        public const float WalkerHeight = 32f;
        public const int WalkerHealth = 3;
        public const float WalkerHitDelay = 0.2f;
        public const float WalkerRecoveryDelay = 0f;

        public static WalkerEnemy CreateWalkerEntity(World world, EnemyEvent enemyEvent, Vector2 enemyPosition)
        {
            WalkerEnemy enemy = new WalkerEnemy(world, enemyEvent);
            enemy.SetUpEnemy(enemyPosition, WalkerWidth, WalkerHeight, WalkerHealth, WalkerHitDelay, WalkerRecoveryDelay);

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
        public const float BatWidth = 22f;
        public const float BatHeight = 26f;
        public const int BatHealth = 2;
        public const float BatHitDelay = 0.2f;
        public const float BatRecoveryDelay = 0f;

        public static BatEnemy CreateBatEntity(World world, EnemyEvent enemyEvent, Vector2 enemyPosition)
        {
            BatEnemy enemy = new BatEnemy(world, enemyEvent);
            enemy.SetUpEnemy(enemyPosition, BatWidth, BatHeight, BatHealth, BatHitDelay, BatRecoveryDelay);

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
