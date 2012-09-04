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
using FarseerPhysics.Dynamics.Joints;

namespace STFU
{
    /// <summary>
    /// A springy physics object.
    /// </summary>
    class SpringPhysicsObject : CompositePhysicsObject
    {
        protected AngleJoint springJoint;

        public SpringPhysicsObject(World world, PhysicsObject physObA, PhysicsObject physObB, Vector2 relativeJointPosition, float springSoftness, float springBiasFactor)
            : base(world, physObA, physObB, relativeJointPosition)
        {
            springJoint = JointFactory.CreateAngleJoint(world, physObA.fixture.Body, physObB.fixture.Body);
            springJoint.TargetAngle = 0;
            springJoint.Softness = springSoftness;
            springJoint.BiasFactor = springBiasFactor;
        }
    }
}