/*
 * AnimationController.cs
 * Copyright (c) 2006 David Astle
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;

namespace Xclna.Xna.Animation
{
   
    /// <summary>
    /// Controls an animation by advancing it's time and affecting
    /// bone transforms
    /// </summary>
    [DebuggerDisplay("{animation.animationName}|{uniqueID}")]
    public abstract class AnimationController : IAnimationController
    {

        private int uniqueID;
        private static int idCounter = 0;
        
        #region Member Variables

        // Contains the interpolated transforms for all bones in an animation
        // animation
        private AnimationInfo animation;

        // Multiplied by the time whenever the animation is advanced; determines
        // the playback speed of the animation
        private double speedFactor = 1.0;

        // The elapsed time in the animation, can not be greater than the
        // animation duration
        private long elapsedTime = 0;

        // Used as a buffer to store the total elapsed ticks every frame so that
        private long elapsed;


        /// <summary>
        /// Fired when the controller is not looping and the animation has ended.
        /// </summary>
        public event EventHandler AnimationEnded;

        // True if the animation is looping
        private bool isLooping = true;

        // True is control of animation progress is external to controller
        private bool isManual = false;

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new animation controller.
        /// </summary>    
        /// <param name="sourceAnimation">The source animation that the controller will use.
        /// This is stored in the ModelAnimator class.</param>
        public AnimationController(
            AnimationInfo sourceAnimation) //: base(game)
        {
            animation = sourceAnimation;

            // This is set so that the controller updates before the 
            // ModelAnimator by default
          //  base.UpdateOrder = 0;
         //   game.Components.Add(this);

            uniqueID = ++idCounter;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value that determines if the animation is looping.
        /// </summary>
        public bool IsLooping
        {
            get
            {
                return isLooping;
            }
            set
            {
                isManual = !value;//mutually exclusive
                isLooping = value;
            }
        }

        /// <summary>
        /// Gets manual boolean
        /// Sets bool and also mutexes the looping and speed-factor members of controller
        /// </summary>
        public bool IsManual
        {
            get
            {
                return isManual;
            }
            set
            {
                isLooping = !value;//mutually exclusive
                isManual = value;

                if (isManual)
                    speedFactor = 0L;// so the playback engine does not do any playbacking
                        //TODO MLo, zero speedFactor will bail at the top of Update, may be bad?
            }
        }

        /// <summary>
        /// Gets the total duration, in ticks, of the animation.
        /// WARNING: Does not regard speed or starting offset!!!
        /// </summary>
      /*  public long Duration
        {
            get { return animation.Duration; }
        }*/


       /* public double DurationInSeconds
        {
            get { return new TimeSpan(animation.Duration).TotalSeconds; }
        }*/

        /// <summary>
        /// Gets the source animation that this controller is using.
        /// </summary>
        public AnimationInfo AnimationInfo
        {
            get { return animation; }
        }

        /// <summary>
        /// Gets or sets the elapsed time for the animation.
        /// </summary>
        public long ElapsedTime
        {
            get { return elapsedTime; }
            set
            {
                if (animation.Name == "idleToSleep" && value < 1000000)
                {
#if DEBUG
                   // Console.WriteLine("idleToSleep elapsed time set: {0}", value);
#endif 

                }

                // Perform argument checking
                if (value < 0) // || value > animation.Duration)
                    throw new ArgumentOutOfRangeException("ElapsedTime",
                        "When setting the ElapsedTime for an animation, the value " +
                        " must be between 0 and the animation duration.");
                elapsedTime = value;
            }
        }

        public long ElapsedTimeMinusStartOffset
        {
            get { return ElapsedTime - animation.StartOffset; }           
        }


        public void SetAnimationAtStart(long? startOffsetTicksToAdd = null)
        {
            ElapsedTime = GetStartOfAnimation(startOffsetTicksToAdd);
        }

        public long GetStartOfAnimation(long? startOffsetTicksToAdd = null)
        {
            long startOffset = animation.StartOffset;
            if (startOffsetTicksToAdd.HasValue)
            {
                startOffset += startOffsetTicksToAdd.Value;
            }

            return startOffset;
        }

        public void SynchronizeAnim(AnimationController animToSynchronizeWith)
        {           
            ElapsedTime = animToSynchronizeWith.ElapsedTime; // startOffset;
        }

        public bool IsAnimationAtStart()
        {
            return ElapsedTime <= animation.StartOffset;
        }

        /// <summary>
        /// Gets or sets the value that is multiplied by the time when it is
        /// advanced to determine the playback speed of the animation.
        /// </summary>
        public double SpeedFactor
        {
            get { return speedFactor; }
            set {

                if (double.IsNaN(speedFactor))
                {
                    throw new Exception("error when setting speed factor (divide by zero?)");
                }

                

                speedFactor = value; 
            }
        }

        /// <summary>
        /// Duration minus StartOffset modified by Speedfactor
        /// when SpeedFactor is zero, the running time becomes infinity!
        /// </summary>
        /// <returns></returns>
      /*  public float GetRunningTimeInSeconds()
        {
            return (float)(animation.GetRunningTime() / speedFactor);
        }*/

        #endregion

        #region Methods

        /// <summary>
        /// Called when the current animation reaches the end.
        /// </summary>
        /// <param name="args">The event args.</param>
        protected virtual void OnAnimationEnded(EventArgs args)
        {
            if (AnimationEnded != null)
                AnimationEnded(this, args);
        }


        /// <summary>
        /// Implicitly thunks the controller into Manual mode, if it isn't already
        /// sets the controller's elapsedTime member to the scaled proportion of the animation's duration
        /// </summary>
        /// <param name="scalar"></param>
        public void UpdateAnimationTimeScalar(double scalar)
        {
            //just calling this method implicitly wants manual control, so we switch it to manual here
            IsManual = true; 

            //in manual mode, ignore speed factor
            //never trigger an OnAnimationEnded event
            //the animation never really 'ends'

            long value = animation.StartOffset + (long)(((double)animation.Duration - animation.StartOffset) * scalar);

            if (animation.Name == "idleToSleep" && value < 1000000)
            {
#if DEBUG
                Console.WriteLine("idleToSleep elapsed time set: {0}, scalar: {1}", value, scalar);
#endif

            }

            // we make sure that it is never possible to play the bind pose frame at the beginning!
          //  elapsedTime = (long)((double)animation.Duration * scalar);
            ElapsedTime = value;

            if (ElapsedTime > animation.Duration)//clamp  
                ElapsedTime = animation.Duration;

            if (ElapsedTime < 0L)//clamp  
                ElapsedTime = 0L;
        }


        /// <summary>
        /// Advances the current time in the animation.
        /// </summary>
        /// <param name="gameTime">Contains the time by which the animation will be advanced</param>
        /// 

#if false
        static private double timeStamp = 0;
        static private List<string> controllers = new List<string>();
#endif

        public void Update(GameTime gameTime)
        {
            // this gets called for each animation controller, active or not, on each model... 1000s potentially. Optimize a bit?
            // MLo: perfect opportunity for the sleepy update system. just make inactive animation controllers sleep forever
            if (speedFactor == 0d)
                return;

           
            //Trace.WriteLine("AnimationController.Update()" + this.AnimationInfo.Name  );


#if false            

            //if a controller gets an update, but it is neither the animationcontroller, nor the blend controller
            //then we need a failsafe here to shut it down



            if (gameTime.ElapsedGameTime.TotalMilliseconds != timeStamp)
            {
                //we must be starting a new game tick
                timeStamp = gameTime.ElapsedGameTime.TotalMilliseconds;
                controllers.Clear();
            }

            controllers.Add(this.AnimationInfo.Name);
            if (controllers.Count > 2)
            {
                //oops, we are updating three or more controllers at the same time... why!?
                controllers.Add("dang");
            }
#endif


            // Speedfactor * elapsed time since last call to update
            elapsed = (long)(speedFactor * gameTime.ElapsedGameTime.Ticks);
            // If the animation is looping
            if (isLooping)
            {
                ElapsedTime += elapsed;
                // If the elapsed time is greater than the duration,
                // raise the animation ended event and restart the animation
                if (ElapsedTime > animation.Duration)
                {
                    OnAnimationEnded(null);

                    // OLD:
//MLo fix?                     elapsedTime %= (animation.Duration + 1);
                    //elapsedTime %= (animation.Duration + 1 + animation.StartOffset);
//MLo fix?                     elapsedTime += animation.StartOffset;

                    //elapsedTime -= (animation.Duration-animation.StartOffset);

                    ElapsedTime = animation.StartOffset; // which is best?
                }
            }
            else // not looping!
            {
                if (speedFactor > 0)
                {
                    // don't do anything if the animation is at the end
                    if (ElapsedTime != animation.Duration)
                    {
                        // go forwards
                        if (elapsed != 0)
                        {
                            // Set the elapsed time to the duration if the animation ends and
                            // raise the AnimationEnded event
                            ElapsedTime = ElapsedTime + elapsed;
                            if (ElapsedTime >= animation.Duration || ElapsedTime < 0)
                            {
                                ElapsedTime = animation.Duration;
                                OnAnimationEnded(null);
                            }
                        }
                    }

                }
                else
                {
                    // go backwards
                    // don't do anything if the animation is at the beginning
                    if (ElapsedTime != 0)
                    {
                        // go forwards
                        if (elapsed != 0)
                        {
                            // Set the elapsed time to 0 if the animation ends and
                            // raise the AnimationEnded event
                            ElapsedTime = ElapsedTime + elapsed;
                            if (ElapsedTime >= animation.Duration || ElapsedTime < 0)
                            {
                                // OLD:
                                //elapsedTime = 0;
                                ElapsedTime = animation.StartOffset;
                                OnAnimationEnded(null);
                            }
                        }
                    }
                }


            }
        }


        /// <summary>
        /// Gets the current transform for the given BonePose object in the animation.
        /// This is only called when a bone pose is affected by the current animation.
        /// </summary>
        /// <param name="pose">The BonePose object querying for the current transform in
        /// the animation.</param>
        /// <returns>The current transform of the bone.</returns>
        public abstract Matrix GetCurrentBoneTransform(BonePose pose);
        

        /*
        public virtual Matrix GetCurrentBoneTransform(BonePose pose)
        {
            AnimationChannelCollection channels = animation.AnimationChannels;
            BoneKeyFrameCollection channel = channels[pose.Name];
            int boneIndex = channel.GetIndexByTime(ElapsedTime); // error here? elapsedTime < 0
            return channel[boneIndex].Transform;
        }*/



      /*  public virtual Matrix GetCurrentBoneTransformInterpolated(BonePose pose)
        {
            AnimationChannelCollection channels = animation.AnimationChannels;
            BoneKeyframeCollection channel = channels[pose.Name];
            int boneIndex = channel.GetIndexByTime(ElapsedTime); // error here? elapsedTime < 0
            
            Matrix transform1 = channel[boneIndex].Transform;

            Matrix transform2 = channel[boneIndex].Transform;


            return Util.SlerpMatrix(transform1, transform2, );
        }*/


        /// <summary>
        /// Returns true if the animation contains a track for the given BonePose.
        /// </summary>
        /// <param name="pose">The BonePose to test for track existence.</param>
        /// <returns>True if the animation contains a track for the given BonePose.</returns>
        public bool ContainsAnimationTrack(BonePose pose)
        {
            return animation.AnimationChannels.AffectsBone(pose.Name);
        }


        /// <summary>
        /// Fired when the tracks change so that different bones can be affected by the controller.
        /// </summary>
        /// <param name="e">The event args.</param>
        protected virtual void OnAnimationTracksChanged(EventArgs e)
        {
            if (AnimationTracksChanged != null)
                AnimationTracksChanged(this, e);
        }
        /// <summary>
        /// Fired when the animation tracks change and different bones are affected.
        /// </summary>
        public event EventHandler AnimationTracksChanged;

        #endregion

        #region IAnimationController Members


        #endregion
    }

}
