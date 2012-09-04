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
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics.Joints;

namespace STFU
{
    /// <summary>
    /// A physics object which joins two other physics object.
    /// </summary>
    class CompositePhysicsObject
    {
        protected PhysicsObject physObA, physObB;
        protected RevoluteJoint revJoint;

        public CompositePhysicsObject(World world, PhysicsObject physObA, PhysicsObject physObB, Vector2 relativeJointPosition)
        {
            this.physObA = physObA;
            this.physObB = physObB;
            revJoint = JointFactory.CreateRevoluteJoint(world, physObA.fixture.Body, physObB.fixture.Body, ConvertUnits.ToSimUnits(relativeJointPosition));
            physObA.fixture.IgnoreCollisionWith(physObB.fixture);
            physObB.fixture.IgnoreCollisionWith(physObA.fixture);
        }
    }
}