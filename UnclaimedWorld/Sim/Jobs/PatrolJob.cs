using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Maps;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.AI;
using UWGame.SimSide.Items;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Jobs
{
    public class PatrolJob : CombatAreaJob
    {       

        public bool AttackVermin = false;
        public bool AttackTargetsOutsideZone = false;

       

        public PatrolJob()
        {
            System.Diagnostics.Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");
        }

        public PatrolJob(Zone zone, EntityGroup entityGroup,  bool attackVermin, bool attackTargetsOutsideZone, int noOfPatrollers)
            : base(zone, entityGroup, noOfPatrollers) 
        {
           
            this.AttackVermin = attackVermin;
            this.AttackTargetsOutsideZone = attackTargetsOutsideZone;
        }

       

        public override string GetName()
        {
            return "Patrolling";
        }

        public override void Destroy(bool removeTakers, Entity entityToExcludeFromCancel = null)
        {
            //remove the zone order
            if (Zone != null) // && MapArea.Zone != null)
            {
                if (Zone.PatrolJob != null)
                {
                    Zone.PatrolJob = null;
                }
            }
            base.Destroy(removeTakers, entityToExcludeFromCancel);

        }

        public override bool CanAttackVermin
        {
            get { return AttackVermin; }
        }

        public override bool CanAttackTargetsOutsideZone
        {
            get { return AttackTargetsOutsideZone; }
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
           
            AttackVermin = sn.DoBool(AttackVermin);
            AttackTargetsOutsideZone = sn.DoBool(AttackTargetsOutsideZone);
            

            return this;
        }

        public override void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);
        }


        #endregion
    }
}
