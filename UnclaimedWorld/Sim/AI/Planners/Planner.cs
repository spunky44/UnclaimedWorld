using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Allegiances;
using UWGame.SimSide.Expeditions;

namespace UWGame.SimSide.AI.Planners
{
    abstract class Planner: ISnapshot
    {
        protected Allegiance allegiance;
        AllegianceID snapshotAllegiance;

        protected Expedition expedition;
        ExpeditionID snapshotExpedition;

        private List<RegionKnowledge> RegionKnowledge;

        protected Motivation motivation;

        /// <summary>
        /// the current plan - most likely it will have 1 action
        /// 
        /// monitor progress - if no progress, apply learning weights, then replan
        /// </summary>
        protected List<GoapAction> currentPlan = new List<GoapAction>();



        protected abstract void CreatePlan();

        protected abstract void DestroyPlan();


        /// <summary>
        /// test motivation - if a plan is needed, create a plan.
        /// if a plan exists, monitor its progress. destroy it and replan if it has stalled.
        /// 
        /// if the motivation is fulfilled, it means no plan is needed. destroy the current plan if it is running.
        /// 
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            InternalUpdate(gameTime); //TEMP. Moving old functionallity out here


            //Design for how this class will look when the code in PhysicalNeedsPlanner gets moved here and then removed
            //We will only use one planner class.
           /* if (motivation.IsFulfilled())
            {
                // no plan is needed.  destroy the current plan if it is running.

            }
            else
            {
                if (currentPlan != null && currentPlan.Count > 0)
                {
                    //if a plan exists, monitor its progress. destroy it and replan if it has stalled.


                }
                else
                {
                    // create a plan.

                }
            }*/
        }

        protected abstract void InternalUpdate(GameTime gameTime);
        protected abstract double ScoreMotivation();

      

        /// <summary>
        /// Goes trough each action in our currentPlan and calls MonitorAction on them
        /// This function will try to do 3 things.
        /// Learn about the failed action so we try not to do it again
        /// Deactivate the action so it no longers runs in the game (remove created jobs etc.)
        /// And then we remove the plan from our currentPlans
        /// </summary>
        protected void MonitorCurrentPlan()
        {
            if (currentPlan != null)
            {
                List<GoapAction> actionsToRemove = null;
                foreach (var action in currentPlan)
                {
                    if (action.MonitorAction() == false)
                    {
                        LearnAboutFailedAction(action);
                        action.Destroy();

                        Common.AddToList(ref actionsToRemove, action);                       
                    }
                }

                if (actionsToRemove != null)
                {
                    foreach (var action in actionsToRemove)
                    {
                        currentPlan.Remove(action);
                    }
                }
            }
        }

        /// <summary>
        /// In here we should learn about an failed action. 
        /// 
        /// TODO: How?
        /// Use RegionKnowledge?
        /// </summary>
        /// <param name="action"></param>
        protected void LearnAboutFailedAction(GoapAction action)
        {

        }
        

        protected void AddNewAction(GoapAction action)
        {

            if (action.Init())
            {

                currentPlan.Add(action);
            }
        }



        #region ISnapshot

        public virtual ISnapshot DoSnapshot(Snapshotter sn)
        {
            this.snapshotAllegiance = (AllegianceID)sn.SnapshotID<Allegiance, AllegianceID>(allegiance);
            this.snapshotExpedition = (ExpeditionID)sn.SnapshotID<Expedition, ExpeditionID>(expedition);

            motivation = (Motivation)sn.DoISnapshot(motivation);

            currentPlan = sn.DoList(currentPlan);


            sn.Postpone(RegionKnowledge);

            return this;

        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public virtual Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public virtual void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            allegiance = LookUp<Allegiance, AllegianceID>.FindByID(snapshotAllegiance);
            expedition = LookUp<Expedition, ExpeditionID>.FindByID(snapshotExpedition);

            if (motivation != null)
            {
                motivation.LoadPostProcess(sn);
            }

            if (currentPlan != null)
            {
                foreach (var item in currentPlan)
                {
                    item.LoadPostProcess(sn);
                }
            }

        }

        #endregion
    }
}
