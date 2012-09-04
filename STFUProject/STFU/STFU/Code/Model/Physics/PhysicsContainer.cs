/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STFU
{
    class PhysicsContainer<TPhysicsObject>
        where TPhysicsObject : PhysicsObject
    {
        public TPhysicsObject Object { get; set; }

        public PhysicsContainer()
        {
            //
        }
    }
}
