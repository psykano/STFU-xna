/*
 * Copyright (c) 2012 Chris Johns
 */
// original idea from http://rabidlion.com/?p=31

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics.Contacts;

namespace STFU
{
    public delegate bool OnCollision(Fixture fixtureA, Fixture fixtureB, Contact contact);
    public delegate void OnSeparation(Fixture fixtureA, Fixture fixtureB);

    /// <summary>
    /// An physics character represents an element in the physics world.
    /// </summary>
    abstract class PhysicsCharacter : PhysicsObject
    {
        public bool OnCeiling { get; protected set; }
        public bool OnRightWall { get; protected set; }
        public bool OnLeftWall { get; protected set; }
        public bool CharOnHead { get; protected set; }

        protected OnCollision onCollision;
        protected OnSeparation onSeparation;
        private int onGroundInt;
        protected bool onGroundDelay;
        protected bool onGroundRay;
        public bool OnGround
        {
            get
            {
                if (onGroundInt > 0 || onGroundRay || onGroundDelay)
                    return true;
                return false;
            }
        }
        protected bool onGroundPhysics
        {
            get
            {
                if (onGroundInt > 0 || onGroundRay)
                    return true;
                return false;
            }
        }

        public bool RayCastIncludesDeath { get; set; }

        public PhysicsCharacter(Entity owner, World world, Vector2 position, float width, float height, float density, OnCollision onCollision, OnSeparation onSeparation)
            : base(owner, world, position, width, height, density)
        {
            OnRightWall = false;
            OnLeftWall = false;
            CharOnHead = false;
            onGroundInt = 0;
            onGroundDelay = false;
            onGroundRay = false;
            RayCastIncludesDeath = true;

            SetUpContactDelegates(onCollision, onSeparation);
        }

        public void SetUpContactDelegates(OnCollision onCollision, OnSeparation onSeparation)
        {
            this.onCollision = onCollision;
            this.onSeparation = onSeparation;
        }

        public virtual void Update(float dt)
        {
            performChecks();
        }

        protected virtual void performChecks()
        {
        }

        public bool CheckIfNotMovingVertically()
        {
            if (Math.Abs(body.LinearVelocity.Y) < 0.1f)
            {
                return true;
            }

            return false;
        }

        public bool CheckIfStill()
        {
            if (Math.Abs(body.LinearVelocity.X) < 0.1f && Math.Abs(body.LinearVelocity.Y) < 0.1f)
            {
                return true;
            }

            return false;
        }

        /*
        protected void checkForGround()
        {
            if (onGroundInt > 0)
                return;

            // check center
            Vector2 rayStart = new Vector2(Position.X, Position.Y); // at the center of the character
            Vector2 rayEnd = rayStart + new Vector2(0, extents.Y + ConvertUnits.ToSimUnits(2.5f)); // check 2 pixels down below the character

            bool rayHitSomething = false;
            world.RayCast((fixture, point, normal, fraction) =>
            {
                if (fixture == null || collidedWithSelf(fixture.Body) || fixture.IsSensor)
                {
                    return -1;
                }
                else
                {
                    if (collidedWithOneWayPlatform(null, fixture))
                    {
                        if (body.LinearVelocity.Y > -1)
                        {
                            rayHitSomething = true;
                            return 0;
                        }
                    }
                    else if (collidedWithGroundFixture(fixture))
                    {
                        rayHitSomething = true;
                        return 0;
                    }
                    return -1;
                }
            }, rayStart, rayEnd);

            onGroundRay = rayHitSomething;
        }
         */

        protected void checkForGround()
        {
            if (onGroundInt > 0)
                return;

            // check center
            Vector2 rayStart = new Vector2(Position.X, Position.Y); // at the center of the character
            Vector2 rayEnd = rayStart + new Vector2(0, extents.Y + ConvertUnits.ToSimUnits(2.1f)); // check 2 pixels down below the character
            //Vector2 widthOffset = new Vector2(extents.X + ConvertUnits.ToSimUnits(0.5f), 0); // the 0.5f helps to fix a bug where the character gets caught perpetually falling in between two characters
            Vector2 widthOffset = new Vector2(extents.X, 0);

            onGroundRay = rayCastForGround(rayStart, rayEnd);

            /* We don't need these so long as we're checking for collisions with the ground as well
            // check left
            if (!onGroundRay)
            {
                rayStart -= widthOffset;
                rayEnd -= widthOffset;
                onGroundRay = rayCastForGround(rayStart, rayEnd);
            }

            // check right
            if (!onGroundRay)
            {
                rayStart += widthOffset * 2f;
                rayEnd += widthOffset * 2f;
                onGroundRay = rayCastForGround(rayStart, rayEnd);
            }
             */
        }
        private bool rayCastForGround(Vector2 rayStart, Vector2 rayEnd)
        {
            bool rayHitSomething = false;
            world.RayCast((fixture, point, normal, fraction) =>
            {
                if (fixture == null || collidedWithSelf(fixture.Body) || fixture.IsSensor)
                {
                    return -1;
                }
                else
                {
                    if (collidedWithOneWayPlatform(null, fixture))
                    {
                        if (body.LinearVelocity.Y > -1)
                        {
                            rayHitSomething = true;
                            return 0;
                        }
                    }
                    else if (collidedWithGroundFixture(fixture))
                    {
                        rayHitSomething = true;
                        return 0;
                    }
                    return -1;
                }
            }, rayStart, rayEnd);

            return rayHitSomething;
        }

        protected void checkForCeiling(bool includePlayers, bool includeEnemies)
        {
            // check center
            Vector2 rayStart = new Vector2(Position.X, Position.Y); // at the center of the character
            Vector2 rayEnd = rayStart + new Vector2(0, -extents.Y - ConvertUnits.ToSimUnits(2)); // check 2 pixels up above the character
            Vector2 widthOffset = new Vector2(extents.X, 0);

            // don't check center
            //OnCeiling = rayCast(rayStart, rayEnd, true, includePlayers, includeEnemies, false);

            // check left
            //if (!OnCeiling)
            //{
                rayStart -= widthOffset;
                rayEnd -= widthOffset;
                OnCeiling = rayCast(rayStart, rayEnd, true, includePlayers, includeEnemies, false);
            //}

            // check right
            if (!OnCeiling)
            {
                rayStart += widthOffset * 2f;
                rayEnd += widthOffset * 2f;
                OnCeiling = rayCast(rayStart, rayEnd, true, includePlayers, includeEnemies, false);
            }

            // for debugging
            /*
            if (OnCeiling)
            {
                Console.Out.WriteLine("on ceiling");
            }
            else
            {
                Console.Out.WriteLine("NOT on ceiling");
            }
             */
        }

        protected void checkForWall(bool includePlayers, bool includeEnemies)
        {
            float heightOffset = extents.Y;

            // check right
            Vector2 rayStart = new Vector2(Position.X, Position.Y + heightOffset); // at the bottom of the character
            Vector2 rayEnd = rayStart + new Vector2(extents.X + ConvertUnits.ToSimUnits(3), 0); // check 2.5 pixels right. 3 instead of 2 for better wall sliding
            
            // don't check center
            //OnRightWall = rayCast(rayStart, rayEnd, true, includePlayers, includeEnemies, false);

            //if (!OnRightWall)
            //{
                rayStart.Y -= heightOffset;
                rayEnd.Y -= heightOffset;
                OnRightWall = rayCast(rayStart, rayEnd, true, includePlayers, includeEnemies, false);
            //}

            if (!OnRightWall)
            {
                rayStart.Y -= heightOffset;
                rayEnd.Y -= heightOffset;
                OnRightWall = rayCast(rayStart, rayEnd, true, includePlayers, includeEnemies, false);
            }

            // check left
            rayStart = new Vector2(Position.X, Position.Y + heightOffset); // at the bottom of the player sprite
            rayEnd = rayStart - new Vector2(extents.X + ConvertUnits.ToSimUnits(3), 0); // check 3 pixels left. 3 instead of 2 for better wall sliding
           
            // don't check center
            //OnLeftWall = rayCast(rayStart, rayEnd, true, includePlayers, includeEnemies, false);

            //if (!OnLeftWall)
            //{
                rayStart.Y -= heightOffset;
                rayEnd.Y -= heightOffset;
                OnLeftWall = rayCast(rayStart, rayEnd, true, includePlayers, includeEnemies, false);
            //}

            if (!OnLeftWall)
            {
                rayStart.Y -= heightOffset;
                rayEnd.Y -= heightOffset;
                OnLeftWall = rayCast(rayStart, rayEnd, true, includePlayers, includeEnemies, false);
            }

            // for debugging
            /*
            if (OnRightWall | OnLeftWall)
            {
                Console.Out.WriteLine("on wall");
            }
            else
            {
                Console.Out.WriteLine("NOT on wall");
            }
             */
        }

        protected void checkForCharOnHead(bool includePlayers, bool includeEnemies)
        {
            // check center
            Vector2 rayStart = new Vector2(Position.X, Position.Y); // at the center of the character
            Vector2 rayEnd = rayStart + new Vector2(0, -(extents.Y + ConvertUnits.ToSimUnits(4))); // check 4 pixels above the player
            Vector2 widthOffset = new Vector2(extents.X, 0);
            CharOnHead = rayCast(rayStart, rayEnd, false, includePlayers, includeEnemies, false);

            // check left
            if (!CharOnHead)
            {
                rayStart -= widthOffset;
                rayEnd -= widthOffset;
                CharOnHead = rayCast(rayStart, rayEnd, false, includePlayers, includeEnemies, false);
            }

            // check right
            if (!CharOnHead)
            {
                rayStart += widthOffset * 2f;
                rayEnd += widthOffset * 2f;
                CharOnHead = rayCast(rayStart, rayEnd, false, includePlayers, includeEnemies, false);
            }

            // for debugging
            /*
            if (CharOnHead)
            {
                Console.Out.WriteLine("char on head");
            }
            else
            {
                Console.Out.WriteLine("NO char on head");
            }
             */
        }

        protected bool rayCast(Vector2 rayStart, Vector2 rayEnd, bool includeGround, bool includePlayers, bool includeEnemies, bool includeBullets)
        {
            bool rayHitSomething = false;
            world.RayCast((fixture, point, normal, fraction) =>
            {
                if (fixture == null || collidedWithSelf(fixture.Body) || fixture.IsSensor)
                {
                    return -1;
                }
                else
                {
                    if (!RayCastIncludesDeath && fixture.CollisionCategories.HasFlag(GameConstants.DeathCollisionCategory))
                    {
                        return -1;
                    }
                    else if (!includeGround && collidedWithGround(fixture.CollisionCategories))
                    {
                        // it's the ground so ignore it
                        return -1;
                    }
                    else if (!includePlayers && collidedWithPlayer(fixture.CollisionCategories))
                    {
                        // it's a player so ignore it
                        return -1;
                    }
                    else if (!includeEnemies && collidedWithEnemy(fixture.CollisionCategories))
                    {
                        // it's an enemy so ignore it
                        return -1;
                    }
                    else if (!includeBullets && collidedWithBullet(fixture.CollisionCategories))
                    {
                        // it's a bullet so ignore it
                        return -1;
                    }
                    else
                    {
                        rayHitSomething = true;
                        return 0;
                    }
                }
            }, rayStart, rayEnd);

            return rayHitSomething;
        }

        protected bool rayCastSight(Vector2 rayStart, Vector2 rayEnd, bool includeGround, bool includePlayers, bool includeEnemies, bool includeBullets)
        {
            bool rayHitSomething = false;

            world.RayCast((fixture, point, normal, fraction) =>
            {
                if (fixture == null || collidedWithSelf(fixture.Body) || fixture.IsSensor)
                {
                    return -1;
                }
                // this goes here instead of below to fix a bug I don't know why is happening otherwise
                else if (!includeBullets && collidedWithBullet(fixture.CollisionCategories))
                {
                    // it's a bullet so ignore it
                    return -1;
                }
                else
                {
                    rayHitSomething = false;

                    if (!includeGround && collidedWithGround(fixture.CollisionCategories))
                    {
                        // it's the ground so don't include it
                        //return fraction;
                    }
                    else if (!includePlayers && collidedWithPlayer(fixture.CollisionCategories))
                    {
                        // it's a player so don't include it
                        //return fraction;
                    }
                    else if (!includeEnemies && collidedWithEnemy(fixture.CollisionCategories))
                    {
                        // it's an enemy so don't include it
                        //return fraction;
                    }
                    else
                    {
                        rayHitSomething = true;
                    }
                }
                return fraction;
            }, rayStart, rayEnd);

            return rayHitSomething;
        }

        protected bool collidedWithSelf(Body _body)
        {
            if (ContainsThisBody(_body))
                return true;
            return false;
        }

        protected bool collidedWithPlayer(Category category)
        {
            if (category.HasFlag(GameConstants.PlayerCollisionCategory))
                return true;
            return false;
        }

        protected bool collidedWithBullet(Category category)
        {
            if (category.HasFlag(GameConstants.PlayerBulletCollisionCategory))
                return true;
            return false;
        }

        protected bool collidedWithEnemy(Category category)
        {
            if (category.HasFlag(GameConstants.EnemyCollisionCategory))
                return true;
            return false;
        }

        protected bool collidedWithGround(Category category)
        {
            if (category.HasFlag(GameConstants.GroundCollisionCategory))
                return true;
            return false;
        }

        public bool onWall()
        {
            if (OnLeftWall || OnRightWall)
                return true;
            else
                return false;
        }

        protected bool collidedWithOneWayPlatform(Fixture fix1, Fixture fix2)
        {
            if (fix2.CollisionCategories.HasFlag(GameConstants.PlatformCollisionCategory))
            {
                return true;
            }

            return false;
        }

        public bool OnCollisionWithOneWayPlatform(Fixture fix1, Fixture fix2)
        {
            // as a precaution to help make sure we don't bounce off the platform by being stuck halfway under
            Entity platform = fix2.Body.UserData as Entity;
            if (fix1.Body.Position.Y > platform.Position.Y) // since fix1.Body.Position is _not_ necessarily the same as this.Position
                return false;

            if (fix1.Body.LinearVelocity.Y < -0.01f) // 0.01 to account for error and so stationary bodies get picked up by moving platforms
            {
                return false;
            }

            return true;
        }

        protected bool onCollisionWithBody(Fixture fix1, Fixture fix2, Contact contact)
        {
            if (fix2.IsSensor)
            {
                return false;
            }

            if (collidedWithOneWayPlatform(fix1, fix2))
            {
                return false;
            }

            return onCollision(fix1, fix2, contact);
        }

        protected bool onCollisionWithBottom(Fixture fix1, Fixture fix2, Contact contact)
        {
            if (fix2.IsSensor)
            {
                return false;
            }

            if (collidedWithOneWayPlatform(fix1, fix2))
            {
                if (OnCollisionWithOneWayPlatform(fix1, fix2))
                {
                    onGroundInt++;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if (collidedWithGroundFixture(fix2))
            {
                Vector2 normal = contact.Manifold.LocalNormal;

                /* could do this other than the below but wasn't working too well
                if (Math.Abs(normal.Y) <= 0.1f)
                    return false;
                 */
                if (Math.Abs(normal.X) >= 0.9f)
                    return false;

                onGroundInt++;
            }

            return onCollision(fix1, fix2, contact);
        }

        protected void onSeparationWithBody(Fixture fix1, Fixture fix2)
        {
            onSeparation(fix1, fix2);
        }

        protected void onSeparationWithBottom(Fixture fix1, Fixture fix2)
        {
            if (collidedWithGroundFixture(fix2))
            {
                onGroundInt--;
            }

            onSeparation(fix1, fix2);
        }

        protected virtual bool collidedWithGroundFixture(Fixture fix2)
        {
            if (fix2.CollisionCategories.HasFlag(GameConstants.GroundCollisionCategory))
            {
                return true;
            }

            return false;
        }
    }
}