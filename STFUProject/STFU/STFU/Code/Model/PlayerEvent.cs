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
    /// An player event is used to communicate between the player's entity and representation.
    /// </summary>
    class PlayerEvent : EntityEvent
    {
        public bool changeState;
        public bool shootWeapon;
    }
}
