/*
 * IAttachable.cs
 * Copyright (c) 2007 David Astle
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
using System.Xml.Serialization;

namespace Xclna.Xna.Animation
{
    /// <summary>
    /// an enum value that describes the point on the attached object (attachee) that is in contact with the attachor
    /// </summary>
    public enum AttacheePoint
    {
        Back,
        RightHand,
        LeftHand,
        Bottom
    }

    /// <summary>
    /// this class should be immutable since it survives save/load!
    /// </summary>
    public class AttachPoint
    {
        public string KeyName;

        public string Tag;

        /// <summary>
        /// it would be nice if this point was replaced by a bone in the model
        /// X: Left to right, when the model is facing 'south', and 0,0 is in the center!
        /// Y: upwards
        /// Z: North/south
        /// </summary>
        public Vector3? Translation;

        public Vector3 Rotation;

        public string BoneName;

        public bool IsHand;

        /// <summary>
        /// this is used to link AttachPoints with body parts as they are used by AttackTypes 
        /// </summary>
       // public string BodyPartName;

      /*  [XmlIgnore]
        public BonePose Bone;*/ // NEW - this class should be immutable since it survives save/load

        /// <summary>
        /// remove this! the animation condition will specify the attach point to use.
        /// </summary>
        public string AttachedAnimationName;

    }

    /// <summary>
    /// this interface is only necessary because the animation library does not know about Renderable, RenderAsModel etc.
    /// An object that can be attached to a BonePose.
    /// </summary>
    public interface IAttachable
    {
        

        /// <summary>
        /// The local transform of the object, before the transform of the attached bone is applied.
        /// </summary>
        Matrix LocalTransform { get;}
        /// <summary>
        /// The world space transform of the object as affected by the bone.
        /// </summary>
        Matrix CombinedTransform { get; set;}
      
        
        /// <summary>
        /// a reference to the ModelAnimator object
        /// </summary>
        ModelAnimator ModelAnimator { get; set; }

        /// <summary>
        /// we need this descriptor if the attachor entity animates in a way that needs a different transform to be applied for certain objects, attached to certain points on the attachor.
        /// </summary>
        AttacheePoint? AttacheePointValue { get; set;  }

        /// <summary>
        /// The bone with which the object is attached.
        /// </summary>
        BonePose AttacheeBone { get; set; }


        /// <summary>
        /// NEW: The bone the object is attached to, on a different entity.
        /// </summary>
        BonePose AttachorBone { get; set; }


        /// <summary>
        /// the Entity/MemoryFact we are attached to...
        /// </summary>
        object AttachedTo { get; set; }

        float Scale { get; }

        /// <summary>
        /// this reference is shared among attachables
        /// </summary>
        AttachPoint AttachorPoint { get; set; }

        /// <summary>
        /// The bone to which the object is attached.
        /// </summary>
    //    BonePose AttachorBone { get; set; }

    }
}
