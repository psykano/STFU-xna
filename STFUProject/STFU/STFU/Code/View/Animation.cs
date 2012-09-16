/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace STFU
{
    /// <summary>
    /// Uses multiple frames in sequence with a predetermined delay between frames.
    /// </summary>
    class Animation : DisplayEntity
    {
        // The name used to search for the animation
        string name;

        // The time since we last updated the frame (in milliseconds)
        int elapsedTime;

        // The time we display a frame until the next one
        private int frameTime;
        public int FrameTime
        {
            get
            {
                return frameTime;
            }
            set
            {
                frameTime = value;
            }
        }

        // The number of frames that the animation contains
        private int frameCount;

        // The index of the current frame we are displaying
        private int currentFrame;
        public int CurrentFrame
        {
            get
            {
                return currentFrame;
            }
        }

        // The state of the Animation
        public bool Active { get; set; }

        // Determines if the animation will keep playing or deactivate after one run
        public bool Looping { get; set; }

        public void Initialize(SpriteSheet spriteSheet, Vector2 position, string name, int frameTime, Color color, float scale, SpriteEffects spriteEffects, bool looping)
        {
            // Keep a local copy of the values passed in
            SpriteSheet = spriteSheet;
            Position = position;
            this.name = name;
            this.frameTime = frameTime;
            this.Color = color;
            this.Opacity = 1f;
            this.Scale = scale;
            SpriteEffects = spriteEffects;
            this.Looping = looping;
            LayerDepth = 0.2f; // default layer depth


            // Set the time to zero
            elapsedTime = 0;
            currentFrame = 1;


            // Figure out the number of frames
            frameCount = 0;
            while (spriteSheet.Map.ContainsKey(name + (frameCount + 1)))
            {
                frameCount++;
            }
            if (frameCount < 1)
            {
                throw new System.ArgumentException("Animation not found", name);
            }
            

            // Set the Animation to active by default
            Active = true;
        }

        public void Update(float dt)
        {
            // Do not update the game if we are not active
            if (Active != false)
            {
                if (frameCount > 1)
                {
                    // Update the elapsed time
                    elapsedTime += (int)(dt * 1000f); // since we need to convert it to milliseconds


                    // If the elapsed time is larger than the frame time
                    // we need to switch frames
                    if (elapsedTime > frameTime)
                    {
                        // Move to the next frame
                        currentFrame++;


                        // If the currentFrame is equal to frameCount reset currentFrame to zero
                        if (currentFrame > frameCount)
                        {
                            // If we are not looping deactivate the animation
                            if (Looping == false)
                            {
                                currentFrame = frameCount;
                                Active = false;
                            }
                            else // otherwise, go back to the first frame
                            {
                                currentFrame = 1;
                            }
                        }


                        // Reset the elapsed time to zero
                        elapsedTime = 0;
                    }
                }
            }


            /*
            // Grab the correct frame in the image strip by multiplying the currentFrame index by the frame width
            sourceRect = new Rectangle(currentFrame * Width, 0, Width, Height);


            // Grab the correct frame in the image strip by multiplying the currentFrame index by the frame width
            destinationRect = new Rectangle((int)Position.X - (int)(Width * scale) / 2,
            (int)Position.Y - (int)(Height * scale) / 2,
            (int)(Width * scale),
            (int)(Height * scale));
             */

            // Grab the correct frame in the sprite sheet
            SourceRect = SpriteSheet.Map[name + currentFrame];
            Width = SourceRect.Width;
            Height = SourceRect.Height;
            origin.X = Width * 0.5f;
            origin.Y = Height * 0.5f;

            // Place the frame in the correct location on the screen
            DestinationRect = new Rectangle((int)Position.X, (int)Position.Y, (int)(Width * Scale), (int)(Height * Scale));
        }


        // Draw the Animation Strip
        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawWithColor(spriteBatch, Color);
        }

        public override void DrawWithColor(SpriteBatch spriteBatch, Color drawColor)
        {
            if (!isVisible())
                return;

            spriteBatch.Draw(SpriteSheet.Sheet, DestinationRect, SourceRect, drawColor * Opacity, Rotation, origin, SpriteEffects, LayerDepth);
        }


        // Reset the animation
        public void Reset()
        {
            // Set the time to zero
            elapsedTime = 0;
            currentFrame = 1;
            Active = true;
        }
    }
}
