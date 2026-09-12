/*
 * AnimationWriter.cs
 * Copyright (c) 2006, 2007 David Astle, Michael Nikonov
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
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.Diagnostics;
#endregion

namespace Xclna.Xna.Animation.Content
{

    /// <summary>
    /// Writes ModelInfo data so it can be read into an object during runtime
    /// </summary>
    [ContentTypeWriter]
    internal sealed class AnimationWriter : ContentTypeWriter<AnimationContentDictionary>
    {
        
        /// <summary>
        /// Writes a ModelInfo object into XNB data
        /// </summary>
        /// <param name="output">The stream that contains the written data</param>
        /// <param name="value">The instance to be serialized</param>
        protected override void Write(ContentWriter output, AnimationContentDictionary value)
        {
            AnimationContentDictionary animations = value;
            output.Write(animations.Count);

            foreach (KeyValuePair<string, AnimationContent> k in animations)
            {
                output.Write(k.Key);

                output.Write(k.Value.Channels.Count);
                foreach (KeyValuePair<string, AnimationChannel> chan in k.Value.Channels)
                {
                    output.Write(chan.Key);
                    output.Write(chan.Value.Count);

                    foreach (AnimationKeyframe keyframe in chan.Value)
                    {
                        //output.Write(keyframe.Transform); 

                        //NEW: decompose at build time and write the vectors instead of the matrix:
                        Quaternion rotation;
                        Vector3 translation, scaling;
                        keyframe.Transform.Decompose(out scaling, out rotation, out translation);

                        output.Write(rotation);
                        output.Write(translation);
                        output.Write(scaling);

                        if (keyframe.Time.Ticks > uint.MaxValue)
                        {
                            throw new Exception("Animation is too long. Maximum duration is 500s");
                        }

                        output.Write(Convert.ToUInt32(keyframe.Time.Ticks)); // NEW - store an uint to save 32 bytes
                       // output.Write(keyframe.Time.Ticks);
                    }
                }


                float actionPoint = (float)k.Value.Duration.TotalSeconds; // the animation end is action point per default.
                if (k.Value.OpaqueData.ContainsKey("ActionPoint"))
                {
                    actionPoint = (float)k.Value.OpaqueData["ActionPoint"];
                }

                output.Write(actionPoint);


                long startOffset = 0;
                if (k.Value.OpaqueData.ContainsKey("StartOffset"))
                {

                    startOffset = (long)k.Value.OpaqueData["StartOffset"];

                    //  Debug.WriteLine("Startoffset {0} found for anim {1}", startOffset, k.Key);

                    //  context.Logger.LogImportantMessage("Startoffset {0} found for anim {1}", startOffset, k.Key);

                    // startOffset = (long)(startOffset / speed);
                }

                output.Write(startOffset);


                bool hideHandAttachments = false;
                if (k.Value.OpaqueData.ContainsKey("HideHandAttachments"))
                {
                    hideHandAttachments = (bool)k.Value.OpaqueData["HideHandAttachments"];

                }
                output.Write(hideHandAttachments);

                bool isGait = false;
                if (k.Value.OpaqueData.ContainsKey("IsGait"))
                {
                    isGait = (bool)k.Value.OpaqueData["IsGait"];

                }
                else
                {
                    string Name = k.Value.Name;
                    if (Name.Contains("walk") || Name.Contains("gait") || Name == "haul")
                    {
                        isGait = true;
                    }
                }

                output.Write(isGait);

            }
        }

        /// <summary>
        /// Returns the string that describes the reader used to convert the
        /// stream of data into a ModelInfo object
        /// </summary>
        /// <param name="targetPlatform">The current platform</param>
        /// <returns>The string that describes the reader used for a ModelInfo object</returns>
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            if (targetPlatform == TargetPlatform.Xbox360)
            {
                return "Xclna.Xna.Animation.Content.AnimationReader, "
                    + "Xclna.Xna.Animation360, "
                    + "Version=" + ContentUtil.VERSION + ", Culture=neutral, PublicKeyToken=null";
            }
            else
            {
                return "Xclna.Xna.Animation.Content.AnimationReader, "
                    + "Xclna.Xna.Animationx86, "
                    + "Version=" + ContentUtil.VERSION + ", Culture=neutral, PublicKeyToken=null";
            }


        }

        /// <summary>
        /// Returns the string that describes what type of object the stream
        /// will be converted into at runtime (ModelInf)
        /// </summary>
        /// <param name="targetPlatform">The current platform</param>
        /// <returns>The string that describes the run time type for the object written into
        /// the stream</returns>
        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            if (targetPlatform == TargetPlatform.Xbox360)
            {
                return "Xclna.Xna.Animation.AnimationInfoCollection, "
                    + "Xclna.Xna.Animation360, "
                    + "Version=" + ContentUtil.VERSION + ", Culture=neutral, PublicKeyToken=null";
            }
            else
            {
                return "Xclna.Xna.Animation.AnimationInfoCollection, "
                    + "Xclna.Xna.Animationx86, "
                    + "Version=" + ContentUtil.VERSION + ", Culture=neutral, PublicKeyToken=null";
            }



        }
    }

}
