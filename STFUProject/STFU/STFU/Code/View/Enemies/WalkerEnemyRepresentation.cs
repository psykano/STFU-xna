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
    /// A walker enemy representation is the visual representation(sorry!) of the walker enemy on the screen.
    /// </summary>
    class WalkerEnemyRepresentation : EnemyRepresentation<WalkerEnemy>
    {
        protected Animation idleAnimation;
        protected Animation walkAnimation;
        protected Animation runAnimation;
        protected Animation fallAnimation;
        protected Animation hitAnimation;
        protected Frame eyesIdleFrame;
        protected Frame eyesForwardFrame;
        protected Frame eyesBigFrame;
        protected Frame eyesSadFrame;
        protected Frame eyesDownFrame;

        private const string idleAnimationName = "walkerenemyidle";
        private const string walkAnimationName = "walkerenemywalk";
        private const string runAnimationName = "walkerenemyrun";
        private const string fallAnimationName = "walkerenemyfall";
        private const string hitAnimationName = "walkerenemyhit";
        private const string eyesIdleFrameName = "walkereyesidle";
        private const string eyesForwardFrameName = "walkereyesforward";
        private const string eyesBigFrameName = "walkereyesbig";
        private const string eyesSadFrameName = "walkereyessad";
        private const string eyesDownFrameName = "walkereyesdown";

        // Constructor
        public WalkerEnemyRepresentation(WalkerEnemy enemy, EnemyEvent enemyEvent)
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
            int idleFrameTime = 30;
            idleAnimation = new Animation();
            idleAnimation.Initialize(spriteSheet, Vector2.Zero, idleAnimationName, idleFrameTime, this.color, 1, spriteEffects, false);
            idleAnimation.LayerDepth = enemyLayerDepth;
            // make this the current animation
            currentAnimation = idleAnimation;

            // walk animation
            int walkFrameTime = 40;
            walkAnimation = new Animation();
            walkAnimation.Initialize(spriteSheet, Vector2.Zero, walkAnimationName, walkFrameTime, this.color, 1, spriteEffects, true);
            walkAnimation.LayerDepth = enemyLayerDepth;

            // run animation
            int runFrameTime = 30;
            runAnimation = new Animation();
            runAnimation.Initialize(spriteSheet, Vector2.Zero, runAnimationName, runFrameTime, this.color, 1, spriteEffects, true);
            runAnimation.LayerDepth = enemyLayerDepth;

            // fall animation
            int fallFrameTime = 120;
            fallAnimation = new Animation();
            fallAnimation.Initialize(spriteSheet, Vector2.Zero, fallAnimationName, fallFrameTime, this.color, 1, spriteEffects, false);
            fallAnimation.LayerDepth = enemyLayerDepth;

            // hit animation
            int hitFrameTime = 30;
            hitAnimation = new Animation();
            hitAnimation.Initialize(spriteSheet, Vector2.Zero, hitAnimationName, hitFrameTime, this.color, 1, spriteEffects, true);
            hitAnimation.LayerDepth = enemyLayerDepth;

            // Load eyes:
            // idle eyes
            eyesIdleFrame = new Frame();
            eyesIdleFrame.Initialize(spriteSheet, Vector2.Zero, eyesIdleFrameName, Color.White, 1, spriteEffects);
            eyesIdleFrame.LayerDepth = enemyLayerDepth - 0.001f;
            // make this the current eyes frame
            currentEyesFrame = eyesIdleFrame;

            // forward eyes
            eyesForwardFrame = new Frame();
            eyesForwardFrame.Initialize(spriteSheet, Vector2.Zero, eyesForwardFrameName, Color.White, 1, spriteEffects);
            eyesForwardFrame.LayerDepth = enemyLayerDepth - 0.001f;

            // big eyes
            eyesBigFrame = new Frame();
            eyesBigFrame.Initialize(spriteSheet, Vector2.Zero, eyesBigFrameName, Color.White, 1, spriteEffects);
            eyesBigFrame.LayerDepth = enemyLayerDepth - 0.001f;

            // sad eyes
            eyesSadFrame = new Frame();
            eyesSadFrame.Initialize(spriteSheet, Vector2.Zero, eyesSadFrameName, Color.White, 1, spriteEffects);
            eyesSadFrame.LayerDepth = enemyLayerDepth - 0.001f;

            // down eyes
            eyesDownFrame = new Frame();
            eyesDownFrame.Initialize(spriteSheet, Vector2.Zero, eyesDownFrameName, Color.White, 1, spriteEffects);
            eyesDownFrame.LayerDepth = enemyLayerDepth - 0.001f;
        }

        protected override void updateWhenAlive(float dt)
        {
            // First, get the behavior
            if (enemy.Behavior.Compare(WalkerBehavior.Walking))
            {
                currentAnimation = walkAnimation;
            }
            else if (enemy.Behavior.Compare(WalkerBehavior.Running))
            {
                currentAnimation = runAnimation;
            }
            else if (enemy.Behavior.Compare(WalkerBehavior.Idle))
            {
                currentAnimation = idleAnimation;
            }
            else
            {
                currentAnimation = idleAnimation;
            }

            // Check which eyes to use
            if (enemy.Vision.EnemySeesPlayer || enemy.Behavior.Compare(WalkerBehavior.Running))
            {
                currentEyesFrame = eyesBigFrame;
            }
            else if (enemy.Behavior.Compare(WalkerBehavior.Walking))
            {
                currentEyesFrame = eyesForwardFrame;
            }
            else
            {
                currentEyesFrame = eyesIdleFrame;
            }

            // Check if he's falling
            if (enemy.IsFalling())
            {
                currentAnimation = fallAnimation;
                currentEyesFrame = eyesDownFrame;
            }
            else
            {
                fallAnimation.Reset();
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

        protected override void adjustEyesOffset()
        {
            eyesOffset.Y = -7;
            eyesOffset.X = 4;

            if (!enemy.FacingRight)
            {
                eyesOffset.X = -eyesOffset.X;
            }

            if (currentAnimation == walkAnimation)
            {
                if (currentAnimation.CurrentFrame >= 1 && currentAnimation.CurrentFrame <= 3)
                {
                    eyesOffset.Y -= 2;
                }
            }
            else if (currentAnimation == runAnimation)
            {
                if (currentAnimation.CurrentFrame >= 1 && currentAnimation.CurrentFrame <= 2)
                {
                    eyesOffset.Y -= 2;
                }
            }
        }
    }
}
