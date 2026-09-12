using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;
using Microsoft.Xna.Framework;

namespace UWGame.SimSide.Entities.Locomotors
{
    public class Rotator: ISnapshot
    {
        float relativeRotation;

        /// <summary>
        /// tracks the rotation of part of the entity. It would be illegal to use a bone rotation in simulations.
        /// </summary>
        public float RelativeRotation
        {
            set
            {
                if (relativeRotation != value)
                {
                    relativeRotation = value;
                    SetBoneAngle();
                }
            }
            get
            {
                return relativeRotation;
            }
        }

        public float AbsoluteRotation
        {
            set
            {
                relativeRotation = value - Parent.Parent.Rotation;
                SetBoneAngle();
            }
            get
            {
                return Parent.Parent.Rotation + relativeRotation;
            }
        }

        public Locomotor Parent;  

        public Rotator()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public Rotator(Locomotor parent)
        {
            this.Parent = parent;
        }

        private void SetBoneAngle()
        {
            Matrix rotationMatrix =
              Matrix.CreateFromYawPitchRoll(-relativeRotation, 0f, 0f); // why negate..?

            Parent.Parent.Renderable.SetModelBoneRotation(Parent.Parent.EntityType.LocomotorType.RotatorType.BoneKeyName, rotationMatrix);
        }

        public void DoneRotating()
        {
            Parent.Parent.Renderable.StopOverridingAnimTransforms(Parent.Parent.EntityType.LocomotorType.RotatorType.BoneKeyName);
        }

        #region ISnapshot


        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.relativeRotation = sn.DoFloat(relativeRotation);

            sn.Ignore(Parent); // gets pushed to us

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            //lookups and other fix-ups
        }


        #endregion
    }
}
