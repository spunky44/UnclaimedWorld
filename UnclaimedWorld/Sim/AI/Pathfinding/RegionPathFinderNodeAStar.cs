using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Pathfinding
{
    
    public class RegionPathFinderNodeAStar: ISnapshot
    {       
        /// <summary>
        /// 'Gone' = actual cost
        /// </summary>
        public float G;
        public ushort Color;

        // NEW: for AStar search:
        public float F; // f = gone + heuristic

        /// <summary>
        /// the region center. used in the AStar heuristic.
        /// </summary>
        public Vector2 CenterLocation;
        


        public ushort Parent;

        public byte Status;



        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            G = sn.DoFloat(G);
            F = sn.DoFloat(F);
            Color = sn.DoUInt16(Color);
            Parent = sn.DoUInt16(Parent);
            Status = sn.DoByte(Status);
            CenterLocation = sn.DoVector2(CenterLocation);

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
