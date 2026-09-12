using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.InGameEvents.PropertyObjects;
using UWGame.SimSide.Entities;
using UWGame.SimSide.Systems;
using UWGame.SimSide.InGameEvents.Conditions;

namespace UWGame.SimSide.InGameEvents.Conditions
{
    /// <summary>
    /// a condition that a specified object or property in the game world has to fulfill
    /// For now, we can only test one entity in a list (the first)
    /// </summary>
    public class CustomCondition : ConditionValue
    {
        /// <summary>
        /// this can be omitted - in which case the Site will be the target object
        /// </summary>
        public TargetObject TargetObject;


        public ListCondition ListCondition;
        public PropertyCondition PropertyCondition;



        /// <summary>
        /// an expression tree that we want to apply to the data tree (IHasExposedProperties)   
        /// the list and filter to produce the object that we want to set a property on.
        /// Can be chained.
        /// </summary>
     /*   public GetObject GetObject;
        public GetList GetList;
        */

        public override bool IsFulfilled(ref Entity triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget)
        {
            if (base.IsFulfilled(ref triggeringEntity, targetEntity, polledEventSource, dynamicTarget))
            {
                List<IHasExposedProperties> resultList = null;
                EntityID? triggeringID = triggeringEntity != null ? triggeringEntity.EntityID : (EntityID?)null;

                // get the object(s) to test:
                if (TargetObject != null)
                {

                    resultList = TargetObject.GetResult(
                        triggeringID,
                        targetEntity,
                        polledEventSource, dynamicTarget);
                }
                else
                {
                    resultList = TargetObject.GetRootElementAsList();
                }


                // now test it:
                if (PropertyCondition != null)
                {
                    // extract a property on the first item and test it.
                    if (resultList != null && resultList.Count > 0)
                    {
                        // Perhaps, in future, we will allow conditions of lists of elements. Such as: all player agents are healthy/hungry/sleeping...
                        if (PropertyCondition.IsFulfilled(resultList[0], triggeringID, targetEntity, polledEventSource, dynamicTarget))
                        {
                            Entity resultAsEntity = resultList[0] as Entity;
                            if (resultAsEntity != null)
                            {
                                // #RETURNLIST
                                triggeringEntity = resultAsEntity; // NEW - store the fulfilling entity for ease of use in Actions
                            }
                            return true;
                        }
                        else return false;
                    }
                }
                else if (ListCondition != null)
                {
                    // test sum functions : min, max, equals etc...
                    return ListCondition.IsFulfilled(triggeringID, targetEntity, polledEventSource, dynamicTarget, resultList);
                }

                // default is false
                return false;
            }
            else return false;
        }

        

       /* public double? GetUpdateInterval()
        {

            return PollInterval ?? ConditionType.DefaultPollInterval;
        }*/

    }
}
