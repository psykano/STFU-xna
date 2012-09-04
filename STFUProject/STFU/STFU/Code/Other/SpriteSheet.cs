/*
 * Copyright (c) 2012 Chris Johns
 */

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace STFU
{
    /// <summary>
    /// A sprite sheet contains frames (animated or not) for sprites.
    /// </summary>
    public class SpriteSheet
    {
        private string[] nameList;

        public Texture2D Sheet { get; set; }
        public Dictionary<string, Rectangle> Map { get; set; }

        public Rectangle this[int index]
        {
            get
            {
                // if nameList has not been set up do it now.
                if (nameList == null) nameList = Map.Keys.ToArray();

                return Map[nameList[index]];
            }
            private set { /* nothing to do here */ }
        }

        public int Count { get { return Map.Count; } private set { } }

        public SpriteSheet() { }
    }
}
