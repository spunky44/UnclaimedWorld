using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Planners
{
    class PhysicalNeedsMotivation : Motivation
    {

        private int maxNrOfHuntingJobs; // uses member count times a fraction
        private int maxNrOfScoutingJobs;

        Allegiance allegiance;
        AllegianceID snapshotAllegiance;

        public PhysicalNeedsMotivation() { }

        public PhysicalNeedsMotivation(Allegiance allegiance)
        {
            this.allegiance = allegiance;
            maxNrOfHuntingJobs = allegiance.Members.Count;
            maxNrOfScoutingJobs = allegiance.Members.Count;
        }

        public bool GetActionsWeAreMotivatedToDo(out int numberOfScoutingActionsNeeded, out int numberOfHuntingActionsNeeded)
        {
            return NeedsMoreResources(out numberOfScoutingActionsNeeded,out numberOfHuntingActionsNeeded);
        }

        private bool NeedsMoreResources(out int numberOfScoutingActionsNeeded, out int numberOfHuntingActionsNeeded)
        {

            

            float members = allegiance.Members.Count;
            maxNrOfScoutingJobs = (int)(members * allegiance.RepresentativeEntityType.IntelligenceType.MembersScoutingFraction);
            maxNrOfHuntingJobs = (int)(members * allegiance.RepresentativeEntityType.IntelligenceType.MembersScoutingFraction);

            numberOfScoutingActionsNeeded = 0;
            numberOfHuntingActionsNeeded = 0;
            if (DoesThisAllegianceNeedMoreHuntingJobs())
            {
                //TODO: Make Hunting Jobs Work
                //numberOfHuntingActionsNeeded = maxNrOfScoutingJobs - allegiance.SharedKnowledge.AllKnownEntities.FindPreyJobs.Count();
                //Create hunting jobs
            }
            if (DoesThisAllegianceNeedMoreScoutingJobs())
            {
                numberOfScoutingActionsNeeded = maxNrOfScoutingJobs - allegiance.SharedKnowledge.AllKnownEntities.ScoutingJobs.Count();
                //Create scouting jobs
            }
            // return the number of resource location jobs (hunt/scout) that are needed 

            // the jobs that are created will depend on the resources that we need
            // protein need should be linked to hunting/scouting for carcasses
            // energy need should be linked to scouting... but the line is blurry, so we should probably use weighted scoring


            return (numberOfScoutingActionsNeeded + numberOfHuntingActionsNeeded > 0);
        }

        

        private bool DoesThisAllegianceNeedMoreHuntingJobs()
        {
            // TEMPORARY FUNCTIONALLITY
            if (allegiance.SharedKnowledge.AllKnownEntities.ScoutingJobs.Count > maxNrOfHuntingJobs)
            {
                return false;
            }
            return true;
            //Look at if this allegiance got enough food to supply its members 

        }

        private bool DoesThisAllegianceNeedMoreScoutingJobs()
        {
            // TEMPORARY FUNCTIONALLITY
            if (allegiance.SharedKnowledge.AllKnownEntities.ScoutingJobs.Count > maxNrOfScoutingJobs)
            {
                return false;
            }
            return true;
            // See if we want to create more scouting jobs for this allegiance
            //Depending on how many jobs we got in the different areas on the map?

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

            this.snapshotAllegiance = (AllegianceID)sn.SnapshotID<Allegiance, AllegianceID>(allegiance);
            this.maxNrOfHuntingJobs = sn.DoInt32(maxNrOfHuntingJobs);
            this.maxNrOfScoutingJobs = sn.DoInt32(maxNrOfScoutingJobs);
         

            return this;
        }

        public override void LoadPostProcess(Snapshots.Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            base.LoadPostProcess(sn);

            allegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegiance);
       
        }


        #endregion

    }
}
