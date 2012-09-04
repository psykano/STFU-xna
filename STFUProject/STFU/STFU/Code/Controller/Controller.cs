/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using Microsoft.Xna.Framework;

namespace STFU
{
    /// <summary>
    /// A controller is used to give commands to an entity.
    /// </summary>
    abstract class Controller
    {
        public virtual void Update(float dt)
        {
        }
    }
}
