using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Snapshots;
using UWGame.SimSide.Resources;

namespace UWGame.SimSide.AI.Goals
{
     /// <summary>  
     /// a special implementation of ILookUpCollectible which handles Goal instances.
     /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="Id"></typeparam>
    public class LookUpGoals: ILookUpCollectible
    {        
       
        private static Dictionary<GoalID, Goal> collection; // = new Dictionary<GoalID, Goal>();
        

        private static LookUpGoals instance;


        public static void Create()
        {
            if (instance == null)
            {
                //instantiate a singleton-like instance of lookup
                instance = new LookUpGoals();
                Sim.AddLookupCollectible(typeof(Goal), instance);

                collection = new Dictionary<GoalID, Goal>();
            }
        }

        //static ctor
       /* static LookUpGoals()
        {
            //instantiate a singleton-like instance of lookup
            instance = new LookUpGoals();
            Sim.AddLookupCollectible(typeof(Goal), instance);
        }*/

      
        public void ClearCollection()
        {            
          
            collection.Clear();
        }

        public bool SnapshotThis
        {
            get
            {
                return true;
            }
        }
      
        public int LoadPostProcessOrder
        {
            get { return 200; /* 100;*/ } // do this collection last.
        }


        public static Goal FindByID(GoalID? id)
        {
            if (id.HasValue)
            {
                Goal goal;
                collection.TryGetValue(id.Value, out goal);

                return goal;
            }

            return null;
        }


        public static List<Goal> GetGoals(Entity entity)
        {
            return collection.Where(g => g.Value.entity == entity).Select(i => i.Value).ToList();
        }


        public static void Remove(Goal goal)
        {
            collection.Remove(goal.ID);
            goal.SetInvalid();
        }


        public static void Add(GoalID goalID, Goal goal)
        {
            collection.Add(goalID, goal);
        }



        #region ISnapshot


        public ISnapshot DoSnapshot(Snapshotter sn)
        {
            
            collection = sn.DoDictionary(collection);

            sn.Ignore(instance);
         
            return this;
        }

        /// <summary>
        /// when changes are made to the fields that should be snapshotted, such as type changes, addition or removal of fields, increase this version number!
        /// </summary>
        Snapshotter.Version version = Snapshotter.Version.Original;
        public Snapshotter.Version DoVersion(Snapshotter sn)
        {
            version = sn.DoVersion(Snapshotter.Version.Original); // increase this number and make sure to add repair code in DoSnapshot to bring older versions up to this new version!
            return version;
        }

        public bool IsSnapshotted { get; set; }

        public void LoadPostProcess(Snapshotter sn)
        {
            sn.RegisterLoadPostProcessCall(this);

            // do goal think first, this creates the evaluators that the subgoals need
            foreach (var item in collection)
            {
                if ((item.Value is GoalThink))
                {
                    item.Value.LoadPostProcess(sn);
                }
            }

            foreach (var item in collection)
            {
                if (!(item.Value is GoalThink))
                {
                    item.Value.LoadPostProcess(sn);
                }
            }




        }


        #endregion


    }

}
