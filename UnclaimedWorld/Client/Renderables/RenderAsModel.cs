using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Xclna.Xna.Animation;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Vehicles;
using Microsoft.Xna.Framework.Graphics;
using GameStateManagement;
using UWGame.SimSide;
using UWGame.SimSide.Entities;
using UWGame.ClientSide;
using UWGame.SimSide.Maps;
using System.Diagnostics;
using UWGame.Client.Particles;
using UWGame.Control;
using UWGame.SimSide.AI;
using UWGame.Client.Audio;
using UWGame.SimSide.Entities.Locomotors.Stances;

namespace UWGame.ClientSide.Renderables
{


    //Decoupling Strategy 
    //DONE 1 add a class here Locomotor  
    //DONE 2 RenderAsModel wil has-a loco
    //DONE 3 migrate all non renderish data and methods into loco
    //DONE 4 indirect all references to the loco members that migrated
    //DONE 5 move loco to SimSide.ENtities namespace
    //DONE 6 move loco source to Entities source folder
    //7 decouple loco from renderAsModel -- RenderAs should query, never set
    //8 change loco to the standard locomotortype






    public class RenderAsModel : RenderAsBase, IAttachable, IUpdatable
    {
        #region IAttachable Members

        private Matrix localTransform = Matrix.Identity;
        public Matrix LocalTransform
        {
            get { return localTransform; }
            set { localTransform = value; }
        }
        public Matrix CombinedTransform { get; set; }
        //public BonePose AttachorBone { get; set; }
        
        public ModelAnimator ModelAnimator { get; set; }

        public AttacheePoint? AttacheePointValue { get; set; }
        public BonePose AttacheeBone { get; set; }
        public BonePose AttachorBone { get; set; }

        public AttachPoint AttachorPoint { get; set; }

        public object AttachedTo { get; set; }

        public float Scale {
            get
            {
                return FinalModelScale;
            }
        }

        #endregion

       
        /// <summary>
        /// this reference can vary across same EntityType...
        /// </summary>
        public ModelData ModelData;


        /// <summary>
        /// these properties override the ModelData ones:
        /// </summary>
        public float FinalModelScale = 1f;
        public string FinalModelName;
        public string FinalModelBasicTextureName;
        public Texture FinalModelBasicTexture;
        public AnimatedModel AnimatedModel;


        /// <summary>
        /// is set to false on memory fact
        /// </summary>
        private bool canAnimate = true;

        /// <summary>
        /// only the anim condition code and the gait anim code should interact with these 2 tracks.
        /// </summary>
        private AnimationTrack mainAnimation;

        private AnimationTrack secondGaitAnimation;

        /// <summary>
        /// we allow goals to start/stop anims on these tracks...
        /// </summary>
        private List<AnimationTrack> additionalAnimationTracks;

        // the animations being played:
        public string CurrentBaseAnimKey //= null;
        {
            get 
            {
                return mainAnimation.CurrentAnimKey;
            }
        }

        public string CurrentAdditionalAnim1Key //= null;
        {
            get
            {
                return additionalAnimationTracks[0].CurrentAnimKey;
            }
        }

        public string CurrentAdditionalAnim2Key //= null;
        {
            get
            {
                return additionalAnimationTracks[1].CurrentAnimKey;
            }
        }

        private RenderAsModelType RenderAsModelType
        {
            get
            {
                return Parent.EntityType.RenderableTypeMode.RenderAsModelType;
            }
        }

       

        /// <summary>
        /// the anim info that was selected as the best match to the entity's state flags
        /// </summary>
        public AnimConditionInfo SelectedAnimInfo;

      /*  protected void ConstructConditionFlags(Type t)
        {
            AnimConditions = new AnimConditions() { Modifiers = new BitMask64(t) };
        }*/

        /// <summary>
        /// keep this bool here, but store the bit flags in Entity..?
        /// </summary>
        private bool animationFlagsAreDirty = true;
      

        private void SetAnimationActionStateFlag(AnimAction state)
        {
            Renderable.SetAnimationActionStateFlag(state);          


        }

        private void SetAnimationStateFlag(AnimModifier state)
        {
            Renderable.SetAnimationStateFlag(state);          

        }

       


        private void ClearAnimationActionStateFlag(AnimAction state)
        {
            Renderable.ClearAnimationActionStateFlag(state);

            
            /*
            if (AnimConditions.Action != state)
                return; // no point clearing a false bit to false


            AnimConditions.Action = null; // ??? set to idle? or prevent anim matching until it has been set...?

           

            animationFlagsAreDirty = true;
            */
        }


        private void ClearAnimationStateFlag(AnimModifier state)
        {
            Renderable.ClearAnimationStateFlag(state);

        }

      

        /// <summary>
        /// another gait animation that may be continously interpolated with the main animation to create new gaits between the 4 main types
        /// </summary>
    /*    private AnimationController secondGaitController;
        public string SecondGaitControllerName
        {
            get
            {
                if (secondGaitController != null)
                    return secondGaitController.AnimationInfo.Name;
                return "------";
            }
        }

        /// <summary>
        /// the gait controller also has a corresponding blend controller. When this gets set to null, and BlendGait2 is less than 1, BlendGait2 will be multiplied with GaitBlend (like a fade-out)
        /// </summary>
        private AnimationController blendSecondGaitController;
        */

       // List<AnimationController> currentAdditiveControllers = new List<AnimationController>();

        //private AnimationController additiveController1;
       // private AnimationController additiveController2;




        // shirt and pants only?
        public Vector3 CustomColor0;
        public Vector3 CustomColor1;

        /// <summary>
        /// hair or spots
        /// </summary>
        public Vector3 CustomColor2;

        /// <summary>
        /// Skin or fur
        /// </summary>
        public Vector3 CustomColor3;

        public RenderAsModel(Entity parent, Renderable renderable)
            : base(parent, renderable)
        {
         //   ConstructConditionFlags(typeof(AnimModifier));

           
        }


        /// <summary>
        /// copy ctor for memoryfact and carcass entity. also called when loading
        /// </summary>
        /// <param name="renderAsModel"></param>
        public RenderAsModel(
           // RenderAsModel original, 
            Vector3 customColor0, Vector3 customColor1, Vector3 customColor2, Vector3 customColor3,
            Matrix localTransform,
            Vector3 location,
            Vector3 facingNormal,
            string finalModelName,
            string finalModelTextureName,
            float finalModelScale,
            AnimatedModel originalAnimatedModel,
            Renderable newRenderable, 
            bool setAnimationFlagsDirty,
            IKnownEntityData parent)
            : base(null, newRenderable)
        {
         
            //ModelData = original.ModelData;

            FinalModelScale = finalModelScale; // original.FinalModelScale;
            //FinalModelBasicTexture = original.FinalModelBasicTexture;

            FinalModelBasicTextureName = finalModelTextureName;            
            if (!string.IsNullOrEmpty(FinalModelBasicTextureName))
            {
                FinalModelBasicTexture = GameData.Instance.ExtraModelTextures[FinalModelBasicTextureName];
            }

            FinalModelName = finalModelName; // original.FinalModelName;
            ModelData = GameData.Instance.AllModels[FinalModelName];

            this.facingNormal = facingNormal; // original.facingNormal;
            this.location = location; // original.location;

            this.localTransform = localTransform; // original.localTransform;

            CustomColor0 = customColor0; // original.CustomColor0;
            CustomColor1 = customColor1; //original.CustomColor1;
            CustomColor2 = customColor2; //original.CustomColor2;
            CustomColor3 = customColor3; //original.CustomColor3;

            Parent = parent;

            canAnimate = setAnimationFlagsDirty;


            if (originalAnimatedModel != null)
            {
                // for new memoryfacts and carcasses, use the existing model data to create a frozen pose:
                switch (ModelData.ModelType)
                {
                    case ModelType.Skinned:
                        AnimatedModel = new SkinnedAnimatedModel((SkinnedAnimatedModel)originalAnimatedModel, canAnimate);

                        break;

                    case ModelType.Stiff:
                        AnimatedModel = new StiffAnimatedModel((StiffAnimatedModel)originalAnimatedModel, canAnimate);
                        break;
                }

                ModelAnimator = AnimatedModel.ModelAnimator;

                // memoryfacts don't animate, they keep the current pose, but items do
                animationFlagsAreDirty = setAnimationFlagsDirty; // false;

                if (animationFlagsAreDirty)
                {
                    CreateAnimationTracks();
                }
            }
            else
            {
                // for snapshotted memory facts, recreate the AnimatedModel from scratch, we don't want to snapshot all its obscure data...
                InitializeAnimatedModel(true); // it is necessary to create tracks... // false);
            }
        }


        public void SetAnimFlagsDirty()
        {
            animationFlagsAreDirty = true;
        }

        /// <summary>
        /// snaps the renderable to the parent's location and direction
        /// </summary>
        public void SetToParentLocation() //Vector3 loc)
        {
            //these values lerp to the parent values each time matrices are computed
            location = Parent.PlaySiteLocation;
            facingNormal = Parent.FacingNormal;

        }

        public void Initialize()
        {
            ModelData = GameData.Instance.AllModels[FinalModelName];

            if (Parent is Entity)
            {
                ((Entity)Parent).BoundingRadius3D = ModelData.BoundingSphereRadius * FinalModelScale;

            }

            if (!string.IsNullOrEmpty(FinalModelBasicTextureName))
            {
                FinalModelBasicTexture = GameData.Instance.ExtraModelTextures[FinalModelBasicTextureName];
            }

            InitializeAnimatedModel(true);

            // used by attachable objects:
            Matrix modelOrientationMatrix = Matrix.Identity;

            Matrix rotate = Matrix.Identity;
            Matrix translate = Matrix.Identity;

            // matrix mult order:        
            Matrix scale = Matrix.CreateScale(FinalModelScale);

            localTransform = scale * rotate * translate;
        }

        private void InitializeAnimatedModel(bool createAnimationTracks)
        {
            switch (ModelData.ModelType)
            {
                case ModelType.Skinned:
                    AnimatedModel = new SkinnedAnimatedModel();
                    break;
                case ModelType.Stiff:
                    AnimatedModel = new StiffAnimatedModel();
                    break;
            }

            if (AnimatedModel != null)
            {
                AnimatedModel.Initialize(ModelData.Model);

                ModelAnimator = AnimatedModel.ModelAnimator;

                if (createAnimationTracks)
                {
                    CreateAnimationTracks();
                }
            }
        }

        private void CreateAnimationTracks()
        {

            mainAnimation = new AnimationTrack(AnimatedModel.ModelAnimator, AnimationTrack.TrackType.Main);
            secondGaitAnimation = new AnimationTrack(AnimatedModel.ModelAnimator, AnimationTrack.TrackType.ExtraGait); // false, true);

            additionalAnimationTracks = new List<AnimationTrack>();
            for (int i = 0; i < 2; i++)
            {
                // add a few additional tracks also:
                additionalAnimationTracks.Add(new AnimationTrack(AnimatedModel.ModelAnimator, AnimationTrack.TrackType.Additional) { Index = i });
            }
        }

        /// <summary>
        /// this computes the shadow transformation and draws model using the shadow effect.
        /// </summary>
        public void DrawShadow()
        {
            AnimatedModel.DrawShadow(new Vector3(0f, 0f, 0f)); //-13));
        }

       

        public double? GetUpdateInterval()
        {
            bool requiresUpdate = false;


            if (AnimatedModel != null && canAnimate)
            {
                // always filled...
                // TODO: detect non-animating objects (box, spear) that should not require Update!
                requiresUpdate = true;
            }

            if (requiresUpdate)
            {
                return 0; // models require updating every frame
            }
            else
            {
                return null; // no updates needed, we can sleep (for inanimate objects like a spear - TODO: test this!!!)
            }


        }

        public void Update(GameTime gameTime) 
        {
         
            // gets called on carcasses and other non animating entities too

            // here we update the AnimationControllers that are active (current + blending)
            if (mainAnimation != null)
            {
                mainAnimation.Update(gameTime);
            }
          
            if (secondGaitAnimation != null)
            {
                secondGaitAnimation.Update(gameTime);
            }

          
            if (additionalAnimationTracks != null)
            {
                foreach (var item in additionalAnimationTracks)
                {
                    item.Update(gameTime);
                }               
            }

            // NEW:
            if (IsAnimated)
            {
                AnimatedModel.Update(gameTime);
            }            
        }


        private bool IsAnimated
        {
            get
            {
                return AnimatedModel != null && canAnimate;
            }
        }

        public void CopyAbsoluteTransforms()
        {
            if (IsAnimated)
            {
                AnimatedModel.ModelAnimator.CopyAbsoluteTransforms();              
            }  
        }
       

        public static float LocationLerpLimit = 0.5f; // a limit of 0.5 causes lerping to switch to actual position when both the x and y distances are 0.5 or less.
        public static float RotationLerpLimit = 0.005f;

        // This overload uses local members
        public void ComputeMatricesForDrawing(AnimatedModel.Transformations transformations, float scale)
        {

           /* if (!Parent.FacingNormal.HasValue) // Parent.Locomotor == null)
            {
                //string name = Parent.Name;
                //throw new Exception("no locomotor in " + name);
                return;
            }
            */
            bool updateLocation = false;
            bool isUnstartedStructure = false;
            if (ParentEntity != null && ParentEntity.IsStarted() == false) // !ParentEntity.IsStarted())
            {
                // structures being placed must update while paused so they can follow the cursor
                updateLocation = true;
                isUnstartedStructure = true;
            }
            else if (!The.Sim.IsPaused)
            {
                // others, only when unpaused
                updateLocation = true;
            }

            if (updateLocation) //!The.Sim.IsPaused)
            {

                bool lerpAndAnimate = true;

#if DEBUG || PROFILE
                // don't animate/lerp when AI is disabled
                Intelligence intelligence;
                if (ParentEntity != null && !isUnstartedStructure && ParentEntity.EntityType.IntelligenceType != null)
                {
                    if (ParentEntity.Find(out intelligence) && intelligence.DisableAI)
                    {
                        lerpAndAnimate = false;
                    }
                }

#endif
                if (lerpAndAnimate)
                {
                    //Lerp location and rotation:
                    // TODO: get rid of lerping. Use steering to smooth out movement instead
                    if (Renderable.CanLerpLocation())
                    {
                        if (Renderable.LerpableWasRenderedLastFrame) 
                        {
                            // NEW: only lerp if we were drawn last frame! otherwise snap to location.
                            // don't lerp while paused
                            LerpLocationTowardsEntity();

                            SetMoveAnimationStates();// memoryfacts do not animate (lerp also???)
                        }
                        else
                        {
                            // first time the renderable is in view:
                            location = Parent.PlaySiteLocation;
                            facingNormal = Parent.FacingNormal;
                        }

                        Renderable.LerpableWasRenderedLastFrame = true;

                    }
                    else
                    {
                        
                        // memoryfacts don't lerp:
                        location = Parent.PlaySiteLocation;
                        facingNormal = Parent.FacingNormal;

                        // if lerping is disabled:
                      /*  if (Renderable.Entity != null
                            && Renderable.Entity.EntityType.LocomotorType != null // only lerp movers
                            && Renderable.Entity.IsStarted() != false) // don't lerp structures being placed
                        {
                            SetMoveAnimationStates();// memoryfacts do not animate
                        }*/
                    }

                }

                previousLocation = location;
                previousFacingNormal = facingNormal;
            }

            ComputeMatricesForDrawing(transformations, scale, location, facingNormal, Parent.Rotation, null, null);
        }

       

        private void LerpLocationTowardsEntity()
        {
            // Lerping (lagging behind) is useful for softening curves when moving... but we don't want any lag between the entity and its renderable when they arrive. Otherwise animations may be cut off.
            // So near the destination we want the renderable to always be at the entity location.


            bool doLocationLerping, doRotationLerping;


            //  if (Math.Abs((location - Parent.Location).LengthSquared()) > LocationLerpLimit)// don't keep lerping forever.
            doLocationLerping = false; // NEW: don't lerp location! // true;

            //   if (Math.Abs((facingNormal - Parent.FacingNormal.Value).LengthSquared()) > RotationLerpLimit)
            doRotationLerping = true;
        

            
            float distanceFactor = 1f;

            if (doLocationLerping || doRotationLerping)
            {
                float distanceToDestination;
                if (ParentEntity.Locomotor != null)
                {
                    distanceToDestination = Common.DistanceOctile(Location, ParentEntity.Locomotor.CurrentMoveTarget);
                }
                else
                {
                    distanceToDestination = 0f;
                }


                distanceFactor = distanceToDestination - DistanceToStopLerping;

                distanceFactor /= (distanceFactorToReduceLerping * DistanceToStopLerping);

                distanceFactor = Common.Clamp(distanceFactor, 0f, 1f);

                //(x-1)^(5) + 1 <- this formula makes distance factor fall off very slowly from 1, which is necessary because of the way lerping works
                distanceFactor = (float)Math.Pow(distanceFactor - 1d, 5d) + 1f;
            }

            if (doLocationLerping
                && !Common.IsZero(distanceFactor) // had to add this in 64 bit, otherwise it would lerp forever
                && !Common.IsLocationEqual(location, Parent.Location.Value)) 
            {
                // a lerpfactor of zero means the renderable will be drawn at the entity's location
                float lerpFactorToUse = MathHelper.Lerp(0f, LerpFactor, distanceFactor);

               
                // TODO: is clamping needed here to prevent very fast movement on slower pcs when running???

              //  location = location * LerpFactor + Parent.Location * (1.0f - LerpFactor);
                location = location * lerpFactorToUse + Parent.Location.Value * (1.0f - lerpFactorToUse);
            }
            else
            {
                location = Parent.Location.Value;
            }

            if (doRotationLerping
                 && !Common.IsZero(distanceFactor)) // had to add this in 64 bit, otherwise it would lerp forever) 
            {
                float lerpFactorToUse = MathHelper.Lerp(0f, LerpRotationFactor, distanceFactor);

                facingNormal = facingNormal * lerpFactorToUse + Parent.FacingNormal * (1.0f - lerpFactorToUse);
                facingNormal.Normalize();
            }
            else
            {
                facingNormal = Parent.FacingNormal;
            }
        }

        private void SetMoveAnimationStates()
        {
            
            // set animation states depending on the renderable's movement, not the entity.
            if ((previousLocation == null ||  !Common.IsLocationEqual(previousLocation.Value, location)) // previousLocation != location
                || (previousFacingNormal == null || !Common.IsDirectionEqual(previousFacingNormal.Value, facingNormal)) 
                || ParentEntity.Locomotor.IsMoving()) // I have seen this method being called where there was no change in location/rotation while the entity was moving. This caused the move flag to be cleared...                    
            {                                             // avoid this by checking move speed also...

                if (ParentEntity.IsHaulingNothingMounted())
                {
                    SetHaulingAnimStates();
                }
                else
                {
                    ClearHaulingAnimStates();

                    Renderable.SetAnimationActionStateFlag(AnimAction.Moving);
                  
                }

                UpdateGaitAnimation();
            }
            else
            {
                ClearHaulingAnimStates();
                ClearAnimationActionStateFlag(AnimAction.Moving);

            }

            
            
        }

      //  const float strideDuration = 1.92f / 2f; // the anims are 1.92 s long, but contain 2 steps 

        private void UpdateGaitAnimation()
        {
            
            if (SelectedAnimInfo == null || SelectedAnimInfo.GaitSetKey == null)
                return;


            float renderableSpeed = 0f;
            float translationSpeed = 0f;

            if (previousLocation.HasValue)
            {
                double timePassedInSeconds = The.Client.ElapsedTimeBetweenDraws.TotalSeconds; // CAN BE ZERO!!! ???

                //    The.Client.GameTime.ElapsedGameTime.TotalSeconds); // <-- NOTE!!! this is NOT the elapsed time between Draw calls (where we lerp) but between Update calls! Draw frames may have been dropped!

                if (timePassedInSeconds == 0d)
                {
                    return; // apparently some Draw calls happen with no time difference. in that case, abort.
                }

                renderableSpeed = (float)(Vector3.Distance(Location, previousLocation.Value) / timePassedInSeconds);

                translationSpeed = renderableSpeed;

               

                if (renderableSpeed < 3f)
                {
                    if (previousFacingNormal.HasValue)
                    {
                        
                        //only take rotation/turning into account when almost standing still...
                        double angle = Common.GetAngleBetweenVectors(previousFacingNormal.Value.ToVector2(), facingNormal.ToVector2());

                        /* //these settings worked with 'normal' turning speed: 
                          
                        double movedArc = 12f * angle; // 6f * angle; // assume x pixels radius...

                        float arcSpeed = (float)(movedArc / timePassedInSeconds);

                        renderableSpeed += arcSpeed;
                        */

                        // these setting work better with the new 'cardboard' turn (as in Jullerup Færgeby, Danish puppet show)
                        // they prevent crazy running in place:
                        double movedArc = 6f * angle; 

                        float arcSpeed = (float)(movedArc / timePassedInSeconds);

                        renderableSpeed += arcSpeed;

                        renderableSpeed = Common.ClampTop(renderableSpeed, 32f);
                        
                    }
                }
                else
                {
                    // prevent crazy running animation when framerate drops:
                    if (ParentEntity.Locomotor.MoveSpeed > 0f) // is only set when performing GoalMoveToPosition or other AI driven movement
                    {
                        //renderableSpeed = Common.ClampTop(renderableSpeed, ParentEntity.Locomotor.MoveSpeed * 1.4f); // Client.MaxSpeed);
                        renderableSpeed = Common.Clamp(renderableSpeed, 0.001f, ParentEntity.Locomotor.MoveSpeed * 1.4f); // Client.MaxSpeed);
                    }
                }
            }

            GaitAnimationBracket anim1, anim2;
            float anim1Weight;
            GetMoveAnimationsToUse(renderableSpeed, SelectedAnimInfo.GaitSetKey, out anim1, out anim2, out anim1Weight);

         //   The.Client.Log.AddLogEvent(The.Client.Log.DebugEvent, null, "   " + translationSpeed.ToString("N2") + "   " + renderableSpeed.ToString("N2") + "   " + anim1.AnimationKey);

            
            float strideLength;
            float strideDuration;
            if (anim2 != null)
            {
                strideLength = MathHelper.Lerp(anim2.StrideLength, anim1.StrideLength, anim1Weight); 
                strideDuration = MathHelper.Lerp(anim2.StrideDuration, anim1.StrideDuration, anim1Weight); 
            }
            else
            {
                strideLength = anim1.StrideLength;
                strideDuration = anim1.StrideDuration;
            }

            // the animations move x frames per stride, where a stride is y pixels. the  character moves at z pixels per second. solve for x.

            // movespeed = k * stridelength / strideDuration
            // k = movespeed * strideDuration / strideLength

            float animationSpeed = /*0.3f **/ /*ParentEntity.Locomotor.MoveSpeed*/ renderableSpeed * strideDuration / strideLength;


            StartGaitAnimations(anim1, anim2, animationSpeed, anim1Weight);

        }

        float previousGaitSpeed;
        string previousMainGait, previousSecondGait;
        float previousAnim1Weight;

        private void StartGaitAnimations(GaitAnimationBracket animBracket1, GaitAnimationBracket animBracket2, float speedFactor, float anim1Weight)
        {

            // see if it is necessary to change the anims:
            if (!((animBracket1 != null ? animBracket1.AnimationKey : null) != previousMainGait
                || (animBracket2 != null ? animBracket2.AnimationKey : null) != previousSecondGait
                || Math.Abs(speedFactor - previousGaitSpeed) > 0.001 
                || Math.Abs(anim1Weight - previousAnim1Weight) > 0.001))
            {
                return;
            }

#if DEBUG || PROFILE
            if ((animBracket1 != null ? animBracket1.AnimationKey : null) != previousMainGait)
            {
                The.MapUI.AddDebugMarker(Location + new Vector3(0f, 20f, 0f), Color.Pink, this);
            }
#endif
           
            previousMainGait = (animBracket1 != null ? animBracket1.AnimationKey : null);
            previousSecondGait = (animBracket2 != null ? animBracket2.AnimationKey : null);
            previousAnim1Weight = anim1Weight;
            previousGaitSpeed = speedFactor;


            //CurrentAnimInfo.CurrentBaseAnimKey = null;


            AnimationController anim1 = null, anim2 = null;
            anim1 = AnimatedModel.ModelAnimator.GetAnimControllerFromKey(animBracket1.AnimationKey);
    
            if (animBracket2 != null)
            {
                anim2 = AnimatedModel.ModelAnimator.GetAnimControllerFromKey(animBracket2.AnimationKey);          
            }



            // always play looped, forward only.
            // start from beginning unless already active.

            // gait anims must always be synchronized!

           // float startingPoint;

            long? startOffset = null;
            StartingPoint startingPoint = StartingPoint.Current;

            if (mainAnimation.currentController == null)
            {
                // no anim is running. Start at the beginning.

                if (anim2 != null)
                {                 

                    startingPoint = StartingPoint.FromBeginning;

                }

                startingPoint = StartingPoint.FromBeginning;


            }
            else
            {
                // an anim is currently running - see if we should replace it
                if (mainAnimation.currentController == anim1)
                {
                    // the main gait anim is already running. 
                 
                    startOffset = anim1.ElapsedTimeMinusStartOffset;
                    startingPoint = StartingPoint.Specified;

                }
                else
                {
                    // a different main anim is running. Replace it:
                    if (mainAnimation.currentController.AnimationInfo.IsGaitAnim)
                    {
                        // only sync if it the current one is a gait too:
                       
                        startOffset = mainAnimation.currentController.ElapsedTimeMinusStartOffset;
                        startingPoint = StartingPoint.Specified;
                    }
                    else
                    {                      
                        // start the gait from the beginning
                        startingPoint = StartingPoint.FromBeginning;
                    }
                                       
                }
            }

            /*
            if (anim1 == null)
            {

            }

            if (speedFactor == 0f)
            {
                MapClient.AddDebugMarker(Location + new Vector3(10f, 20f, 0f), Color.Violet, this);

            }*/

            bool replacedAnim = false;
            mainAnimation.StartAnimation(anim1, Playback.Forwards, startingPoint, BlendMode.Normal, out replacedAnim, speedFactor, 
                Looping.Yes, startOffset, random: The.Client.ClientRandomGenerator.Random);

          /*  if (replacedAnim)
            {
                MapClient.AddDebugMarker(Location + new Vector3(0f, 20f, 0f), Color.Pink, this);
            }
            */
            // stay sync'ed!
            startOffset = anim1.ElapsedTimeMinusStartOffset; 
         
          //  secondGaitAnimation.StartAnimation(anim2, Playback.Forwards, startingPoint, BlendMode.Normal, speedFactor, Looping.Yes, startOffset);

            // give the second gait anim a weight that is equal to the inverse of the main anim's weight:
            secondGaitAnimation.WeightFactor = 1f - anim1Weight;

        /*    if (mainAnimation.currentController.AnimationInfo.IsGaitAnim
               && mainAnimation.currentController.SpeedFactor == 0f)
            {
                MapClient.AddDebugMarker(Location + new Vector3(0f, 26f, 0f), Color.Red, this);

            }
           
            if (mainAnimation.currentController.AnimationInfo.IsGaitAnim
              && secondGaitAnimation.currentController != null
              && secondGaitAnimation.currentController.SpeedFactor != speedFactor)
            {

            }*/

        /*    if (mainAnimation.currentController.AnimationInfo.IsGaitAnim)
            {
                mainAnimation.currentController = AnimatedModel.ModelAnimator.GetAnimControllerFromKey("gaitWalk");
                mainAnimation.controllerBeingBlendedTo = AnimatedModel.ModelAnimator.GetAnimControllerFromKey("gaitJog");

                mainAnimation.currentController.SpeedFactor = speedFactor;
                mainAnimation.controllerBeingBlendedTo.SpeedFactor = speedFactor;

                mainAnimation.blendingProgress = 0.1f;

                AnimatedModel.ModelAnimator.UpdateModelBones(mainAnimation); // currentController);
               
            }
 */
            /*
            if (mainAnimation.currentController.AnimationInfo.IsGaitAnim 
              && secondGaitAnimation.currentController != null
              && secondGaitAnimation.currentController.SpeedFactor > 0f)
            {
                if (mainAnimation.currentController.SpeedFactor != secondGaitAnimation.currentController.SpeedFactor)
                {
                    secondGaitAnimation.currentController.SpeedFactor = mainAnimation.currentController.SpeedFactor;
                }
            }*/
        }

        


        /// <summary>
        /// second gait anim not currently used
        /// </summary>
        /// <param name="moveSpeed"></param>
        /// <param name="gaitAnimsKey"></param>
        /// <param name="anim1"></param>
        /// <param name="anim2"></param>
        /// <param name="anim1Weight"></param>
        private void GetMoveAnimationsToUse(float moveSpeed, string gaitAnimsKey, out GaitAnimationBracket anim1, out GaitAnimationBracket anim2, out float anim1Weight)
        {
           
            GaitAnimationBracket[] anims = RenderAsModelType.GaitAnimations[gaitAnimsKey];

            anim1 = null;
            anim2 = null;
            anim1Weight = 1f;

            GaitAnimationBracket currentAnim;

            //float anim1Weight;

            float distanceFromAnim1MaxSpeed = 0f; 
            float anim1MaxSpeed = 0f;

            // iterate through the brackets to find 1 or 2 (if in the overlapping interval) anims to play
            for (int i = 0; i < anims.Length; i++)
            {
                currentAnim = anims[i];

                if (moveSpeed >= currentAnim.MinimumSpeed && moveSpeed <= currentAnim.MaximumSpeed)
                {
                    // the speed falls inside the bracket. how far are we from the 'edge':
                    
                    if (anim1 == null)
                    {
                        anim1 = currentAnim;

                        distanceFromAnim1MaxSpeed = currentAnim.MaximumSpeed - moveSpeed;

                        anim1MaxSpeed = currentAnim.MaximumSpeed;

                        anim1Weight = 1f;
                    }
                    else
                    {
                        anim2 = currentAnim;

                     //   distanceFromAnim2MinEdge = moveSpeed - currentAnim.MinimumSpeed;

                        // compute the weighting:
                        float overlappingInterval = anim1MaxSpeed - currentAnim.MinimumSpeed;

                        anim1Weight = distanceFromAnim1MaxSpeed / overlappingInterval; // MathHelper.Lerp(0f, 1f,  

                        anim1Weight = Common.Clamp(anim1Weight, 0f, 1f);

                        break; // done
                    }
                }
            }

            if (anim1 == null)
            {
                anim1Weight = 1f;
                anim1 = anims[anims.Length - 1]; // default anim to use in case of very high speeds
            }
                    

        }

        private void SetHaulingAnimStates()
        {
            ClearAnimationActionStateFlag(AnimAction.Moving);

            SetAnimationActionStateFlag(AnimAction.Hauling);
            
            float burdenPercentage = ParentEntity.AgentStorage.GetHaulingPercentageOfCapacity();
           
            AnimModifier? burdenStateFlag = Renderable.GetBurdenAnimStateFlag(burdenPercentage); // storedPercentage);

            if (burdenStateFlag == AnimModifier.Heavy)
            {
                SetAnimationStateFlag(AnimModifier.HaulHeavy);
            }
            else
            {              
                ClearAnimationStateFlag(AnimModifier.HaulHeavy);
            }

            BoxHandlingWhenHauling boxHandling = Renderable.RenderableType.BoxHandlingWhenHauling;

            // this logic would be better if placed in AnimConditions with TemporaryAttachables. But there is also attachment code in pickup and drop...
            // I abandoned temporary attachables because it is too fickle...
            if (burdenStateFlag.HasValue /*burdenStateFlag == AnimModifier.Heavy
                &&*/ 
                && boxHandling != null
                && boxHandling.BoxHandling != BoxHandlingWhenHauling.BoxHandlingType.AlwaysInHand)
            {
                if (boxHandling.BoxHandling == BoxHandlingWhenHauling.BoxHandlingType.AlwaysOnBack
                    || (boxHandling.BoxHandling == BoxHandlingWhenHauling.BoxHandlingType.OnlyOnBackWhenHeavyAndHaulingFar
                        && burdenStateFlag == AnimModifier.Heavy
                        && Renderable.AnimConditions.Modifiers.Test(AnimModifier.Far)))
                {
                    if (boxHandling.UseHeavyBackpack)
                    {
                        // attach the backpack model if needed:  
                        // cannot attach this if other object is attached to back already
                        Renderable.AttachModelToBack("backpackHeavy", AttacheePoint.Back);
                    }
                    else
                    {
                        Renderable.AttachModelToBack("box", AttacheePoint.Bottom);
                    }
                }
                else 
                {
                    // cannot attach this if other object is attached to hand already                    
                    Renderable.AttachBoxModelToHand(burdenStateFlag); // /*false, boxHandling*/);
                }
            }
            else
            {
                // don't remove the box.
                // it gets removed in AgentStorage........
            }

        }

        public static void GetAttachTransformations(Renderable attachable, AttachPoint attachor, AttacheePoint? attacheePointName, AnimConditionInfo animCondition, out AttachPoint attacheePoint, out Vector3 translation, out Vector3 rotation)
        {
            AppliedAttachableTransforms appliedRotationTransform;
            AppliedAttachableTransforms appliedTranslationTransform;
            GetAttachTransformations(attachable, attachor, attacheePointName, animCondition, out attacheePoint, out translation, out rotation, out appliedRotationTransform, out appliedTranslationTransform);
        }

        public enum AppliedAttachableTransforms { Attachor, Attachee, Animation }
        /// <summary>
        /// if the current anim state has defined another attach point transform, return that
        /// 
        /// the priority is:
        /// 1. AnimCondition
        /// 2. Attachee
        /// 3. Attachor
        /// </summary>
        /// <returns></returns>
        public static void GetAttachTransformations(Renderable attachable, AttachPoint attachor, AttacheePoint? attacheePointName, 
            AnimConditionInfo animCondition, 
            out AttachPoint attacheePoint, out Vector3 translation, out Vector3 rotation,
            out AppliedAttachableTransforms appliedRotationTransform,
            out AppliedAttachableTransforms appliedTranslationTransform)
        {
            string attachableKey = attachable.RenderableType.KeyName; 

            translation = Vector3.Zero;
            rotation = Vector3.Zero;

            ModelData modelData = attachable.RenderAsModel.ModelData;

            attacheePoint = modelData.GetAttacheePoint(attacheePointName);


            if (animCondition != null)
            {
                // are there special attach transforms defined for the current anim?
                if (animCondition.AttachPoints != null)
                {
                    appliedTranslationTransform = AppliedAttachableTransforms.Animation;
                    appliedRotationTransform = AppliedAttachableTransforms.Animation;

                    // find the best match...
                    foreach (var item in animCondition.AttachPoints)
                    {
                        if (item.AttacheePoint == attacheePointName && item.RenderableTypeKey == attachableKey)
                        {
                            translation = item.Translation;
                            rotation = item.Rotation;
                           
                            return;
                        }
                    }

                    foreach (var item in animCondition.AttachPoints)
                    {
                        if (item.AttacheePoint == attacheePointName)
                        {
                            translation = item.Translation;
                            rotation = item.Rotation;
                           
                            return;
                        }
                    }

                    foreach (var item in animCondition.AttachPoints)
                    {
                        if (item.RenderableTypeKey == attachableKey)
                        {
                            translation = item.Translation;
                            rotation = item.Rotation;
                           
                            return; // item.AttachPoint;
                        }
                    }

                    translation = animCondition.AttachPoints[0].Translation; // use a default transform...
                    rotation = animCondition.AttachPoints[0].Rotation;
                                       
                    return;
                }
            }

            //else look at the default settings:

            // attachee data overrides attachor data!
             // attachee transformations should override attachor!
            if (attacheePoint != null && attacheePoint.Translation.HasValue)
            {
                translation = attacheePoint.Translation.Value;
                appliedTranslationTransform = AppliedAttachableTransforms.Attachee;
            }
            else if (attachor.Translation.HasValue)
            {
                translation = attachor.Translation.Value;
                appliedTranslationTransform = AppliedAttachableTransforms.Attachor;
            }
            else
            {
                appliedTranslationTransform = AppliedAttachableTransforms.Attachor;
            }

            if (attacheePoint != null && attacheePoint.Rotation != new Vector3(0f, 0f, 0f))
            {
                rotation = attacheePoint.Rotation;
                appliedRotationTransform = AppliedAttachableTransforms.Attachee;
           
            }
            else if (attachor.Rotation != new Vector3(0f, 0f, 0f))
            {
                rotation = attachor.Rotation;
                appliedRotationTransform = AppliedAttachableTransforms.Attachor;
            }
            else
            {

                appliedRotationTransform = AppliedAttachableTransforms.Attachor;
            }
        }

        private void ClearHaulingAnimStates()
        {
            ClearAnimationActionStateFlag(AnimAction.Hauling);
          
            ClearAnimationStateFlag(AnimModifier.HaulHeavy);
        }

        /// <summary>
        /// NOTE: this location may be different from the Renderable location (which is the same as the Entity location). It will lerp towards the renderable's location.
        /// </summary>
        public Vector3 Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
            }
        }
        private Vector3 location = Vector3.Zero;
        private Vector3? previousLocation;

        /// <summary>
        /// NOTE: this heading may be different from the Renderable heading (which is the same as the Entity location). It will lerp towards the renderable's location.
        /// </summary>
        public Vector3 FacingNormal
        {
            get
            {
                return facingNormal;
            }
            set
            {
                facingNormal = value;
            }
        }

        private Vector3 facingNormal = Vector3.UnitX;
        private Vector3? previousFacingNormal;

        static public float LerpFactor = 0.87f; // 0.93f; // 0.97f; // <- a bit looser // 0.87f <- almost no lagging
        static public float LerpRotationFactor = 0.85f; // 0.9f; // 0.90f -make rotation a bit snappier    MP:...wanted to tweak the rotation speed with wich they turn to face each other (it's very slow and robotic.."dzchiiiiiiii"). 
                                                                                                    //but it's not affected by this number. LARS: No, this is only for smoothing of the real rotation speed. 2f makes them walk backwards. 0.1f: not sure what that did?

        /// <summary>
        /// the max distance the renderable is allowed to separate itself from its real position
        /// </summary>
        public static float DistanceToStopLerping = 24f; // 48f;

        /// <summary>
        /// a multiple of the lag cutoff distance where we want to gradually reduce lagging from.
        /// </summary>
        private const float distanceFactorToReduceLerping = 8f; // 4f; 



        // This overload is for external callers, like GameWorldRenderer
        public void ComputeMatricesForDrawing(AnimatedModel.Transformations transformations, float scale,
                                                                       Vector3 _LOC_, Vector3 _DIR_, float _ROT_,
                                                                        float? _ROLL_, float? _PITCH_)
        {
            
            //  scale *= 2f;  /// CONVENIENT PLACE TO HACK THE SCALE OF ALL ANIMATED MODELS


            //Vehicle vehicleComponent;
            if (!_ROLL_.HasValue)  //!Parent.Find(out vehicleComponent)) //???
            {
                // for some reason, we must invert the facing direction...
                Vector3 direction = -(new Vector3(_DIR_.X, _DIR_.Y, 0f));
                Vector3 Up = -Vector3.UnitZ;
                Vector3 Right = Vector3.Cross(direction, Up);

                AnimatedModel.ComputeMatricesForDrawing(direction, Up, Right, scale, _LOC_,
                                        ModelData.ModelOffset, transformations);
            }
            else
            {   // OLD: Vehicles can Roll (others too?)...

                Matrix modelOrientationMatrix = Matrix.Identity;

                // start off with a pair of rotations to fit the strange coordinate system in the game:
                modelOrientationMatrix *= Matrix.CreateFromAxisAngle(Vector3.UnitZ, -MathHelper.PiOver2);
                modelOrientationMatrix *= Matrix.CreateFromAxisAngle(modelOrientationMatrix.Right, -MathHelper.PiOver2);


                float roll = _ROLL_.HasValue ? _ROLL_.Value : 0;
                float pitch = _PITCH_.HasValue ? _PITCH_.Value : 0;


                // now we can do the real rotations:
                modelOrientationMatrix *= Matrix.CreateFromAxisAngle(Vector3.UnitZ, _ROT_);
                modelOrientationMatrix *= Matrix.CreateFromAxisAngle(modelOrientationMatrix.Backward, roll);
                modelOrientationMatrix *= Matrix.CreateFromAxisAngle(modelOrientationMatrix.Right, pitch);


                AnimatedModel.ComputeMatricesForDrawing(modelOrientationMatrix.Forward, modelOrientationMatrix.Up,
                                                                            modelOrientationMatrix.Right, scale,
                                                                            _LOC_, ModelData.ModelOffset,
                                                                            transformations);
            }

            // do attached objects here
            AnimatedModel.ModelAnimator.ComputeTransformsForAttachedObjects(scale);

        }



        private bool ReplaceAnimationConditionState()
        {
            if (RenderAsModelType.AnimConditions == null
                && RenderAsModelType.DefaultInfo == null) // spears and other objects don't have anims
                return true;

            
            //TODO if we implement an LOD system, then at low LOD we should bail here

            //do the best match trick, on the current bits
            //this retrieves a reference to the read-only data defined in RenderAsModelType

            AnimConditionInfo info;
            RenderAsModelType.FindBestAnimInfo(Renderable.AnimConditions, out info);

                   
            // filter state changes that we don't want for various reasons:
            if (WaitWithFillerAnim())
            {
                return false; // leave the flag dirty for another couple of frames...
            }

            if (IsUnnecessaryIdleChange(info))
            {
                return true;
            }
            else if (IsUnnecessaryStanceChange(info))
            {
                return true;
            }
            else if (info != null)
            {
                if (info.SoundAndAnimationSet != null 
                    && info.SoundAndAnimationSet.BaseAnimations != null 
                    && info.SoundAndAnimationSet.BaseAnimations.Contains("idleToSleep"))
                {

                }

                AdoptAnimInfo(info);
            }

            return true;
         
        }

        /// <summary>
        /// purpose: avoid twitching to the default anim when the same flags are cleared and set again the next atomic goal, like DoProduce
        /// </summary>
        /// <returns></returns>
        private bool WaitWithFillerAnim()
        {
            if (Renderable.AnimConditions.Action == null && noOfFramesWeHaveBeenDirty < 4) // are we temporarily without an action flag?
            {
                if (SelectedAnimInfo != null && SelectedAnimInfo.Looping == Looping.Yes)
                {
                    //leave the current looping animation running for another frame or two... 
                    //don't start the filler default anim yet. see if we don't get an action flag again soon...
                    return true;
                }
            }

            return false;
        }


        private bool IsUnnecessaryIdleChange(AnimConditionInfo info)
        {
            // This will avoid anim changes after each GoalDoTakeFive finishes, when Idle flag is cleared. Don't play the idle filler anim, keep the current one and wait until the next Goal starts.
            // test if newInfo is DefaultInfo (Idle?)              
            // and if we are already playing an anim from an (idle) set that contains this anim key... then continue playing it... to avoid 'twitching' from kneeling idle for instance
            // because GoalDoTakeFive will clear Idle state and set it right after - this results in twitching when playing the standing idle for a brief moment      
            if ((info == RenderAsModelType.DefaultInfo || 
                (RenderAsModelType.DefaultStances != null && RenderAsModelType.DefaultStances.Contains(info))) // NEW!
              && SelectedAnimInfo != null
              && SelectedAnimInfo.ConditionSet != null 
              && SelectedAnimInfo.ConditionSet.Action == AnimAction.Idle)
            {
                return true;
            }
             
            return false;
        }

        /// <summary>
        /// if it is a ChangeStance action, returns the resulting end stance
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private AnimModifier? GetEndStance(AnimConditionInfo info)
        {

            if (info.ConditionSet.Modifiers == null)
            {
                // standing? probably not a valid case.
                return null;
            }
            else
            {
                BitMask64 modifiers = info.ConditionSet.Modifiers;

                bool kneelingIsSet = modifiers.Test(AnimModifier.Kneeling);
                bool sittingIsSet = modifiers.Test(AnimModifier.Sitting);
                bool lyingIsSet = modifiers.Test(AnimModifier.Lying);


                if (modifiers.Test(AnimModifier.Reverse))
                {    // getting down...

                    if (lyingIsSet)
                    {
                        return AnimModifier.Lying;
                    }
                    else if (sittingIsSet)
                    {
                        return AnimModifier.Sitting;
                    }
                    else if (kneelingIsSet)
                    {
                        return AnimModifier.Kneeling;
                    }

                }
                else
                {
                    // getting up...
                    if (lyingIsSet)
                    {
                        if (sittingIsSet)
                        {
                            return AnimModifier.Sitting;
                        }
                        else if (kneelingIsSet)
                        {
                            return AnimModifier.Kneeling;
                        }
                        else
                        {
                            return null; // -> stand
                        }

                    }
                    else if (sittingIsSet)
                    {
                        if (kneelingIsSet)
                        {
                            return AnimModifier.Kneeling;
                        }
                        else
                        {
                            return null; // -> stand
                        }
                    }
                    else if (kneelingIsSet)
                    {
                        return null; // -> stand
                    }

                }

            }

           

            return null;
        }

        /// <summary>
        /// the purpose of this is to avoid stance related anim errors that may have originated in AI goals,
        /// or when when doing client anim stuff...
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool IsUnnecessaryStanceChange(AnimConditionInfo info)
        {
            if (info == null
               || info.ConditionSet == null
               || info.ConditionSet.Action != AnimAction.ChangingStance)
            {
                return false;
            }

            StancesType stancesType = Parent.EntityType.LocomotorType.StancesType; 


            // null is for Standing/Default...
            AnimModifier? endStance = stancesType.GetEndStance(info); // GetEndStance(info);

            if (endStance != null)
            {
                if (SelectedAnimInfo != null
                    && SelectedAnimInfo.ConditionSet != null
                    && SelectedAnimInfo.ConditionSet.Modifiers != null
                    && SelectedAnimInfo.ConditionSet.Modifiers.Test(endStance.Value))
                {
                    return true; // we are already in this stance!
                }               
            }
            else 
            {
                // changing to standing:
                if (SelectedAnimInfo != null
                    && (stancesType.IsDefaultStance(SelectedAnimInfo.ConditionSet)))
                    //IsStanding(SelectedAnimInfo.ConditionSet)))
                {
                    return true; // already standing!
                }
            }

            return false;
        }

        private bool IsStanding(AnimConditions conditionSet) 
        {
            return conditionSet == null 
                || conditionSet.Modifiers == null // test that no stance flags are currently set
                || !(conditionSet.Modifiers.Test(AnimModifier.Sitting) 
                        || conditionSet.Modifiers.Test(AnimModifier.Kneeling)
                        || conditionSet.Modifiers.Test(AnimModifier.Lying));
        }
              
        /*
        public void CycleAlternateAnims()
        {

            if (SelectedAnimInfo == null)
                return;

            //random chance of switching to a different anim
            if (SelectedAnimInfo.HasManyKeys && Globals.Instance.Random.Next(99999) < 2)
                AdoptAnimInfo(SelectedAnimInfo);

        }
        */


        /// <summary>
        /// play the animations in the new set.
        /// 
        /// Note that setting a condition flag that was already set does not restart or start a new random anim.
        /// </summary>
        /// <param name="newInfo"></param>
        /// <returns></returns>
        private bool AdoptAnimInfo(AnimConditionInfo newInfo)
        {
            
            if (newInfo == null)
            {
                newInfo = RenderAsModelType.DefaultInfo; //default is always good
            }

             

            // store the currently playing anims before assigning the new info
            string previousBaseAnimKey = SelectedAnimInfo != null ? CurrentBaseAnimKey : null;
            Looping? previousBaseAnimIsLooping = SelectedAnimInfo != null ? SelectedAnimInfo.Looping : (Looping?)null;
            string previousBaseAnimBlendKey = SelectedAnimInfo != null ? mainAnimation.AnimKeyBeingBlendedTo : null;

            string previousAdditionalAnim1Key = SelectedAnimInfo != null ? CurrentAdditionalAnim1Key : null;
            string previousAdditionalAnim1BlendKey = SelectedAnimInfo != null ? additionalAnimationTracks[0].AnimKeyBeingBlendedTo : null;

            string previousAdditionalAnim2Key = SelectedAnimInfo != null ? CurrentAdditionalAnim2Key : null;
            string previousAdditionalAnim2BlendKey = SelectedAnimInfo != null ? additionalAnimationTracks[1].AnimKeyBeingBlendedTo : null;
      

            AnimConditionInfo oldInfo = SelectedAnimInfo;
            SelectedAnimInfo = newInfo;

            SoundData oldSound = null; // we have already tested for the same anim...
            SoundData newSound = null;

            if (SelectedAnimInfo.SoundAndAnimationSet != null)
            {
                if (SelectedAnimInfo.SoundAndAnimationSet.BaseAnimations != null)
                {
                    // handle any new animations here.
                    // gait animations are handled elsewhere (UpdateGaitAnim)


                    // select new base anim:
                    int? baseAnimIndex; // use this to pick a sound
                    string baseAnimKey = GetRandomAnimToSwitchTo(mainAnimation, SelectedAnimInfo.SoundAndAnimationSet.BaseAnimations, out baseAnimIndex);

                   
                    
                    if (baseAnimIndex != null
                        && baseAnimKey != null // will be null if GaitKey is filled (haulLight)...
                        && OKToStartAnimation(previousBaseAnimKey, baseAnimKey, previousBaseAnimBlendKey, // never restart a looping anim - there's no reason for it, and it will look wonky.     
                                    previousBaseAnimIsLooping, SelectedAnimInfo.Looping))
                    {

                        bool setCallback = false;

                        if (SelectedAnimInfo.Looping == Looping.Yes
                            && SelectedAnimInfo.SoundAndAnimationSet.BaseAnimations.Length > 1)
                        {
                            // if looping, pick a new random anim continually:
                            setCallback = true;

                        }

                        //MLo: It was not unusual to find diamondBirds flapping and landing in perfect unison.
                        //this adds just enough variance to animation speed to make them seem organic
                        float speedVariance = 0f;
                        if (Parent.EntityType.IsFlyer)
                        {
                            speedVariance = The.Client.ClientRandomGenerator.RandomBetween(-0.01f, 0.01f);
                        }


                        StartAnimation(baseAnimKey, mainAnimation, //CurrentAnimInfo.CurrentBaseAnimKey, 
                            SelectedAnimInfo.Playback,
                            SelectedAnimInfo.StartingPoint,
                            SelectedAnimInfo.BlendMode,
                            SelectedAnimInfo.SpeedFactor + speedVariance,
                            SelectedAnimInfo.Looping,
                            SelectedAnimInfo.StartingPointInSeconds, setCallback, PickNewRandomAnim);

                        if (SelectedAnimInfo.SoundAndAnimationSet.SoundData != null) // NEWSOUND: delete this after migrating
                        {
                            // start a new sound in sync
                            newSound = SelectedAnimInfo.SoundAndAnimationSet.SoundData[baseAnimIndex.Value];
                        }

                        // let's make sure that the secondary gait anim is not running by starting a null anim on it:
                        bool replacedAnim;
                        secondGaitAnimation.StartAnimation((AnimationController)null, Playback.Forwards, StartingPoint.FromBeginning, BlendMode.Normal, out replacedAnim, random: The.Client.ClientRandomGenerator.Random);

                    }        
                   
                }
                else if (SelectedAnimInfo.SoundAndAnimationSet.SoundData != null)
                {
                    // pick a random sound independently of animation:
                    newSound = Common.GetRandomListMember(SelectedAnimInfo.SoundAndAnimationSet.SoundData, The.Client.ClientRandomGenerator);
                }

                // play up to 2 additional anims in the set... can be null                 
                int? otherIndex; // not used
                string newAdditionalKey1 = GetRandomAnimToSwitchTo(additionalAnimationTracks[0], SelectedAnimInfo.SoundAndAnimationSet.AdditionalAnimations1, out otherIndex); // CurrentAnimInfo.AnimationSet.GetRandomAdditionalAnim1(CurrentAdditionalAnim1Key);

                string newAdditionalKey2 = GetRandomAnimToSwitchTo(additionalAnimationTracks[1], SelectedAnimInfo.SoundAndAnimationSet.AdditionalAnimations2, out otherIndex); //CurrentAnimInfo.AnimationSet.GetRandomAdditionalAnim2(CurrentAdditionalAnim2Key);

                if (newAdditionalKey1 != null)
                {

                }

                AdoptAdditionalAnim(previousBaseAnimIsLooping, previousAdditionalAnim1Key, previousAdditionalAnim1BlendKey, newAdditionalKey1, additionalAnimationTracks[0]);
                AdoptAdditionalAnim(previousBaseAnimIsLooping, previousAdditionalAnim2Key, previousAdditionalAnim2BlendKey, newAdditionalKey2, additionalAnimationTracks[1]);

            }


           /* if (newSound != null)
            {*/ // playing null will stop any state sound.
                // not sure if the same sound should be allowed to restart by default. I guess it depends on what it is. Like gunshots...
                Renderable.AdoptStateSound(newSound,
                        oldSound /*oldInfo != null ? oldInfo.SoundData : null*/, SelectedAnimInfo.Looping); 

          //  }

            if (oldInfo != SelectedAnimInfo)
            {
                Renderable.AdoptParticleEffects(SelectedAnimInfo.ParticleEmitters, oldInfo != null ? oldInfo.ParticleEmitters : null); //oldInfo);

             //   Renderable.AdoptStateSound(SelectedAnimInfo.SoundData, oldInfo != null ? oldInfo.SoundData : null, SelectedAnimInfo.Looping); //oldInfo);

                AdoptAttachedRenderables(oldInfo);

                AdoptAttachedTransformationChanges(oldInfo);                

            }

            
            return true;
        }

        public void StartAnimation(string animKey, AnimationTrack track, Playback playback, StartingPoint startingPoint,
                                  BlendMode mode, float speedFactor = 1f, Looping looping = Looping.No, float? startOffsetToAdd = null,
                                  bool? setCallback = false, EventHandler /* Action<AnimationController>*/ pickNewRandomAnim = null)
        {

            // Debug.Assert(ALLOWED,"startanimation was called from a disallowed class");

            if (animKey == "sleepToIdle")
            {

            }

            AnimationController anim = null;
            if (animKey != null)
            {
                anim = AnimatedModel.ModelAnimator.GetAnimControllerFromKey(animKey);

                Renderable.HideHandAttachments = anim.AnimationInfo.HideHandAttachments;
            }

            

            long? startOffSet = null;
            if (startOffsetToAdd.HasValue)
            {
                startOffSet = new TimeSpan(0, 0, 0, 0, (int)(1000f * startOffsetToAdd)).Ticks;
            }

            bool replaced;
            track.StartAnimation(anim, playback, startingPoint, mode, out replaced, speedFactor, looping, startOffSet, setCallback, pickNewRandomAnim, random: The.Client.ClientRandomGenerator.Random);
        }

       
        public void PickNewRandomAnim(object sender, EventArgs e)
        {
            if (The.Client == null)
                return; // 
           
             ((AnimationController)sender).AnimationEnded -= PickNewRandomAnim;

            // only switch anims if the same(?) set is still selected:
             if (SelectedAnimInfo.SoundAndAnimationSet != null
                 && SelectedAnimInfo.SoundAndAnimationSet.BaseAnimations != null
                 && SelectedAnimInfo.SoundAndAnimationSet.BaseAnimations.Contains(((AnimationController)sender).AnimationInfo.Name))
             {

                 int? index = null;
                 string newBaseAnim = GetRandomAnimToSwitchTo(mainAnimation, SelectedAnimInfo.SoundAndAnimationSet.BaseAnimations, out index);

                 StartAnimation(newBaseAnim, mainAnimation, 
                        SelectedAnimInfo.Playback,
                        SelectedAnimInfo.StartingPoint,
                        SelectedAnimInfo.BlendMode,
                        SelectedAnimInfo.SpeedFactor,
                        SelectedAnimInfo.Looping,
                        SelectedAnimInfo.StartingPointInSeconds, true, PickNewRandomAnim);

                 if (index.HasValue)
                 {
                     // start a new corresponding sound also:
                     SoundData newSound = null;

                     if (SelectedAnimInfo.SoundAndAnimationSet.SoundData != null) // NEWSOUND: delete this after migrating
                     {
                         // start a new sound in sync
                         newSound = SelectedAnimInfo.SoundAndAnimationSet.SoundData[index.Value];

                     }

                     // not sure if the same sound should be allowed to restart by default. I guess it depends on what it is. Like gunshots...
                     Renderable.AdoptStateSound(newSound,
                             null, SelectedAnimInfo.Looping);
                 }
             }
        }

        /// <summary>
        /// the atomic goals (like GoalDoProduceAtomic) will keep clearing/setting the same flag, but we don't want to replace random anims each time that happens. 
        /// so there is logic here to ensure continuity...
        /// </summary>
        /// <param name="track"></param>
        /// <param name="animationsToChooseFrom"></param>
        /// <returns></returns>
        private string GetRandomAnimToSwitchTo(AnimationTrack track, string[] animationsToChooseFrom, out int? newSelectedIndex)
        {
            newSelectedIndex = null;
            if (animationsToChooseFrom == null)
            {                
                return null;
            }

            // if an anim has been running for some time, allow it to finish... this situation may occur when GoalTakeFive ends in the middle of an anim, and then starts again     
            if (AllowAnimToContinue(track.controllerBeingBlendedTo, animationsToChooseFrom))
            {
                return track.controllerBeingBlendedTo.AnimationInfo.Name; // keep blending
            }
            else if (AllowAnimToContinue(track.currentController, animationsToChooseFrom))
            {
                newSelectedIndex = Array.IndexOf(animationsToChooseFrom, track.currentController.AnimationInfo.Name);
                return track.currentController.AnimationInfo.Name; // keep playing it
            }


            return GetRandomAnim(animationsToChooseFrom, track, out newSelectedIndex); // currentAnimKey);


        }

        private bool AllowAnimToContinue(AnimationController controller, string[] animationsToChooseFrom)
        {
            if (controller != null
               && animationsToChooseFrom.Contains(controller.AnimationInfo.Name)
               && controller.SpeedFactor > 0f // is it still running?
               && controller.ElapsedTimeMinusStartOffset > 0
               && controller.ElapsedTime < controller.AnimationInfo.Duration)
            {
                return true; // keep playing it
            }

            return false;
        }

        private string GetRandomAnim(string[] anims, AnimationTrack track, out int? selectedIndex)
        {
            selectedIndex = null;

            if (anims != null)
            {
                // pick the next anim intelligently to avoid characters acting in unison
                if (anims.Length > 1)
                {
                    GetNearbyAgentsOfSameType(ref nearByEntitiesOfSameType);

                    string currentAnim = track.CurrentAnimKey;
                    long? elapsed = track.CurrentAnimElapsed;

                    float score;
                    float bestScore = 0f;
                    int? bestIndex = null;
                   // string bestAnim = null;

                    string anim;
                    for (int i = 0; i < anims.Length; i++)
                    {
                        anim = anims[i];
                   
                        score = ScoreRandomAnim(anim, elapsed, currentAnim, nearByEntitiesOfSameType);

                        if (score > 0f && score > bestScore)
                        {
                            bestIndex = i; // anim;
                            bestScore = score;
                        }
                    }

                    // all scored 0. well, we have to pick one...
                    if (bestIndex == null)
                    {
                        bestIndex = Common.GetRandomListMemberIndex(anims, The.Client.ClientRandomGenerator);
                    }

                    selectedIndex = bestIndex;
                    return anims[selectedIndex.Value];
                }
                else
                {
                    selectedIndex = 0;
                    return anims[0];
                }
            }

            return null;
        }

        /// <summary>
        /// scores a random anim based on whether it is currently run by us, any nearby agents are running it, and are in sync with us
        /// </summary>
        /// <returns></returns>
        private float ScoreRandomAnim(string animKey, long? currentAnimElapsed, string currentAnimKey, List<Pair<Entity, Vector2>> nearByEntitiesOfSameType)
        {
            // let's start at 1:
            float score = 1f;
                       

          /*  if (currentAnimKey != null && currentAnimElapsed.HasValue)
            {*/
                bool isInSync;
                
                foreach (var nearbyEntity in nearByEntitiesOfSameType)
                {
                    if (nearbyEntity.First.Renderable != null // needed check during PostLoad
                        && nearbyEntity.First.Renderable.RenderAsModel.IsRunningAnim(animKey, 0, out isInSync))
                    {
                        if (isInSync)
                        {
                            //another entity running this anim in sync with us is an instant 0 score:
                            return 0f;
                        }
                        else
                        {
                            score -= 0.1f; // score gets lower the more people that are running the anim

                            score = Common.ClampBottom(score, 0.1f);
                        }
                    }
                }
         //   }

            if (animKey == currentAnimKey)
            {
                // did we just run this anim?
                score -= 0.1f;
            }

            // add a random factor too:
            score += The.Client.ClientRandomGenerator.RandomBetween(0f, 0.05f);
            
            return score;
        }

        private void GetNearbyAgentsOfSameType(ref List<Pair<Entity, Vector2>> nearByEntitiesOfSameType)
        {
            nearByEntitiesOfSameType.Clear();

            The.AgentQuadTree.GetEntitiesInRange(Parent.Location.Value.ToVector2(), 300f,
                e => 
                    e.EntityType == ParentEntity.EntityType
                    && e != ParentEntity,
                ref nearByEntitiesOfSameType);


        }

        List<Pair<Entity, Vector2>> nearByEntitiesOfSameType = new List<Pair<Entity, Vector2>>();
        /// <summary>
        /// when picking a random anim, avoid ones that are already played nearby. This will look especially bad if they are in sync...
        /// </summary>
        /// <returns></returns>
        private bool AnimIsPlayedByNearbyAgent(string animKey, long animProgress)
        {
            if (ParentEntity != null)
            {
                nearByEntitiesOfSameType.Clear();
                The.AgentQuadTree.GetEntitiesInRange(Parent.PlaySiteLocation.ToVector2(), 300f, 
                    e => e.EntityType == ParentEntity.EntityType, 
                    ref nearByEntitiesOfSameType);

                bool animIsInSync;
                foreach (var item in nearByEntitiesOfSameType)
                {
                    if (IsRunningAnim(animKey, animProgress, out animIsInSync))
                    {

                    }
                }
            }

            return false;
        }

        private void AdoptAdditionalAnim(Looping? previousAnimIsLooping, string previousAdditionalAnimKey, string previousAdditionalBlendAnimKey, string currentAdditionalAnimKey, AnimationTrack track)
        {
            if (currentAdditionalAnimKey != null)
            {

                if (OKToStartAnimation(previousAdditionalAnimKey, currentAdditionalAnimKey, previousAdditionalBlendAnimKey, //CurrentAnimInfo.CurrentAdditionalAnim1Key,
                    previousAnimIsLooping, SelectedAnimInfo.Looping))
                {
                    StartAnimation(currentAdditionalAnimKey, track, //CurrentAnimInfo.CurrentAdditionalAnim1Key,
                        SelectedAnimInfo.Playback,
                        SelectedAnimInfo.StartingPoint,
                        BlendMode.Normal,
                        SelectedAnimInfo.SpeedFactor,
                        SelectedAnimInfo.Looping);
                }
            }
            else
            {
                // stop any currently running anim on this track:
               
                if (track.RunsAnimation())
                {
                    track.StopAnimation();
                }
            }
        }

        

        private void AdoptAttachedTransformationChanges(AnimConditionInfo oldInfo)
        {
            if (oldInfo == null || oldInfo.AttachPoints != SelectedAnimInfo.AttachPoints)
            {
                // make the changes to attached objects:

                // is blending necessary...?        

                // use a callback since ModelAnimator is in a different library
                ModelAnimator.IterateAttachedEntities(RecomputeLocalTransformForAttachable); 
               // ModelAnimator.RecomputeLocalTransformsForAttachedObjects();

            }
        }

        private void AdoptAttachedRenderables(AnimConditionInfo oldInfo)
        {
            bool oldAndNewAreEqual = false;

            if (oldInfo != null)
            {
                oldAndNewAreEqual = TemporaryRenderablesToAttachAreEquals(oldInfo.TemporaryRenderablesToAttach, SelectedAnimInfo.TemporaryRenderablesToAttach);// i don't think we can compare references
            }

            // see if we should remove previous temporary attachments
            if (oldInfo != null && oldInfo.TemporaryRenderablesToAttach != null
                && !oldAndNewAreEqual) 
            {
                foreach (var item in oldInfo.TemporaryRenderablesToAttach) 
                {
                    // remove:
                    Renderable.RemoveAttachables(item.RenderableTypeKey, item.AttachorTag);                                           
                }
            }


            if (oldInfo == null || !oldAndNewAreEqual)
            {
                // see if we want to attach new renderables:
                if (SelectedAnimInfo.TemporaryRenderablesToAttach != null)
                {
                    foreach (var renderable in SelectedAnimInfo.TemporaryRenderablesToAttach)
                    {
                        Renderable.AttachPooledObjectIfPossible(renderable.RenderableTypeKey,
                                            renderable.AttachorTag,
                                            renderable.AttacheePoint ?? AttacheePoint.RightHand,
                                            false); // don't save these to snapshot since they will be re-adopted from the anim flags.
                        
                    }
                }
            }
        }

        private bool TemporaryRenderablesToAttachAreEquals(AnimConditionInfo.TemporaryAttachable[] renderablesToAttach1, AnimConditionInfo.TemporaryAttachable[] renderablesToAttach2)
        {
            if (renderablesToAttach1 == null)
            {
                if (renderablesToAttach2 == null)
                {
                    return true;
                }

                return false;
            }
            else
            {
                if (renderablesToAttach2 == null)
                {
                    return false;
                }
                else
                {
                    if (renderablesToAttach1.Length != renderablesToAttach2.Length)
                    {
                        return false;
                    }
                    else
                    {
                        for (int i = 0; i < renderablesToAttach1.Length; i++)
                        {
                            if (!renderablesToAttach1[i].Equals(renderablesToAttach2[i]))
                            {
                                return false;
                            }
                        }

                        return true;
                    }
                }
            }
        }

        public static void RecomputeLocalTransformForAttachable(IAttachable attachable)
        {
            AttachPoint attachee;

            Renderable renderableToAttach = ((RenderAsModel)attachable).Renderable;

            Vector3 translationToUse, rotationToUse;

            RenderAsModel attachedToRAM = ((Entity)attachable.AttachedTo).Renderable.RenderAsModel;

            // see if the current anim specifies a transform/attach point:
            RenderAsModel.GetAttachTransformations(renderableToAttach, 
                attachable.AttachorPoint, attachable.AttacheePointValue, attachedToRAM.SelectedAnimInfo, out attachee, out translationToUse, out rotationToUse); //, attachableKey);

            // create the new transform and assign it:
            Matrix localTransform = Renderable.CreateTransformForAttachedEntity(attachable.Scale, // scaling, 
                attachable.AttachorPoint, attachee, translationToUse, rotationToUse);

            renderableToAttach.RenderAsModel.LocalTransform = localTransform;
            
        }

        /// <summary>
        /// makes sure that we never restart a looping anim - there's no reason for it, and it will look wonky.     
        /// </summary>
        /// <param name="previousAnimKey"></param>
        /// <param name="newAnimKey"></param>
        /// <param name="previousLoopingState"></param>
        /// <param name="newLoopingState"></param>
        /// <returns></returns>
        private bool OKToStartAnimation(string previousAnimKey, string newAnimKey, string blendingToAnimKey, Looping? previousLoopingState, Looping newLoopingState)
        {
            // make sure that we are not:
            if (!(previousAnimKey == newAnimKey // running the same anim
                && (blendingToAnimKey == null || blendingToAnimKey == newAnimKey) // not blending
               && previousLoopingState == Looping.Yes && newLoopingState == Looping.Yes)) // and looping before and after
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// return true if this renderable is currently running, or blending to, the same animation, and specify whether they are in sync
        /// </summary>
        /// <param name="animKey"></param>
        /// <param name="animIsInSync"></param>
        /// <returns></returns>
        public bool IsRunningAnim(string animKey, long animProgress, out bool animIsInSync)
        {
            animIsInSync = false;

            bool mainIsInSync;
            bool mainIsRunning = IsRunningAnim(animKey, animProgress, mainAnimation.currentController, out mainIsInSync);

            bool blendIsInSync = false;
            bool blendIsRunning = false;
            if (!mainIsRunning || !mainIsInSync)                
            {
                // if not fully in sync, check the blending anim:
                blendIsRunning = IsRunningAnim(animKey, animProgress, mainAnimation.controllerBeingBlendedTo, out blendIsInSync);                
            }

            animIsInSync = mainIsInSync || blendIsInSync;
            
            return mainIsRunning || blendIsRunning;
        }

        const long elapsedDistanceToConsiderInSync = 6000000;
        private static bool IsRunningAnim(string animKey, long animProgress, AnimationController animController, out bool animIsInSync)
        {
            animIsInSync = false;

            if (animController != null && animController.AnimationInfo != null)
            {
                if (animController.AnimationInfo.Name == animKey)
                {
                    // same anim is playing, check for syncing:
                    long distance;

                    if (animController.IsLooping)
                    {
                        long duration = animController.AnimationInfo.Duration;
                        distance = Math.Abs(animProgress - animController.ElapsedTime);

                        if (distance > duration / 2) // handle wrap-around:
                        {
                            distance = duration - distance;
                        }

                        /*
                        dx = abs(x1 - x2);
                        if (dx > width / 2)
                            dx = width - dx;*/
                    }
                    else
                    {
                        distance = Math.Abs(animController.ElapsedTime - animProgress);                       
                    }

                    if (distance < elapsedDistanceToConsiderInSync)
                    {
                        animIsInSync = true; 
                    }

                    return true;
                }                
            }

            return false;
        }

        public string GetCurrentMainAnimation()
        {
            if (mainAnimation.currentController != null && mainAnimation.currentController.AnimationInfo != null)
                return mainAnimation.currentController.AnimationInfo.Name;

            return null;
            
        }

        internal void AppendAnimDebugInfo(StringBuilder states)
        {
            states.Append("    current main:  " + mainAnimation.CurrentAnimKey);
            states.Append("\n");
            states.Append("    blend main:    " + mainAnimation.AnimKeyBeingBlendedTo);
          
            states.Append("\n");
            states.Append("\n");

            if (secondGaitAnimation.FinalWeightFactor > 0f && (secondGaitAnimation.currentController != null || secondGaitAnimation.controllerBeingBlendedTo != null))
            {
                states.Append("    current 2nd gait:  " + secondGaitAnimation.CurrentAnimKey);
                states.Append("\n");
                states.Append("    blend 2nd gait:    " + secondGaitAnimation.AnimKeyBeingBlendedTo);
                states.Append("\n");
                states.Append("\n");
                states.Append("    weight:  " + secondGaitAnimation.FinalWeightFactor);
                states.Append("\n");
            }

            int count = 1;
            foreach (var track in additionalAnimationTracks)
            {
                if (track.currentController != null || track.controllerBeingBlendedTo != null)
                {
                    states.Append("    " + "additional track #" + count + ", current: " + track.CurrentAnimKey);
                    states.Append("\n");
                    states.Append("    " + "additional track #" + count + ", blend: " + track.AnimKeyBeingBlendedTo);
                    states.Append("\n");
                    states.Append("\n");
                }
            }

        }

        public void UpdateAnimationManuallyByTimeScalar(double scalar)
        {
            if (animationFlagsAreDirty == false)
            {
                mainAnimation.UpdateAnimationManuallyByTimeScalar(The.Client.GameTime, scalar);
            }
        }

        public void StartMainAnimation(string animKey, Playback playback, StartingPoint startingPoint,
                                    BlendMode mode, float speedFactor = 1f, Looping looping = Looping.No, float? startOffsetToAdd = null)
        {
            StartAnimation(animKey, mainAnimation, playback, startingPoint, mode, speedFactor, looping, startOffsetToAdd);
        }
       
     

        public void StartAdditionalAnimation(string animKey, Playback playback, StartingPoint startingPoint,
                                    BlendMode mode, float speedFactor = 1f, Looping looping = Looping.No, float? startOffsetToAdd = null)
        {
            // see if there is a free animation track...
            foreach (var track in additionalAnimationTracks)
            {
                if (track.currentController == null)
                {
                    StartAnimation(animKey, track, playback, startingPoint, mode, speedFactor, looping, startOffsetToAdd);
                }
            }

        }

        public void StopMainAnimation()
        {
            mainAnimation.StopAnimation();
        }

        private int noOfFramesWeHaveBeenDirty = 0;
        public void UpdateAnimationConditionState()
        {          
            if (animationFlagsAreDirty)
            {
                if (ReplaceAnimationConditionState()) // sometimes we want to delay switching in order to sow (Atomic) goals together...
                {
                    animationFlagsAreDirty = false;
                    noOfFramesWeHaveBeenDirty = 0;

                    previousMainGait = null; // reset after a possible new gait anim set has been selected
                }
                else
                {
                    noOfFramesWeHaveBeenDirty++;
                }
            }
        }
    }



}
