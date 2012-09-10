/*
 * Copyright (c) 2012 Chris Johns
 */
// original idea from http://rabidlion.com/?p=31

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Common;
using FarseerPhysics.Collision.Shapes;
using EasyConfig;

namespace STFU
{
    /// <summary>
    /// A physics character which is composed of a body and wheel, mainly for walking/running.
    /// The body remains upright and the wheel remains below the body. The two are connected by a joint.
    /// The body is a rectangle and the wheel is a circle.
    /// </summary>
    class CompositePhysicsCharacter : PhysicsCharacter
    {
        public Body wheelBody;
        public FixedAngleJoint fixedAngleJoint;
        public FixedAngleJoint brakeJoint;
        public RevoluteJoint motor;

        private float centerOffset;

        // Resources
        protected float brakeSpeed;
        protected float defaultBodyFriction;
        protected float defaultBodyRestitution;
        protected float defaultWheelFriction;
        protected float defaultWheelRestitution;

        private const string physicsSettings = "CompositePhysicsCharacter";
        public const float defaultMaxMotorTorque = 0.3f; //set this higher for some more juice

        public CompositePhysicsCharacter(Entity owner, World world, Vector2 position, float width, float height, float density, OnCollision onCollision, OnSeparation onSeparation)
            : base(owner, world, position, width, height, density, onCollision, onSeparation)
        {
            if (width > height)
            {
                throw new Exception("Error width > height: can't make character because wheel would stick out of body");
            }
        }

        protected override void SetUpPhysics(Entity owner, World world, Vector2 position, float width, float height, float density)
        {
            // Load resources
            ConfigFile configFile = PhysicsSystem.GetPhysicsConfigFile();
            brakeSpeed = configFile.SettingGroups[physicsSettings].Settings["brakeSpeed"].GetValueAsFloat();
            defaultBodyFriction = configFile.SettingGroups[physicsSettings].Settings["defaultBodyFriction"].GetValueAsFloat();
            defaultBodyRestitution = configFile.SettingGroups[physicsSettings].Settings["defaultBodyRestitution"].GetValueAsFloat();
            defaultWheelFriction = configFile.SettingGroups[physicsSettings].Settings["defaultWheelFriction"].GetValueAsFloat();
            defaultWheelRestitution = configFile.SettingGroups[physicsSettings].Settings["defaultWheelRestitution"].GetValueAsFloat();

            //Create a fixture with a body almost the size of the entire object
            //but with the bottom part cut off.
            float upperBodyHeight = height - (width / 2);

            body = BodyFactory.CreateRectangle(world, (float)ConvertUnits.ToSimUnits(width), (float)ConvertUnits.ToSimUnits(upperBodyHeight), density / 2);
            body.BodyType = BodyType.Dynamic;

            // tested a rounded rectangle.. didn't work so well. Should I revisit? ***
            //body = BodyFactory.CreateRoundedRectangle(world, ConvertUnits.ToSimUnits(width), ConvertUnits.ToSimUnits(upperBodyHeight), ConvertUnits.ToSimUnits(0.5f), ConvertUnits.ToSimUnits(0.5f), 2, density / 2, ConvertUnits.ToSimUnits(position));
            //body.BodyType = BodyType.Dynamic;

            // player needs low restitution so he won't bounce on the ground
            // and low friction to avoid the "edge catching" problem
            body.Restitution = defaultBodyRestitution;
            body.Friction = defaultBodyFriction;
            fixture = body.FixtureList[0];

            //also shift it up a tiny bit to keep the new object's center correct
            body.Position = ConvertUnits.ToSimUnits(position - (Vector2.UnitY * (width / 4)));
            centerOffset = position.Y - (float)ConvertUnits.ToDisplayUnits(body.Position.Y); //remember the offset from the center for drawing

            //Now let's make sure our upperbody is always facing up.
            fixedAngleJoint = JointFactory.CreateFixedAngleJoint(world, body);

            // Create our bottom part of the player which interacts with the terrain
            wheelBody = BodyFactory.CreateCircle(world, (float)ConvertUnits.ToSimUnits(width / 2), density / 2);

            //And position its center at the bottom of the upper body
            wheelBody.Position = body.Position + ConvertUnits.ToSimUnits(Vector2.UnitY * (upperBodyHeight / 2));
            wheelBody.BodyType = BodyType.Dynamic;
            wheelBody.Restitution = defaultWheelRestitution;
            //Set the friction higher for fast stopping/starting
            //or set it lower to make the character slip.
            wheelBody.Friction = defaultWheelFriction;

            //These two bodies together are width wide and height high :)
            //So lets connect them together
            motor = JointFactory.CreateRevoluteJoint(world, body, wheelBody, Vector2.Zero);
            motor.MotorEnabled = true;
            motor.MaxMotorTorque = defaultMaxMotorTorque;
            motor.MotorSpeed = 0;

            //Make sure the two fixtures don't collide with each other
            wheelBody.IgnoreCollisionWith(body);
            body.IgnoreCollisionWith(wheelBody);

            // make sure the bodies point to the owners
            wheelBody.UserData = owner;
            body.UserData = owner;

            // so that we don't slip off the edge
            brakeJoint = JointFactory.CreateFixedAngleJoint(world, wheelBody);

            // Set contact handlers
            body.OnCollision += new OnCollisionEventHandler(onCollisionWithBody);
            body.OnSeparation += new OnSeparationEventHandler(onSeparationWithBody);
            wheelBody.OnCollision += new OnCollisionEventHandler(onCollisionWithBottom);
            wheelBody.OnSeparation += new OnSeparationEventHandler(onSeparationWithBottom);
        }

        public override Category CollisionCategory
        {
            set
            {
                body.CollisionCategories = value;
                wheelBody.CollisionCategories = value;
                base.CollisionCategory = value;
            }
        }

        public override bool ContainsThisBody(Body _body)
        {
            if (_body == body || _body == wheelBody)
                return true;
            return false;
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            if (OnGround)
            {
                if (Math.Abs(motor.JointSpeed) < brakeSpeed)
                {
                    if (!brakeJoint.Enabled)
                    {
                        brakeJoint.TargetAngle = motor.JointAngle;
                        brakeJoint.Enabled = true;
                    }
                }
            }
            else
            {
                brakeJoint.Enabled = false;
            }

            if (CharOnHead)
            {
                body.Friction = 0.4f;
            }
            else
            {
                body.Friction = defaultBodyFriction;
            }
        }

        protected bool checkEdgeCatching()
        {
            if (Math.Abs(body.LinearVelocity.Y) < 0.02f)
                return true;
            return false;
        }

        protected void adjustForEdgeCatching()
        {
            // To stop edge catching if it occurs
            if (!OnGround && OnWall() && checkEdgeCatching())
            {
                // old method
                //body.LinearVelocity = new Vector2(body.LinearVelocity.X, 1);

                // new method
                float velocityX = body.LinearVelocity.X;
                if (OnLeftWall)
                {
                    velocityX += 0.1f;
                }
                else if (OnRightWall)
                {
                    velocityX -= 0.1f;
                }

                body.LinearVelocity = new Vector2(velocityX, 1f);
            }
        }

        public override Vector2 Position
        {
            get
            {
                return ConvertUnits.ToSimUnits((ConvertUnits.ToDisplayUnits(body.Position) + Vector2.UnitY * centerOffset)); // should fix this up ***
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            if (wheelBody == null)
                return;
            wheelBody.Dispose();
        }

        public override void Disable()
        {
            base.Disable();
            wheelBody.Enabled = false;
        }

        // Movement:
        protected virtual void moveOnGround()
        {
            brakeJoint.Enabled = false;
        }

        protected virtual void moveInAir()
        {
            motor.MotorSpeed = 0;
        }
    }
}
