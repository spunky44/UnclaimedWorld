using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps.MapEditor;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Trees;

namespace UWGame.SimSide.Entities
{
    /// <summary>
    /// a temporary vessel for editor data
    /// </summary>
    public class EditorData: Component
    {        
        public string AllegianceKey;

        public string ExpeditionName;

        /// <summary>
        /// only if allegiance is not filled:
        /// </summary>
        public string ThreatGroupName;

        //public long SpawnTimeInMilliseconds;

       // public float? SpawnTimeOfDay;
     //   public int? SpawnDay;

      //  public DateAndTime.TimeDateYear? SpawnDate;

        /// <summary>
        /// Only gets filled if the map designer has selected Young or Grown when placing the entity.
        /// </summary>
        public AgeGroup? TreeAgeGroup;

        /// <summary>
        /// designer placed resources and modifiers
        /// </summary>
        public Resource[] Resources;

        public EditorData(Entity parent)
            : base(parent)
        {
         
        }

        public EditorData()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }
       

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            //left blank. EditorData has no simulation role
            return this;
        }

        public override Snapshotter.Version DoVersion(Snapshotter sn)
        {
            //left blank. EditorData has no simulation role
            return Snapshotter.Version.Original;
        }
       
    }
}
