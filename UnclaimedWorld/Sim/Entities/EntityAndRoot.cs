using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWGame.SimSide.AI;
using UWGame.SimSide.AI.Goals;

namespace UWGame.SimSide.Entities
{
    /// <summary>
    /// keeping these references together makes it possible to validate them (ActingOn!) in Goals, Evaluators, Processes and managers
    /// And prevents mistakes when passing them as parameters
    /// 
    /// use this when the target is/can be a part, like the sentry machine gun
    /// 
    /// use this globally??? instead of EntityID? Seems drastic!
    /// 
    /// SimProcess needs this, because it can be an abstract object that is created in the FOW on non-existing entities.
    /// 
    /// Agents that don't validate this will chase parts (or moving entities?)
    /// store its location too?? contained? rename the struct to EntityStatus or something
    /// </summary>
    public struct EntityAndRoot
    {
        public EntityID Entity;
        public EntityID Root;

        public EntityAndRoot(EntityID entity, EntityID root)
        {
            this.Entity = entity;
            this.Root = root;
        }


        public static EntityID? GetEntity(EntityAndRoot? entityAndRoot)
        {
            if (entityAndRoot.HasValue)
            {
                return entityAndRoot.Value.Entity;
            }
            else return null;
        }

        public bool IsValid(SharedKnowledge sharedKnowledge, out IKnownEntityData actingOnEntityData)
        {
            actingOnEntityData = null;
            EntityResult result = sharedKnowledge.GetKnownData(Entity, out actingOnEntityData);

            if (GoalEvaluator.EntityDataResultCausesSkip(result)) // == EntityResult.Destroyed || result == EntityResult.EntityStatusIsNowUnknown)
            {
                return false;               
            }

            if (actingOnEntityData.RootEntityID != Root) // did the root change? the part was salvaged, that makes it invalid
            {
                return false;               
            }

            // test the root as well:
            if (Entity != Root)
            {
                IKnownEntityData rootData;
                result = sharedKnowledge.GetKnownData(Root, out rootData);

                if (GoalEvaluator.EntityDataResultCausesSkip(result)) 
                {
                    return false;
                }
            }


            return true;
        }

    }
}
