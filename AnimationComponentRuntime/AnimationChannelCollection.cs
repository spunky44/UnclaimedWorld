using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;

namespace Xclna.Xna.Animation
{

    /// <summary>
    /// A collection of animation channels or tracks, which are sections of an
    /// animation that run for one bone.
    /// </summary>
    public class AnimationChannelCollection : ReadOnlyCollection<BoneKeyFrameCollection>
    {
        // Allow quick access to channels by BoneName
        private Dictionary<string, BoneKeyFrameCollection> dict =
            new Dictionary<string, BoneKeyFrameCollection>();

        // The bones affected by the tracks contained in this collection
        private ReadOnlyCollection<string> affectedBones;

        // This immutable data structure should not be created by the library user
        internal AnimationChannelCollection(IList<BoneKeyFrameCollection> channels)
            : base(channels)
        {
            // Find the affected bones
            List<string> affected = new List<string>();
            foreach (BoneKeyFrameCollection frames in channels)
            {
                dict.Add(frames.BoneName, frames);
                affected.Add(frames.BoneName);
            }
            affectedBones = new ReadOnlyCollection<string>(affected);

        }

        /// <summary>
        /// Gets the BoneKeyframeCollection that is associated with the given bone.
        /// </summary>
        /// <param name="boneName">The name of the bone that contains a track in this
        /// AnimationChannelCollection.</param>
        /// <returns>The track associated with the given bone.</returns>
        public BoneKeyFrameCollection this[string boneName]
        {
            // We got an exception here once because an anim had a different amount of bones to the main mdoel/other anims...
            get { return dict[boneName]; }
        }

        // See AnimationInfo's equivalent method for documentation
        internal bool AffectsBone(string boneName)
        {
            return dict.ContainsKey(boneName);
        }

        // See AnimationInfo's equivalent method for documentation
        internal ReadOnlyCollection<string> AffectedBones
        {
            get { return affectedBones; }
        }
    }

}
