using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xclna.Xna.Animation;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Entities
{
    public class SteerableFrontWheels: Component
    {
      
        const float MaxWheelAngle = 0.8f;
        const float MaxWheelSteeringAngleChange = 2f;

        public float WheelsAngle = 0f;

      //  BonePose leftFrontWheel;
    //    BonePose rightFrontWheel;


        const string leftWheelName = "wheel_front_left_joint";
        const string rightWheelName = "wheel_front_right_joint";

        public SteerableFrontWheels(Entity parent): base(parent)
        {
            if (parent.Renderable == null)
                return;

           
          //  leftFrontWheel = parent.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.BonePoses["wheel_front_left_joint"];
         //   rightFrontWheel = parent.Renderable.RenderAsModel.AnimatedModel.ModelAnimator.BonePoses["wheel_front_right_joint"];

        }

        public SteerableFrontWheels()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }
        

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            sn.DoFloat(this.WheelsAngle);

            return this;
        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            base.DoVersion(sn); // each class in the class hierarchy snapshots and maintains their own version.

            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        // same as for aircraft Roll
        public void SetAngle(float headingDifference, float elapsedTime)
        {
            float desiredAngle = headingDifference; 
            float angleDifference = desiredAngle - WheelsAngle;

            if (!Common.IsEqual(angleDifference, 0f))
            {
                // wheel rotate Speed
                float maxWheelSteeringAngleChange = MaxWheelSteeringAngleChange * elapsedTime;
                angleDifference = MathHelper.Clamp(angleDifference, -maxWheelSteeringAngleChange, maxWheelSteeringAngleChange);

                WheelsAngle += angleDifference;

                WheelsAngle = MathHelper.Clamp(WheelsAngle, -MaxWheelAngle, MaxWheelAngle);

            }

            
            SetWheelAngle(WheelsAngle, leftWheelName); // leftFrontWheel);
            SetWheelAngle(WheelsAngle, rightWheelName); // rightFrontWheel);
        }



        private void SetWheelAngle(float rotate, string wheelBoneName) //BonePose duct)
        {
          //  Matrix defaultTransform = parent.Renderable.GetModelBoneDefaultTransformation(wheelBoneName);

            Matrix rotationMatrix =
              Matrix.CreateFromYawPitchRoll(-rotate, 0f, 0f);

            Parent.Renderable.SetModelBoneRotation(wheelBoneName, rotationMatrix);

            /*
            duct.MainAnimationTrack = null;
            duct.CurrentBlendController = null;

            duct.DefaultTransform =
              Matrix.CreateFromYawPitchRoll(-rotate, 0f, 0f) *
                Matrix.CreateTranslation(duct.DefaultTransform.Translation);
            */

        }



    }
}
