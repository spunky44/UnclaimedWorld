using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Items;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Commands;
using UWGame.SimSide.Expeditions;
using System.Diagnostics;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.AI.Planners
{

    /// <summary>
    /// PhysicalNeedsPlanner monitors the physical needs of the allegiance (food, water)
    /// creates jobs that increase the changes of getting more resources if needed
    /// 
    /// Likely, we will have more planners later - 
    /// each planner has a Motivation - Safe home area, physical needs met
    /// each planner can make a plan, and the plans will execute at the same time/independent
    /// the planners will compete for resources like members/workers/ etc.
    /// 
    /// 
    /// SecurityPlanner will organize defenses and attacks, 
    /// BuildPlanner (caves/tunnels)
    /// EmigratePlanner will make jobs for moving the whole allegiance to a new area if the area is depleted
    /// 
    /// 
    /// 
    /// All planners use Commands - the Commands create Jobs
    /// If actions do require Jobs, they belong in a differnet system (OtherJobManager?)
    /// 
    /// 
    /// Divide map in areas (use Regions?? they are continuous)
    /// score them and learn from the success - Reinforcement Learning (RL)
    /// 
    /// Job zones could cover more than one Region, like a group
    /// 
    /// [14:41:58] Lars Pedersen: we will need some OverPlanner that makes sure that the Planners don't give create contradicting commands
    /// 
    /// The planners should work through Actions - an Action has a method that can monitor itself, what its status is, whether it is getting fulfilled...
    /// An Action, when executed gives a Command. The command may create one or more Jobs.
    /// </summary>
    class PhysicalNeedsPlanner : Planner
    {               

        int numberOfScoutingActionsNeeded = 0;
        int numberOfHuntingActionsNeeded = 0;


      //  private PhysicalNeedsMotivation motivation;
               


        public PhysicalNeedsPlanner(Allegiance allegiance, Expedition expedition)
        {
            this.allegiance = allegiance;
            this.expedition = expedition;
        }

        public PhysicalNeedsPlanner()         
        {
            Debug.Assert(Snapshotter.IsSnapshotting, "Never call the empty ctor.");     
        }



        protected override void DestroyPlan()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// This is an temporary funtion. 
        /// 
        /// This code should be moved to the Planner.cs and this class should later on be removed.
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void InternalUpdate(GameTime gameTime)
        {
            if (motivation == null)
            {
                motivation = new PhysicalNeedsMotivation(allegiance);
            }

            MonitorCurrentPlan();
            CreatePlan();

        }


        /// <summary>
        /// score how well the physical needs motivation is being met.
        /// This may cause a plan to be created to fulfill the motivation.
        /// Motivation is an end state - when planning, actions are ordered to make state changes that end in the end state.
        /// 
        /// TODO: As it looks now this could be an bool instead of double as we eather are motivated to create new task or we are not.
        /// 
        /// </summary>
        /// <returns></returns>
        protected override double ScoreMotivation()
        {
            
            if ((motivation as PhysicalNeedsMotivation).GetActionsWeAreMotivatedToDo(out numberOfScoutingActionsNeeded, out numberOfHuntingActionsNeeded))
            {
                return 1;//We want to create actions for our PhysicalNeeds
            }
            return 0;//We do not need to create any actions.

        }

        /// <summary>
        /// Goes trough numberOfScoutingActionsNeeded and numberOfHuntingActionsNeeded 
        /// created by our physicalNeedsMotivation
        /// And cretes one action for each needed and adds this to our currentPlan.
        /// 
        /// TODO:
        /// Later on we will remove numberofscouting jobs and hunting jobs and instead each planner has a set of actions
        /// Then we will go trough our motivations and use the sets of actions that we got assigned to create our plan
        /// This will make this class not needed and all code that is left after this change can be moved to Planner.cs
        /// </summary>
        protected override void CreatePlan()
        {

            if (ScoreMotivation() > 0)
            {

                for (int i = 0; i < numberOfScoutingActionsNeeded; i++)
                {
                    GoapScoutAction action = new GoapScoutAction(allegiance,expedition);
                    AddNewAction(action);
                }

                for (int i = 0; i < numberOfHuntingActionsNeeded; i++)
                {
                    GoapFindPreyAction action = new GoapFindPreyAction(allegiance,expedition);
                    AddNewAction(action);
                }
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

            this.numberOfScoutingActionsNeeded = sn.DoInt32(numberOfScoutingActionsNeeded);
            this.numberOfHuntingActionsNeeded = sn.DoInt32(numberOfHuntingActionsNeeded);


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
