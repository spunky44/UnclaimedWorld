using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using Microsoft.Xna.Framework.Storage;
using Xclna.Xna.Animation;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Entities;
using UWGame;
using UWGame.SimSide;
using UWGame.ClientSide.Map;










namespace UWGame.ClientSide.Renderables
{
    /// <summary>
    /// i am afraid that the classes RenderAsModel and AnimatedModel do not have a clear enough distinction.. perhaps they should just be merged.
    /// </summary>
    public abstract class AnimatedModel 
    {
        protected static BlendState overlayBlendState;

        
  
        
      //  public float BlendingProgress = 0;

        /// <summary>
        /// 0 - 1
        /// when at 1, the blending phase of two animations is over.
        /// </summary>
      //  public float BlendGait2Progress = 0f;


       

        // ANIMATION
        public ModelAnimator ModelAnimator;


        // RENDERING
        protected Matrix translation, shadowTransformation, world, scaling;

        public Matrix StandardDrawingWorldTransformation;
        protected Matrix uncorrectedStandardDrawingWorldTransformation;
        protected float worldPositionY;

        private Matrix shadowMatrix;


        public Matrix[] transforms; // = new Matrix[ModelAnimator.Model.Bones.Count]; 

        static AnimatedModel() // static constructor
        {
            overlayBlendState = new BlendState();
            overlayBlendState.ColorSourceBlend = Blend.SourceAlpha;
            overlayBlendState.ColorDestinationBlend = Blend.One;


        }

        public AnimatedModel()
        {
            
        }

        public AnimatedModel(AnimatedModel original, 
            bool canAnimate)
        {
            // copy everything we need to display a frozen pose...
            translation = original.translation;
            shadowTransformation = original.shadowTransformation;
            world = original.world;
            scaling = original.scaling;

            StandardDrawingWorldTransformation = original.StandardDrawingWorldTransformation;
            uncorrectedStandardDrawingWorldTransformation = original.uncorrectedStandardDrawingWorldTransformation;
            worldPositionY = original.worldPositionY;

            shadowMatrix = original.shadowMatrix;

            transforms = original.transforms;

            ModelAnimator = new ModelAnimator(original.ModelAnimator, canAnimate);

        }

        public void Initialize(Model model)
        {

            ModelAnimator = new ModelAnimator(model);

         //   ModelAnimator.Visible = false;

            
        }

        // NEW:
        public void Update(GameTime gameTime)
        {                                
            // ModelAnimator updates after AnimationController
            ModelAnimator.Update(gameTime);
        }

        protected Vector2 GetScanLinesDimensions()
        {
            Dimension dim = The.Client.Controller.DrawArea;
            //Viewport viewport = The.Client.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(dim.Width, dim.Height);
            Vector2 scanlinesTextureDimensions =
                new Vector2(viewportSize.X / The.Client.Renderer.Scanlines.Width,
                            viewportSize.Y / The.Client.Renderer.Scanlines.Height);

            return scanlinesTextureDimensions;
        }

        

        public enum Transformations { All, OnlyRotation, OnlyYCorrection}
        public void ComputeMatricesForDrawing(Vector3 forward, Vector3 up, Vector3 right, float scale, Vector3 location, Vector3 offset, Transformations transformations)
        {

            if (The.Client == null)
                return; //should be unneccessary after decouple

            CreateBoneTransformMatrixArray();

            // The same instability may cause the 3 orientation vectors may
            // also diverge. Either the Up or Direction vector needs to be
            // re-computed with a cross product to ensure orthagonality
            //   Up = Vector3.Cross(Right, Direction);

            // Reconstruct the ship's world matrix
            world = Matrix.Identity;
            world.Forward = forward; // drawDirection
            world.Up = up;
            world.Right = right;
            // position model: invert z and add offset

            scaling = Matrix.CreateScale(scale);

            if (transformations == Transformations.All)
            {
                float yDrawn = The.Client.Renderer.CorrectModelYPositionForDrawing(location.Y);//(float)(UWGame.SimSide.Instance.CameraTarget.Y + yDistanceFromCameraTarget / Math.Cos(MathHelper.PiOver2 - UWGame.SimSide.Instance.CameraViewingAngle));

                translation = Matrix.CreateTranslation(new Vector3(location.X, yDrawn, -location.Z) + offset);

                Plane lightPlane = new Plane(-Vector3.UnitZ, location.Z); // new Plane(new Vector3(0f, 0f, -1.2f), location.Z); // new Plane(-Vector3.UnitZ, location.Z);
                shadowMatrix = Matrix.CreateShadow(The.Sim.DateAndTime.SunPosition, lightPlane);

                StandardDrawingWorldTransformation = scaling * world * translation;

                worldPositionY = location.Y;

                // calculate a matrix withou the y-correction:
                Matrix unCorrectedTranslation = Matrix.CreateTranslation(new Vector3(location.X, location.Y, -location.Z) + offset);
                uncorrectedStandardDrawingWorldTransformation = scaling * world * unCorrectedTranslation;
                
            }
           /* else if (transformations == Transformations.OnlyYCorrection)
            {


            }*/
            else
            {
                StandardDrawingWorldTransformation = scaling * world;

                uncorrectedStandardDrawingWorldTransformation = scaling * world;
            }

            // NEW: remember to set the world matrix used by any attached objects:
            ModelAnimator.World = StandardDrawingWorldTransformation;

            ModelAnimator.UncorrectedWorld = uncorrectedStandardDrawingWorldTransformation;

        }


        public void CreateBoneTransformMatrixArray()
        {
            transforms = new Matrix[ModelAnimator.Model.Bones.Count];
            ModelAnimator.Model.CopyAbsoluteBoneTransformsTo(transforms);

        }


        public void DrawStandard(GameWorldRenderer.RenderTechnique technique, Matrix view, Matrix projection, 
            Vector3? replaceColor0, Vector3? replaceColor1, Vector3? replaceColor2, Vector3? replaceColor3, 
            float drawWithAlpha, float lightIntensity, float dirtLevel, Texture basicTexture = null, Color? tintColor = null) // bool stealth = false)
        {
            
            DrawModel(ref StandardDrawingWorldTransformation, ref view,
                ref projection, technique, replaceColor0, replaceColor1, replaceColor2, replaceColor3, drawWithAlpha, lightIntensity, dirtLevel, basicTexture, tintColor); 

        }

        public void DrawShadow(Vector3 displacement)
        {
            //draw the model's shadow
            // displace the model so feet are touching the plane:
            world.Translation = displacement; // new Vector3(0f, 0f, -13);
                        
            shadowTransformation = scaling * world * shadowMatrix * translation;
           // shadowTransformation = scaling * world * The.Sim.DateAndTime.GroundObjectsShadowMatrix * translation;

            DrawModel(ref shadowTransformation, ref The.Client.Renderer.View,
                ref The.Client.Projection, GameWorldRenderer.RenderTechnique.NoLighting,
                null, null, null, null, 1f, 1f, 0f);

            /*
            DrawModel(ref shadowTransformation, ref UWGame.SimSide.Instance.Map.Renderer.view,
                ref UWGame.SimSide.Instance.Projection, Map.GameWorldRenderer.RenderTechnique.NoLighting,
                null, null, null, null);*/
        }

       /* public virtual void DrawModel(ref Matrix world, ref Matrix view, ref Matrix projection, 
            Map.GameWorldRenderer.RenderTechnique technique, Vector3? replaceColor0, Vector3? replaceColor1, Vector3? replaceColor2, Vector3? replaceColor3) 
        {          

            DrawModel(ref world, ref view, ref projection, technique, replaceColor0, replaceColor1, replaceColor2, replaceColor3, 1f, 1f, 0f);
        }*/

        public virtual void DrawModel(ref Matrix world, ref Matrix view, ref Matrix projection,
            GameWorldRenderer.RenderTechnique technique, Vector3? replaceColor0, Vector3? replaceColor1, Vector3? replaceColor2, Vector3? replaceColor3,
            float alphaFactor, float lightIntensity, float dirtLevel, Texture basicTexture = null, Color? tintColor = null)
        {

        }

        public static bool HasAttachorModel(Entity entity, string attachorBoneName)
        {
            List<IAttachable> attachedObjectList;
            if (entity.Renderable./*TODO DECOUPLE*/RenderAsModel.AnimatedModel.ModelAnimator.AttachedObjects.TryGetValue(attachorBoneName, out attachedObjectList))
            {

                foreach (IAttachable attachedObject in attachedObjectList)
                {
                    if (((RenderAsModel)attachedObject).Parent.EntityType.Name == "Box")
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool HasAttachedModel(Entity entity, string attachedEntityKey, string attachorBoneName, string attacheeBoneName) //, string objectBoneName)
        {
            List<IAttachable> attachedObjectList;
            if (entity.Renderable./*TODO DECOUPLE*/RenderAsModel.AnimatedModel.ModelAnimator.AttachedObjects.TryGetValue(attachorBoneName, out attachedObjectList))
            {

                foreach (IAttachable attachedObject in attachedObjectList)
                {
                    if (((RenderAsModel)attachedObject).Renderable.RenderableType.KeyName == attachedEntityKey &&
                        attachedObject.AttacheeBone != null && 
                        attachedObject.AttacheeBone.Name == attacheeBoneName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /*
        public static bool HasAttachedBoxModel(Entity entity, string entityBoneName) //, string objectBoneName)
        {
            List<IAttachable> attachedObjectList;
            if (entity.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.AttachedObjects.TryGetValue(entityBoneName, out attachedObjectList))
            {

                foreach (IAttachable attachedObject in attachedObjectList)
                {
                    if (((RenderAsModel)attachedObject).Parent.EntityType.Name == "Box")
                    {
                        return true;
                    }
                }
            }

            return false;
        }*/

       

       

        public static void OrientAttachedModel(Entity entity, string attachorBoneName, Vector3 euler, Vector3 trans)
        {
            List<IAttachable> attachedObjectList;
            if (entity.Renderable./*TODO DECOUPLE*/RenderAsModel.AnimatedModel.ModelAnimator.AttachedObjects.TryGetValue(attachorBoneName, out attachedObjectList))
            {
                for (int i = 0; i < attachedObjectList.Count; i++)
                {
                    RenderAsModel attachedRenderAsModel = attachedObjectList[i] as RenderAsModel;
                    if (attachedRenderAsModel != null)
                    {
                        Matrix translateMatrix = Matrix.CreateTranslation(trans);
                         
                        Matrix rotateMatrix = Matrix.Identity;
                        //   Matrix translate = Matrix.Identity;
                        //we'll probably add an offset, so use translation all up in here
                        rotateMatrix *= Matrix.CreateFromAxisAngle(Vector3.UnitX, MathHelper.ToRadians(euler.X));
                        rotateMatrix *= Matrix.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(euler.Y));
                        rotateMatrix *= Matrix.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.ToRadians(euler.Z));

                        attachedRenderAsModel.LocalTransform = rotateMatrix * translateMatrix;

                        attachedRenderAsModel = null;

                    }
                }
            }

        }


        // "FFFFFF".ToColorVector3();
        protected static Vector3 mainLightColorConstant = new Vector3(1f, 1f, 0.85f); //Vector3.One; // intensity * Vector3.One;
        protected static Vector3 fillLightColorConstant = new Vector3(0.8f, 0.8f, 1f);  //Vector3.One;
        // blue tint:
        protected static Vector3 backLightColorConstant = new Vector3(0.8f, 0.8f, 1f);


        protected Vector3 mainLightColor;
        protected Vector3 fillLightColor;
        protected Vector3 backLightColor;
        protected Vector3 mainLightDirection;
        protected Vector3 fillLightDirection;
        protected Vector3 backLightDirection;

        protected void CreateLights(float lightIntensity,/* out Vector3 mainLightColor, out Vector3 fillLightColor, out Vector3 backLightColor, out Vector3 mainLightDirection,
            out Vector3 fillLightDirection, out Vector3 backLightDirection,*/
            Color? tintColor = null)
        {

            if (tintColor.HasValue)
            {
                Vector3 color = tintColor.Value.ToVector3();

                mainLightColor = lightIntensity * color;
                fillLightColor = 0.6f * lightIntensity * color;
                backLightColor = lightIntensity * color;
            }
            else
            {
                // yellow light:
                mainLightColor = lightIntensity * mainLightColorConstant; //Vector3.One; // intensity * Vector3.One;
                fillLightColor = 0.6f * lightIntensity * fillLightColorConstant;  //Vector3.One;
                // blue tint:
                backLightColor = lightIntensity * backLightColorConstant; //Vector3.One; 
            }

            SetLightDirections();
            
            /*  backLightDirection = -mainLightDirection;
              backLightDirection.Normalize();*/
        }

        protected void SetLightDirections()
        {
            mainLightDirection = -The.Sim.DateAndTime.SunPosition;

            fillLightDirection = Vector3.Cross(mainLightDirection, Vector3.UnitY);
            if (mainLightDirection.X != 0f)
            {
                fillLightDirection.X *= Math.Sign(mainLightDirection.X); // opposite right/left from main light
            }
            fillLightDirection.Z = Math.Abs(fillLightDirection.Z); // make fill light always from above

            // has no real effect... or?
            backLightDirection = -The.Client.Renderer.CameraDirection;
            backLightDirection.Normalize();
        }



        public void Destroy()
        {
            // called on Finalize...
            // it would be nice if dead entities could have their ModelAnimators removed here.

            if (The.Sim != null) // is the game still active?
            {
                /* OLD:
                The.Sim.ScreenManager.Game.Components.Remove(ModelAnimator);*/
                ModelAnimator = null;
            }
        }
/*
        /// <summary>
        /// TODO: a full body anim may override the additive anim moments later after it finishes blending (haulLight + gaitWalk). 
        /// How do we solve this? Perhaps with separate channels/controllers with fixed priority
        /// </summary>
        /// <param name="controller"></param>
        public void RunControllerAdditively(AnimationController controller)
        {
            foreach (BonePose p in ModelAnimator.BonePoses)
            {
                if (controller.ContainsAnimationTrack(p))
                {
                    p.CurrentController = controller;
                    p.CurrentBlendController = null;
                }
                else
                {

                }
            }
        }

        public void StopRunningController(AnimationController controller)
        {
            //if (controller.AnimationSource.)
            foreach (BonePose p in ModelAnimator.BonePoses)
            {
                if (p.CurrentController == controller)
                {
                    p.CurrentController = null;
                    p.CurrentBlendController = null;
                }                
            }
        }

        public void RunController(AnimationController controller)
        {
            //if (controller.AnimationSource.)
            foreach (BonePose p in ModelAnimator.BonePoses)
            {
                p.CurrentController = controller;
                p.CurrentBlendController = null;
            }
        }

        /// <summary>
        /// while blending, this must be called each frame to update the blend progress (interpolation value) on the bones
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="blendController"></param>
        /// <param name="blendFactor"></param>
        public void RunController(AnimationController controller, AnimationController blendController, float blendFactor)
        {
            foreach (BonePose p in ModelAnimator.BonePoses)
            {
                p.CurrentController = controller;
                p.CurrentBlendController = blendController;
                p.BlendFactor = blendFactor;
            }
        }

        public void RunGaitControllers(AnimationController controller, AnimationController secondGaitController, float gaitBlendFactor)
        {
            foreach (BonePose p in ModelAnimator.BonePoses)
            {
                p.CurrentController = controller;
                p.SecondGaitController = secondGaitController;
                p.GaitBlendFactor = gaitBlendFactor;
            }
        }
        */

        protected void HandleEmitterModelParts(GameWorldRenderer.RenderTechnique technique)
        {
            switch (technique)
            {
                case GameWorldRenderer.RenderTechnique.NormalsAndDepth:
                    TurnonEmitterModelParts(EmitterRenderMode.All); // we still want an outline around the missing parts!
                    break;

                case GameWorldRenderer.RenderTechnique.DrawModelEmitters:
                    TurnonEmitterModelParts(EmitterRenderMode.EmittersOnly);
                    break;

                case GameWorldRenderer.RenderTechnique.Standard:
                    TurnonEmitterModelParts(EmitterRenderMode.All); // during the day only?
                    break;

              /*  case GameWorldRenderer.RenderTechnique.DepthHeightBillboardAlpha:
                    TurnonEmitterModelParts(EmitterRenderMode.NoEmitters); // this would block the light source, so omit them... 
                                                                //- but omitting them screws up overlays (selection circle can be seen through the model)!!!
                                                                // solution: draw the emitting parts translated back a little, so the lights are in front...
                    break;
                    */
                default:
                    TurnonEmitterModelParts(EmitterRenderMode.All); // CRT, overlay etc.
                    break;

            }
        }

        protected enum EmitterRenderMode { NoEmitters, EmittersOnly, All }
        private void TurnonEmitterModelParts(EmitterRenderMode renderMode)
        {
            foreach (ModelMesh mesh in ModelAnimator.Model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    if (renderMode == EmitterRenderMode.All)
                    {
                        meshPart.Tag = null;
                    }
                    else
                    {
                        Vector3 emissiveColor = meshPart.Effect.Parameters["EmissiveColor"].GetValueVector3();

                        switch (renderMode)
                        {
                            case EmitterRenderMode.NoEmitters:

                                if (emissiveColor != Vector3.Zero)
                                {
                                    meshPart.Tag = "skip";
                                }
                                else
                                {
                                    meshPart.Tag = null;
                                }
                                break;

                            case EmitterRenderMode.EmittersOnly:

                                if (emissiveColor != Vector3.Zero)
                                {
                                    meshPart.Tag = null;
                                }
                                else
                                {
                                    meshPart.Tag = "skip";
                                }
                                break;
                        }
                    }
                }
            }
        }
    }
}
