/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace STFU
{
    /// <summary>
    /// A player representation is the visual representation(sorry!) of the player on the screen.
    /// </summary>
    class PlayerRepresentation : LivingEntityRepresentation
    {
        public Camera2D Cam { get; set; }
        private Vector2 camOffset;
        public Vector2 CamOffset
        {
            get
            {
                return camOffset;
            }
            set
            {
                camOffset = value;
            }
        }
        public PlayerIndex PlayerIndex
        {
            get
            {
                return player.PlayerIndex;
            }
        }

        protected Player player;
        protected Color color;
        protected SpriteEffects spriteEffects;
        protected Animation currentAnimation;
        protected Animation idleAnimation;
        protected Animation idleFrozenAnimation;
        protected Animation idleShootAnimation;
        protected Animation runAnimation;
        protected Animation dashAnimation;
        protected Animation jumpAnimation;
        protected Animation fallAnimation;
        protected WeaponRepresentation<PlayerWeapon> gunRepresentation;
        protected WeaponRepresentation<PlayerWeapon> swordRepresentation;
        protected Frame eyesFrame;
        protected TrailParticleEmitter dashTrailEmitter;

        private PlayerEvent playerEvent;
        private const string idleAnimationName = "playeridle";
        private const string idleShootAnimationName = "playeridleshoot";
        private const string runAnimationName = "playerrun";
        private const string dashAnimationName = "playerdash";
        private const string jumpAnimationName = "playerjump";
        private const string fallAnimationName = "playerfall";
        private const string gunAnimationName = "playergun";
        private const string gunShotAnimationName = "playergunshot";
        private const string swordAnimationName = "playersword";
        private const string swordShotAnimationName = "playerswordslash";
        private const string eyesFrameName = "playereyes";
        private const float maxCamOffsetX = 60;
        private const float camScrollRateX = 60;
        private Vector2 eyesOffset;
        private const float eyesOffsetX = 2;
        private const float eyesOffsetY = -6;
        private const float gunOffsetX = 0;
        private const float gunOffsetY = 0;
        private const float swordOffsetX = 4;
        private const float swordOffsetY = -6;

        // Constructor
        public PlayerRepresentation(Player player, PlayerEvent playerEvent)
            : base(player)
        {
            this.player = player;
            this.playerEvent = playerEvent;
            this.color = Color.White;
            dashTrailEmitter = new TrailParticleEmitter();
        }

        public void SetUpCamera(Viewport viewport, float levelMapWidth, float levelMapHeight)
        {
            // set up camera
            this.Cam = new Camera2D(viewport);
            float halfViewWidth = 320; // viewport.Width * 0.5f
            float halfViewHeight = 180; // viewport.Height * 0.5f
            this.Cam.MinPosition = new Vector2(halfViewWidth, halfViewHeight);
            this.Cam.MaxPosition = new Vector2(levelMapWidth - halfViewWidth, levelMapHeight - halfViewHeight);
            this.CamOffset = Vector2.Zero;
        }

        public override void LoadContent(ContentManager content)
        {
            // Choose color
            if (player.PlayerIndex == PlayerIndex.One) // Green
                this.color = new Color(0, 255, 0);
            else if (player.PlayerIndex == PlayerIndex.Two) // Blue
                this.color = new Color(0, 255, 255);
            else if (player.PlayerIndex == PlayerIndex.Three) // Yellow
                this.color = new Color(255, 255, 0);
            else if (player.PlayerIndex == PlayerIndex.Four) // Pink
                this.color = new Color(255, 200, 255);

            spriteSheet = new SpriteSheet();
            spriteSheet.Sheet = content.Load<Texture2D>("animationsheet");
            spriteSheet.Map = content.Load<Dictionary<string, Rectangle>>("animationsheetmap");

            spriteEffects = SpriteEffects.None;
            float spritePaddingScale = 1f;

            // Load animations:
            // idle animation
            int idleFrameTime = 120;
            idleAnimation = new Animation();
            idleAnimation.Initialize(spriteSheet, Vector2.Zero, idleAnimationName, idleFrameTime, this.color, spritePaddingScale, spriteEffects, true);
            // make this the current animation
            currentAnimation = idleAnimation;

            // idle frozen animation
            int idleFrozenFrameTime = 0;
            idleFrozenAnimation = new Animation();
            idleFrozenAnimation.Initialize(spriteSheet, Vector2.Zero, idleAnimationName, idleFrozenFrameTime, this.color, spritePaddingScale, spriteEffects, false);

            // idle shoot animation
            int idleShootFrameTime = 30;
            idleShootAnimation = new Animation();
            idleShootAnimation.Initialize(spriteSheet, Vector2.Zero, idleShootAnimationName, idleShootFrameTime, this.color, spritePaddingScale, spriteEffects, false);

            // running animation
            int runFrameTime = 25;
            runAnimation = new Animation();
            runAnimation.Initialize(spriteSheet, Vector2.Zero, runAnimationName, runFrameTime, this.color, spritePaddingScale, spriteEffects, true);

            // dash animation
            int dashFrameTime = 0;
            dashAnimation = new Animation();
            dashAnimation.Initialize(spriteSheet, Vector2.Zero, dashAnimationName, dashFrameTime, this.color, spritePaddingScale, spriteEffects, false);

            // jumping animation
            int jumpFrameTime = 120;
            jumpAnimation = new Animation();
            jumpAnimation.Initialize(spriteSheet, Vector2.Zero, jumpAnimationName, jumpFrameTime, this.color, spritePaddingScale, spriteEffects, false);

            // falling animation
            int fallFrameTime = 120;
            fallAnimation = new Animation();
            fallAnimation.Initialize(spriteSheet, Vector2.Zero, fallAnimationName, fallFrameTime, this.color, spritePaddingScale, spriteEffects, false);

            // gun weapon animations
            int gunFrameTime = 30;
            int gunShotFrameTime = 30;
            gunRepresentation = new WeaponRepresentation<PlayerWeapon>(player, player.Gun, new Vector2(gunOffsetX, gunOffsetY));
            gunRepresentation.InitializeAnimation(spriteSheet, Vector2.Zero, gunAnimationName, gunFrameTime, this.color, 1, spriteEffects, false);
            gunRepresentation.WeaponAnimation.LayerDepth = 0.05f;
            gunRepresentation.InitializeBullets(spriteSheet, Vector2.Zero, gunShotAnimationName, gunShotFrameTime, this.color, 1, spriteEffects, true);
            gunRepresentation.BulletsLayerDepth(0.05f);

            // sword weapon animations
            int swordFrameTime = 30;
            int swordShotFrameTime = 30;
            swordRepresentation = new WeaponRepresentation<PlayerWeapon>(player, player.Sword, new Vector2(swordOffsetX, swordOffsetY));
            swordRepresentation.InitializeAnimation(spriteSheet, Vector2.Zero, swordAnimationName, swordFrameTime, this.color, 1, spriteEffects, false);
            swordRepresentation.WeaponAnimation.LayerDepth = 0.05f;
            swordRepresentation.InitializeBullets(spriteSheet, Vector2.Zero, swordShotAnimationName, swordShotFrameTime, this.color, 1, spriteEffects, true);
            swordRepresentation.BulletsLayerDepth(0.05f);

            // eyes frame
            eyesFrame = new Frame();
            eyesFrame.Initialize(spriteSheet, Vector2.Zero, eyesFrameName, Color.White, spritePaddingScale, spriteEffects);

            // dash trail particle emitter
            dashTrailEmitter.Initialize(5, 0.05f, 0.2f);
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            if (!player.Health.Dead)
            {
                // Check whether we should be facing left
                if (player.FacingRight)
                {
                    spriteEffects = SpriteEffects.None;

                    camOffset.X += camScrollRateX * dt;
                    if (this.CamOffset.X > maxCamOffsetX)
                    {
                        camOffset.X = maxCamOffsetX;
                    }
                }
                else
                {
                    spriteEffects = SpriteEffects.FlipHorizontally;

                    camOffset.X -= camScrollRateX * dt;
                    if (this.CamOffset.X < -maxCamOffsetX)
                    {
                        camOffset.X = -maxCamOffsetX;
                    }
                }

                // Update camera
                this.Cam.Update(dt);
                this.Cam.Position = player.ScreenPosition + this.CamOffset;

                // Update running speed
                if (Math.Abs(player.PhysicsContainer.Object.body.LinearVelocity.X) < 3)
                {
                    runAnimation.FrameTime = 50;
                }
                else
                {
                    runAnimation.FrameTime = 25;
                }

                // First, get the state
                if (player.State == State.Idle)
                {
                    if (player.Health.Hit)
                    {
                        currentAnimation = idleFrozenAnimation;
                    }
                    else if (player.Dashing)
                    {
                        currentAnimation = dashAnimation;
                    }
                    else if (player.WeaponActive())
                    {
                        if (player.WeaponActiveAndVertical())
                        {
                            currentAnimation = idleFrozenAnimation;
                        }
                        else
                        {
                            currentAnimation = idleShootAnimation;
                        }
                    }
                    else
                    {
                        currentAnimation = idleAnimation;
                    }
                }
                else if (player.State == State.Running)
                {
                    currentAnimation = runAnimation;
                }
                else if (player.State == State.Jumping)
                {
                    currentAnimation = jumpAnimation;
                }
                else if (player.State == State.Falling)
                {
                    currentAnimation = fallAnimation;
                }

                // Next, check the events
                CheckEvents();

                // Only update the animation for the current state
                currentAnimation.Position = player.ScreenPosition + shakePositionOffset;
                currentAnimation.Rotation = player.ScreenRotation;
                currentAnimation.Update(dt);
                currentAnimation.SpriteEffects = spriteEffects;

                // Update the eyes
                adjustEyesOffset();
                eyesFrame.Position = player.ScreenPosition + eyesOffset + shakePositionOffset;
                eyesFrame.Rotation = player.ScreenRotation;
                eyesFrame.SpriteEffects = spriteEffects;
            }

            // Update the weapons, regardless whether the player is dead
            gunRepresentation.Update(dt);
            swordRepresentation.Update(dt);

            // Update the dashing trail
            if (player.Dashing && !player.Health.Dead)
            {
                dashTrailEmitter.Run();
            }
            else
            {
                dashTrailEmitter.Stop();
            }
            dashTrailEmitter.DisplayEntity = currentAnimation;
            dashTrailEmitter.Update(dt);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            // Draw the player
            if (!blink && !player.Health.Dead)
            {
                float playerLayerDepth = 0.19f;

                // Draw the current animation
                currentAnimation.Draw(spriteBatch);
                currentAnimation.LayerDepth = playerLayerDepth;

                // Draw the eyes
                eyesFrame.Draw(spriteBatch);
                eyesFrame.LayerDepth = currentAnimation.LayerDepth - 0.001f;

                // Draw the weapons if the player isn't hit
                if (!player.Health.Hit)
                {
                    gunRepresentation.Draw(spriteBatch);
                    swordRepresentation.Draw(spriteBatch);
                }
            }

            // Draw the bullets
            gunRepresentation.DrawBullets(spriteBatch);
            swordRepresentation.DrawBullets(spriteBatch);

            // Draw the dash trails
            dashTrailEmitter.Draw(spriteBatch);
        }

        private void adjustEyesOffset()
        {
            eyesOffset.X = eyesOffsetX;

            if (currentAnimation == idleShootAnimation)
            {
                eyesOffset.X += 2;
            }

            if (!player.FacingRight)
            {
                eyesOffset.X = -eyesOffset.X;
            }

            eyesOffset.Y = eyesOffsetY;
            if (currentAnimation == idleAnimation)
            {
                if (currentAnimation.CurrentFrame == 1)
                {
                    //

                    //
                }
                else if (currentAnimation.CurrentFrame == 2)
                {
                    //eyesOffset.Y += 1;

                    //
                }
                else if (currentAnimation.CurrentFrame == 3)
                {
                    //eyesOffset.Y += 2;

                    eyesOffset.Y += 2;
                }
                else if (currentAnimation.CurrentFrame == 4)
                {
                    //eyesOffset.Y += 1;

                    eyesOffset.Y += 2;
                }
            }
            else if (currentAnimation == runAnimation)
            {
                if (currentAnimation.CurrentFrame >= 1 && currentAnimation.CurrentFrame <= 2)
                {
                    eyesOffset.Y -= 2;
                }
            }
            else if (currentAnimation == dashAnimation)
            {
                eyesOffset.Y += 2;
            }
        }

        private void CheckEvents()
        {
            if (playerEvent.changeState)
            {
                playerEvent.changeState = false;

                // reset the current animation
                currentAnimation.Reset();
            }

            if (playerEvent.shootWeapon)
            {
                playerEvent.shootWeapon = false;

                if (player.Gun.Shooting)
                {
                    // it was the gun
                    gunRepresentation.AddBullet();
                }
                else if (player.Sword.Shooting)
                {
                    // it was the sword
                    swordRepresentation.AddBullet();
                }
            }
        }
    }
}
