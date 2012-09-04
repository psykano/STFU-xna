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
    /// <summary>
    /// Holds all current game controllers.
    /// </summary>
    class ControllerManager
    {
        public List<PlayerController> PlayerControllers { get; protected set; }
        private List<Controller> controllers;

        // Constructor
        public ControllerManager()
        {
            controllers = new List<Controller>();
            PlayerControllers = new List<PlayerController>();
        }

        public void Update(float dt)
        {
            foreach (Controller controller in controllers)
            {
                controller.Update(dt);
            }
        }

        public void UpdatePlayerControllers(float dt)
        {
            foreach (PlayerController controller in this.PlayerControllers)
            {
                controller.Update(dt);
            }
        }

        public void Add(Controller controller)
        {
            PlayerController playerController = controller as PlayerController;
            if (playerController != null)
                this.PlayerControllers.Add(playerController);
            else
                controllers.Add(controller);
        }

        public void Remove(Controller controller)
        {
            PlayerController playerController = controller as PlayerController;
            if (playerController != null)
                this.PlayerControllers.Remove(playerController);
            else
                controllers.Remove(controller);
        }

        public PlayerController GetPlayerControllerWithIndex(PlayerIndex playerIndex)
        {
            foreach (PlayerController playerController in this.PlayerControllers)
            {
                if (playerController.PlayerIndex == playerIndex)
                {
                    return playerController;
                }
            }

            return null;
        }
    }
}
