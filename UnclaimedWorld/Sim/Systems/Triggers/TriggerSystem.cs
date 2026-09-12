using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using UWGame.SimSide.Entities;
using Microsoft.Xna.Framework;
using UWGame.SimSide.Maps;
using UWGame.SimSide.Snapshots;

namespace UWGame.SimSide.Systems.Triggers
{
    public enum TriggerPriority { Highest = 0, High = 1, Normal = 2, Low = 3, Lowest = 4 };
   
    public class TriggerSystem: ISnapshot
    {
       
        /// <summary>
        /// TODO: this quad tree will most likely contain all agents, plus a portion of static entities. Not like the 10.000s of entities though.
        /// </summary>
        //private PointQuadTree<Entity> triggerListenersQuadTree;

        /// <summary>
        /// we have several sleepy update lists, one for each priority level:
        /// </summary>
        private SortedList<TriggerPriority, SleepyUpdater<Trigger>> allTriggers = new SortedList<TriggerPriority,SleepyUpdater<Trigger>>();
        private Dictionary<TriggerPriority, List<TriggerID>> snapshotAllTriggers = new Dictionary<TriggerPriority, List<TriggerID>>();

        public TriggerSystem()
        {
           /* if (Snapshotter.IsSnapshotting)
                return;*/

            bool staggerUpdates = true;

            // populate when loading too:
            allTriggers.Add(TriggerPriority.Lowest, new SleepyUpdater<Trigger>(Module.Sim, staggerUpdates));
            allTriggers.Add(TriggerPriority.Low, new SleepyUpdater<Trigger>(Module.Sim, staggerUpdates));
            allTriggers.Add(TriggerPriority.Normal, new SleepyUpdater<Trigger>(Module.Sim, staggerUpdates));
            allTriggers.Add(TriggerPriority.High, new SleepyUpdater<Trigger>(Module.Sim, staggerUpdates));
            allTriggers.Add(TriggerPriority.Highest, new SleepyUpdater<Trigger>(Module.Sim, staggerUpdates));

        }
             

        /// <summary>
        /// Call Entity AttachTrigger instead if possible!
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="entity"></param>
        public void RegisterTrigger(Trigger trigger)
        {          
            allTriggers[trigger.TriggerType.Priority].Add(trigger); 
        }

        /// <summary>
        /// Call Entity.DeleteTrigger if possible instead!
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="entity"></param>
        public void DeleteTrigger(Trigger trigger)
        {
            allTriggers[trigger.TriggerType.Priority].Remove(trigger);
        }



        public void Update(GameTime elapsed)       
        {
          
            //CleanupExpiredTriggers();

            UpdateTriggers(elapsed);
            
           // FireTriggers();

            #region OLD CODE
            /*for (int entityIndex = 0; entityIndex < game.Site.Entities.Count; entityIndex++)
            {
                currentEntity = game.Site.Entities[entityIndex];

                Intelligence currentEntityIntelligence;
                if (currentEntity.Find(out currentEntityIntelligence))
                {
                    if (currentEntity.TriggerUpdateRegulator.IsReady())
                    {
                        for (int i = 0; i < triggers.Count; i++) // highest priority first!!!
                        {
                            Dictionary<Trigger, Trigger> triggersAtThisLevel = triggers[(TriggerPriority)i];
                            foreach (KeyValuePair<Trigger, Trigger> kvp in triggersAtThisLevel)
                            {
                                currentTrigger = kvp.Value;
                                if (currentTrigger.Source != currentEntity)
                                {
                                    if (currentEntityIntelligence.ListeningForTriggers[(int)currentTrigger.TriggerType])
                                    {
                                        if (Common.DistanceOctile(currentEntity.MapPosition, (currentTrigger.MoveTrigger == Trigger.TriggerMovement.Attached ? currentTrigger.Source.MapPosition : currentTrigger.MapPosition))
                                            < currentTrigger.RadiusManhattan)
                                        {
                                            // fire trigger by sending message:
                                            currentTrigger.FireTrigger(currentEntity);
                                            break; // no more triggers for this entity. 
                                        }
                                    }
                                }

                            }
                        }
                    }
                }

            }*/
            #endregion

        }

        /*
        List<Trigger> expiredTriggers = new List<Trigger>();
        private void FireTriggers()
        {
                                 
            expiredTriggers.Clear();

            UpdateTriggers();
            

            for (int i = expiredTriggers.Count - 1; i >= 0; i--)
            {
                DeleteTrigger(expiredTriggers[i]);
            }
        }
        */
       

       

        private void UpdateTriggers(GameTime gameTime)
        {           
         
           
            // staggered and priority sorted update
            for (int index = 0; index < allTriggers.Count; index++)
            {
                var triggersAtThisLevel = allTriggers[(TriggerPriority)index];

                triggersAtThisLevel.Update(gameTime);              

            }

          /*  for (int index = 0; index < triggers.Count; index++)
            {                
                var triggersAtThisLevel = triggers[(TriggerPriority)index];

                // was the list changed? it needs sorting then.
                listIsDirty = triggerListIsDirty[index];

                SleepyUpdater.UpdateTimedUpdatables(triggersAtThisLevel, ref listIsDirty);

                // save the dirty status:
                triggerListIsDirty[index] = listIsDirty;

                
            }*/
            

            return;
        }


        #region ISnapshot

        public bool IsSnapshotted { get; set; }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
           
            if (sn.mode != Snapshotter.Mode.Load)
            {
                foreach (var item in allTriggers)
                {
                    List<TriggerID> triggerIDs = new List<TriggerID>();
                    item.Value.IterateItems(t => triggerIDs.Add(t.ID));
                    snapshotAllTriggers.Add(item.Key, triggerIDs);
                }                
            }
            snapshotAllTriggers = sn.DoMultiMap(snapshotAllTriggers);


            sn.Ignore(allTriggers); // recreate post-load

            return this;
        }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            // recreate the Sleepy Updaters:
            foreach (var item in snapshotAllTriggers)
            {
                foreach (var triggerID in item.Value)
                {
                    RegisterTrigger(LookUp<Trigger, TriggerID>.FindByID(triggerID));
                }                
            }
            snapshotAllTriggers.Clear();

        }

        #endregion
    }



    
}
