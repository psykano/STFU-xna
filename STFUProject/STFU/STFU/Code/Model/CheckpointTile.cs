/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;

namespace STFU
{
    /// <summary>
    /// A checkpoint is a new spawnpoint for all the players.
    /// In the physics world, it's only a rectangular sensor to detect when a player is near the checkpoint.
    /// </summary>
    class CheckpointTile : Entity
    {
        public bool Activated { get; protected set; }
        protected StaticPhysicsObject physicsObj;
        protected EntityManager entityManager;

        // Constructor
        public CheckpointTile(World world, Vector2 position, float width, float height, EntityManager entityManager)
            : base(world)
        {
            this.Width = ConvertUnits.ToSimUnits(width);
            this.Height = ConvertUnits.ToSimUnits(height);
            this.Activated = false;
            this.entityManager = entityManager;

            // Make the checkpoint in the physics world only as a sensor
            physicsObj = new StaticPhysicsObject(this, this.world, position, width, height);
            physicsObj.body.IsSensor = true;
            physicsObj.body.OnCollision += new OnCollisionEventHandler(onCollision);
        }

        protected bool onCollision(Fixture fix1, Fixture fix2, Contact contact)
        {
            if (fix2.CollisionCategories.HasFlag(GameConstants.PlayerCollisionCategory))
            {
                Activated = true;
                setPlayerSpawnPositionsHere();
                physicsObj.body.OnCollision -= new OnCollisionEventHandler(onCollision);
            }

            return false;
        }

        public override Vector2 Position
        {
            get
            {
                return physicsObj.Position;
            }
        }

        public override float Rotation
        {
            get
            {
                return physicsObj.body.Rotation;
            }
        }

        protected void setPlayerSpawnPositionsHere()
        {
            List<Player> players = entityManager.Players;
            foreach (Player player in players)
            {
                player.SpawnPosition = this.ScreenPosition;
            }
        }
    }
}
