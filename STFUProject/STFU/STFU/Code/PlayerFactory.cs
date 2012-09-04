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
    /// Creates players: creates their entity, controller and representation.
    /// </summary>
    class PlayerFactory
    {
        public const float PlayerWidth = 13f;
        public const float PlayerHeight = 22f;
        public const int PlayerHealth = 2;

        public static Player CreatePlayer(World world, PlayerIndex playerIndex, PlayerEvent playerEvent, Vector2 playerPosition)
        {
            Player player = new Player(world, (PlayerIndex)playerIndex, playerEvent);
            player.SetUpPlayer(playerPosition, PlayerWidth, PlayerHeight, PlayerHealth);

            // these should be resources
            player.enableWallJumping();
            player.enableWallSliding();
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
