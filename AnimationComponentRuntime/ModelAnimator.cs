/*
 * ModelAnimator.cs
 * Copyright (c) 2007 David Astle, Michael Nikonov
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

#define GI
#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
#endregion

namespace Xclna.Xna.Animation
{

    /// <summary>
    /// Animates and draws a model that was processed with AnimatedModelProcessor
    /// </summary>
    public  class ModelAnimator //: DrawableGameComponent
    {
        #region Member Variables
        // Stores the world transform for the animation controller.
        private Matrix world = Matrix.Identity;

        // Model to be animated
        private readonly Model model;

        // This stores all of the "World" matrix parameters for an unskinned model
        // These are pointers to effect parameters!
        private readonly EffectParameter[] worldParams, matrixPaletteParams;

        // A flattened array of effects, one for each ModelMeshPart
        private Effect[] modelEffects;
        private ReadOnlyCollection<Effect> effectCollection;

        // Skeletal structure containg transforms
        private BonePoseCollection bonePoses;

        private AnimationInfoCollection animations;



        private Dictionary<string, List<IAttachable>> attachedObjects = new Dictionary<string, List<IAttachable>>();
        

        // Store the number of meshes in the model
        private readonly int numMeshes;
        
        // Stores the number of effects/ModelMeshParts
        private readonly int numEffects;

        // Used to avoid reallocation
        private static Matrix skinTransform;
        // Buffer for storing absolute bone transforms
        private Matrix[] pose;
        // Array used for the matrix palette
        private Matrix[][] palette;
        // Inverse reference pose transforms
        private SkinInfoCollection[] skinInfo;

        private Dictionary<string, bool> MeshesToHide;

        #endregion


        /// <summary>
        /// keep a controller for each animation that the model is capable of playing
        /// 
        /// TODO: make these private
        /// </summary>
        public Dictionary<string, AnimationController> AnimationControllers = new Dictionary<string, AnimationController>();



        #region General Properties

        

        /// <summary>
        /// Gets or sets the world matrix for the animation scene.
        /// </summary>
        public Matrix World
        {
            get
            {
                return world;
            }
            set
            {
                world = value;
            }
        }

        public Matrix UncorrectedWorld
        {
            get; set;           
        }

        /// <summary>
        /// Returns the number of effects used by the model, one for each ModelMeshPart
        /// </summary>
        protected int EffectCount
        {
            get { return numEffects; }
        }

        /// <summary>
        /// Gets the model associated with this controller.
        /// </summary>
        public Model Model
        { get { return model; } }

        /// <summary>
        /// Gets the animations that were loaded in from the content pipeline
        /// for this model.
        /// </summary>
        public AnimationInfoCollection Animations
        { get { return animations; } }

        #endregion

      
        #region Constructors



        /// <summary>
        /// Creates a new instance of ModelAnimator. Every renderable has one.
        /// 
        /// </summary>     
        /// <param name="model">The model to be animated.</param>
        public ModelAnimator(Model model)// : base(game)
        {
            this.model = model;

            animations = AnimationInfoCollection.FromModel(model);
            bonePoses = BonePoseCollection.FromModelBoneCollection(model.Bones);
            numMeshes = model.Meshes.Count;

            // Find total number of effects used by the model
            numEffects = 0;
            foreach (ModelMesh mesh in model.Meshes)
                foreach (Effect effect in mesh.Effects)
                    numEffects++;


            // Initialize the arrays that store effect parameters
            modelEffects = new Effect[numEffects];
            worldParams = new EffectParameter[numEffects];
            matrixPaletteParams = new EffectParameter[numEffects];
            InitializeEffectParams();

            pose = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(pose);

            // Get all the skinning info for the model
            Dictionary<string, object> modelTagInfo = (Dictionary<string, object>)model.Tag;
            if (modelTagInfo == null)
                throw new Exception("Model Processor must subclass AnimatedModelProcessor.");

            skinInfo = (SkinInfoCollection[])modelTagInfo["SkinInfo"];
            if (skinInfo == null)
                throw new Exception("Model processor must pass skinning info through the tag.");

            palette = new Matrix[model.Meshes.Count][];
            for (int i = 0; i < skinInfo.Length; i++)
            {
                if (Util.IsSkinned(model.Meshes[i]))
                    palette[i] = new Matrix[skinInfo[i].Count];
                else
                    palette[i] = null;
            }

            // Update after AnimationController by default
          //  base.UpdateOrder = 1;
       
            // Test to see if model has too many bones
            for (int i = 0; i < model.Meshes.Count; i++ )
            {
                if (palette[i] != null && matrixPaletteParams[i] != null)
                {
                    Matrix[] meshPalette = palette[i];
                    try
                    {
                        matrixPaletteParams[i].SetValue(meshPalette);
                    }
                    catch
                    {
                        throw new Exception("Model has too many skinned bones for the matrix palette.");
                    }
                }
            }


            // store controllers for all the other animations here...
            foreach (KeyValuePair<string, AnimationInfo> anim in Animations)
            {
                AnimationController controller;

                // NEW: always interpolate. We want to compress animations
                controller = new InterpolationController(anim.Value); //, InterpolationMethod.SplineBased);
              
                /*if (anim.Value.IsGaitAnim)
                {
                    controller = new InterpolationController(anim.Value, InterpolationMethod.SplineBased);
                }
                else
                {
                    controller = new AnimationController(anim.Value);
                }*/

                controller.SpeedFactor = 0f; // we use this to start/stop the animation. If it is nonzero, it will start running immediately.
                controller.IsLooping = false;   //!!

                AnimationControllers.Add(anim.Key, controller);

            }
        }


        /// <summary>
        /// copy ctor for memoryfact
        /// and carcass too 
        /// For memory facts which don't animate, one is still needed because it contains the frozen pose, transforms etc. needed when drawing.
        /// </summary>
        /// <param name="original"></param>
        public ModelAnimator(ModelAnimator original, bool canAnimate)
          //  : base(original.Game)
        {
            model = original.model;

            numMeshes = original.numMeshes;
            numEffects = original.numEffects;

            // copy the original's last pose:
            pose = new Matrix[original.pose.Length];
            Array.Copy(original.pose, pose, original.pose.Length);
            
            // bonePoses are only needed when we are animating! NOT for memoryfacts!
            bonePoses = BonePoseCollection.FromModelBoneCollection(model.Bones);

            if (original.palette[0] != null)
            {
                // skinned:
                InitJaggedArray(ref palette, original.palette.Length, original.palette[0].Length);
                CopyJaggedArray(original.palette, palette);
            }
            else
            {
                // unskinned:
                palette = new Matrix[original.palette.Length][]; // is filled with null

            }

            if (original.MeshesToHide != null)
            {
                MeshesToHide = original.MeshesToHide.ToDictionary(k => k.Key, k => k.Value);
            }

            skinInfo = original.skinInfo; // point to original

            modelEffects = new Effect[original.modelEffects.Length];
            Array.Copy(original.modelEffects, modelEffects, original.modelEffects.Length);

            // OK to copy pointers????
            matrixPaletteParams = new EffectParameter[original.matrixPaletteParams.Length];
            Array.Copy(original.matrixPaletteParams, matrixPaletteParams, original.matrixPaletteParams.Length);

            // OK to copy pointers????
            worldParams = new EffectParameter[original.worldParams.Length];
            Array.Copy(original.worldParams, worldParams, original.worldParams.Length);

            if (canAnimate)
            {   // for a carcass, reference additional data from the living entity that we need to show a carcass in its Dead pose:
                // (more efficient than creating them from new)
                this.animations = original.animations;

                this.AnimationControllers = original.AnimationControllers;
            }
        }


        public AnimationController GetAnimControllerFromKey(string animKey)
        {
            AnimationController anim;
            if (!AnimationControllers.TryGetValue(animKey, out anim))
            {
                throw new Exception("Animation " + animKey + " not found... check MergeAnimations property and spelling. Also check CreatureLoader  reference");
            }
            return anim;
        }

        private static void InitJaggedArray<T>(ref T[][] map, int width, int height)
        {
            map = new T[width][];

            for (int x = 0; x < width; x++)
            {
                map[x] = new T[height];
            }

        }

        private static void CopyJaggedArray<T>(T[][] fromArray, T[][] toArray)
        {
            // Array.Copy(newAllNodes, AllNodes, newAllNodes.Length);

            Parallel.For(0, fromArray.Length, (x) =>
            {
                T[] fromSubArray = fromArray[x];
                T[] toSubArray = toArray[x];
                Array.Copy(fromSubArray, toSubArray, fromSubArray.Length);

            });
        }

        #endregion
        /// <summary>
        /// Returns skinning information for a mesh.
        /// </summary>
        /// <param name="index">The index of the mesh.</param>
        /// <returns>Skinning information for the mesh.</returns>
        public SkinInfoCollection GetMeshSkinInfo(int index)
        {
            return skinInfo[index];
        }

        /// <summary>
        /// Call this to make meshes invisible on the model.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="show"></param>
        public void ShowMesh(string name, bool show)
        {
            if (show)
            {
                if (MeshesToHide != null && MeshesToHide.Count > 0)
                {
                    if (MeshesToHide.ContainsKey(name))
                    {
                        MeshesToHide.Remove(name);
                    }
                }
            }
            else
            {   // hide mesh
                if (MeshesToHide == null)
                {
                    MeshesToHide = new Dictionary<string, bool>();
                }

                if (!MeshesToHide.ContainsKey(name))
                {
                    MeshesToHide.Add(name, true);
                }
            }
        }

        /// <summary>
        /// Called during creation and calls to InitializeEffectParams.  Returns the list of
        /// effects used during rendering.
        /// </summary>
        /// <returns>A flattened list of effects used during rendering, one for each ModelMeshPart</returns>
        protected virtual IList<Effect> CreateEffectList()
        {
            List<Effect> effects = new List<Effect>();
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    effects.Add(part.Effect);
                }
            }
            return effects;
        }

        /// <summary>
        /// Initializes the effect parameters.  Should be called after the effects
        /// on the model are changed.
        /// </summary>
        public void InitializeEffectParams()
        {
            IList<Effect> effects = CreateEffectList();
            if (effects.Count != numEffects)
                throw new Exception("The number of effects in the list returned by CreateEffectList "
                    + "must be equal to the number of ModelMeshParts.");
            effects.CopyTo(modelEffects, 0);
            effectCollection = new ReadOnlyCollection<Effect>(modelEffects);

            // store the parameters in the arrays so the values they refer to can quickly be set
            for (int i = 0; i < numEffects; i++)
            {
                worldParams[i] = modelEffects[i].Parameters["World"];
                matrixPaletteParams[i] = modelEffects[i].Parameters["MatrixPalette"];
            }
        }

        /// <summary>
        /// Gets a collection of effects, one per ModelMeshPart, that are used by 
        /// the ModelAnimator. The first index of the collection corresponds to the
        /// effect used to draw the first ModelMeshPart of the first Mesh, and the 
        /// last index corresponds to the effect used to drwa the last ModelMeshPart
        /// of the last Mesh.
        /// </summary>
        public ReadOnlyCollection<Effect> Effects
        {
            get { return effectCollection; }
        }

        #region Animation and Update Routines


      
        public void Update(GameTime gameTime)
        {
            CopyAbsoluteTransforms();              

        }

        /// <summary>
        /// Updates the animator by finding the current absolute transforms.
        /// 
        /// Lars: if this step is not performed after creation, the models won't be rendered...
        /// 
        /// Lars: not really sure how this works. I think it looks like it could be skipped when not drawing, or?
        /// </summary>
        /// <param name="gameTime">The GameTime.</param>
        public void CopyAbsoluteTransforms()
        {
            bonePoses.CopyAbsoluteTransformsTo(pose);
            for (int i = 0; i < skinInfo.Length; i++)
            {
                if (palette[i] == null)
                    continue;

                SkinInfoCollection infoCollection = skinInfo[i];

                foreach (SkinInfo info in infoCollection)
                {
                    skinTransform = info.InverseBindPoseTransform;
                    Matrix.Multiply(ref skinTransform, ref pose[info.BoneIndex],
                       out palette[i][info.PaletteIndex]);
                }
            }
        }

       
        
        /// <summary>
        /// call this anytime current and blend controllers are switched or change their animations
        /// </summary>
        /// <param name="track"></param>
        public void UpdateModelBones(AnimationTrack track) 
        {           
            foreach (BonePose p in BonePoses)
            {
                p.UpdateAnimControllerStatus(track);

            }
        }
        

        public delegate void IterateMethod(IAttachable attachable);

        public void IterateAttachedEntities(IterateMethod iterateMethod)
        {
            foreach (var list in attachedObjects)
            {
                foreach (var attachable in list.Value)
                {
                    iterateMethod(attachable);
                   

                }
            }

        }



        public void ComputeTransformsForAttachedObjects(float scale)
        {            
            if (attachedObjects.Count > 0)
            {
                // what is this for...? not needed...
               // Matrix inverseScale = Matrix.CreateScale(1f / scale);

                foreach (KeyValuePair<string, List<IAttachable>> kvp in attachedObjects)
                {
                    Matrix[] attacheeBoneTransforms;//, attachorBoneTransforms; 
                    Model attacheeModel; 

                    foreach (IAttachable attachedObject in kvp.Value)
                    {                      
                        attacheeModel = attachedObject.ModelAnimator.model;


                        //TODO instead of changin the attachee's local transform
                        //we should instead change the pose in the attachedObject.AttacheeBone 
                        //we'd have to do this before copying the array here V
                        //in fact, why copy the whole array?
                        //we only need two absolute bones
                        attacheeBoneTransforms = new Matrix[attacheeModel.Bones.Count];
                        attacheeModel.CopyAbsoluteBoneTransformsTo(attacheeBoneTransforms);


                        if (attachedObject.AttacheeBone != null)
                        {
                            attachedObject.CombinedTransform =

                                //the twist applied to the attached object
                                attachedObject.LocalTransform *

                                    //the possible reorientation of the attached object's attach point bone
                                    Matrix.Invert(attacheeBoneTransforms[attacheeModel.Meshes[0].ParentBone.Index]) *
                                        attacheeBoneTransforms[attachedObject.AttacheeBone.Index] *                                

                                    //the reorientation of our bone being attached-to
                                    Matrix.Invert(pose[model.Root.Index]) *
                                        pose[attachedObject.AttachorBone.Index] *
                                        world; 
                            //worldspace transform  
                                                

                            /*
                            Matrix.Invert(attacheeBoneTransforms[attacheeModel.Meshes[0].ParentBone.Index]) *
                                        attacheeBoneTransforms[attachedObject.AttacheeBone.Index] *  */

                        }
                    }                    
                }
            }
        }

       /*  public void AttachObject(IAttachable attachable, AttachPoint attachorPoint, AttachPoint attachee, //string attachorBoneName, BonePose attacheeBone, 
            AttacheePoint? attacheePoint)
        {
            BonePose attachorBone = bonePoses[attachorBoneName];

            AttachObject(attachable, attachorBone, attacheeBone, attacheePoint);

           attachable.AttachorBone = attachorBone;

            List<IAttachable> attachedList;
            if (!attachedObjects.TryGetValue(attachorBoneName, out attachedList))
            {
                attachedList = new List<IAttachable>();
                attachedObjects.Add(attachorBoneName, attachedList);
            }

            attachedList.Add(attachable);

            attachable.AttacheeBone = attacheeBone;

        }*/

        public void AttachObject(IAttachable attachable, AttachPoint attachorPoint, AttachPoint attachee,
            //BonePose attachorBone, BonePose attacheeBone, 
            AttacheePoint? attacheePoint, BonePose attacheeBone, object attachorEntity)
        {
            BonePose attachorBone = bonePoses[attachorPoint.BoneName];
            
            List<IAttachable> attachedList;
            if (!attachedObjects.TryGetValue(attachorBone.Name, out attachedList))
            {
                attachedList = new List<IAttachable>();
                attachedObjects.Add(attachorBone.Name, attachedList);
            }

            attachedList.Add(attachable);


            attachable.AttachedTo = attachorEntity;

            attachable.AttachorPoint = attachorPoint; 
           // attachable.AttachorPoint.Bone = attachorBone; // don't save instance data in AttachPoint, it should be immutable.
            attachable.AttachorBone = attachorBone; // NEW

            attachable.AttacheeBone = attacheeBone; 
            attachable.AttacheePointValue = attacheePoint;          
           
        }

        public void DeattachObject(IAttachable attachable, string attachorBoneName)
        {            
            List<IAttachable> attachedList;
            if (attachedObjects.TryGetValue(attachorBoneName, out attachedList))
            {
                attachedList.Remove(attachable);
             //   attachable.AttachorPoint.Bone = null;
                attachable.AttachedTo = null;
                attachable.AttacheePointValue = null;
                attachable.AttacheeBone = null;
                attachable.AttachorPoint = null;
                attachable.AttachorBone = null; // NEW
            }
          /*  else
            {
                throw new Exception("error?");
            }*/
      
        }

        public bool HasAttachedObject(string attachorBoneName)
        {
            List<IAttachable> attachedObjectsToBone;
            if (attachedObjects.TryGetValue(attachorBoneName, out attachedObjectsToBone))
            {
                return attachedObjectsToBone.Count > 0;
            }
            else return false;

            //return attachedObjects.ContainsKey(boneName);
                       
        }


        /// <summary>
        /// Lars: get transform of an object attached to a particular bone
        /// </summary>
        /// <param name="localTransform"></param>
        /// <param name="bonePose"></param>
        /// <returns></returns>
      /*  public Matrix ComputeAttachedObjectTransform(Matrix localTransform, BonePose bonePose)
        {
            return localTransform *
                    Matrix.Invert(pose[model.Meshes[0].ParentBone.Index]) * pose[bonePose.Index] * world;

        }*/

        /// <summary>
        /// Copies the current absolute transforms to the specified array.
        /// </summary>
        /// <param name="transforms">The array to which the transforms will be copied.</param>
     /*   public void CopyAbsoluteTransformsTo(Matrix[] transforms)
        {
            pose.CopyTo(transforms, 0);
        }*/

        /// <summary>
        /// Gets the current absolute transform for the given bone index.
        /// </summary>
        /// <param name="boneIndex"></param>
        /// <returns>The current absolute transform for the bone index.</returns>
        public Matrix GetAbsoluteTransform(int boneIndex)
        {
            return pose[boneIndex];
        }


        /// <summary>
        /// Gets a list of objects that are attached to a bone in the model.
        /// </summary>
      /*  public IList<IAttachable> AttachedObjects
        {
            get { return attachedObjects; }
        }*/

        public Dictionary<string, List<IAttachable>> AttachedObjects
        {
            get { return attachedObjects; }
        }

        /// <summary>
        /// Gets the BonePoses associated with this ModelAnimator.
        /// </summary>
        public BonePoseCollection BonePoses
        {
            get { return bonePoses; }
        }

        /// <summary>
        /// Draws the current frame
        /// </summary>
        /// <param name="gameTime">The game time</param>
        public /*override*/ void Draw(GameTime gameTime)
        {
            try
            {
                int index = 0;

                bool meshesToHide = MeshesToHide != null && MeshesToHide.Count > 0;


                // Update all the effects with the palette and world and draw the meshes
                for (int i = 0; i < numMeshes; i++)
                {
                    ModelMesh mesh = model.Meshes[i];

                    // The starting index for the modelEffects array
                    int effectStartIndex = index;
                    if (matrixPaletteParams[index] != null)
                    {
                        foreach (Effect effect in mesh.Effects)
                        {                   
                            worldParams[index].SetValue(world);  // where does this get passed to the shader???              

                            matrixPaletteParams[index].SetValue(palette[i]);
                            index++;
                        }
                    }
                    else
                    { // this code seems to never get hit.
                        foreach (Effect effect in mesh.Effects)
                        {                           
                            worldParams[index].SetValue(pose[mesh.ParentBone.Index] * world);
                            index++;
                        }
                    }
                    
                    if (meshesToHide)
                    {
                        if (MeshesToHide.ContainsKey(mesh.Name))
                        {   // don't draw this mesh.
                            continue;
                        }
                    }

                    int numParts = mesh.MeshParts.Count;
                    GraphicsDevice device = mesh.MeshParts[0].VertexBuffer.GraphicsDevice;
                    device.Indices = mesh.MeshParts[0].IndexBuffer;
                    for (int j = 0; j < numParts; j++ )
                    {
                        ModelMeshPart currentPart = mesh.MeshParts[j];

                        if (((string)currentPart.Tag) == "skip")
                        {
                            continue;
                        }

                        if (currentPart.NumVertices == 0 || currentPart.PrimitiveCount == 0)
                            continue;

                        Effect currentEffect = modelEffects[effectStartIndex+j];

                        // TESTIN******
                     /*   var vertexDeclaration = currentPart.VertexBuffer.VertexDeclaration;
                        var vertexElements = vertexDeclaration.GetVertexElements();
                                               
                        var positionElement = vertexElements.First(e => e.VertexElementUsage == VertexElementUsage.BlendIndices);
                     
                        // var positions = new Vector3[currentPart.NumVertices];
                        var positions = new UInt32[currentPart.NumVertices];
                        currentPart.VertexBuffer.GetData(
                          currentPart.VertexOffset * vertexDeclaration.VertexStride + positionElement.Offset,
                          positions,
                          0,
                          currentPart.NumVertices,
                          vertexDeclaration.VertexStride);
                        */
                        // TESTIN******

                        device.SetVertexBuffer(currentPart.VertexBuffer);
                        
                        EffectPassCollection passes = currentEffect.CurrentTechnique.Passes;
                        int numPasses = passes.Count;
                        for (int k = 0; k < numPasses; k++)
                        {
                            EffectPass pass = passes[k];
                            pass.Apply();
                            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, currentPart.VertexOffset,
                                0, currentPart.NumVertices, currentPart.StartIndex, currentPart.PrimitiveCount);
                           
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                throw new InvalidOperationException("The effects on the model for a " +
                    "ModelAnimator were changed without calling ModelAnimator.InitializeEffectParams().");
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException("ModelAnimator has thrown an InvalidCastException.  This is " +
                    "likely because the model uses too many bones for the matrix palette.  The default palette size "
                    + "is 56 for windows and 40 for Xbox.");
            }
            
        }
        #endregion
    }
}
