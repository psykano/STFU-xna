/*
 * Copyright (c) 2012 Chris Johns
 */
// original idea from http://rabidlion.com/?p=31

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;

namespace STFU
{
    /// <summary>
    /// A physics object which doesn't move.
    /// </summary>
    class StaticPhysicsObject : PhysicsObject
    {
        public StaticPhysicsObject(Entity owner, World world, Vector2 position, float width, float height)
            : base(owner, world, position, width, height, 1f)
        {
            body.BodyType = BodyType.Static;
        }
    }
}