using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Xclna.Xna.Animation;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI;
using UWGame.SimSide;

namespace UWGame.ClientSide.Renderables
{
    /// <summary>
    /// controls head turns and what the person is looking at - note: this should stay purely a client effect - should not affect Sim/AI at all.
    /// </summary>
    public class AnimatedHead
    {

        Renderable parent;

      
       // float returnToNormalProgress = 1f;


        public AnimatedHead(Renderable parent)
        {
            this.parent = parent;
        }

       
        
        public void TurnToLook(Vector2 lookTarget, float lookAngle, float lookTargetAngle) //, out bool isLookingAtTarget, out bool isOutOfView)
        {
            
            /*
            Vector2 headLookingDirection = parent.RenderAsModel.FacingNormal.ToVector2();

            //     float dotProduct = Vector2.Dot(headLookingDirection, lookTargetDirection);


            //    float perpDot = headLookingDirection.X * lookTargetDirection.Y - headLookingDirection.Y * lookTargetDirection.X;

            float desiredHeadAngle = MathHelper.WrapAngle(lookTargetAngle - bodyFacingRotation); //bodyFacingRotation - lookTargetAngle; // -(float)Math.Atan2(perpDot, dotProduct);

            float lerpFactor = parent.RenderableType.AnimatedHeadType.TurnToLookLerpFactor;



            //add a portion of the difference between the current head orientation and a perfect straight-on angle
            //lookAngle = lookAngle * (1f - lerpFactor) + targetAngle * lerpFactor;
            lookAngle = Common.EaseInValueTowardsTarget(lookAngle, desiredHeadAngle, lerpFactor);
            */
            // Vector2 newHeadLookDirection = Common.AngleToVector(lookAngle);

            float numBonesAffected = parent.RenderableType.AnimatedHeadType.SpineBones.Length; // 5f;
            float boneAngle = lookAngle / numBonesAffected; // give each bone in the spine column a relative angle to the one below

            Vector3 rollAxis = new Vector3(0, 1, 0);//positive is dip right wing
            Vector3 yawAxis = new Vector3(1, 0, 0);//positive is bearing left
            Vector3 pitchAxis = new Vector3(0, 0, 1);//positive is dive

            Matrix middleTwist = Matrix.Identity;
            middleTwist *= Matrix.CreateFromAxisAngle(yawAxis, -boneAngle);//positive angle looks to character's left
            // TODO: tip head more than spine???
            middleTwist *= Matrix.CreateFromAxisAngle(pitchAxis, parent.RenderableType.AnimatedHeadType.PitchForward);//tip forward, otherwise the face will look straight up in the air

            string boneName;

            Matrix twistToUse;

            for (int i = 0; i < numBonesAffected; i++)
            {
                boneName = parent.RenderableType.AnimatedHeadType.SpineBones[i];

                BonePose bonePose = parent.RenderAsModel.AnimatedModel.ModelAnimator.BonePoses[boneName];

                twistToUse = middleTwist;

                if (numBonesAffected > 2)
                {
                    if (i == 0)
                    {
                        // lower spine - reduce angle
                        twistToUse = Matrix.Identity;
                        twistToUse *= Matrix.CreateFromAxisAngle(yawAxis, -0.5f * boneAngle);//positive angle looks to character's left

                        twistToUse *= Matrix.CreateFromAxisAngle(pitchAxis, 1f * parent.RenderableType.AnimatedHeadType.PitchForward);

                        // Matrix.deco
                    }
                    else if (i == numBonesAffected - 1)
                    {
                        // head - increase angle
                        twistToUse = Matrix.Identity;
                        twistToUse *= Matrix.CreateFromAxisAngle(yawAxis, -1.5f * boneAngle);// turn head more than spine/neck

                        twistToUse *= Matrix.CreateFromAxisAngle(pitchAxis, 1f * parent.RenderableType.AnimatedHeadType.PitchForward);

                    }
                }

                TwistBone(bonePose, ref twistToUse);
            }


        }


        

        private bool isLerpingBackSpine = false;
        private float lerpBackProgress = 0f;

        /*
        public void AnimateHead()
        {
            
            if (interestLevel <= 0f)
            {
                // head turn not active, OR turning back to neutral
                HandleNoInterest();
            }
            else
            {
                // update head turn towards the target of interest

                //having this in Draw instead of Update makes the counter decrease even when paused, hmmm:
                interestLevel -= GameData.Instance.Constants.InterestLevelDropOffPerFrame; //become less interesting // Lars: why is it ok to tie this to the frame rate...? perhaps it looks smoother / doesn't matter?

                Vector2? lookAt = GetPointToLookAt();
                               

                if (lookAt.HasValue)
                {
                    HandleTurnToInterest(lookAt);
                }               
            }
        }
        */
        

       

        public void Update()
        {

        }

       


      //  public static float TurnToLookLerpFactor = 0.06f;

       
     

        /// <summary>
        /// don't twist the head too much if it is not that interesting...
        /// </summary>
        /// <returns></returns>
     /*   private bool IsInterestHighEnough(float interestLevel)
        {
             Vector2 facing = parent.FacingNormal.Value.ToVector2();
             Vector2 looking = lookAt - (Parent.Location.ToVector2());
             looking.Normalize();

             float perpDot = facing.X * looking.Y - facing.Y * looking.X;

             if (perpDot > 0.95f) //almost facing now
                 return;
            

            return true;
        } */

     /*   private void TurnHeadBackToNormal()
        {
            // turn head back with same speed as before
            returnToNormalProgress = Common.EaseInValueTowardsTarget(returnToNormalProgress, 1f, parent.RenderableType.AnimatedHeadType.TurnToLookLerpFactor);

            if (returnToNormalProgress > 0.95f)
            {
                returnToNormalProgress = 1f;
                lookAngle = 0f;

                foreach (var boneName in parent.RenderableType.AnimatedHeadType.SpineBones)
                {
                    SwitchSpineBoneToNormal(boneName);
                    //TurnSpineBoneToNormal(boneName);
                }
            }

            return;
                    
           
        }*/

        public void StartLerpingBackSpine()
        {
            isLerpingBackSpine = true;
            lerpBackProgress = 0f;
        }

        public void UpdateLerpingBackSpine(GameTime gameTime)
        {
            if (isLerpingBackSpine)
            {
                lerpBackProgress += (float)gameTime.ElapsedGameTime.TotalSeconds * 1f; // 0.1f;

                if (lerpBackProgress >= 6f) // 1f)
                {
                    isLerpingBackSpine = false; // end it now
                }


                // gradually restore the spine bone DefaultTransforms
                foreach (var boneName in parent.RenderableType.AnimatedHeadType.SpineBones)
                {
                    LerpSpineBoneToNormal(boneName, isLerpingBackSpine);
                }
            }
        }

        private void TwistBone(BonePose bonePose, ref Matrix twist, bool resetAnimator = false)
        {

            // the translation part stays constant during the process! 
            Matrix targetTransform = twist * Matrix.CreateTranslation(bonePose.DefaultTransform.Translation);

            float lerpFactor = parent.RenderableType.AnimatedHeadType.TurnToLookLerpFactor;

            //add a portion of the difference between the current transform and the end transform - goes fast, then eases in..
            //targetTransform = Common.EaseInValueTowardsTarget(bonePose.DefaultTransform, targetTransform, lerpFactor);


            // perhaps make a separate property that overrides all animation transforms while head is turned? instead of the flag?
            bonePose.DefaultTransform = targetTransform; // twist * Matrix.CreateTranslation(bonePose.DefaultTransform.Translation); // ???
           

            // this makes us override animation transforms:
            bonePose.UseSpecialTransform = true; 
        }

        private void SwitchSpineBoneToNormal(string boneName)
        {
            // snap spine bones back to normal
         
          //  (twitches when snapping)

            BonePose bonePose = parent.RenderAsModel.AnimatedModel.ModelAnimator.BonePoses[boneName];

            // gets the original model transform not affected by animations:
            Matrix defaultModelBoneTransform = parent.RenderAsModel.AnimatedModel.ModelAnimator.Model.Bones[bonePose.Index].Transform;


            bonePose.DefaultTransform = defaultModelBoneTransform;

            bonePose.UseSpecialTransform = false; // true;
           

        }

        private void LerpSpineBoneToNormal(string boneName, bool isStillLerping)
        {
             BonePose bonePose = parent.RenderAsModel.AnimatedModel.ModelAnimator.BonePoses[boneName];

            // the default model pose:
            Matrix modelTransform = parent.RenderAsModel.AnimatedModel.ModelAnimator.Model.Bones[bonePose.Index].Transform;

           
            // when true, this overrides animation poses for the bone:
            bonePose.UseSpecialTransform = isStillLerping; 

            
            // smoothly turn spine bones back to normal
            if (isStillLerping)
            {
                float lerpFactor = parent.RenderableType.AnimatedHeadType.TurnToLookLerpFactor;
                
                //add a portion of the difference between the current transform and the end transform - goes fast, then eases in..
                Matrix transform = Common.EaseInValueTowardsTarget(bonePose.DefaultTransform, modelTransform, lerpFactor);


                bonePose.DefaultTransform = transform;
                
            }
            else
            {
                bonePose.DefaultTransform = modelTransform;
            }
        }


        /*
        private void TurnSpineBoneToNormal(string boneName)
        {
            // quickly lerp spine bones back to normal
            //float progress;

          //  return;

            BonePose bonePose = parent.RenderAsModel.AnimatedModel.ModelAnimator.BonePoses[boneName];

            Matrix defaultModelBoneTransform = parent.RenderAsModel.AnimatedModel.ModelAnimator.Model.Bones[bonePose.Index].Transform;

            Matrix twist;
            Util.SlerpMatrix(
                        ref bonePose.DefaultTransform,
                        ref defaultModelBoneTransform,
                        returnToNormalProgress,
                        out twist);

            bonePose.DefaultTransform = twist; // *Matrix.CreateTranslation(bonePose.DefaultTransform.Translation); // ???

            bonePose.useDefaultTransform = returnToNormalProgress >= 1f;

           // TwistBone(bonePose, ref twist, returnToNormalProgress >= 1f);

        }*/

    }
}
