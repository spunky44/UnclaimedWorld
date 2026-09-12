using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;

namespace Xclna.Xna.Animation
{
    /// <summary>
    /// A collection of BoneKeyFrames that represents an animation track.
    /// </summary>
    public class BoneKeyFrameCollection : ReadOnlyCollection<BoneKeyFrame>
    {
        #region Member Variables
        // The name of the bone represented by this animation track
        private string boneName;

        /// <summary>
        /// Duration of the track 
        /// NEW: maximum duration is 500s!
        /// </summary>
        private uint duration;
        // private long duration;
        #endregion

        #region Constructors
        // Only allow creation from inside the library (only in AnimationReader)
        internal BoneKeyFrameCollection(string boneName,
            IList<BoneKeyFrame> list)
            : base(list)
        {
            this.boneName = boneName;
            duration = list[list.Count - 1].Time;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the duration of the animation track.
        /// </summary>
        public uint /* long*/ Duration
        {
            get { return duration; }
        }

        /// <summary>
        /// Gets the name of the bone associated with the animation track.
        /// </summary>
        public string BoneName
        { get { return boneName; } }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the index in the track at the given time.
        /// </summary>
        /// <param name="ticks">The time for which the index is found.</param>
        /// <returns>The index in the track at the given time.</returns>
        public int GetIndexByTime(long ticks)
        {
            // Since the animation is usually interpolated to 60 fps, this will
            // almost always be the index to return
            int firstFrameIndexToCheck = (int)(ticks / Util.SampleDistance); // Util.TICKS_PER_60FPS);
            // Do out of bounds checking
            if (firstFrameIndexToCheck >= base.Count)
                firstFrameIndexToCheck = base.Count - 1;
            // Increment the index until the time at the next index is greater than the
            // specified time
            while (firstFrameIndexToCheck < base.Count - 1
                    && base[firstFrameIndexToCheck + 1].Time < ticks)
            {
                ++firstFrameIndexToCheck;
            }
            // Decrement the index till the time at the index is not greater than the
            // specified time
            while (firstFrameIndexToCheck >= 0 && base[firstFrameIndexToCheck].Time >
                ticks)
            {
                --firstFrameIndexToCheck;
            }

            return firstFrameIndexToCheck;
        }

        /*
        public void GetIndexesByTime(long ticks, out int index1, out int? index2, out double? fraction)
        {
            
            // Since the animation is usually interpolated to 60 fps, this will
            // almost always be the index to return
          //  int firstFrameIndexToCheck = (int)(ticks / Util.TICKS_PER_60FPS);

           // double firstFrame = (double)ticks / (double)Util.TICKS_PER_60FPS;

            long remainder;
            long firstFrameIndex = Math.DivRem(ticks, Util.SampleDistance , out remainder);

            fraction = (double)remainder / (double)Util.SampleDistance; // Util.TICKS_PER_60FPS;

            int firstFrameIndexToCheck = (int)firstFrameIndex;

            bool wasModified = false;

            // Do out of bounds checking
            if (firstFrameIndexToCheck >= base.Count)
                firstFrameIndexToCheck = base.Count - 1;
            // Increment the index until the time at the next index is greater than the
            // specified time
            while (firstFrameIndexToCheck < base.Count - 1
                    && base[firstFrameIndexToCheck + 1].Time < ticks)
            {
                ++firstFrameIndexToCheck;
                wasModified = true;
            }
            // Decrement the index till the time at the index is not greater than the
            // specified time
            while (firstFrameIndexToCheck >= 0 && base[firstFrameIndexToCheck].Time >
                ticks)
            {
                --firstFrameIndexToCheck;
                wasModified = true;
            }


            if (!wasModified)
            {
                index1 = firstFrameIndexToCheck;

                // can we wrap interpolate too..?
                if (index1 + 1 < base.Count)
                {
                    index2 = index1 + 1;
                }
                else
                {
                    fraction = null;
                    index2 = null;
                }

            }
            else
            {
                fraction = null;
                index2 = null;
                index1 = firstFrameIndexToCheck;
            }
           
        }*/

        #endregion
    }
}
