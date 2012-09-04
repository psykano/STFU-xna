/*
 * Copyright (c) 2012 Chris Johns
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace STFU
{
    /// <summary>
    /// A bat enemy representation is the visual representation(sorry!) of the bat enemy on the screen.
    /// </summary>
    class BatEnemyRepresentation : EnemyRepresentation<BatEnemy>
    {
        protected Animation idleAnimation;
        protected Animation flyAnimation;
        protected Animation diveAnimation;
        protected Animation hitAnimation;
        protected Frame eyesIdleFrame;
        protected Frame eyesBigFrame;
        protected Frame eyesSadFrame;

        private const string idleAnimationName = "batenemyidle";
        private const string flyAnimationName = "batenemyfly";
        private const string diveAnimationName = "batenemydive";
        private const string hitAnimationName = "batenemyhit";
        private const string eyesIdleFrameName = "bateyesidle";
        private const string eyesBigFrameName = "bateyesbig";
        private const string eyesSadFrameName = "bateyessad";

        // Constructor
        public BatEnemyRepresentation(BatEnemy enemy, EnemyEvent enemyEvent)
            : base(enemy, enemyEvent)
        {
        }

        public override void LoadContent(ContentManager content)
        {
            spriteSheet = new SpriteSheet();
            spriteSheet.Sheet = content.Load<Texture2D>("animationsheet");
            spriteSheet.Map = content.Load<Dictionary<string, Rectangle>>("animationsheetmap");

            spriteEffects = SpriteEffects.None;

            // Load animations:
            // idle animation
            int idleFrameTime = 40;
            idleAnimation = new Animation();
            idleAnimation.Initialize(spriteSheet, Vector2.Zero, idleAnimationName, idleFrameTime, this.color, 1, spriteEffects, false);
            // make this the current animation
            currentAnimation = idleAnimation;

            // fly animation
            int flyFrameTime = 30;
            flyAnimation = new Animation();
            flyAnimation.Initialize(spriteSheet, Vector2.Zero, flyAnimationName, flyFrameTime, this.color, 1, spriteEffects, false);

            // dive animation
            int diveFrameTime = 30;
            diveAnimation = new Animation();
            diveAnimation.Initialize(spriteSheet, Vector2.Zero, diveAnimationName, diveFrameTime, this.color, 1, spriteEffects, true);

            // hit animation
            int hitFrameTime = 30;
            hitAnimation = new Animation();
            hitAnimation.Initialize(spriteSheet, Vector2.Zero, hitAnimationName, hitFrameTime, this.color, 1, spriteEffects, true);

            // Load eyes:
            // idle eyes
            eyesIdleFrame = new Frame();
            eyesIdleFrame.Initialize(spriteSheet, Vector2.Zero, eyesIdleFrameName, Color.White, 1, spriteEffects);
            // make this the current eyes frame
            currentEyesFrame = eyesIdleFrame;

            // big eyes
            eyesBigFrame = new Frame();
            eyesBigFrame.Initialize(spriteSheet, Vector2.Zero, eyesBigFrameName, Color.White, 1, spriteEffects);

            // sad eyes
            eyesSadFrame = new Frame();
            eyesSadFrame.Initialize(spriteSheet, Vector2.Zero, eyesSadFrameName, Color.White, 1, spriteEffects);
        }

        protected override void updateWhenAlive(float dt)
        {
            // First, get the behavior
            if (enemy.Behavior.Compare(BatBehavior.Flying))
            {
                currentAnimation = flyAnimation;
            }
            else if (enemy.Behavior.Compare(BatBehavior.Diving))
            {
                currentAnimation = diveAnimation;
            }
            else if (enemy.Behavior.Compare(BatBehavior.Idle))
            {
                currentAnimation = idleAnimation;
            }
            else
            {
                currentAnimation = idleAnimation;
            }

            // Check whether to use big eyes
            if (enemy.Vision.EnemySeesPlayer || enemy.Behavior.Compare(BatBehavior.Diving))
            {
                currentEyesFrame = eyesBigFrame;
            }
            else
            {
                currentEyesFrame = eyesIdleFrame;
            }

            // Check if he's been hit
            if (enemy.Health.Hit)
            {
                currentAnimation = hitAnimation;
                currentEyesFrame = eyesSadFrame;
            }
            else
            {
                hitAnimation.Reset();
            }

            // Check whether we should be facing left
            if (enemy.FacingRight)
            {
                spriteEffects = SpriteEffects.None;
            }
            else
            {
                spriteEffects = SpriteEffects.FlipHorizontally;
            }

            // Next, check the events
            CheckEvents();

            // Only update the animation for the current state
            updateCurrentAnimation(dt);
        }

        protected override void updateWhenDead(float dt)
        {
            currentAnimation = idleAnimation;
            currentEyesFrame = eyesIdleFrame;
            updateCurrentAnimationWhenDead(dt);
        }

        protected override void CheckEvents()
        {
            base.CheckEvents();

            if (enemyEvent.flap)
            {
                enemyEvent.flap = false;

                // reset the current animation
                currentAnimation.Reset();
            }
        }

        protected override void adjustEyesOffset()
        {
            eyesOffset.Y = -5;
        }
    }
}
