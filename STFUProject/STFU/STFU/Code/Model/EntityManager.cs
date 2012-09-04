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
    /// Holds all current game entities.
    /// </summary>
    class EntityManager
    {
        public List<Player> Players { get; protected set; }
        private List<Entity> entities;

        // Constructor
        public EntityManager()
        {
            entities = new List<Entity>();
            Players = new List<Player>();
        }

        public void Update(float dt)
        {
            foreach (Entity entity in entities)
            {
                entity.Update(dt);
            }
        }

        public void UpdatePlayers(float dt)
        {
            foreach (Player entity in this.Players)
            {
                entity.Update(dt);
            }
        }

        public void Add(Entity entity)
        {
            Player player = entity as Player;
            if (player != null)
                this.Players.Add(player);
            else
                entities.Add(entity);
        }

        public void Remove(Entity entity)
        {
            Player player = entity as Player;
            if (player != null)
                this.Players.Remove(player);
            else
                entities.Remove(entity);
        }

        public Player GetPlayerWithIndex(PlayerIndex playerIndex)
        {
            foreach (Player player in this.Players)
            {
                if (player.PlayerIndex == playerIndex)
                {
                    return player;
                }
            }

            return null;
        }

        public void SpawnLivingEntities()
        {
            foreach (Player player in this.Players)
            {
                player.Spawn();
            }

            foreach (Entity entity in entities)
            {
                LivingEntity livingEntity = entity as LivingEntity;
                if (livingEntity != null)
                {
                    livingEntity.Spawn();
                }
            }
        }
    }
}
