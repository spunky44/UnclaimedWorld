using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Pathfinding
{
    public class RegionPathFinderNodeBFS: ISnapshot
    {       
        /// <summary>
        /// 'Gone' = actual cost
        /// </summary>
        public float G;
        public ushort Color;

      
        public byte Status;


        public RegionPathFinderNodeBFS() { }

        public RegionPathFinderNodeBFS(float cost, ushort region)
        {
            G = cost;
            Color = region;

            Status = RegionSearcher.StatusClosed;
        }


        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {

            G = sn.DoFloat(G);
            Color = sn.DoUInt16(Color);
            Status = sn.DoByte(Status);
                      

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
