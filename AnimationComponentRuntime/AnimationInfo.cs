/*
 * AnimationInfo.cs
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
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;

namespace Xclna.Xna.Animation
{
   

    /// <summary>
    /// Contains information about an animation.
    /// </summary>
    public class AnimationInfo
    {
        private uint /*long*/ duration = 0;
        private double durationInSeconds = 0;
        private string animationName;

        private long startOffset = 0;

        /// <summary>
        /// this value is per default set past frame 0 which contains the bind pose. We never want to play the bind pose, but it is necessary for the animation to work at all.
        /// </summary>
        public long StartOffset
        {
            get { return startOffset; }
            set 
            { 
                startOffset = value;
                startOffsetInSeconds = new TimeSpan(startOffset).TotalSeconds;
            }
        }

        public bool OKToBlendAnimation = true;

        public bool HideHandAttachments = false;

        private double startOffsetInSeconds = 0d;
        
        // The bone animation tracks
        private AnimationChannelCollection boneAnimations;
        
        // Internal because it should only be created by the AnimationReader
        internal AnimationInfo(string animationName, AnimationChannelCollection 
            anims)
        {
            this.animationName = animationName;
            boneAnimations = anims;
            foreach (BoneKeyFrameCollection channel in anims)
            {
                if (channel.Duration > duration)
                {
                    duration = channel.Duration;
                    durationInSeconds = new TimeSpan(duration).TotalSeconds;
                }

                //LPE: note down the how long keyframe 0 lasts - it contains the bind pose, and we want to skip it when playing the animation:
              /*  if (channel[1].Time > StartOffset)
                {
                    StartOffset = channel[1].Time;
                }     */           
            }

           
        }

        private bool isGaitAnim = false;
        /// <summary>
        /// there are restrictions on what we can do with these anims...
        /// </summary>
        public bool IsGaitAnim
        {
            get
            {
                return isGaitAnim;
            }

            set
            {
                isGaitAnim = value;
            }
        }

        /// <summary>
        /// Gets a collection of channels that represent the bone animation
        /// tracks for this animation.
        /// </summary>
        public AnimationChannelCollection AnimationChannels
        { get { return boneAnimations; } }


        /// <summary>
        /// Gets a collection of bones that have tracks in this animation.
        /// </summary>
        public ReadOnlyCollection<string> AffectedBones
        { get { return boneAnimations.AffectedBones; } }

        /// <summary>
        /// Gets the total duration of this animation in ticks.
        /// 
        /// Maximum is 500s
        /// </summary>
        public uint /* long*/ Duration
        {
            get { return duration; }
        }

        public double DurationInSeconds
        {
            get { return durationInSeconds;  }
        }


        /// <summary>
        /// Gets the name of the animation.
        /// </summary>
        public string Name
        {
            get { return animationName; }
        }

        /// <summary>
        /// no longer used... gets set from Sim now.
        /// </summary>
        public float ActionPointInSeconds
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if the animation contains any tracks that affect the given
        /// bone.
        /// </summary>
        /// <param name="boneName">The bone to test for track information.</param>
        /// <returns>True if the animation contains any tracks that affect the given
        /// bone.</returns>
        public bool AffectsBone(string boneName)
        { return boneAnimations.AffectsBone(boneName); }
    }

    /// <summary>
    /// A collection of AnimationInfo objects.
    /// </summary>
    public class AnimationInfoCollection : SortedList<string, AnimationInfo>
    {
        // New instances should only be created by the AnimationReader
        internal AnimationInfoCollection()
        {
        }

        /// <summary>
        /// Gets a collection of animations stored in the model.
        /// </summary>
        /// <param name="model">The model that contains the animations.</param>
        /// <returns>The animations stored in the model.</returns>
        public static AnimationInfoCollection FromModel(Model model)
        {
            // Grab the tag that was set in the processor; this is a dictionary so that users can extend
            // the processor and pass their own data into the program without messing up the animation data
            Dictionary<string, object> modelTagData = (Dictionary<string, object>)model.Tag;
            if (modelTagData == null || !modelTagData.ContainsKey("Animations"))
            {
                return new AnimationInfoCollection();
            }
            else
            {
                AnimationInfoCollection animations = (AnimationInfoCollection)modelTagData["Animations"];
                return animations;
            }
        }

        /// <summary>
        /// Gets the AnimationInfo object at the given index.
        /// </summary>
        /// <param name="index">The index of the AnimationInfo object.</param>
        /// <returns>The AnimationInfo object at the given index.</returns>
        public AnimationInfo this[int index]
        {
            get
            {
                return this.Values[index];
            }
        }

    }




}
