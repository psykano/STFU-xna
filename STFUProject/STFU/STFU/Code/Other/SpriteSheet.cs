/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
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

        private Texture2D sheet;
        public Texture2D Sheet
        {
            get
            {
                return sheet;
            }
            set
            {
                // We don't want to load the texture more than once nor do we want to load a different texture
                if (sheet == null)
                {
                    Console.WriteLine("setting up textures from spritesheet");

                    sheet = value;

                    Color[] data = new Color[sheet.Width * sheet.Height];
                    sheet.GetData(data);

                    for (int i = 0; i < data.Length; i++)
                    {
                        if (data[i].A == 255)
                        {
                            data[i] = Color.White;
                        }
                    }

                    whiteSheet = new Texture2D(sheet.GraphicsDevice, sheet.Width, sheet.Height);
                    whiteSheet.SetData(data);
                }
                else
                {
                    throw new ArgumentException("textures already loaded from spritesheet");
                }
            }
        }
        private Texture2D whiteSheet;
        public Texture2D WhiteSheet
        {
            get
            {
                return whiteSheet;
            }
        }


        public Dictionary<string, Rectangle> Map { get; set; }

        public Rectangle this[int index]
        {
            get
            {
                // if nameList has not been set up do it now.
                if (nameList == null) nameList = Map.Keys.ToArray();

                return Map[nameList[index]];
            }
        }

        public int Count { get { return Map.Count; } private set { } }

        public SpriteSheet()
        {
        }
    }
}
