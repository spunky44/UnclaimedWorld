using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;

namespace Xclna.Xna.Animation
{
     /// <summary>
    /// Represents a keyframe in an animation track.
    /// </summary>
    public struct BoneKeyFrame
    {
        /// <summary>
        /// The transform for the keyframe.
        /// </summary>
       // public readonly Matrix Transform;

        public readonly Quaternion Rotation;
        public readonly Vector3 Translation;
        public readonly Vector3 Scaling;
                


        /// <summary>
        /// The time for the keyframe.
        /// </summary>
        public readonly uint Time;
        // public readonly long Time;


         /// <summary>
        /// Creats a new BoneKeyframe.
        /// </summary>
        /// <param name="transform">The transform for the keyframe.</param>
        /// <param name="time">The time in ticks for the keyframe.</param>
        public BoneKeyFrame(Quaternion rotation, Vector3 translation, Vector3 scaling, uint time) // long time)
        {
            this.Rotation = rotation;
            this.Scaling = scaling;
            this.Translation = translation;

            this.Time = time;
        }

        /*
        public BoneKeyFrame(Matrix transform, uint time) // long time)
        {
            this.Transform = transform;
            this.Time = time;
        }*/


        public Matrix ComputeMatrix()
        {
            return Matrix.CreateScale(Scaling)
                   * Matrix.CreateFromQuaternion(Rotation)
                   * Matrix.CreateTranslation(Translation);

        }
       
    }
}
