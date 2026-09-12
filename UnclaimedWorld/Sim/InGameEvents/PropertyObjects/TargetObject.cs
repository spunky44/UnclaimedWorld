using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UWGame.SimSide.Entities;
using UWGame.SimSide.InGameEvents.Actions;

namespace UWGame.SimSide.InGameEvents.PropertyObjects
{
    public enum TargetObjectType { 
        Root, // The (Current) Site object?? Default??? Can we make this the current site, or the playsite?
        World, // New...
        TriggeringEntity, 
        TargetEntity, 
        PolledEventSource, // the item that has the polled event attached to it
        DynamicTarget, // typically an entity from the list of ActionTargets currently being iterated over
        LastSpawnActionResult // the top-level entity that was the result of the last spawn action
    }

    /// <summary>
    /// has info for retrieving a target object (or list) for either setting or getting a property
    /// </summary>
    public class TargetObject
    {
        /// <summary>
        /// the starting point for applying the optional expression tree.
        /// the default is 'Site' - the global root object...
        /// </summary>
        public TargetObjectType TargetObjectType;


        /// <summary>
        /// an expression tree that we want to apply to the root element (IHasExposedProperties)   
        /// Can be chained.
        /// </summary>      
        public GetList GetList;

        public List<IHasExposedProperties> GetResult(EventAction eventAction) 
        {
            return GetResult(eventAction.TriggeringEntity, eventAction.TargetEntity, eventAction.PolledEventSource, eventAction.DynamicTarget);
        }

        /// <summary>
        /// call this to resolve the expression and get the result
        /// </summary>
        /// <param name="triggeringEntity"></param>
        /// <param name="targetEntity"></param>
        /// <returns></returns>
        public List<IHasExposedProperties> GetResult(EntityID? triggeringEntity, EntityID? targetEntity, IHasExposedProperties polledEventSource, IHasExposedProperties dynamicTarget) //, ref IHasExposedProperties firstResult)
        {
            IHasExposedProperties startObject = null;

            List<IHasExposedProperties> resultList = new List<IHasExposedProperties>();

            // first get the starting point for applying any expression elements

            switch (TargetObjectType)
            {
                case TargetObjectType.Root:
                    startObject = GetRootElement();
                    break;
                case PropertyObjects.TargetObjectType.World:
                    startObject = The.Sim.World;
                    break;
                case TargetObjectType.TriggeringEntity:
                    if (triggeringEntity.HasValue)
                    {
                        startObject = Entity.FindByID(triggeringEntity.Value);
                    }
                    break;
                case TargetObjectType.TargetEntity:
                    if (targetEntity.HasValue)
                    {
                        startObject = Entity.FindByID(targetEntity.Value);
                    }
                    break;
                case TargetObjectType.PolledEventSource:
                    startObject = polledEventSource;
                    break;
                case TargetObjectType.DynamicTarget:
                    startObject = dynamicTarget;
                    break;
                case TargetObjectType.LastSpawnActionResult:
                    startObject = Entity.FindByID(The.Sim.World.LastSpawnedEntity);
                    break;
            }

            if (startObject != null)
            {
                resultList.Add(startObject);            

                // if needed, get the object from applying the expression tree (it has only one branch):             
               
                if (GetList != null)
                {
                    resultList = GetList.GetResult(resultList, triggeringEntity, targetEntity, polledEventSource, dynamicTarget);
                }
            }     
                
            return resultList;          
        }

        public static IHasExposedProperties GetRootElement()
        {
            // may later be changed to the World??
            return The.Sim.PlaySite;
        }

        public static List<IHasExposedProperties> GetRootElementAsList()
        {            
            return new List<IHasExposedProperties>(){ GetRootElement() };
        }

        public override string ToString()
        {
            return TargetObjectType.ToString();

        }
    }
}
