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
    /// Holds all current game representations.
    /// </summary>
    class RepresentationManager
    {
        public ContentManager Content { get; protected set; }
        public MapRepresentation MapRepresentation { get; protected set; }
        protected List<EntityRepresentation> entityRepresentations;
        protected List<PlayerRepresentation> playerRepresentations;

        // Constructor
        public RepresentationManager(ContentManager content)
        {
            this.Content = content;
            entityRepresentations = new List<EntityRepresentation>();
            playerRepresentations = new List<PlayerRepresentation>();
        }

        public void Update(float dt)
        {
            // update the players as well
            foreach (PlayerRepresentation representation in playerRepresentations)
            {
                representation.Update(dt);
            }

            foreach (EntityRepresentation representation in entityRepresentations)
            {
                representation.Update(dt);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // draw the players as well
            foreach (PlayerRepresentation representation in playerRepresentations)
            {
                representation.Draw(spriteBatch);
            }

            foreach (EntityRepresentation representation in entityRepresentations)
            {
                representation.Draw(spriteBatch);
            }
        }

        public void Add(EntityRepresentation representation)
        {
            PlayerRepresentation playerRepresentation = representation as PlayerRepresentation;
            if (playerRepresentation != null)
                playerRepresentations.Add(playerRepresentation);
            else
            {
                entityRepresentations.Add(representation);

                if (this.MapRepresentation == null)
                {
                    MapRepresentation mapRepresentation = representation as MapRepresentation;
                    if (mapRepresentation != null)
                        this.MapRepresentation = mapRepresentation;
                }
            }
        }

        public void Remove(EntityRepresentation representation)
        {
            PlayerRepresentation playerRepresentation = representation as PlayerRepresentation;
            if (playerRepresentation != null)
                playerRepresentations.Remove(playerRepresentation);
            else
            {
                entityRepresentations.Remove(representation);
            }
        }

        public PlayerRepresentation GetPlayerRepresentationWithIndex(PlayerIndex playerIndex)
        {
            foreach (PlayerRepresentation representation in playerRepresentations)
            {
                if (representation.PlayerIndex == playerIndex)
                    return representation;
            }

            return null;
        }
    }
}
