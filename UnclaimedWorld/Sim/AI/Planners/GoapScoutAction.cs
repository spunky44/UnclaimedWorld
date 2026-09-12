using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Jobs;
using UWGame.SimSide.Entities;
using UWGame.SimSide.AI.Goals;
using UWGame.SimSide.Expeditions;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Planners
{
    class GoapScoutAction : GoapAction
    {
        ScoutingJob currentJob = null;
        JobID? snapshotJob;


        public GoapScoutAction()
        {

        }
     

        public GoapScoutAction(Allegiance allegiance, Expedition expedition): base(allegiance, expedition)
        {
           

        }

        public override bool Init()
        {
           
            MapArea mapArea = ScoreAndGetScoutingJobLocation();
            if (mapArea == null)
            {
                return false;
            }

            Scout scoutArea = new Scout(mapArea, false, allegiance.SharedKnowledge.AllKnownEntities.ID);
            scoutArea.Execute(false);

            int numberOfScoutingJobs = allegiance.SharedKnowledge.AllKnownEntities.ScoutingJobs.Count;

            //TEMP WAY TO SAVE THE JOB
            //Get it from the action?
            currentJob = allegiance.SharedKnowledge.AllKnownEntities.ScoutingJobs[numberOfScoutingJobs - 1] as ScoutingJob;
            //Set currentJob to the job we just created.
            return true;
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
            if (!allegiance.SharedKnowledge.AllKnownEntities.ScoutingJobs.Contains(currentJob))
            {
                return false;
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
                
                //for (int i = 0; i < currentJob.TakenBy.Count; i++)
                //{
                //    currentJob.Abandon(currentJob.TakeJob[i]);
                //    //Remove all takers etc.
                //    //Remove job from list where it was added in RunAction

                //}
                currentJob.Destroy(true);
            }
        }

        public MapArea ScoreAndGetScoutingJobLocation()
        {
            return GoapAction.GetRandomMapArea(expedition,allegiance);
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



            sn.Ignore(currentJob);

            return this;
        }

        public override void LoadPostProcess(Snapshots.Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            if (snapshotJob.HasValue)
            {
                this.currentJob = (ScoutingJob)LookUp<Job, JobID>.FindByID(snapshotJob);
                snapshotJob = null;
            }
        }

        #endregion

    }
}
