using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs
{
    /// <summary>
    /// SCOUT + EXAMINE
    /// </summary>
    public class ScoutingJob: Job
    {
        /// <summary>
        /// TODO: delete this in a new version...
        /// use ONE of the below:
        /// </summary>
        public Vector3? Location;
       
        public Zone Zone;
        ZoneID? snapshotZone;

        public bool Examine //= false;
        {
            get;
            private set;
        }

        /*
        public ScoutingJob(Vector3 location, EntityGroup entityGroup, bool examine) //, Priority priority)
            : base(entityGroup) //, priority)
        {
            Location = location;
            Examine = examine;

            ComputeJobType();
            SetDefaultPriority(entityGroup);
        }*/

        public ScoutingJob(Zone mapArea, EntityGroup entityGroup, bool examine) //, Priority priority)
            : base(entityGroup) //, priority)
        {
            Zone = mapArea;
            Examine = examine;

            ComputeJobType();
            SetDefaultPriority(entityGroup);
        }

       

        public ScoutingJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public override void Destroy(bool removeTakers, Entity entityToExcludeFromCancel = null)
        {
            if (Zone != null) // && MapArea.Zone != null)
            {
                if (Examine)
                {
                    // MapArea.Zone.Forage = false;

                    if (Zone.ExamineJob != null)
                    {
                        Zone.ExamineJob = null;
                    }
                }
                else
                {
                    //  MapArea.Zone.Scout = false;

                    if (Zone.ScoutingJob != null)
                    {
                        Zone.ScoutingJob = null;
                    }
                }
            }
            base.Destroy(removeTakers, entityToExcludeFromCancel);

        }
        /*
        public override JobTypes.JobType GetJobType()
        {
            return base.GetJobType();
        }*/

        public override Vector3? GetCircaLocation()
        {
            return Zone?.MapArea?.GetCenter();
        }

        public override void GetLocation(out Point? tile, out EntityID? targetEntity, out ZoneID? zoneID)
        {
           
            tile = null;
            targetEntity = null;
            zoneID = null;

            if (Zone == null && Location.HasValue)
            {
                tile = MapManager.WorldPosToTile(Location.Value);
            }
            else if (Zone != null) // .MapArea.StartDragTile.HasValue == true)
            {
                zoneID = Zone.ID;              
            }                    
        }

        //public override void RemoveFinishedJob()
        //{
        //    // remove the zone order
        //    if (Zone != null) // && MapArea.Zone != null)
        //    {
        //        if (ForageForResources)
        //        {
        //           // MapArea.Zone.Forage = false;

        //            if (Zone.ForageJob != null)
        //            {
        //                Zone.ForageJob = null;
        //            }
        //        }
        //        else
        //        {
        //          //  MapArea.Zone.Scout = false;

        //            if (Zone.ScoutingJob != null)
        //            {
        //                Zone.ScoutingJob = null;
        //            }
        //        }
        //    }

        //    base.RemoveFinishedJob();
        //}

        public override string GetName()
        {
            if (Examine)
            {
                return "Examining area"; //MP changed from "Foraging"
            }
            else
            {
                return "Scouting";
            }
        }

        #region ISnapshot

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

        public override ISnapshot DoSnapshot(Snapshotter sn)
        {
            base.DoSnapshot(sn);

            snapshotZone = sn.SnapshotID<Zone, ZoneID>(Zone);
            Location = sn.DoVector3Nullable(Location);
            Examine = sn.DoBool(Examine);


            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);
            
            Zone = LookUp<Zone, ZoneID>.FindByID(snapshotZone);
        } 


        #endregion
    }
}
