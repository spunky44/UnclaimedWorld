using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.AI.Planners;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Planners
{
    class GoapFindPreyAction : GoapAction
    {
        FindPreyJob currentJob = null;
        JobID? snapshotJob;


        public GoapFindPreyAction()
        { }

        public GoapFindPreyAction(Allegiance allegiance, Expedition expedition)
            : base(allegiance, expedition)
        {
           
        }


        public override bool Init()
        {            
            MapArea mapArea = ScoreAndGetHuntingJobLocation();
            if (mapArea == null)
            {
                return false;
            }

            Zone huntZone = null;
            foreach (var item in allegiance.RepresentativeEntityType.IntelligenceType.PreyTypes)
            {
                HuntArea huntAreaCommand;

                // hunt one critter at a time
                if (huntZone == null)
                {
                    huntAreaCommand = new HuntArea(mapArea, false, item, 1, true, allegiance.SharedKnowledge.AllKnownEntities.ID);
                }
                else
                {
                    huntAreaCommand = new HuntArea(huntZone.ID, false, item, 1, true, allegiance.SharedKnowledge.AllKnownEntities.ID);
                }

                huntAreaCommand.Execute(false);

                huntZone = huntAreaCommand.GetZone();
            }


            
            return true;
            //TODO: Set current job
        }

        public override bool MonitorAction()
        {
           
            if (currentJob == null)
            {
                return false; //Bailout. Remove this aciton as it does not have any job anymore.
            }
            if (currentJob.TakenBy.Count > 0)
            {
                return true;
            }

            //Noone is currently working with this action.
            //Perhaps add a timer that automaticly removes this job if it has not been taken for x ammount of time?
            /*actionCurrentInProgressTimer += elapsedTime;
              if(actionCurrentlyInProgressTimer > thisActionHasBeenUnactiveForToLong)
              {
                return false;
              }
             */
            return true;
        
        }

        public override void Destroy()
        {
            if (currentJob != null)
            {
                for (int i = 0; i < currentJob.TakenBy.Count; i++)
                {
                    //Remove all takers etc.
                    //Remove job from list where it was added in RunAction

                }
            }
        }

        public MapArea ScoreAndGetHuntingJobLocation()
        {
            return GoapAction.GetRandomMapArea(expedition, allegiance);
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

        public override Snapshots.ISnapshot DoSnapshot(Snapshots.Snapshotter sn)
        {
            base.DoSnapshot(sn);

            this.snapshotJob = sn.SnapshotID<Job, JobID>(currentJob);

            return this;
        }

        public override void LoadPostProcess(Snapshots.Snapshotter sn)
        {
            base.LoadPostProcess(sn);

            if (snapshotJob.HasValue)
            {
                this.currentJob = (FindPreyJob)LookUp<Job, JobID>.FindByID(snapshotJob);
                snapshotJob = null;
            }
        }

        #endregion
    }
}
