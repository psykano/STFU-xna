/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace STFU
{
    /// <summary>
    /// An entity representation is the visual representation(sorry!) of the entity on the screen.
    /// </summary>
    abstract class EntityRepresentation
    {
        protected SpriteSheet spriteSheet;

        public EntityRepresentation()
        {
        }

        public virtual void LoadContent(ContentManager content)
        {
        }

        public virtual void Update(float dt)
        {
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
        }
    }
}
