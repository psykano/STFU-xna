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
        // The sprite sheet representing the collection of images used for animation
        SpriteSheet spriteSheet;


        // The name used to search for the animation
        string name;


        // The scale used to display the sprite strip
        public float Scale { get; set; }


        // The time since we last updated the frame (in milliseconds)
        int elapsedTime;


        // The time we display a frame until the next one
        public int frameTime;


        // The number of frames that the animation contains
        int frameCount;


        // The index of the current frame we are displaying
        private int currentFrame;
        public int CurrentFrame
        {
            get
            {
                return currentFrame;
            }
        }


        // The color of the frame we will be displaying
        private Color color;
        public Color Color
        {
            get 
            {
                return color;
            }
            set 
            {
                color = value;
            }
        }


        // The area of the image strip we want to display
        Rectangle sourceRect = new Rectangle();


        // The area where we want to display the image strip in the game
        Rectangle destinationRect = new Rectangle();


        Vector2 origin;


        // The state of the Animation
        public bool Active { get; set; }


        // Determines if the animation will keep playing or deactivate after one run
        public bool Looping { get; set; }


        // Rotation for the animation
        public float Rotation { get; set; }


        // Any sprite effects for the animation
        public SpriteEffects SpriteEffects { get; set; }


        public float LayerDepth { get; set; }


        public void Initialize(SpriteSheet spriteSheet, Vector2 position, string name, int frameTime, Color color, float scale, SpriteEffects spriteEffects, bool looping)
        {
            // Keep a local copy of the values passed in
            this.spriteSheet = spriteSheet;
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
            sourceRect = spriteSheet.Map[name + currentFrame];
            Width = sourceRect.Width;
            Height = sourceRect.Height;

            // Place the frame in the correct location on the screen
            destinationRect = new Rectangle((int)Position.X, (int)Position.Y, (int)(Width * Scale), (int)(Height * Scale));
        }


        // Draw the Animation Strip
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!isVisible())
                return;

            origin.X = Width * 0.5f;
            origin.Y = Height * 0.5f;
            spriteBatch.Draw(spriteSheet.Sheet, destinationRect, sourceRect, Color * this.Opacity, Rotation, origin, SpriteEffects, LayerDepth);
        }


        // Reset the animation
        public void Reset()
        {
            // Set the time to zero
            elapsedTime = 0;
            currentFrame = 1;
            Active = true;
        }

        // Set the opacity
        public float Opacity { get; set; }

        // Set the alpha
        public float Alpha
        {
            get
            {
                return (Color.A / 255f);
            }
            set
            {
                if (value > 1f)
                    value = 1f;
                else if (value < 0f)
                    value = 0f;
                color.A = (byte)(value * 255f);
            }
        }
    }
}
