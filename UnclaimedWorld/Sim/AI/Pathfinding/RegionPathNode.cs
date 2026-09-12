using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Pathfinding
{
    [DebuggerDisplay("{RegionCenterInSubtiles}")]
    public class RegionPathNode: ISnapshot
    {
        public Point RegionCenterInSubtiles;


        public RegionPathNode()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor");
        }

        public RegionPathNode(RegionPathFinderNodeAStar pathNode)
        {
            RegionCenterInSubtiles = Maps.MapManager.WorldPosToSubtile(pathNode.CenterLocation);

        }



        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            RegionCenterInSubtiles = sn.DoPoint(RegionCenterInSubtiles);
          
            return this;
        }

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

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }

        #endregion
    }
}
