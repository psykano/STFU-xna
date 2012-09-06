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
    /// Creates players: creates their entity, controller and representation.
    /// </summary>
    class PlayerFactory
    {
        private const string generalSettings = "General";

        public static Player CreatePlayer(World world, PlayerIndex playerIndex, PlayerEvent playerEvent, Vector2 playerPosition)
        {
            ConfigFile configFile = new ConfigFile(Player.SettingsIni);
            int health = configFile.SettingGroups[generalSettings].Settings["health"].GetValueAsInt();
            bool enableWallJumping = configFile.SettingGroups[generalSettings].Settings["enableWallJumping"].GetValueAsBool();
            bool enableWallSliding = configFile.SettingGroups[generalSettings].Settings["enableWallSliding"].GetValueAsBool();
            bool enableShooting = configFile.SettingGroups[generalSettings].Settings["enableShooting"].GetValueAsBool();

            Player player = new Player(world, (PlayerIndex)playerIndex, playerEvent);
            player.SetUpPlayer(playerPosition, health);

            if (enableWallJumping)
                player.enableWallJumping();
            if (enableWallSliding)
                player.enableWallSliding();
            if (enableShooting)
                player.enableShooting();

            return player;
        }

        public static PlayerRepresentation CreatePlayerRepresentation(Player player, PlayerEvent playerEvent, ContentManager content, Viewport playerViewport, float levelWidth, float levelHeight)
        {
            PlayerRepresentation playerRepresentation = new PlayerRepresentation(player, playerEvent);
            playerRepresentation.LoadContent(content);
            playerRepresentation.SetUpCamera(playerViewport, levelWidth, levelHeight);

            return playerRepresentation;
        }

        public static PlayerController CreatePlayerController(Player player)
        {
            PlayerController playerController = new PlayerController(player);

            return playerController;
        }
    }
}
