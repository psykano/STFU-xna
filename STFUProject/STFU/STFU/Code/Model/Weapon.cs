/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using FarseerPhysics.Dynamics;

namespace STFU
{
    /// <summary>
    /// A weapon is used by a living entity to cause harm to another living entity.
    /// </summary>
    class Weapon : Entity
    {
        public bool Active { get; set; }
        public bool Shooting { get; set; }
        public int MaxBullets { get; protected set; }
        public Bullet LastBulletShot { get; protected set; }

        // Constructor
        public Weapon(World world)
            : base(world)
        {
            LastBulletShot = null;
        }

        public virtual int GetBulletCount()
        {
            return 0;
        }
    }

    class Weapon<TBullet> : Weapon, IDisposable
        where TBullet : Bullet
    {
        public List<TBullet> Bullets { get; set; }

        // Constructor
        public Weapon(World world)
            : base(world)
        {
        }

        public override int GetBulletCount()
        {
            int i = 0;
            foreach (TBullet bullet in this.Bullets)
            {
                if (bullet.Visible)
                {
                    i++;
                }
            }
            return i;
        }

        public void Dispose()
        {
            foreach (TBullet bullet in Bullets)
            {
                bullet.Dispose();
            }
        }
    }
}
