using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WindowSystem
{
    /// <summary>
    /// Controls playback of an Animation.
    /// </summary>
    public class Animation2DPlayer
    {
        Animation2D animation;
        /// <summary>
        /// Gets the animation which is currently playing.
        /// </summary>
        public Animation2D Animation
        {
            get { return animation; }
        }
        
        private bool isEnded = true;

        public bool IsFinished
        {
            get { return isEnded; }
        }

        public delegate void AnimationEnded();
        public event AnimationEnded AnimationEndedEvent;

        public delegate void FrameChanged();
        public event FrameChanged FrameChangedEvent;

        int frameIndex;
        /// <summary>
        /// Gets the index of the current frame in the animation.
        /// </summary>
        public int FrameIndex
        {
            get { return frameIndex; }
        }
        

        /// <summary>
        /// The amount of time in seconds that the current frame has been shown for.
        /// </summary>
        private float time;

        private float totalTimeElapsed = 0f;


        public Animation2DPlayer() { }

        /// <summary>
        /// for MemoryFact - shows a frozen image (Update is not called)
        /// </summary>
        /// <param name="original"></param>
        public Animation2DPlayer(Animation2DPlayer original) 
        {
            isEnded = original.isEnded;
            time = original.time;
            totalTimeElapsed = original.totalTimeElapsed;
            frameIndex = original.frameIndex;

            // we can share the animation - not need to copy the cells.
            animation = original.animation; // new Animation2D(animation);
        
        }

        /// <summary>
        /// Gets a texture origin at the bottom center of each frame.
        /// </summary>
     /*   public Vector2 Origin
        {
            get { return new Vector2(Animation.FrameWidth / 2.0f, Animation.FrameHeight); }
        }*/

        /// <summary>
        /// Begins or continues playback of an animation.
        /// </summary>
        public void StartAnimation(Animation2D animation = null)
        {
            // If this animation is already running, do not restart it.
          /*  if (Animation == animation)
                return;*/

            // Start the new animation.

            if (animation != null)
            {
                this.animation = animation;
            }

            this.frameIndex = 0;
            this.time = 0.0f;
            this.totalTimeElapsed = 0f;

            isEnded = false;
        }

        public void StopAnimation()
        {
            isEnded = true; // works??
        }

        public float GetProgress()
        {
            float fraction = totalTimeElapsed / (animation.FrameTime * animation.FrameCount);

            return (fraction <= 1f ? fraction : 1f);

        }

        public bool RequiresUpdate
        {
            get
            {
                return Animation != null && !isEnded;
            }
        }

        public void Update(GameTime gameTime)
        {
            if (RequiresUpdate)
            {
                // Originally in Draw!!!! Does this cause skipping? Is slowdown better???
                // Process passing time.
                time += (float)gameTime.ElapsedGameTime.TotalSeconds;

                totalTimeElapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;

                while (time > Animation.FrameTime)
                {
                    time -= Animation.FrameTime;

                    if (FrameChangedEvent != null)
                    {
                        FrameChangedEvent.Invoke();
                    }

                    // Advance the frame index; looping or clamping as appropriate.
                    if (Animation.IsLooping)
                    {
                        frameIndex = (frameIndex + 1) % Animation.FrameCount;
                    }
                    else
                    {
                        frameIndex = Math.Min(frameIndex + 1, Animation.FrameCount - 1);

                        if (frameIndex == Animation.FrameCount - 1)
                        {
                            isEnded = true;
                            if (AnimationEndedEvent != null)
                            {
                                AnimationEndedEvent.Invoke();
                            }
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Advances the time position and draws the current frame of the animation.
        /// NOT used - see AnimatedImage.cs
        /// </summary>
     /*   public void Draw(SpriteBatch spriteBatch, Rectangle destination, Color? tintingColor) //Vector2 position) //, SpriteEffects spriteEffects)
        {
            if (Animation == null)
                return; //throw new NotSupportedException("No animation is currently playing.");

            

            // Calculate the source rectangle of the current frame.
            Rectangle source = GetFrame(); // animation.Cells[frameIndex].Frame; // new Rectangle(FrameIndex * Animation.Texture.Height, 0, Animation.Texture.Height, Animation.Texture.Height);


            Color color = GetCurrentColor(tintingColor);

            // Draw the current frame.
            spriteBatch.Draw(Animation.Texture, destination, source, color); //, 0.0f, Origin, 1.0f); //, spriteEffects, 0.0f);
        }*/



        public Color GetCurrentColor(Color? tintingColor)
        {
            Color color = animation.Cells[frameIndex].Color;

            if (animation.DoColorInterpolation)
            {
                if (animation.Cells.Count > frameIndex + 1)
                {
                    // interpolate the color between this frame and the next.
                    color = Color.Lerp(color, animation.Cells[frameIndex + 1].Color, time / Animation.FrameTime);
                }
            }

            if (tintingColor.HasValue)
            {
                color = new Color(color.ToVector4() * tintingColor.Value.ToVector4());
            }
            return color;
        }

        public Rectangle GetFrame()
        {
            return animation.Cells[frameIndex].Frame.Value; // will crash here if a sprite rectangle has not been set...
        }
    }
}
