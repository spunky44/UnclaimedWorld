using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Processes;

namespace UWGame.SimSide.Jobs
{
    public class RepairJob : ISnapshot
    {
        /// <summary>
        /// the root!
        /// </summary>
      /*  public EntityID EntityToRepair;
                
       
        /// <summary>
        /// if filled, this will be the SimProcess.ActingOnEntity!
        /// </summary>
        public EntityID? PartToFix;
        */

        public EntityID EntityToRepair;

        /// <summary>
        /// optional
        /// </summary>
        public EntityAndRoot? PartToFix;


        public RepairAction RepairActionToUse;


        public RepairJob(EntityID entityToRepair, RepairAction repairAction, EntityAndRoot? partToFix) // EntityID? partToFix) 
            //: base(belongsTo, priority)
        {
            this.EntityToRepair = entityToRepair;
            this.PartToFix = partToFix;
            this.RepairActionToUse = repairAction;          

        }




        public RepairJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

       

      
        #region ISnapshot

        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            EntityToRepair = sn.DoEnum(EntityToRepair);
            RepairActionToUse = sn.DoEnum(RepairActionToUse);
            PartToFix = sn.DoEntityAndRootNullable(PartToFix);
           // PartToFix = sn.DoEnumNullable(PartToFix);
            
            return this;
        }

        Snapshotter.Version version;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original);
            return version;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

        }

        public bool IsSnapshotted
        {
            get;
            set;
        }


        #endregion
    }
}
