/*
 * AnimationReader.cs
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

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Xclna.Xna.Animation.Content
{


    // Reads in processed animation info written in the pipeline
    internal sealed class AnimationReader : ContentTypeReader<AnimationInfoCollection>
    {
        /// <summary> 
        /// Reads in an XNB stream and converts it to a ModelInfo object
        /// </summary>
        /// <param name="input">The stream from which the data will be read</param>
        /// <param name="existingInstance">Not used</param>
        /// <returns>The unserialized ModelAnimationCollection object</returns>
        protected override AnimationInfoCollection Read(ContentReader input, AnimationInfoCollection existingInstance)
        {
            AnimationInfoCollection dict = new AnimationInfoCollection();
            int numAnimations = input.ReadInt32();
            

            // Read all the animations
            for (int i = 0; i < numAnimations; i++)
            {
                string animationName = input.ReadString();

                int numBoneAnimations = input.ReadInt32();

                List<BoneKeyFrameCollection> animList = new List<BoneKeyFrameCollection>();

                // Read all the animation tracks for the current animation
                for (int j = 0; j < numBoneAnimations; j++)
                {
                    string boneName = input.ReadString();
                    int numKeyFrames = input.ReadInt32();
                    List<BoneKeyFrame> boneAnimationList = new List<BoneKeyFrame>();

                    // Read all the keyframes for the current animation track
                    for (int k = 0; k < numKeyFrames; k++)
                    {                       
                        BoneKeyFrame frame = new BoneKeyFrame(
                           input.ReadQuaternion(),
                           input.ReadVector3(),
                           input.ReadVector3(),
                           input.ReadUInt32()); 
                       

                        /*
                        BoneKeyFrame frame = new BoneKeyFrame(
                            input.ReadMatrix(),                           
                            input.ReadInt64());*/

                        boneAnimationList.Add(frame);
                    }

                    BoneKeyFrameCollection boneAnimation = new BoneKeyFrameCollection(boneName, boneAnimationList);

                    animList.Add(boneAnimation);
                }

                AnimationInfo anim = new AnimationInfo(animationName, new AnimationChannelCollection(animList));

                // input.ReadString();
                anim.ActionPointInSeconds = (float)input.ReadSingle();
                /* anim.Speed = (float)input.ReadSingle();*/

                anim.StartOffset = input.ReadInt64();

             //   anim.OKToBlendAnimation = input.ReadBoolean();

                anim.HideHandAttachments = input.ReadBoolean();

                anim.IsGaitAnim = input.ReadBoolean();

                dict.Add(animationName, anim);
            }
            return dict;
        }
    }
}