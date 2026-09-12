using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xclna.Xna.Animation;
using Microsoft.Xna.Framework;

//namespace UWGame.ClientSide.Renderables
namespace Xclna.Xna.Animation
{
    /// <summary>
    /// Lars: not sure backwards works...
    /// Manual is for letting an outside class (like a goal) set the progress T-value - used by GoalBeingHit (though I'm not sure why regular playback couldn't be used)
    /// </summary>
    public enum Playback
    {
        Forwards,
        Backwards,
        Manual
    }
    public enum StartingPoint
    {
        Current,
        FromBeginning,
        Specified,
        Random // NEW - only when restarting
    }
    public enum BlendMode
    {
        Normal,
        NoBlending
            /*,
        Additive*/
    }
    public enum Looping
    {
        Yes,
        No
    }

    /// <summary>
    /// controls an animation, its blending into a different anim and its priority over other animation tracks currently running on the renderable
    /// </summary>
    public class AnimationTrack
    {
        /// <summary>
        /// for how long should two animations blend into each other?
        /// this time is constant - not affected by changing speeds, doing manual updates etc.
        /// </summary>
        public static float BlendPeriodInMilliseconds = 500f;

        /// <summary>
        /// this helps smooth out constant vascillations between 2 gait anims and prevents stuttering...
        /// </summary>
        public static float GaitBlendPeriodInMilliseconds = 1000f;

        private ModelAnimator /*AnimatedModel*/ modelAnimator;

       


        public AnimationController currentController;
        public AnimationController controllerBeingBlendedTo;



        /// <summary>
        /// 0 - 1
        /// when at 1, the blending phase of two animations is over.
        /// </summary>
        public float blendingProgress;

        private bool blendToNull = false;


        private float? weightFactor;// = 1f;

        /// <summary>
        /// this sets the weight of this track relative to other tracks
        /// </summary>
        public float? WeightFactor
        {
            get { return weightFactor; }
            set { weightFactor = value; }
        }


        private float finalWeightFactor;

        /// <summary>
        /// the final weight factor after considering "fading" (blending to/from null)
        /// </summary>
        public float FinalWeightFactor
        {
            get
            {
                return finalWeightFactor;
            }
        }

       /* {
            get
            {
                return 0f;
            }
            set { }
        }*/
       /* {
            get 
            { return gaitInterpolationFactor; }
            set { gaitInterpolationFactor = value; }
        }*/

       /* private bool isGaitChannel;
        public bool IsGaitChannel
        {
            get { return isGaitChannel; }
        }*/


        /// <summary>
        /// Fired when the animation controllers change and different bones are affected.
        /// </summary>
       // public event EventHandler AnimationControllersChanged;


        public enum TrackType { Main, ExtraGait, Additional }

        public TrackType TrackTypeValue;

        public int Index;

        public AnimationTrack(ModelAnimator animatedModel, TrackType trackType)
        {
           // this.isGaitChannel = trackType;

            this.TrackTypeValue = trackType;
                      
            this.modelAnimator = animatedModel;

            foreach (var bonePose in modelAnimator.BonePoses)
            {
                bonePose.SetAnimationTrack(this);

            }
        }


        public string CurrentAnimKey
        {
            get
            {
                if (currentController != null)
                    return currentController.AnimationInfo.Name;

                return null;
            }
        }

        public long? CurrentAnimElapsed
        {
            get
            {
                if (currentController != null)
                    return currentController.ElapsedTime;

                return null;
            }
        }

        public string AnimKeyBeingBlendedTo
        {
            get
            {
                if (controllerBeingBlendedTo != null)
                    return controllerBeingBlendedTo.AnimationInfo.Name;

                return null;
            }
        }


        /// <summary>
        /// this controller is diffrent from the main controller in that it can blend to null (nothing.)
        /// </summary>
      /*  private void FinishGait2Blending()
        {

            if (currentController != null)
            {
                currentController.SpeedFactor = 0f; // stop the previous animation
            }

            currentController = controllerBeingBlendedTo; // can be null

            controllerBeingBlendedTo = null;


            //  currentMainController = blendMainController;
            //   blendMainController = null;

            BlendToNull = false;

            BlendingProgress = 0f;
            //AnimatedModel.BlendGait2Progress = 0f;

            // do this in the normal place...
            //AnimatedModel.RunGaitControllers(); //RunController(currentMainController); //??
        }*/

        
        public void FinishBlending()
        {
            
            if (currentController != null)
            {
               
                currentController.SpeedFactor = 0f; // stop the previous animation
            }

            currentController = controllerBeingBlendedTo;
            controllerBeingBlendedTo = null;


            blendingProgress = 0f;

            blendToNull = false;

          //  modelAnimator.UpdateModelBones(this); 
        }


        public void Update(GameTime gameTime)
        {
            // NEW:
            if (currentController != null)
                currentController.Update(gameTime);

            if (controllerBeingBlendedTo != null)
                controllerBeingBlendedTo.Update(gameTime);


            AnimationController previousController = currentController;
            AnimationController previousBlendController = controllerBeingBlendedTo;



            if (TrackTypeValue == TrackType.Main) 
            {
                // the main controller can never blend to/from null!
                if (controllerBeingBlendedTo != null)
                {

                    float blendPeriodToUse;
                    blendPeriodToUse = GetBlendPeriodToUse();

                    blendingProgress += (float)gameTime.ElapsedGameTime.TotalMilliseconds / blendPeriodToUse;

                  /*  if (controllerBeingBlendedTo.AnimationInfo.Name == "sleep")
                    {
                        Console.WriteLine("Blending to sleep, current: {0}, current elapsed: {1}, progress: {2}", currentController.AnimationInfo.Name, currentController.ElapsedTime.ToString(), blendingProgress.ToString());
           
                    }*/

                    if (blendingProgress >= 1f)
                    {
                        // blending is finished

                        FinishBlending();
                    }
                  /*  else
                    {
                     // done at the end:
                       // modelAnimator.UpdateModelBones(this); //currentController, controllerBeingBlendedTo, BlendingProgress);
                    }*/

                }

            }
            else
            {
                // the second gait controller can blend into and out from null
                if (((blendToNull && currentController != null ) 
                    || controllerBeingBlendedTo != null)) 
                {
                    float blendPeriodToUse;
                    blendPeriodToUse = GetBlendPeriodToUse();

                    blendingProgress += (float)gameTime.ElapsedGameTime.TotalMilliseconds / blendPeriodToUse;

                    if (blendingProgress >= 1f)
                    {
                        // blending is finished
                        FinishBlending();

                    }

                }

                // if we are blending into or out from null, modify the BlendFactorWithOtherTracks with the blend progress to decrease the wieght of this anim:
                if (blendToNull
                    || (currentController == null && controllerBeingBlendedTo != null))
                {
                    finalWeightFactor = blendingProgress * (WeightFactor.HasValue ? WeightFactor.Value : 0f);
                }
                else
                {
                    finalWeightFactor = (WeightFactor.HasValue ? WeightFactor.Value : 0f);
                }
            }


            if (currentController != previousController || controllerBeingBlendedTo != previousBlendController)
            {
                // update bone poses if necessary:
                modelAnimator.UpdateModelBones(this);
            }
        }

        private float GetBlendPeriodToUse()
        {
            float blendPeriodToUse;

            if (currentController.AnimationInfo.IsGaitAnim &&
                controllerBeingBlendedTo.AnimationInfo.IsGaitAnim)
            {
                blendPeriodToUse = GaitBlendPeriodInMilliseconds;
            }
            else
            {
                blendPeriodToUse = BlendPeriodInMilliseconds;
            }

            return blendPeriodToUse;
        }

        public void RunWithoutBlending(AnimationController anim)
        {
            controllerBeingBlendedTo = null;

            currentController = anim;

            blendingProgress = 0;

            modelAnimator.UpdateModelBones(this); //currentController);

            //Trace.WriteLine("RunWithoutBlending(), the anim param is: " + anim.AnimationInfo.Name);
            //if (currentController != null)
            //    Trace.WriteLine(" Current: " + currentController.AnimationInfo.Name);
            //if (blendController != null)
            //    Trace.WriteLine(" Blend: " + blendController.AnimationInfo.Name);

            //Trace.WriteLine("----------------------------------------------");



        }

        /// <summary>
        /// this advances animation and performs blending manually. normal updates are disabled because SpeedFactor is set to 0.
        /// </summary>
        /// <param name="scalar"></param>
        public void UpdateAnimationManuallyByTimeScalar(GameTime gameTime, double scalar)
        {
            if (currentController != null 
                && controllerBeingBlendedTo == null) // NEW: only update the 'latest' anim. We don't want to restart the previous anim with a scalar factor 0f 0.01...
            {
                // never update move anims manually. they may have been set because we are still lerping or being pushed by someone...
                if (!CanBeScaled(currentController)) // currentController.AnimationInfo.IsGaitAnim) //
                {
                    return;
                }
                else
                {
                    currentController.UpdateAnimationTimeScalar(scalar);
                }
            }

            if (controllerBeingBlendedTo != null)
            {
                if (CanBeScaled(controllerBeingBlendedTo)) 
                {
                    controllerBeingBlendedTo.UpdateAnimationTimeScalar(scalar);
                }                

                // blending is performed as normal. a higher speed would not affect the length of the blending period either.
                blendingProgress += (float)gameTime.ElapsedGameTime.TotalMilliseconds / GetBlendPeriodToUse();

                if (blendingProgress >= 1f)
                {
                    // blending is finished

                    FinishBlending();

                    modelAnimator.UpdateModelBones(this); 
                }
              /*  else
                {
                    modelAnimator.UpdateModelBones(this); //???
                }*/
            }
        }

        private bool CanBeScaled(AnimationController controller)
        {
            if (controller.AnimationInfo.IsGaitAnim   // never update move anims manually. they may have been set because we are still lerping or being pushed by someone...
                || controller.IsLooping  // never scale anims that are looping, or can loop (this is a precaution against a wrong/alternative anim match, being played sped up (giving a horror movie effect))
               ) 
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Important:
        /// The start offset is useful for starting an anim at the Action Point (pickup/hit/shoot etc.) of an animation.
        /// but be careful with the number supplied... it gets added to the default start offset which skips frame 0 (bindpose).
        /// so if the anim has the action point at frame 20, and bindpose is frame 0 as usual, then 19 should be supplied here!
        /// </summary>
        /// <param name="animKey"></param>
        /// <param name="playback"></param>
        /// <param name="startingPoint"></param>
        /// <param name="mode"></param>
        /// <param name="speedFactor"></param>
        /// <param name="looping"></param>
        /// <param name="startOffsetToAdd"></param>
        /// <returns></returns>
     /*   public void StartAnimation(string animKey, Playback playback, StartingPoint startingPoint,
                                    BlendMode mode, float speedFactor = 1f, Looping looping = Looping.No, float? startOffsetToAdd = null,
                                    bool? setCallback = false, EventHandler  pickNewRandomAnim = null)
        {

            // Debug.Assert(ALLOWED,"startanimation was called from a disallowed class");

            AnimationController anim = null;
            if (animKey != null)
            {
                anim = modelAnimator.GetAnimControllerFromKey(animKey);
            }

           
            long? startOffSet = null;
            if (startOffsetToAdd.HasValue)
            {
                startOffSet = new TimeSpan(0, 0, 0, 0, (int)(1000f * startOffsetToAdd)).Ticks;
            }

            bool replaced;
            StartAnimation(anim, playback, startingPoint, mode, out replaced, speedFactor, looping, startOffSet, setCallback, pickNewRandomAnim);
        }*/

       // long startGaitCounter = 0;
     //   long replaceGaitCounter = 0;

        /// <summary>
        /// it is permitted to start a null animation on certain tracks.
        /// </summary>
        /// <param name="animController"></param>
        /// <param name="playback"></param>
        /// <param name="startingPoint"></param>
        /// <param name="mode"></param>
        /// <param name="speedFactor"></param>
        /// <param name="looping"></param>
        /// <param name="startOffsetToAdd"></param>
        public void StartAnimation(AnimationController animController, Playback playback,
                                    StartingPoint startingPoint, BlendMode mode, out bool replacedAnim, float speedFactor = 1f,
                                    Looping looping = Looping.No, long? startOffsetToAdd = null, 
                                    bool? setCallback = null,
                                    EventHandler /* Action<AnimationController>*/ pickNewRandomAnim = null,
                                    Random random = null)
        {
           

            replacedAnim = false;

            if (animController == null) 
            {
                if (blendToNull == false) // controllerBeingBlendedTo != null)
                {
                    //blend to null!!!
                    blendToNull = true;

                    controllerBeingBlendedTo = null;
                    blendingProgress = 0f;

                    modelAnimator.UpdateModelBones(this); // currentController);
                }

                return;
            }


            if (currentController != null && currentController.AnimationInfo.IsGaitAnim)
            {
                currentController.SpeedFactor = speedFactor; // may be stopped later

            }


            long? elapsedTimeToSet = null; 

            if (playback == Playback.Backwards)
            {
                animController.SpeedFactor = -1f * speedFactor; //Math.Abs(anim.SpeedFactor);
                if (startingPoint == StartingPoint.FromBeginning)
                {
                    elapsedTimeToSet = animController.AnimationInfo.Duration;
                    //animController.ElapsedTime = animController.AnimationInfo.Duration;
                }
                else
                {
                    if (animController.IsAnimationAtStart()) // anim.ElapsedTime == 0)
                    {  // Continue animation, if the animation is already at the beginning
                        if (looping == Looping.Yes)
                        {
                            elapsedTimeToSet = animController.AnimationInfo.Duration;// if we are looping, we restart:
                            //animController.ElapsedTime = animController.AnimationInfo.Duration;// if we are looping, we restart:
                        }
                        else
                        {
                            return; // null;//animation could not be run (maybe change this)
                        }
                    }
                }
            }
            else if (playback == Playback.Forwards)
            {
                animController.SpeedFactor = speedFactor;
                if (startingPoint == StartingPoint.FromBeginning)
                {
                    //animController.SetAnimationAtStart(startOffsetToAdd);

                    elapsedTimeToSet = animController.GetStartOfAnimation(startOffsetToAdd);
                }
                else if (startingPoint == StartingPoint.Current) // StartingPoint.FromBeginning)
                {
                    // Continue animationm if the animation is already at the end
                    if (animController.ElapsedTime == animController.AnimationInfo.Duration
                        && looping == Looping.Yes)
                    {
                        throw new Exception("you can't use manual playback on a looping anim like: " + animController.AnimationInfo.Name);
                    }
                }
                else if (startingPoint == StartingPoint.Specified)
                {
                    //animController.SetAnimationAtStart(startOffsetToAdd);

                    elapsedTimeToSet = animController.GetStartOfAnimation(startOffsetToAdd);
                }
                else if (startingPoint == StartingPoint.Random)
                {
                    // birds use this
                    elapsedTimeToSet = (long)(animController.AnimationInfo.StartOffset + random.NextDouble() * (animController.AnimationInfo.Duration - animController.AnimationInfo.StartOffset)); //animController.GetStartOfAnimation(startOffsetToAdd);
                }
            }
            else if (playback == Playback.Manual)
            {
                animController.SpeedFactor = 0d; // the animation engine must not advance the animation by time
                //TODO MLo: zero SpeedFactor will also bail at the top of update, this may be bad????

                // set at start... skip past the bind pose:
                //animController.SetAnimationAtStart(startOffsetToAdd);
                elapsedTimeToSet = animController.GetStartOfAnimation(startOffsetToAdd);

                animController.IsManual = true;
            }
            else
            {
                throw new Exception("Animation playback type of undefined type: " + playback);
            }

            if (setCallback == true)
            {
                animController.AnimationEnded += pickNewRandomAnim;
            }
            else
            {
                if (pickNewRandomAnim != null)
                {
                    animController.AnimationEnded -= pickNewRandomAnim;
                }
            }

            if (playback != Playback.Manual && looping == Looping.Yes)
            {
                animController.IsLooping = true;
            }
            else
            {
                animController.IsLooping = false;
            }

           // mode = BlendMode.NoBlending;
             
            // now control blending:
            if (mode == BlendMode.Normal)
            {
                /*
#if DEBUG
                if (animController != null && currentController != null)
                {
                    Console.WriteLine("Starting anim {0}, current: {1}", animController.AnimationInfo.Name, currentController.AnimationInfo.Name);
                }              

#endif
                */

                if (currentController == null)//this is the first animation
                {
                    currentController = animController;
                    currentController.ElapsedTime = elapsedTimeToSet.Value;
                    modelAnimator.UpdateModelBones(this); //currentController);
                }
                else /*if (animController.AnimationInfo.OKToBlendAnimation && // is it ok to blend these two anims together? 
                    (currentMainController == null || currentMainController.AnimationInfo.OKToBlendAnimation))*/
                {
                    // do blending if this new 'anim' is not already playing or blending

                   
                   // MapClient.AddDebugMarker(Location + new Vector3(10f, 20f, 0f), Color.Violet, this);


                    if (controllerBeingBlendedTo == animController)
                    {
                        // we are already blending into this animation - do nothing...


                    }
                    else if (currentController == animController)
                    {   // we are running this anim already as main anim,

                        if (controllerBeingBlendedTo != null)
                        {
                            // - but blending into a different anim. End the blending now!                            
                            blendingProgress = 0f;

                            if (pickNewRandomAnim != null)
                            {
                                controllerBeingBlendedTo.AnimationEnded -= pickNewRandomAnim;
                            }
                            controllerBeingBlendedTo.SpeedFactor = 0f;
                            controllerBeingBlendedTo = null;
                            
                            modelAnimator.UpdateModelBones(this); // currentController);

                            replacedAnim = true;
                                                    
                            //restart (we already made sure that we are not looping)
                            // without this, pickupHeavySame and pickupLightSame and drop will not play in sequence
                            if (!currentController.IsLooping)
                            {
                                currentController.ElapsedTime = elapsedTimeToSet.Value;
                            }
                        }
                        else if (blendToNull == true)
                        {   // - but blending to null. end it now:
                            blendingProgress = 0f;
                            blendToNull = false;

                            modelAnimator.UpdateModelBones(this); 
                        }
                        else
                        {
                            // - and not blending - restart (we already made sure that we are not looping)
                            currentController.ElapsedTime = elapsedTimeToSet.Value;

                        }

                    }
                    else
                    {
                        // we are running a different anim currently,

                        if (controllerBeingBlendedTo != null)
                        {
                            // we want to end the blending:
                            if (pickNewRandomAnim != null)
                            {
                                controllerBeingBlendedTo.AnimationEnded -= pickNewRandomAnim;
                            }

                            // - and blending into a different anim. 
                            if (blendingProgress > 0.25f)
                            {
                                // finish blending quickly by switching the two previous anims
                                FinishBlending();
                            }
                            else
                            {
                                // cancel the current blending prematurely since it has not run for very long. keep the main anim running.
                                blendingProgress = 0f;
                                controllerBeingBlendedTo.SpeedFactor = 0f;
                            }

                            
                            
                            controllerBeingBlendedTo = animController; // restart blending with the 2 new anims
                            controllerBeingBlendedTo.ElapsedTime = elapsedTimeToSet.Value;

                            modelAnimator.UpdateModelBones(this); //currentController, controllerBeingBlendedTo, BlendingProgress);

                        }
                        else if (blendToNull == true)
                        {   // - but blending to null. end it now and start blending into the new anim:
                            blendingProgress = 0f;
                            blendToNull = false;

                            controllerBeingBlendedTo = animController; // restart blending with the 2 new anims
                            controllerBeingBlendedTo.ElapsedTime = elapsedTimeToSet.Value;

                            modelAnimator.UpdateModelBones(this); //currentController, controllerBeingBlendedTo, BlendingProgress);

                        }
                        else
                        {
                            // and not blending. start blending into the new anim:
                            blendingProgress = 0f;
                            controllerBeingBlendedTo = animController;
                            controllerBeingBlendedTo.ElapsedTime = elapsedTimeToSet.Value;

                            modelAnimator.UpdateModelBones(this); //currentController, controllerBeingBlendedTo, BlendingProgress);
                        }
                    }

                }

            }
            else if (mode == BlendMode.NoBlending)
            {
                RunWithoutBlending(animController);
            }


            // try to make sure that gait anims run in sync:
            if (/*currentController != null && currentController.AnimationInfo.IsGaitAnim
                &&*/ controllerBeingBlendedTo != null && controllerBeingBlendedTo.AnimationInfo.IsGaitAnim)
            {               
                controllerBeingBlendedTo.SpeedFactor = speedFactor;
            }


            return;
        }




        
        /*
        public void ApplyGaitBlendAnim(float speedFactor, AnimationController anim2)
        {
            
            if (currentController == null) // secondGaitController == null)
            {
                if (anim2 != null)
                {
                    // start the gait blend anim 
                    currentController = anim2;
                    //secondGaitController = anim2;
                }
            }
            else
            {
                // a gait blend anim is running.
                if (anim2 != null)
                {
                    if (currentController == anim2)
                    {
                        // already running the required anim. Set its speed:
                        currentController.SpeedFactor = speedFactor;
                    }
                    else
                    {
                        // the gait blend is a different anim. Stop it and switch to the new one:
                        currentController.SpeedFactor = 0f;

                        currentController = anim2;
                    }
                }
                else
                {
                    // we don't want a gait blend anim to run. So stop it:
                    currentController.SpeedFactor = 0f;
                    currentController = null;
                }
            }
        }
        */

        /// <summary>
        /// stops animations immediately. Most of the time we want to fade it out instead - do this by starting a null anim
        /// </summary>
        public void StopAnimation()
        {
            if (currentController != null)
            {
                currentController.SpeedFactor = 0f;
                currentController = null;
            }

            if (controllerBeingBlendedTo != null)
            {
                controllerBeingBlendedTo.SpeedFactor = 0f;
                controllerBeingBlendedTo = null;
            }
            

            blendToNull = false;


            modelAnimator.UpdateModelBones(this);
        }


      
        public bool RunsAnimation()
        {
            return currentController != null || controllerBeingBlendedTo != null;
        }


        public Matrix GetCurrentTransform(BonePose bonePose, bool doesAnimContainChannel, bool doesBlendContainChannel)
        {
            Matrix blendMatrix;
   
            Matrix returnMatrix;
            

            // If the bone is not currently affected by an animation
            if (currentController == null || !doesAnimContainChannel)
            {
                // If the bone is affected by a blend animation,
                // blend the defaultTransform with the blend animation
                if (controllerBeingBlendedTo != null && doesBlendContainChannel)
                {
                    blendMatrix = controllerBeingBlendedTo.GetCurrentBoneTransform(bonePose);
                                    

                    Util.SlerpMatrix(
                        ref bonePose.DefaultTransform, // defaultMatrix,
                        ref blendMatrix,
                        blendingProgress,
                        out returnMatrix);
                }
                else// else return the default transform
                    return bonePose.DefaultTransform;
            }
            else// The bone is affected by an animation
            {
                // Find the current transform in the animation for the bone
                Matrix currentMatrixBuffer = currentController.GetCurrentBoneTransform(bonePose);

                //return currentMatrixBuffer;

                // If the bone is affected by a blend animation, blend the
                // current animation transform with the current blend animation
                // transform
                if (controllerBeingBlendedTo != null && doesBlendContainChannel)
                {
                    blendMatrix = controllerBeingBlendedTo.GetCurrentBoneTransform(bonePose);

                    Util.SlerpMatrix(
                        ref currentMatrixBuffer,
                        ref blendMatrix,
                        blendingProgress,
                        out returnMatrix);
                }
                else// Else just return the current animation transform
                {
                    return currentMatrixBuffer;
                }
            }


            return returnMatrix;
        }

    }

}
