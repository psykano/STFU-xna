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
    /// The current behavior of an enemy.
    /// The possible behaviors are enumerated and are defined in the enemy's class.
    /// </summary>
    class EnemyBehavior<T>
        where T : struct
    {
        public T Value { get; set; }
        private EnemyEvent enemyEvent;

        // constructor
        public EnemyBehavior(EnemyEvent enemyEvent)
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            this.enemyEvent = enemyEvent;
        }

        public bool ChangeTo(T newBehavior)
        {
            //if (newBehavior != this.Value)
            if (!Compare(newBehavior))
            {
                this.Value = newBehavior;
                enemyEvent.changeBehavior = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Compare(T x)
        {
            return EqualityComparer<T>.Default.Equals(Value, x);
        }
    }
}
