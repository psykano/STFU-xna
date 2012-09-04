/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STFU
{
    /// <summary>
    /// An enemy event is used to communicate between the enemy's entity and representation.
    /// </summary>
    class EnemyEvent : EntityEvent
    {
        public bool changeBehavior;
        public bool flap;
    }
}
