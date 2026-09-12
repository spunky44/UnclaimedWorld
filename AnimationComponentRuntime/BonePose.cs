/*
 * BonePose.cs
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

namespace Xclna.Xna.Animation
{

    /// <summary>
    /// A collection of BonePose objects that represent the bone transforms of a model
    /// as affected by animations.
    /// </summary>
    public class BonePoseCollection 
        : System.Collections.ObjectModel.ReadOnlyCollection<BonePose>
    {
        // A dictionary for quick access to bone poses based on bone name
        private Dictionary<string, BonePose> boneDict 
            = new Dictionary<string, BonePose>();

        // This class should not be externally instantiated
        internal BonePoseCollection(IList<BonePose> anims)
            :
            base(anims)
        {
            for (int i = 0; i < anims.Count; i++)
            {
                string boneName = anims[i].Name;
                if (boneName != null && boneName != "" && !boneDict.ContainsKey(boneName))
                {
                    boneDict.Add(boneName, anims[i]);
                }
            }
        }

        // Creates a set of bonepose objects from a skeleton
        internal static BonePoseCollection FromModelBoneCollection(
            ModelBoneCollection bones)
        {
            BonePose[] anims = new BonePose[bones.Count];
            for (int i = 0; i < bones.Count; i++)
            {
                if (bones[i].Parent==null)
                {
                    BonePose ba = new BonePose(
                        bones[i],
                        bones,
                        anims);

                }
            }

            return new BonePoseCollection(anims);
        }

        /// <summary>
        /// Computes the absolute transforms for the collection and copies
        /// the values.
        /// </summary>
        /// <param name="transforms">The array into which the transforms will be 
        /// copied.</param>
        public void CopyAbsoluteTransformsTo(Matrix[] transforms)
        {
            for (int i = 0; i < transforms.Length; i++)
            {
                if (i > 0) // not root
                {
                    // This works because the skeleton is always flattened;
                    // the parent index is always lower than the child index.
                    Matrix curTransform = this[i].GetCurrentTransform();
                    Matrix parentTransform = transforms[this[i].Parent.Index];
                    Vector3 currentTranslation = curTransform.Translation;
                    Matrix parentRotation = Matrix.CreateFromQuaternion(
                        Quaternion.CreateFromRotationMatrix(parentTransform));
                    Matrix currentRotation = Matrix.CreateFromQuaternion(
                        Quaternion.CreateFromRotationMatrix(curTransform));

                    currentTranslation = Vector3.Transform(currentTranslation,
                        parentRotation);
                    currentTranslation += parentTransform.Translation;
                    currentTranslation = parentTransform.Translation + curTransform.Translation; // WTF
                    
                    transforms[i] = currentRotation * parentRotation;
                    transforms[i] = curTransform * parentTransform; // WTF

   
                }
                else
                {
                    transforms[i] = this[i].GetCurrentTransform();
                }
            }

           
        }


        /// <summary>
        /// Gets a BonePose object.
        /// </summary>
        /// <param name="boneName">The name of the bone for which the BonePose 
        /// will be returned.</param>
        /// <returns>The BonePose associated with the bone name.</returns>
        public BonePose this[string boneName]
        {
            get 
            {
                if (boneName == null)
                    return null;

                BonePose ret;
                if (boneDict.TryGetValue(boneName, out ret))
                    return ret;

                return null; 
            }
        }

    }

    /// <summary>
    /// Represents the current pose of a model bone.
    /// </summary>
    public class BonePose
    {
        
      //  private Matrix defaultMatrix;
        // Buffers for interpolation when blending
        private static Matrix currentMatrixBuffer;
        private int index;

        // The bone name
        private string name;
        private BonePose parent = null;

        /// <summary>
        /// a reference to the track on RenderAsModel
        /// </summary>
        private AnimationTrack mainAnimationTrack;


        /// <summary>
        /// Gets or sets the current animation that affects this bone.  If null,
        /// then DefaultTransform will be used for this bone's transform.
        /// </summary>
        public AnimationTrack MainAnimationTrack
        {
            get
            {
                return mainAnimationTrack;
            }
        }


 
        /// <summary>
        /// we can run 2 gaits simultaneously
        /// NOT USED currently!
        /// </summary>
        private AnimationTrack secondGaitTrack;

                     

        private BonePoseCollection children;

        /// <summary>
        /// true when blending in, false when blending out, null when not blending
        /// </summary>
        private bool? blendToSpecialTransform = null;

        private bool useSpecialTransform = false;
        public bool UseSpecialTransform
        {
            get
            {
                return useSpecialTransform;
            }
            set
            {
                if (value != useSpecialTransform)
                {

                    if (value == true)
                    {
                        blendToSpecialTransform = true;
                      
                        if (specialTransformBlendingProgress == null)
                        {
                            specialTransformBlendingProgress = 0f; // start blending from the start
                        }
                    }
                    else
                    {
                        // blend out again:
                        blendToSpecialTransform = false;

                        if (specialTransformBlendingProgress == null)
                        {
                            specialTransformBlendingProgress = 1f; // start blending back again
                        }
                        // else continue from where we are. We were interrupted while blending.
                    }

                    useSpecialTransform = value;

                }
            }
        }

        // True if the current animation contains a track for this bone
        private bool doesMainAnimAffectBone = false;
        // True if the current blend animation contains a track for this bone
        private bool doesMainBlendAnimAffectBone = false;

        // True if the current animation contains a track for this bone
        private bool doesSecondGaitAnimAffectBone = false;
        // True if the current blend animation contains a track for this bone
        private bool doesSecondGaitBlendAnimAffectBone = false;


        private List<AnimationTrack> additionalAnimationTracks = new List<AnimationTrack>();
        private List<bool> additionalAnimAffectsBone = new List<bool>();
        private List<bool> additionalBlendAnimAffectsBone = new List<bool>();


        // Internal creation
        internal BonePose(ModelBone bone, 
            ModelBoneCollection bones,
            BonePose[] anims)
        {
            // Set the values according to the bone
            index = bone.Index;
            name = bone.Name;
            DefaultTransform = bone.Transform;

            if (bone.Parent != null)
                parent = anims[bone.Parent.Index];

            anims[index] = this;

            // Recurse on children
            List<BonePose> childList = new List<BonePose>();
            foreach (ModelBone child in bone.Children)
            {
                BonePose newChild = new BonePose(
                    bones[child.Index],
                    bones,
                    anims);

                childList.Add(newChild);
            }
            children = new BonePoseCollection(childList);
        }

        /// <summary>
        /// Gets the immediate children of the current bone.
        /// </summary>
        public BonePoseCollection Children
        {
            get { return children; }
        }

        // Finds the hierarchy for which this bone is the root
        private void FindHierarchy(List<BonePose> poses)
        {
            poses.Add(this);
            foreach (BonePose child in children)
            {
                child.FindHierarchy(poses);
            }
        }

        /// <summary>
        /// Finds a collection of bones that represents the tree of BonePoses with
        /// the current BonePose as the root.
        /// </summary>
        public BonePoseCollection GetHierarchy()
        {
 
                List<BonePose> poses = new List<BonePose>();
                FindHierarchy(poses);
                return new BonePoseCollection(poses);
            
        }


        /// <summary>
        /// Gets the bone's parent.
        /// </summary>
        public BonePose Parent
        {
            get { return parent; }
        }

        /// <summary>
        /// Gets the index of the bone.
        /// </summary>
        public int Index
        {
            get { return index; }
        }

        /// <summary>
        /// Gets the name of the bone.
        /// </summary>
        public string Name
        {
            get { return name; }
        }


        public void SetAnimationTrack(AnimationTrack track)
        {
            switch (track.TrackTypeValue)
            {
                case AnimationTrack.TrackType.Main:
                    mainAnimationTrack = track;
                    break;

                case AnimationTrack.TrackType.ExtraGait:
                    secondGaitTrack = track;
                    break;

                case AnimationTrack.TrackType.Additional:
                    additionalAnimationTracks.Add(track);

                    additionalAnimAffectsBone.Add(false);
                    additionalBlendAnimAffectsBone.Add(false);

                    break;

            }

        }




        /// <summary>
        /// sets a flag so the bone pose does not have to be checked against the animation every frame.
        /// </summary>
        /// <param name="track"></param>
        public void UpdateAnimControllerStatus(AnimationTrack track)
        {
            switch (track.TrackTypeValue)
            {
                case AnimationTrack.TrackType.Main:
                    if (track.currentController != null)
                    {
                        doesMainAnimAffectBone = track.currentController.AnimationInfo.AffectsBone(this.name);
                    }
                    else
                    {
                        doesMainAnimAffectBone = false;
                    }

                    if (track.controllerBeingBlendedTo != null)
                    {
                        doesMainBlendAnimAffectBone = track.controllerBeingBlendedTo.AnimationInfo.AffectsBone(this.name);
                    }
                    else
                    {
                        doesMainBlendAnimAffectBone = false;
                    }

                    break;

                case AnimationTrack.TrackType.ExtraGait:
                    if (track.currentController != null)
                    {
                        doesSecondGaitAnimAffectBone = track.currentController.AnimationInfo.AffectsBone(this.name);
                    }
                    else
                    {
                        doesSecondGaitAnimAffectBone = false;
                    }

                    if (track.controllerBeingBlendedTo != null)
                    {
                        doesSecondGaitBlendAnimAffectBone = track.controllerBeingBlendedTo.AnimationInfo.AffectsBone(this.name);
          
                    }
                    else
                    {
                        doesSecondGaitBlendAnimAffectBone = false;             
                    }

                    break;
                case AnimationTrack.TrackType.Additional:
                    
                    if (track.currentController != null)
                    {
                        additionalAnimAffectsBone[track.Index] = track.currentController.AnimationInfo.AffectsBone(this.name);
                    }
                    else
                    {
                        additionalAnimAffectsBone[track.Index] = false;
                    }

                    if (track.controllerBeingBlendedTo != null)
                    {
                        additionalBlendAnimAffectsBone[track.Index] = track.controllerBeingBlendedTo.AnimationInfo.AffectsBone(this.name);
          
                    }
                    else
                    {
                        additionalBlendAnimAffectsBone[track.Index] = false;
                    }

                    break;
            }

        }

        
        
        /// <summary>
        /// Represents the matrix used by the BonePose when it is not affected by
        /// an animation or when the animation does not contain a track for the bone.
        /// 
        /// 
        /// this transform gets changed when head turns and twists are applied. the real 'default' is saved in ModelBone.Transform
        /// </summary>
        public Matrix DefaultTransform;
      /*  {
            get { return defaultMatrix; }
            set { defaultMatrix = value; }
        }*/


        private float? specialTransformBlendingProgress = null;

        /// <summary>
        /// Calculates the current transform, based on the animations, for the bone
        /// represented by the BonePose object.
        /// </summary>
        public Matrix GetCurrentTransform()
        {
            // The Special Transform is used in head turning and overrides anim poses on some bones. 
            // we need to blend into and out from the anim pose to avoid sudden changes.
            if (useSpecialTransform == true // use the transform instead of the anim pose
                && blendToSpecialTransform == null) // not blending
            {
                return DefaultTransform;
            }


            bool doesAdditionalAnimAffectBone, doesAdditionalBlendAnimAffectBone;

            //these tracks take precedence over the other tracks, if they affect the same bone...
            // they also have established priority over each other.
            foreach (var track in additionalAnimationTracks)
	        {
                doesAdditionalAnimAffectBone = additionalAnimAffectsBone[track.Index];
                doesAdditionalBlendAnimAffectBone = additionalBlendAnimAffectsBone[track.Index];

		        if (doesAdditionalAnimAffectBone || doesAdditionalBlendAnimAffectBone)
                {
                    Matrix additionalTransform = track.GetCurrentTransform(this, doesAdditionalAnimAffectBone, doesAdditionalBlendAnimAffectBone);

                    // overrides all other tracks...
                    return additionalTransform; 
                }
            }


            Matrix mainTransform = mainAnimationTrack.GetCurrentTransform(this, doesMainAnimAffectBone, doesMainBlendAnimAffectBone);


            if (blendToSpecialTransform.HasValue)
            //specialTransformBlendingProgress.HasValue)
            {

                Util.SlerpMatrix(
                    ref mainTransform,
                    ref DefaultTransform,
                    specialTransformBlendingProgress.Value,
                    out mainTransform);

                // update blending:
                if (blendToSpecialTransform == true)
                {
                    specialTransformBlendingProgress += 0.05f;

                    if (specialTransformBlendingProgress > 1f)
                    {
                        specialTransformBlendingProgress = null;
                        blendToSpecialTransform = null;
                    }
                }
                else
                {
                    specialTransformBlendingProgress -= 0.05f;

                    if (specialTransformBlendingProgress < 0f)
                    {
                        specialTransformBlendingProgress = null;
                        blendToSpecialTransform = null;
                    }
                }

            }

            return mainTransform;




            // second gait anim - not used..
          /*  if (secondGaitTrack.FinalWeightFactor > 0f 
                && (doesSecondGaitAnimAffectBone || doesSecondGaitBlendAnimAffectBone))
            {
                // mix with the gait anim:
                Matrix secondGaitTransform = secondGaitTrack.GetCurrentTransform(this, doesSecondGaitAnimAffectBone, doesSecondGaitBlendAnimAffectBone);

                Util.SlerpMatrix(
                       ref mainTransform,
                       ref secondGaitTransform,
                       secondGaitTrack.FinalWeightFactor, //GaitBlendFactor.Value,
                       out currentMatrixBuffer);

                return currentMatrixBuffer;
            }
            else
            {
                return mainTransform;
            }
            */
            
        }
    }
}
