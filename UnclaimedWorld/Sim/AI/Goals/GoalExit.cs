using System;
using System.Collections.Generic;
using System.Text;
using UWGame.SimSide.Items;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Buildings;
using Microsoft.Xna.Framework;
using UWGame.ClientSide.Map;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.InGameEvents.Actions;
namespace UWGame.SimSide.AI.Goals
{
    
    class GoalExit: Goal
    {

        bool isDisembarking;

        public GoalExit(Entity owner, bool isDisembarking = false)  
            : base(owner)
        {
            this.isDisembarking = isDisembarking;
        }

       


        public GoalExit()
        {
        }


        protected override void Activate()
        {
            Entity entityToExit;
            if (!entity.GetContainedBy(out entityToExit))
            {
                Status = Goals.Status.Failed;
                return;
            }

            if (entityToExit != null)
            {
                //here, reserve an exit from the IExit of the insideBuilding's container 


                Status = Status.Active;
            }
            else 
            {
                Status = Status.Completed;
            }

        }

        public override bool IsSame(Jobs.Job job)
        {
            return false;
        }

        protected override void ProcessWhileActive(GameTime elapsed)
        {
            //if status is inactive, call Activate()
        /*    ActivateIfInactive();

            if (Status == Status.Active)
            {*/

                Entity buildingWeLeft;
                if (!entity.GetContainedBy(out buildingWeLeft))
                {
                    Status = Goals.Status.Failed;
                  //  ExitIfFailedOrCompleted();

                    return; // Status;
                }

               // buildingWeLeft.Contains.Remove(entity);
                Vector3 exitLocation = Vector3.Zero; 
                
                //here, get the door location from the IExit
                ExitDoor doorThatWasUsed;
                ((IExit)(buildingWeLeft.Contains)).GetDoorPosition(ref exitLocation, true, out doorThatWasUsed);

                // perhaps put this code in IExit.UseDoor()
                buildingWeLeft.Contains.Uncontain(entity, shouldQueue: true, placeOnGround: exitLocation);
              //  buildingWeLeft.Contains.Uncontain(entity, false, true, null, null, null, null, exitLocation);

                // go to rally point?
              //  ((IExit)(buildingWeLeft.Contains)).GetRallyPoint();
               

              //  entity.Location = buildingWeLeft.Location;


                int noOfPeopleInside = ((IGarrison)(buildingWeLeft.Contains)).GetNoOfAgentsInside();
                if (noOfPeopleInside == 0)
                {
                    buildingWeLeft.TurnOffTheLight();                   
                }


                if (isDisembarking)
                {
                    if (entityIntelligence.Allegiance.AllegianceType == AllegianceType.Player)
                    {
                        // fire triggers, events etc. if disembarking for the first time
                        List<ActionSets> defaultActionSets;
                        entity.EntityType.IntelligenceType.EventActions.TryGetValue(AgentActionHooks.DisembarkedNewPlayerAllegianceMember, out defaultActionSets);

                        Goal.FireEventActions(entity, null, defaultActionSets, null);
                    }
                }


                Status = Status.Completed;
           /* }

            ExitIfFailedOrCompleted();

            return Status;*/
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

            isDisembarking = sn.DoBool(isDisembarking);

            return this;
        }

        #endregion
    }
}
