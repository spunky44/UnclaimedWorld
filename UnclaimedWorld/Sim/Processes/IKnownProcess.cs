using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.AI;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Entities.Containers;
using UWGame.SimSide.Maps;

namespace UWGame.SimSide.Processes
{
   

    /// <summary>
    /// this makes it possible for a job to track a process in the FOW, without knowing if it has ended...
    /// 
    /// if the process is attached to all the involved entities, it should not be possible for an entity to appear as destroyed without at the same time seeing the process directly.
    /// So either the process is 
    /// 1. a memory fact and all entities are memory facts too, or 
    /// 2. an entity is seen and the process is seen too. If an entity is seen to have been destroyed, then the process should be destroyed as well...
    /// </summary>
    public interface IKnownProcess
    {
       
        SimProcessID ProcessID { get; }

        Point? MapPosition { get; set; }

        ProcessType ProcessType { get; }

        Dictionary<EntityType, List<Tuple<EntityID, WorldLocation>>> AssignedInputs { get; set; }

        EntityID? ImmovableTool { get; set; }
        EntityID? ImmovableInput { get; set; } // this needs to be assigned during evaluation for the input hauling jobs
        Vector3? GroundLocation { get; set; } // needs this for structures. is also used for Special actions - NEW: replenish jobs also

        EntityID? ContainerToPlaceOutputsIn { get; set; }

        List<EntityID> StationaryTools { get; set; }

        /// <summary>
        /// NOTE: Salvage does not use this property, it uses ImmovableInput/input
        /// </summary>
        EntityAndRoot? ActingOnEntity { get; set; }
     

        UpgradeCategory UpgradeCategory { get; }

        /// <summary>
        /// only usable when completed
        /// </summary>
        Productivity Productivity { get; }

       
      

        bool IsStarted { get; }

        float ProgressSpeed { get; }

        bool GetKnownProgress(SharedKnowledge sharedKnowledge, out float progress);

        
        // don't think they can output entitydata...
        bool GetCurrentLocation(out Vector3? location, SharedKnowledge sharedKnowledge, bool ignoreKnowledge = false); //, ref IKnownEntityData inputData, ref IKnownEntityData toolData, SharedKnowledge sharedKnowledge);
        bool GetFixedJobLocation(out Vector3? location, SharedKnowledge sharedKnowledge, bool ignoreKnowledge = false);
        bool GetProductionSiteLocation(out Vector3? location, SharedKnowledge sharedKnowledge, bool ignoreKnowledge = false);
        bool HasFixedLocation();

        bool IsCompleted(out bool isCompleted, SharedKnowledge sharedKnowledge);
        bool OutputExists(out bool outputExists, SharedKnowledge sharedKnowledge);

        List<EntityID> OutputEntities { get; }


      //  string OutputAndLocationAsString();

       
       // bool GetOutputEntity(Predicate<Entity> predicate, out Entity matchingEntity);
      //  bool GetOutputEntityData(Predicate<IKnownEntityData> predicate, out IKnownEntityData matchingData, SharedKnowledge sharedKnowledge);
    

    }
}
